using OSHManagement.Models.DTOs.Dropdowns;

namespace OSHManagement.Services
{
    /// <summary>
    /// Service for Organization Category dropdown data
    /// Categories are reference data - NO scope filtering applied
    /// All users see all categories
    /// </summary>
    public interface IOrganizationService
    {
        /// <summary>
        /// Get all active organization categories for dropdown
        /// No scope filtering - reference data
        /// </summary>
        Task<List<CategoryDropdownDto>> GetActiveCategoriesAsync();

        /// <summary>
        /// Get category by ID
        /// </summary>
        Task<CategoryDropdownDto?> GetCategoryByIdAsync(int categoryId);

        /// <summary>
        /// Check if category exists and is active
        /// </summary>
        Task<bool> CategoryExistsAsync(int categoryId);
    }
}
