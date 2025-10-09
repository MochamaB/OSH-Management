using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OSHManagement.Data;
using OSHManagement.Models.Authorization;
using OSHManagement.Services;

namespace OSHManagement.Controllers
{
    /// <summary>
    /// Base controller with scope-based data access filtering
    /// All controllers that need scope protection should inherit from this
    /// </summary>
    [Authorize]
    public abstract class ScopedController : Controller
    {
        protected readonly OshDbContext _context;
        protected readonly IScopeFilterService _scopeFilter;
        protected readonly ILogger _logger;

        /// <summary>
        /// Current user's scope - lazy loaded from claims
        /// </summary>
        private UserScope? _currentScope;
        protected UserScope? CurrentScope
        {
            get
            {
                if (_currentScope == null)
                {
                    _currentScope = _scopeFilter.GetCurrentUserScope();
                }
                return _currentScope;
            }
        }

        protected ScopedController(
            OshDbContext context,
            IScopeFilterService scopeFilter,
            ILogger logger)
        {
            _context = context;
            _scopeFilter = scopeFilter;
            _logger = logger;
        }

        /// <summary>
        /// Applies scope filtering to a query
        /// This is the primary method controllers should use to ensure scope protection
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="query">Base query</param>
        /// <returns>Scoped query</returns>
        protected IQueryable<T> ApplyScope<T>(IQueryable<T> query) where T : class
        {
            return _scopeFilter.ApplyScope(query, CurrentScope);
        }

        /// <summary>
        /// Helper to check if current user can see all organization data
        /// </summary>
        protected bool CanSeeAllData => CurrentScope?.CanSeeAllData ?? false;

        /// <summary>
        /// Helper to check if current user can only see their own data
        /// </summary>
        protected bool CanOnlySeeSelf => CurrentScope?.CanOnlySeeSelf ?? true;

        /// <summary>
        /// Helper to check if current user has a specific role
        /// </summary>
        protected bool HasRole(params string[] roleNames)
        {
            return CurrentScope?.HasRole(roleNames) ?? false;
        }

        /// <summary>
        /// Gets current user's employee ID
        /// </summary>
        protected int CurrentUserId => CurrentScope?.UserId ?? 0;

        /// <summary>
        /// Gets current user's payroll number
        /// </summary>
        protected string CurrentPayrollNo => CurrentScope?.PayrollNo ?? string.Empty;

        /// <summary>
        /// Gets current user's station ID (if assigned)
        /// </summary>
        protected int? CurrentStationId => CurrentScope?.StationId;

        /// <summary>
        /// Gets current user's department ID (if assigned)
        /// </summary>
        protected int? CurrentDepartmentId => CurrentScope?.DepartmentId;

        /// <summary>
        /// Checks if current user can access a specific employee's data
        /// </summary>
        protected async Task<bool> CanAccessEmployeeAsync(int employeeId)
        {
            if (CurrentScope == null)
                return false;

            if (CurrentScope.IsOrganizationScope)
                return true;

            if (CurrentScope.IsSelfScope)
                return employeeId == CurrentScope.UserId;

            // For other scopes, check if employee is in scope
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                return false;

            return CurrentScope.Level switch
            {
                ScopeLevel.Station => employee.StationId == CurrentScope.StationId,
                ScopeLevel.Department => employee.DepartmentId == CurrentScope.DepartmentId,
                ScopeLevel.Team => employee.SupervisorPayroll == CurrentScope.PayrollNo || employee.EmployeeId == CurrentScope.UserId,
                _ => false
            };
        }

        /// <summary>
        /// Returns a Forbid result with logging
        /// </summary>
        protected IActionResult ForbidWithLog(string message)
        {
            _logger.LogWarning($"Access denied for user {CurrentUserId}: {message}");
            return Forbid();
        }
    }
}
