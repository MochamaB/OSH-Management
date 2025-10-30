using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Models.Authorization;
using OSHManagement.Models.DTOs;
using OSHManagement.Models.DTOs.Dropdowns;

namespace OSHManagement.Services
{
    /// <summary>
    /// Implementation of IEmployeeService
    /// Handles Employee queries with STRICT scope enforcement
    /// FAIL-SAFE: Returns empty list if no scope provided
    /// </summary>
    public class EmployeeService : IEmployeeService
    {
        private readonly OshDbContext _context;
        private readonly IScopeFilterService _scopeFilterService;

        public EmployeeService(
            OshDbContext context,
            IScopeFilterService scopeFilterService)
        {
            _context = context;
            _scopeFilterService = scopeFilterService;
        }

        public async Task<List<EmployeeDropdownDto>> GetActiveEmployeesAsync(UserScope? scope = null)
        {
            var query = _context.Employees.Where(e => e.EmploymentStatus == "Active");

            // CRITICAL: Always apply scope for employees (security-critical)
            // Fail-safe: Return empty if no scope provided
            if (scope == null)
            {
                return new List<EmployeeDropdownDto>();
            }

            query = _scopeFilterService.ApplyScope(query, scope);

            return await query
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .Select(e => new EmployeeDropdownDto
                {
                    EmployeeId = e.EmployeeId,
                    PayrollNo = e.PayrollNo,
                    FullName = e.FirstName + " " + e.LastName,
                    StationId = e.StationId,
                    DepartmentId = e.DepartmentId
                })
                .ToListAsync();
        }

        public async Task<List<EmployeeDropdownDto>> GetEmployeesByStationAsync(int stationId, UserScope? scope = null)
        {
            var query = _context.Employees
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                .Where(e => e.EmploymentStatus == "Active" && e.StationId == stationId);

            // CRITICAL: Apply scope BEFORE returning (security first!)
            // Scope takes precedence over station filter
            if (scope == null)
            {
                return new List<EmployeeDropdownDto>();
            }

            query = _scopeFilterService.ApplyScope(query, scope);

            return await query
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .Select(e => new EmployeeDropdownDto
                {
                    EmployeeId = e.EmployeeId,
                    PayrollNo = e.PayrollNo,
                    FullName = e.FirstName + " " + e.LastName,
                    StationId = e.StationId,
                    DepartmentId = e.DepartmentId,
                    Designation = e.Designation,
                    RoleNames = string.Join(", ", e.EmployeeRoles.Where(er => er.IsActive).Select(er => er.Role.RoleName))
                })
                .ToListAsync();
        }

        public async Task<List<EmployeeDropdownDto>> GetEmployeesByDepartmentAsync(int departmentId, UserScope? scope = null)
        {
            var query = _context.Employees
                .Where(e => e.EmploymentStatus == "Active" && e.DepartmentId == departmentId);

            // CRITICAL: Apply scope filtering
            if (scope == null)
            {
                return new List<EmployeeDropdownDto>();
            }

            query = _scopeFilterService.ApplyScope(query, scope);

            return await query
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .Select(e => new EmployeeDropdownDto
                {
                    EmployeeId = e.EmployeeId,
                    PayrollNo = e.PayrollNo,
                    FullName = e.FirstName + " " + e.LastName,
                    StationId = e.StationId,
                    DepartmentId = e.DepartmentId
                })
                .ToListAsync();
        }

        public async Task<List<EmployeeDropdownDto>> GetEmployeesByStationAndDepartmentAsync(
            int stationId, 
            int? departmentId = null, 
            UserScope? scope = null)
        {
            var query = _context.Employees
                .Where(e => e.EmploymentStatus == "Active" && e.StationId == stationId);

            // Apply department filter if provided
            if (departmentId.HasValue)
            {
                query = query.Where(e => e.DepartmentId == departmentId.Value);
            }

            // CRITICAL: Apply scope filtering
            if (scope == null)
            {
                return new List<EmployeeDropdownDto>();
            }

            query = _scopeFilterService.ApplyScope(query, scope);

            return await query
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .Select(e => new EmployeeDropdownDto
                {
                    EmployeeId = e.EmployeeId,
                    PayrollNo = e.PayrollNo,
                    FullName = e.FirstName + " " + e.LastName,
                    StationId = e.StationId,
                    DepartmentId = e.DepartmentId
                })
                .ToListAsync();
        }

