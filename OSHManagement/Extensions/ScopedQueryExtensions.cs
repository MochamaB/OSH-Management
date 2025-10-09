using OSHManagement.Models;
using OSHManagement.Models.Authorization;

namespace OSHManagement.Extensions
{
    /// <summary>
    /// Extension methods for applying scope filtering to queries
    /// Provides fluent API for filtering data based on user scope
    /// </summary>
    public static class ScopedQueryExtensions
    {
        /// <summary>
        /// Filters employees based on user scope
        /// </summary>
        public static IQueryable<Employee> WithinScope(this IQueryable<Employee> query, UserScope? scope)
        {
            if (scope == null)
                return query.Where(_ => false);

            return scope.Level switch
            {
                ScopeLevel.Organization => query,
                ScopeLevel.Station => query.Where(e => e.StationId == scope.StationId),
                ScopeLevel.Department => query.Where(e => e.DepartmentId == scope.DepartmentId),
                ScopeLevel.Team => query.Where(e => e.SupervisorPayroll == scope.PayrollNo || e.EmployeeId == scope.UserId),
                ScopeLevel.Self => query.Where(e => e.EmployeeId == scope.UserId),
                _ => query.Where(_ => false)
            };
        }

        /// <summary>
        /// Filters incidents based on user scope (by station)
        /// </summary>
        public static IQueryable<Incident> WithinScope(this IQueryable<Incident> query, UserScope? scope)
        {
            if (scope == null)
                return query.Where(_ => false);

            return scope.Level switch
            {
                ScopeLevel.Organization => query,
                ScopeLevel.Station => query.Where(i => i.StationId == scope.StationId),
                ScopeLevel.Department => query.Where(i => i.StationId == scope.StationId), // Department users see station-level incidents
                ScopeLevel.Team => query.Where(i => i.ReportedByPayroll == scope.PayrollNo || i.PersonAffectedPayroll == scope.PayrollNo),
                ScopeLevel.Self => query.Where(i => i.ReportedByPayroll == scope.PayrollNo || i.PersonAffectedPayroll == scope.PayrollNo),
                _ => query.Where(_ => false)
            };
        }

        /// <summary>
        /// Filters hazards based on user scope (by station)
        /// </summary>
        public static IQueryable<Hazard> WithinScope(this IQueryable<Hazard> query, UserScope? scope)
        {
            if (scope == null)
                return query.Where(_ => false);

            return scope.Level switch
            {
                ScopeLevel.Organization => query,
                ScopeLevel.Station => query.Where(h => h.StationId == scope.StationId),
                ScopeLevel.Department => query.Where(h => h.StationId == scope.StationId), // Department users see station-level hazards
                _ => query.Where(_ => false)
            };
        }

        /// <summary>
        /// Helper to check if employee is within scope
        /// </summary>
        public static bool IsWithinScope(this Employee employee, UserScope? scope)
        {
            if (scope == null)
                return false;

            return scope.Level switch
            {
                ScopeLevel.Organization => true,
                ScopeLevel.Station => employee.StationId == scope.StationId,
                ScopeLevel.Department => employee.DepartmentId == scope.DepartmentId,
                ScopeLevel.Team => employee.SupervisorPayroll == scope.PayrollNo || employee.EmployeeId == scope.UserId,
                ScopeLevel.Self => employee.EmployeeId == scope.UserId,
                _ => false
            };
        }
    }
}
