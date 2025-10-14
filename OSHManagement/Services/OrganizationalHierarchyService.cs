using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Models.Authorization;
using OSHManagement.Models.DTOs.Dropdowns;

namespace OSHManagement.Services
{
    /// <summary>
    /// Implementation of IOrganizationalHierarchyService
    /// Handles Station, Department, and Section queries with scope awareness
    /// Uses ScopeFilterService for consistent scope enforcement
    /// </summary>
    public class OrganizationalHierarchyService : IOrganizationalHierarchyService
    {
        private readonly OshDbContext _context;
        private readonly IScopeFilterService _scopeFilterService;

        public OrganizationalHierarchyService(
            OshDbContext context,
            IScopeFilterService scopeFilterService)
        {
            _context = context;
            _scopeFilterService = scopeFilterService;
        }

        #region Stations

        public async Task<List<StationDropdownDto>> GetActiveStationsAsync(UserScope? scope = null)
        {
            var query = _context.Stations.Where(s => s.IsActive);

            // Apply scope filtering (Organization scope sees all, others see their station)
            if (scope != null)
            {
                query = _scopeFilterService.ApplyScope(query, scope);
            }

            return await query
                .OrderBy(s => s.StationName)
                .Select(s => new StationDropdownDto
                {
                    StationId = s.StationId,
                    StationName = s.StationName,
                    OrgCategoryId = s.OrgCategoryId
                })
                .ToListAsync();
        }

        public async Task<List<StationDropdownDto>> GetStationsByCategoryAsync(int categoryId, UserScope? scope = null)
        {
            var query = _context.Stations
                .Where(s => s.IsActive && s.OrgCategoryId == categoryId);

            // CRITICAL: Apply scope BEFORE returning (security first!)
            // Scope takes precedence over category filter
            if (scope != null)
            {
                query = _scopeFilterService.ApplyScope(query, scope);
            }

            return await query
                .OrderBy(s => s.StationName)
                .Select(s => new StationDropdownDto
                {
                    StationId = s.StationId,
                    StationName = s.StationName,
                    OrgCategoryId = s.OrgCategoryId
                })
                .ToListAsync();
        }

        public async Task<StationDropdownDto?> GetStationByIdAsync(int stationId, UserScope? scope = null)
        {
            var query = _context.Stations
                .Where(s => s.StationId == stationId && s.IsActive);

            // Apply scope validation
            if (scope != null)
            {
                query = _scopeFilterService.ApplyScope(query, scope);
            }

            return await query
                .Select(s => new StationDropdownDto
                {
                    StationId = s.StationId,
                    StationName = s.StationName,
                    OrgCategoryId = s.OrgCategoryId
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> StationExistsAsync(int stationId, UserScope? scope = null)
        {
            var query = _context.Stations
                .Where(s => s.StationId == stationId && s.IsActive);

            // Apply scope validation
            if (scope != null)
            {
                query = _scopeFilterService.ApplyScope(query, scope);
            }

            return await query.AnyAsync();
        }

        public async Task<StationDropdownDto?> GetCurrentUserStationAsync(UserScope scope)
        {
            // Organization scope users don't have a specific station
            if (scope.StationId == null)
                return null;

            return await _context.Stations
                .Where(s => s.StationId == scope.StationId && s.IsActive)
                .Select(s => new StationDropdownDto
                {
                    StationId = s.StationId,
                    StationName = s.StationName,
                    OrgCategoryId = s.OrgCategoryId
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int?> GetCurrentUserStationCategoryAsync(UserScope scope)
        {
            // Organization scope users don't have a specific station
            if (scope.StationId == null)
                return null;

            return await _context.Stations
                .Where(s => s.StationId == scope.StationId)
                .Select(s => s.OrgCategoryId)
                .FirstOrDefaultAsync();
        }

        #endregion

        #region Departments

        public async Task<List<DepartmentDropdownDto>> GetActiveDepartmentsAsync(UserScope? scope = null)
        {
            var query = _context.Departments.Where(d => d.IsActive);

            // Apply scope filtering
            // Organization: All departments
            // Station: Departments in user's station
            // Department: ONLY user's department (Principle of Least Privilege)
            if (scope != null)
            {
                query = _scopeFilterService.ApplyScope(query, scope);
            }

            return await query
                .OrderBy(d => d.DepartmentName)
                .Select(d => new DepartmentDropdownDto
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    StationId = d.StationId
                })
                .ToListAsync();
        }

        public async Task<List<DepartmentDropdownDto>> GetDepartmentsByStationAsync(int stationId, UserScope? scope = null)
        {
            var query = _context.Departments
                .Where(d => d.IsActive && d.StationId == stationId);

            // CRITICAL: Apply scope BEFORE returning (security first!)
            // Department scope users will only see THEIR department even if they query by station
            if (scope != null)
            {
                query = _scopeFilterService.ApplyScope(query, scope);
            }

            return await query
                .OrderBy(d => d.DepartmentName)
                .Select(d => new DepartmentDropdownDto
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    StationId = d.StationId
                })
                .ToListAsync();
        }

        public async Task<DepartmentDropdownDto?> GetDepartmentByIdAsync(int departmentId, UserScope? scope = null)
        {
            var query = _context.Departments
                .Where(d => d.DepartmentId == departmentId && d.IsActive);

            // Apply scope validation
            if (scope != null)
            {
                query = _scopeFilterService.ApplyScope(query, scope);
            }

            return await query
                .Select(d => new DepartmentDropdownDto
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    StationId = d.StationId
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DepartmentExistsAsync(int departmentId, UserScope? scope = null)
        {
            var query = _context.Departments
                .Where(d => d.DepartmentId == departmentId && d.IsActive);

            // Apply scope validation
            if (scope != null)
            {
                query = _scopeFilterService.ApplyScope(query, scope);
            }

            return await query.AnyAsync();
        }

        public async Task<DepartmentDropdownDto?> GetCurrentUserDepartmentAsync(UserScope scope)
        {
            // Organization/Station scope users don't have a specific department
            if (scope.DepartmentId == null)
                return null;

            return await _context.Departments
                .Where(d => d.DepartmentId == scope.DepartmentId && d.IsActive)
                .Select(d => new DepartmentDropdownDto
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    StationId = d.StationId
                })
                .FirstOrDefaultAsync();
        }

        #endregion

        #region Sections

        public async Task<List<SectionDropdownDto>> GetActiveSectionsAsync(UserScope? scope = null)
        {
            var query = _context.Sections.Where(s => s.IsActive);

            // Apply scope filtering (filtered by user's station)
            if (scope != null)
            {
                query = _scopeFilterService.ApplyScope(query, scope);
            }

            return await query
                .OrderBy(s => s.SectionName)
                .Select(s => new SectionDropdownDto
                {
                    SectionId = s.SectionId,
                    SectionName = s.SectionName,
                    StationId = s.StationId
                })
                .ToListAsync();
        }

        public async Task<List<SectionDropdownDto>> GetSectionsByStationAsync(int stationId, UserScope? scope = null)
        {
            var query = _context.Sections
                .Where(s => s.IsActive && s.StationId == stationId);

            // Apply scope filtering
            if (scope != null)
            {
                query = _scopeFilterService.ApplyScope(query, scope);
            }

            return await query
                .OrderBy(s => s.SectionName)
                .Select(s => new SectionDropdownDto
                {
                    SectionId = s.SectionId,
                    SectionName = s.SectionName,
                    StationId = s.StationId
                })
                .ToListAsync();
        }

        #endregion
    }
}
