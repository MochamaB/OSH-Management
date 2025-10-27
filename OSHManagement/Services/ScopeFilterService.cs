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

            // Station filtering (Organizational Hierarchy)
            if (entityType == typeof(Station))
            {
                return (IQueryable<T>)ApplyStationScope(query.Cast<Station>(), scope);
            }

            // Department filtering (Organizational Hierarchy)
            if (entityType == typeof(Department))
            {
                return (IQueryable<T>)ApplyDepartmentScope(query.Cast<Department>(), scope);
            }

            // Section filtering (Organizational Hierarchy)
            if (entityType == typeof(Section))
            {
                return (IQueryable<T>)ApplySectionScope(query.Cast<Section>(), scope);
            }

            // Employee filtering (Transactional)
            if (entityType == typeof(Employee))
            {
                return (IQueryable<T>)ApplyEmployeeScope(query.Cast<Employee>(), scope);
            }

            // Incident filtering (Transactional)
            if (entityType == typeof(Incident))
            {
                return (IQueryable<T>)ApplyIncidentScope(query.Cast<Incident>(), scope);
            }

            // Hazard filtering (Transactional)
            if (entityType == typeof(Hazard))
            {
                return (IQueryable<T>)ApplyHazardScope(query.Cast<Hazard>(), scope);
            }

            // Team filtering (Transactional)
            if (entityType == typeof(Team))
            {
                return (IQueryable<T>)ApplyTeamScope(query.Cast<Team>(), scope);
            }

            // MediaFile filtering (Media Management)
            if (entityType == typeof(MediaFile))
            {
                return (IQueryable<T>)ApplyMediaFileScope(query.Cast<MediaFile>(), scope);
            }

            // MediaCollection filtering (Media Management)
            if (entityType == typeof(MediaCollection))
            {
                return (IQueryable<T>)ApplyMediaCollectionScope(query.Cast<MediaCollection>(), scope);
            }

            // MediaAssociation filtering (Media Management)
            if (entityType == typeof(MediaAssociation))
            {
                return (IQueryable<T>)ApplyMediaAssociationScope(query.Cast<MediaAssociation>(), scope);
            }

            // Default: if entity doesn't have scope filtering, return as-is
            // (e.g., OrgCategories, Roles, Permissions - reference data)
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

        /// <summary>
        /// Apply scope filtering to Team queries
        /// Teams are station-scoped entities - users can only see teams within their scope
        /// Team/Self scope users can only see teams they are members of
        /// </summary>
        private IQueryable<Team> ApplyTeamScope(IQueryable<Team> query, UserScope scope)
        {
            return scope.Level switch
            {
                // Station scope: Users can see all teams in their station
                ScopeLevel.Station => query.Where(t => t.StationId == scope.StationId),

                // Department scope: Users can see all teams in their station (teams are station-level, not department-level)
                ScopeLevel.Department => query.Where(t => t.StationId == scope.StationId),

                // Team/Self scope: Users can only see teams where they are active members
                ScopeLevel.Team => query.Where(t => t.TeamMembers.Any(tm =>
                    tm.EmployeePayroll == scope.PayrollNo && tm.IsActive)),
                ScopeLevel.Self => query.Where(t => t.TeamMembers.Any(tm =>
                    tm.EmployeePayroll == scope.PayrollNo && tm.IsActive)),

                // Organization scope sees all teams (handled in ApplyScope method)
                _ => query
            };
        }

        /// <summary>
        /// Apply scope filtering to Station queries
        /// Stations represent organizational hierarchy - users can only see stations within their scope
        /// </summary>
        private IQueryable<Station> ApplyStationScope(IQueryable<Station> query, UserScope scope)
        {
            return scope.Level switch
            {
                // Station scope: Users can ONLY see their assigned station
                ScopeLevel.Station => query.Where(s => s.StationId == scope.StationId),
                
                // Department/Team/Self: Inherit from station - can only see their station
                ScopeLevel.Department => query.Where(s => s.StationId == scope.StationId),
                ScopeLevel.Team => query.Where(s => s.StationId == scope.StationId),
                ScopeLevel.Self => query.Where(s => s.StationId == scope.StationId),
                
                // Organization scope sees all stations (handled in ApplyScope method)
                _ => query
            };
        }

        /// <summary>
        /// Apply scope filtering to Department queries
        /// CRITICAL: Department scope users can ONLY see THEIR department (Principle of Least Privilege)
        /// This prevents Department Heads from assigning employees to other departments
        /// </summary>
        private IQueryable<Department> ApplyDepartmentScope(IQueryable<Department> query, UserScope scope)
        {
            return scope.Level switch
            {
                // Station scope: Users can see ALL departments in their station
                ScopeLevel.Station => query.Where(d => d.StationId == scope.StationId),
                
                // ⚠️ CRITICAL: Department scope users can ONLY see their own department
                // This enforces Principle of Least Privilege - they cannot assign to other departments
                // BUG FIX: Using DepartmentId (not StationId) to restrict to single department
                ScopeLevel.Department => query.Where(d => d.DepartmentId == scope.DepartmentId),
                
                // Team/Self: Inherit from department - can only see their department
                ScopeLevel.Team => query.Where(d => d.DepartmentId == scope.DepartmentId),
                ScopeLevel.Self => query.Where(d => d.DepartmentId == scope.DepartmentId),
                
                // Organization scope sees all departments (handled in ApplyScope method)
                _ => query
            };
        }

        /// <summary>
        /// Apply scope filtering to Section queries
        /// Sections follow station hierarchy for scope determination
        /// </summary>
        private IQueryable<Section> ApplySectionScope(IQueryable<Section> query, UserScope scope)
        {
            return scope.Level switch
            {
                // Station scope: Users can see all sections in their station
                ScopeLevel.Station => query.Where(s => s.StationId == scope.StationId),
                
                // Department/Team/Self: Inherit from station - can see sections in their station
                // Note: Sections are linked to Stations directly via StationId
                ScopeLevel.Department => query.Where(s => s.StationId == scope.StationId),
                ScopeLevel.Team => query.Where(s => s.StationId == scope.StationId),
                ScopeLevel.Self => query.Where(s => s.StationId == scope.StationId),
                
                // Organization scope sees all sections (handled in ApplyScope method)
                _ => query
            };
        }

        /// <summary>
        /// Apply scope filtering to MediaFile queries
        /// Users see files based on who uploaded them
        /// </summary>
        private IQueryable<MediaFile> ApplyMediaFileScope(IQueryable<MediaFile> query, UserScope scope)
        {
            return scope.Level switch
            {
                // Organization: See all files
                ScopeLevel.Organization => query,

                // Station: See files uploaded by station members
                ScopeLevel.Station => query.Where(mf =>
                    mf.UploadedBy.StationId == scope.StationId
                ),

                // Department: See files uploaded by department members
                ScopeLevel.Department => query.Where(mf =>
                    mf.UploadedBy.DepartmentId == scope.DepartmentId
                ),

                // Team/Self: See only own uploaded files
                ScopeLevel.Team => query.Where(mf =>
                    mf.UploadedByPayroll == scope.PayrollNo
                ),

                ScopeLevel.Self => query.Where(mf =>
                    mf.UploadedByPayroll == scope.PayrollNo
                ),

                _ => query.Where(_ => false)
            };
        }

        /// <summary>
        /// Apply scope filtering to MediaCollection queries
        /// Collections are filtered by role-based access and visibility
        /// </summary>
        private IQueryable<MediaCollection> ApplyMediaCollectionScope(IQueryable<MediaCollection> query, UserScope scope)
        {
            // Organization scope sees all collections
            if (scope.IsOrganizationScope)
                return query;

            // Filter by AllowedRoles (stored as JSON array) and public visibility
            // Note: This is a simplified version - actual implementation would need JSON parsing
            return query.Where(mc =>
                // Public collections visible to all
                mc.IsPublic ||
                // Collections with no role restrictions
                mc.AllowedRoles == null
                // For role-based filtering, you would parse JSON and check scope.Roles
            );
        }

        /// <summary>
        /// Apply scope filtering to MediaAssociation queries
        /// Associations are filtered based on the associated media file's uploader
        /// </summary>
        private IQueryable<MediaAssociation> ApplyMediaAssociationScope(IQueryable<MediaAssociation> query, UserScope scope)
        {
            return scope.Level switch
            {
                // Organization: See all associations
                ScopeLevel.Organization => query,

                // Station: See associations for station files
                ScopeLevel.Station => query.Where(ma =>
                    ma.Media.UploadedBy.StationId == scope.StationId
                ),

                // Department: See associations for department files
                ScopeLevel.Department => query.Where(ma =>
                    ma.Media.UploadedBy.DepartmentId == scope.DepartmentId
                ),

                // Team/Self: See associations for own files
                ScopeLevel.Team => query.Where(ma =>
                    ma.Media.UploadedByPayroll == scope.PayrollNo
                ),

                ScopeLevel.Self => query.Where(ma =>
                    ma.Media.UploadedByPayroll == scope.PayrollNo
                ),

                _ => query.Where(_ => false)
            };
        }
    }
}
