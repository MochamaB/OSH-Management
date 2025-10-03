using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models.ViewModels;

namespace OSHManagement.Controllers
{
    [Authorize]
    public class OrganizationController : Controller
    {
        private readonly OshDbContext _context;

        public OrganizationController(OshDbContext context)
        {
            _context = context;
        }

        // GET: Organization/Categories
        public async Task<IActionResult> Categories(string? search, string? status)
        {
            // Start with base query
            var query = _context.OrgCategories
                .Include(c => c.Stations)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(c =>
                    c.CategoryName.ToLower().Contains(search) ||
                    (c.Description != null && c.Description.ToLower().Contains(search))
                );
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(c => c.IsActive);
                }
                else if (status.Equals("inactive", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(c => !c.IsActive);
                }
            }

            // Execute query and project to ViewModel
            var categories = await query
                .OrderBy(c => c.CategoryName)
                .Select(c => new OrgCategoryViewModel
                {
                    OrgCategoryId = c.OrgCategoryId,
                    CategoryName = c.CategoryName,
                    Description = c.Description ?? "",
                    IsActive = c.IsActive,
                    StationCount = c.Stations.Count,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            // Calculate statistics for dashboard cards
            ViewBag.TotalCategories = categories.Count;
            ViewBag.ActiveCategories = categories.Count(c => c.IsActive);
            ViewBag.InactiveCategories = categories.Count(c => !c.IsActive);
            ViewBag.TotalStations = categories.Sum(c => c.StationCount);

            // Pass filter values to view for maintaining state
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;

            return View(categories);
        }
    }
}
