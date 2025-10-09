using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models.Authorization;

namespace OSHManagement.Services
{
    public class UserScopeService : IUserScopeService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OshDbContext _context;
        private readonly ILogger<UserScopeService> _logger;

        public UserScopeService(IHttpContextAccessor httpContextAccessor, OshDbContext context, ILogger<UserScopeService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _logger = logger;
        }

        public UserScope? GetCurrentUserScope()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity?.IsAuthenticated == true)
                return null;

            // Extract claims
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return null;

            var scopeClaim = user.FindFirst("Scope")?.Value;
            var stationIdClaim = user.FindFirst("StationId")?.Value;
            var departmentIdClaim = user.FindFirst("DepartmentId")?.Value;
            var payrollNo = user.FindFirst("PayrollNo")?.Value ?? string.Empty;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // Parse scope level
            ScopeLevel scopeLevel = ScopeLevel.Self; // Default to most restrictive
            if (!string.IsNullOrEmpty(scopeClaim) && Enum.TryParse<ScopeLevel>(scopeClaim, out var parsedScope))
            {
                scopeLevel = parsedScope;
            }

            // Parse station and department IDs
            int? stationId = null;
            if (!string.IsNullOrEmpty(stationIdClaim) && int.TryParse(stationIdClaim, out int parsedStationId))
            {
                stationId = parsedStationId;
            }

            int? departmentId = null;
            if (!string.IsNullOrEmpty(departmentIdClaim) && int.TryParse(departmentIdClaim, out int parsedDepartmentId))
            {
                departmentId = parsedDepartmentId;
            }

            return new UserScope
            {
                UserId = userId,
                PayrollNo = payrollNo,
                Level = scopeLevel,
                StationId = stationId,
                DepartmentId = departmentId,
                Roles = roles
            };
        }

        public async Task<UserScope?> GetUserScopeAsync(int userId)
        {
            var employee = await _context.Employees
                .Include(e => e.EmployeeRoles.Where(er => er.IsActive))
                    .ThenInclude(er => er.Role)
                .FirstOrDefaultAsync(e => e.EmployeeId == userId);

            if (employee == null)
                return null;

            var scopeLevel = await DetermineScopeLevelAsync(userId);
            var roles = employee.EmployeeRoles
                .Where(er => er.IsActive)
                .Select(er => er.Role.RoleName)
                .ToList();

            return new UserScope
            {
                UserId = employee.EmployeeId,
                PayrollNo = employee.PayrollNo,
                Level = scopeLevel,
                StationId = employee.StationId,
                DepartmentId = employee.DepartmentId,
                Roles = roles
            };
        }

        /// <summary>
        /// Determines scope level for a user based on their active roles
        /// Primary: Role-based scope (from Role.ScopeLevel)
        /// Fallback: Position-based scope (HOD/Supervisor)
        /// </summary>
        public async Task<ScopeLevel> DetermineScopeLevelAsync(int userId)
        {
            var employee = await _context.Employees
                .Include(e => e.EmployeeRoles.Where(er => er.IsActive))
                    .ThenInclude(er => er.Role)
                .FirstOrDefaultAsync(e => e.EmployeeId == userId);

            if (employee == null)
            {
                _logger.LogWarning($"Employee {userId} not found during scope determination");
                return ScopeLevel.Self;
            }

            // Get all active roles
            var activeRoles = employee.EmployeeRoles
                .Where(er => er.IsActive && er.Role.IsActive)
                .Select(er => er.Role)
                .ToList();

            if (!activeRoles.Any())
            {
                _logger.LogInformation($"Employee {userId} ({employee.PayrollNo}) has no active roles, checking position-based scope");

                // Fallback: Check position-based scope
                if (await IsHODAsync(employee.PayrollNo))
                {
                    _logger.LogInformation($"Employee {userId} is HOD (position-based), granting Department scope");
                    return ScopeLevel.Department;
                }

                if (await IsSupervisorAsync(employee.PayrollNo))
                {
                    _logger.LogInformation($"Employee {userId} is Supervisor (position-based), granting Team scope");
                    return ScopeLevel.Team;
                }

                _logger.LogInformation($"Employee {userId} has no roles or position, defaulting to Self scope");
                return ScopeLevel.Self;
            }

            // Get the broadest scope (lowest enum value = broadest access)
            // Organization = 1, Station = 2, Department = 3, Team = 4, Self = 5
            var broadestScope = activeRoles.Min(r => r.ScopeLevel);

            _logger.LogDebug($"Employee {userId} ({employee.PayrollNo}) has {activeRoles.Count} active role(s), broadest scope: {broadestScope}");

            return broadestScope;
        }

        /// <summary>
        /// Checks if the employee is a HOD (has other employees reporting to them as HOD)
        /// </summary>
        private async Task<bool> IsHODAsync(string payrollNo)
        {
            return await _context.Employees
                .AnyAsync(e => e.HodPayroll == payrollNo && e.EmploymentStatus == "Active");
        }

        /// <summary>
        /// Checks if the employee is a Supervisor (has other employees reporting to them)
        /// </summary>
        private async Task<bool> IsSupervisorAsync(string payrollNo)
        {
            return await _context.Employees
                .AnyAsync(e => e.SupervisorPayroll == payrollNo && e.EmploymentStatus == "Active");
        }
    }
}
