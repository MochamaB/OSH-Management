using OSHManagement.Models.Authorization;
using OSHManagement.Models.DTOs.Dropdowns;

namespace OSHManagement.Services
{
    /// <summary>
    /// Service for Organizational Hierarchy dropdown data (Stations, Departments, Sections)
    /// CONDITIONAL scope filtering applied based on user scope level
    /// Uses ScopeFilterService for consistent scope enforcement
    /// </summary>
    public interface IOrganizationalHierarchyService
    {
        #region Stations

        /// <summary>
        /// Get all active stations for dropdown (with scope filtering)
        /// Organization scope: All stations
        /// Station/Dept/Team/Self: Only user's station
        /// </summary>
        Task<List<StationDropdownDto>> GetActiveStationsAsync(UserScope? scope = null);

        /// <summary>
        /// Get stations filtered by category (with scope filtering)
        /// Scope takes precedence over category filter
        /// </summary>
        Task<List<StationDropdownDto>> GetStationsByCategoryAsync(int categoryId, UserScope? scope = null);

        /// <summary>
        /// Get station by ID (with scope validation)
        /// Returns null if station is outside user's scope
        /// </summary>
        Task<StationDropdownDto?> GetStationByIdAsync(int stationId, UserScope? scope = null);

        /// <summary>
        /// Check if station exists and is within user's scope
        /// </summary>
        Task<bool> StationExistsAsync(int stationId, UserScope? scope = null);

        /// <summary>
        /// Get current user's station (for auto-selection in forms)
        /// Returns null if user is Organization scope
        /// </summary>
        Task<StationDropdownDto?> GetCurrentUserStationAsync(UserScope scope);

        /// <summary>
        /// Get the category ID of the current user's station
        /// Used for auto-selecting category in forms
        /// </summary>
        Task<int?> GetCurrentUserStationCategoryAsync(UserScope scope);

        #endregion

        #region Departments

        /// <summary>
        /// Get all active departments for dropdown (with scope filtering)
        /// Organization scope: All departments
        /// Station scope: Departments in user's station
        /// Department scope: ONLY user's department (Principle of Least Privilege)
        /// </summary>
        Task<List<DepartmentDropdownDto>> GetActiveDepartmentsAsync(UserScope? scope = null);

        /// <summary>
        /// Get departments filtered by station (with scope filtering)
        /// Scope takes precedence over station filter
        /// </summary>
        Task<List<DepartmentDropdownDto>> GetDepartmentsByStationAsync(int stationId, UserScope? scope = null);

        /// <summary>
        /// Get department by ID (with scope validation)
        /// Returns null if department is outside user's scope
        /// </summary>
        Task<DepartmentDropdownDto?> GetDepartmentByIdAsync(int departmentId, UserScope? scope = null);

        /// <summary>
        /// Check if department exists and is within user's scope
        /// </summary>
        Task<bool> DepartmentExistsAsync(int departmentId, UserScope? scope = null);

        /// <summary>
        /// Get current user's department (for auto-selection in forms)
        /// Returns null if user is Organization/Station scope
        /// </summary>
        Task<DepartmentDropdownDto?> GetCurrentUserDepartmentAsync(UserScope scope);

        #endregion

        #region Sections

        /// <summary>
        /// Get all active sections for dropdown (with scope filtering)
        /// Filtered by user's station
        /// </summary>
        Task<List<SectionDropdownDto>> GetActiveSectionsAsync(UserScope? scope = null);

        /// <summary>
        /// Get sections filtered by station (with scope filtering)
        /// </summary>
        Task<List<SectionDropdownDto>> GetSectionsByStationAsync(int stationId, UserScope? scope = null);

        #endregion
    }
}
