using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using System.Security.Claims;

namespace OSHManagement.Services
{
    public interface IAuthenticationService
    {
        Task<(bool Success, Employee? Employee, List<Claim>? Claims)> AuthenticateAsync(string username, string password);
        Task MigratePasswordAsync(int employeeId, string password);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly OshDbContext _context;
        private readonly IPasswordHashService _passwordHashService;
        private readonly ILegacyPasswordService _legacyPasswordService;
        private readonly IUserScopeService _userScopeService;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            OshDbContext context,
            IPasswordHashService passwordHashService,
            ILegacyPasswordService legacyPasswordService,
            IUserScopeService userScopeService,
            ILogger<AuthenticationService> logger)
        {
            _context = context;
            _passwordHashService = passwordHashService;
            _legacyPasswordService = legacyPasswordService;
            _userScopeService = userScopeService;
            _logger = logger;
        }

        public async Task<(bool Success, Employee? Employee, List<Claim>? Claims)> AuthenticateAsync(
            string username,
            string password)
        {
            // Find employee by username or payroll number
            var employee = await _context.Employees
                .Include(e => e.Station)
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(e =>
                    (e.Username == username || e.PayrollNo == username) &&
                    e.EmploymentStatus == "Active");

            if (employee == null)
            {
                _logger.LogWarning($"Login failed: User not found - {username}");
                return (false, null, null);
            }

            bool isPasswordValid = false;
            bool needsPasswordMigration = false;

            // Step 1: Try PasswordHash verification (modern approach)
            if (!string.IsNullOrEmpty(employee.PasswordHash))
            {
                isPasswordValid = _passwordHashService.VerifyPassword(password, employee.PasswordHash);
                _logger.LogInformation($"SHA256 verification for {username}: {isPasswordValid}");
            }
            // Step 2: Try LegacyPassword verification (pwdcompare)
            else if (employee.LegacyPassword != null && employee.LegacyPassword.Length > 0)
            {
                isPasswordValid = await _legacyPasswordService.VerifyLegacyPasswordAsync(
                    employee.PayrollNo,
                    password);

                if (isPasswordValid)
                {
                    needsPasswordMigration = true;
                    _logger.LogInformation($"Legacy password verification successful for {username}. Needs migration.");
                }
            }

            if (!isPasswordValid)
            {
                _logger.LogWarning($"Login failed: Invalid password - {username}");
                return (false, null, null);
            }

            // Migrate password if using legacy
            if (needsPasswordMigration)
            {
                await MigratePasswordAsync(employee.EmployeeId, password);
            }

            // Build claims
            var claims = await BuildClaimsAsync(employee);

            _logger.LogInformation($"Login successful: {username}");
            return (true, employee, claims);
        }

        public async Task MigratePasswordAsync(int employeeId, string password)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee != null)
                {
                    employee.PasswordHash = _passwordHashService.HashPassword(password);
                    employee.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Password migrated for EmployeeId: {employeeId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error migrating password for EmployeeId: {employeeId}");
            }
        }

        private async Task<List<Claim>> BuildClaimsAsync(Employee employee)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
                new Claim(ClaimTypes.Name, employee.Username ?? employee.PayrollNo),
                new Claim("PayrollNo", employee.PayrollNo),
                new Claim("FullName", $"{employee.FirstName} {employee.LastName}"),
                new Claim("Email", employee.EmailAddress ?? ""),
                new Claim("StationId", employee.StationId.ToString()),
                new Claim("StationName", employee.Station?.StationName ?? "")
            };

            // Determine and add scope claims
            var scopeLevel = await _userScopeService.DetermineScopeLevelAsync(employee.EmployeeId);
            claims.Add(new Claim("Scope", scopeLevel.ToString()));

            // Add DepartmentId if available
            if (employee.DepartmentId.HasValue)
            {
                claims.Add(new Claim("DepartmentId", employee.DepartmentId.Value.ToString()));
            }

            // Add roles
            foreach (var employeeRole in employee.EmployeeRoles.Where(er => er.IsActive))
            {
                claims.Add(new Claim(ClaimTypes.Role, employeeRole.Role.RoleName));

                // Add permissions from this role
                foreach (var rolePermission in employeeRole.Role.RolePermissions)
                {
                    claims.Add(new Claim("Permission", rolePermission.Permission.PermissionName));
                }
            }

            return claims;
        }
    }
}
