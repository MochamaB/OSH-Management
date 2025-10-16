using OSHManagement.Models.Authorization;
using OSHManagement.Models.DTOs;
using OSHManagement.Models.DTOs.Dropdowns;

namespace OSHManagement.Services
{
    /// <summary>
    /// Service for Employee dropdown data
    /// ALWAYS applies scope filtering - security-critical
    /// Uses ScopeFilterService for consistent scope enforcement
    /// Fail-safe: Returns empty list if no scope provided
    /// </summary>
    public interface IEmployeeService
    {
        /// <summary>
        /// Get all active employees for dropdown (with scope filtering)
        /// CRITICAL: Always applies scope - returns empty if no scope provided
        /// </summary>
        Task<List<EmployeeDropdownDto>> GetActiveEmployeesAsync(UserScope? scope = null);

        /// <summary>
        /// Get employees filtered by station (with scope filtering)
        /// Scope takes precedence over station filter
        /// </summary>
        Task<List<EmployeeDropdownDto>> GetEmployeesByStationAsync(int stationId, UserScope? scope = null);

        /// <summary>
        /// Get employees filtered by department (with scope filtering)
        /// </summary>
        Task<List<EmployeeDropdownDto>> GetEmployeesByDepartmentAsync(int departmentId, UserScope? scope = null);

        /// <summary>
        /// Get employees filtered by station and optionally by department (with scope filtering)
        /// Used for cascading dropdowns: Station → Department → Employee
        /// </summary>
        Task<List<EmployeeDropdownDto>> GetEmployeesByStationAndDepartmentAsync(
            int stationId, 
            int? departmentId = null, 
            UserScope? scope = null);

        /// <summary>
        /// Get HOD candidates (with scope filtering)
        /// Typically employees eligible to be Department Heads
        /// </summary>
        Task<List<EmployeeDropdownDto>> GetHodCandidatesAsync(UserScope? scope = null);

        /// <summary>
        /// Get supervisor candidates (with scope filtering)
        /// Typically employees eligible to be Supervisors
        /// </summary>
        Task<List<EmployeeDropdownDto>> GetSupervisorCandidatesAsync(UserScope? scope = null);

        /// <summary>
        /// Get employee names by payroll numbers (with scope filtering)
        /// Used for displaying supervisor/HOD names in lists
        /// Returns dictionary: PayrollNo → FullName
        /// </summary>
        Task<Dictionary<string, string>> GetEmployeeNamesByPayrollsAsync(
            List<string> payrolls,
            UserScope? scope = null);

        /// <summary>
        /// Get employees for role assignment (with scope filtering)
        /// Includes station and department names for display in checkboxes
        /// Used in role creation wizard to assign initial employees
        /// CRITICAL: Always applies scope - returns empty if no scope provided
        /// </summary>
        Task<List<EmployeeSelectionDto>> GetEmployeesForRoleAssignmentAsync(UserScope? scope = null);
    }
}
