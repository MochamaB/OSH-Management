using Microsoft.EntityFrameworkCore;
using OSHManagement.Models;
using OSHManagement.Models.Authorization;

namespace OSHManagement.Services
{
    public class ScopeFilterService : IScopeFilterService
    {
        private readonly IUserScopeService _userScopeService;

        public ScopeFilterService(IUserScopeService userScopeService)
        {
            _userScopeService = userScopeService;
        }

        public UserScope? GetCurrentUserScope()
        {
            return _userScopeService.GetCurrentUserScope();
        }

        public IQueryable<T> ApplyScope<T>(IQueryable<T> query, UserScope? userScope = null) where T : class
        {
            var scope = userScope ?? GetCurrentUserScope();

            // If no scope (unauthenticated), return empty result
            if (scope == null)
                return query.Where(_ => false);

            // Organization scope sees everything
            if (scope.IsOrganizationScope)
                return query;

            // Apply filtering based on entity type and scope level
            return ApplyScopeByType(query, scope);
        }

        private IQueryable<T> ApplyScopeByType<T>(IQueryable<T> query, UserScope scope) where T : class
        {
            var entityType = typeof(T);

            // Employee filtering
            if (entityType == typeof(Employee))
            {
                return (IQueryable<T>)ApplyEmployeeScope(query.Cast<Employee>(), scope);
            }

            // Incident filtering
            if (entityType == typeof(Incident))
            {
                return (IQueryable<T>)ApplyIncidentScope(query.Cast<Incident>(), scope);
            }

            // Hazard filtering
            if (entityType == typeof(Hazard))
            {
                return (IQueryable<T>)ApplyHazardScope(query.Cast<Hazard>(), scope);
            }

            // Add other entity types as needed...

            // Default: if entity doesn't have scope filtering, return as-is
            return query;
        }

        private IQueryable<Employee> ApplyEmployeeScope(IQueryable<Employee> query, UserScope scope)
        {
            return scope.Level switch
            {
                ScopeLevel.Station => query.Where(e => e.StationId == scope.StationId),
                ScopeLevel.Department => query.Where(e => e.DepartmentId == scope.DepartmentId),
                ScopeLevel.Team => query.Where(e => e.SupervisorPayroll == scope.PayrollNo || e.PayrollNo == scope.PayrollNo),
                ScopeLevel.Self => query.Where(e => e.EmployeeId == scope.UserId),
                _ => query
            };
        }

        private IQueryable<Incident> ApplyIncidentScope(IQueryable<Incident> query, UserScope scope)
        {
            return scope.Level switch
            {
                ScopeLevel.Station => query.Where(i => i.StationId == scope.StationId),
                ScopeLevel.Department => query.Where(i => i.StationId == scope.StationId), // Department users see station-level incidents
                ScopeLevel.Team => query.Where(i => i.ReportedByPayroll == scope.PayrollNo || i.PersonAffectedPayroll == scope.PayrollNo),
                ScopeLevel.Self => query.Where(i => i.ReportedByPayroll == scope.PayrollNo || i.PersonAffectedPayroll == scope.PayrollNo),
                _ => query
            };
        }

        private IQueryable<Hazard> ApplyHazardScope(IQueryable<Hazard> query, UserScope scope)
        {
            return scope.Level switch
            {
                ScopeLevel.Station => query.Where(h => h.StationId == scope.StationId),
                ScopeLevel.Department => query.Where(h => h.StationId == scope.StationId), // Department users see station-level hazards
                _ => query
            };
        }
    }
}
