using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models.DTOs.Dropdowns;

namespace OSHManagement.Services
{
    /// <summary>
    /// Implementation of IOrganizationService
    /// Handles Organization Category queries (reference data - no scope)
    /// </summary>
    public class OrganizationService : IOrganizationService
    {
        private readonly OshDbContext _context;

        public OrganizationService(OshDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDropdownDto>> GetActiveCategoriesAsync()
        {
            return await _context.OrgCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .Select(c => new CategoryDropdownDto
                {
                    OrgCategoryId = c.OrgCategoryId,
                    CategoryName = c.CategoryName
                })
                .ToListAsync();
        }

        public async Task<CategoryDropdownDto?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.OrgCategories
                .Where(c => c.OrgCategoryId == categoryId && c.IsActive)
                .Select(c => new CategoryDropdownDto
                {
                    OrgCategoryId = c.OrgCategoryId,
                    CategoryName = c.CategoryName
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await _context.OrgCategories
                .AnyAsync(c => c.OrgCategoryId == categoryId && c.IsActive);
        }
    }
}