        public async Task<List<EmployeeDropdownDto>> GetHodCandidatesAsync(UserScope? scope = null)
        {
            // HOD candidates: Active employees (could add additional criteria)
            var query = _context.Employees
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                .Where(e => e.EmploymentStatus == "Active");

            // CRITICAL: Apply scope filtering
            if (scope == null)
            {
                return new List<EmployeeDropdownDto>();
            }

            query = _scopeFilterService.ApplyScope(query, scope);

            return await query
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .Select(e => new EmployeeDropdownDto
                {
                    EmployeeId = e.EmployeeId,
                    PayrollNo = e.PayrollNo,
                    FullName = e.FirstName + " " + e.LastName,
                    StationId = e.StationId,
                    DepartmentId = e.DepartmentId,
                    Designation = e.Designation,
                    RoleNames = string.Join(", ", e.EmployeeRoles.Where(er => er.IsActive).Select(er => er.Role.RoleName))
                })
                .ToListAsync();
        }

        public async Task<List<EmployeeDropdownDto>> GetSupervisorCandidatesAsync(UserScope? scope = null)
        {
            // Supervisor candidates: Active employees (could add additional criteria)
            var query = _context.Employees
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                .Where(e => e.EmploymentStatus == "Active");

            // CRITICAL: Apply scope filtering
            if (scope == null)
            {
                return new List<EmployeeDropdownDto>();
            }

            query = _scopeFilterService.ApplyScope(query, scope);

            return await query
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .Select(e => new EmployeeDropdownDto
                {
                    EmployeeId = e.EmployeeId,
                    PayrollNo = e.PayrollNo,
                    FullName = e.FirstName + " " + e.LastName,
                    StationId = e.StationId,
                    DepartmentId = e.DepartmentId,
                    Designation = e.Designation,
                    RoleNames = string.Join(", ", e.EmployeeRoles.Where(er => er.IsActive).Select(er => er.Role.RoleName))
                })
                .ToListAsync();
        }

        public async Task<Dictionary<string, string>> GetEmployeeNamesByPayrollsAsync(
            List<string> payrolls, 
            UserScope? scope = null)
        {
            if (payrolls == null || !payrolls.Any())
            {
                return new Dictionary<string, string>();
            }

            var query = _context.Employees
                .Where(e => payrolls.Contains(e.PayrollNo));

            // CRITICAL: Apply scope filtering
            if (scope == null)
            {
                return new Dictionary<string, string>();
            }

            query = _scopeFilterService.ApplyScope(query, scope);

            return await query
                .Select(e => new
                {
                    e.PayrollNo,
                    FullName = e.FirstName + " " + e.LastName
                })
                .ToDictionaryAsync(e => e.PayrollNo, e => e.FullName);
        }

        public async Task<List<EmployeeSelectionDto>> GetEmployeesForRoleAssignmentAsync(UserScope? scope = null)
        {
            var query = _context.Employees
                .Where(e => e.EmploymentStatus == "Active");

            // CRITICAL: Always apply scope for employees (security-critical)
            // Fail-safe: Return empty if no scope provided
            if (scope == null)
            {
                return new List<EmployeeSelectionDto>();
            }

            query = _scopeFilterService.ApplyScope(query, scope);

            return await query
                .Include(e => e.Station)
                .Include(e => e.Department)
                .OrderBy(e => e.Station.StationName)
                    .ThenBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                .Select(e => new EmployeeSelectionDto
                {
                    EmployeeId = e.EmployeeId,
                    PayrollNo = e.PayrollNo,
                    FullName = e.FirstName + " " + e.LastName,
                    Designation = e.Designation,
                    StationId = e.StationId,
                    StationName = e.Station.StationName,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department != null ? e.Department.DepartmentName : null
                })
                .ToListAsync();
        }
    }
}
