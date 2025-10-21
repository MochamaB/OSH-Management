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

        // GET: Organization/CategoryDetails/5
        public async Task<IActionResult> CategoryDetails(int id)
        {
            var category = await _context.OrgCategories
                .Include(c => c.Stations)
                .FirstOrDefaultAsync(c => c.OrgCategoryId == id);

            if (category == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction(nameof(Categories));
            }

            var model = new OrgCategoryViewModel
            {
                OrgCategoryId = category.OrgCategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description ?? "",
                IsActive = category.IsActive,
                StationCount = category.Stations.Count,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            // Calculate basic statistics
            ViewBag.TotalDepartments = _context.Departments
                .Count(d => category.Stations.Select(s => s.StationId).Contains(d.StationId));

            ViewBag.TotalSections = _context.Sections
                .Count(s => category.Stations.Select(st => st.StationId).Contains(s.StationId));

            return View(model);
        }

        // GET: Organization/CreateCategory
        public IActionResult CreateCategory()
        {
            return View();
        }

        // POST: Organization/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(OrgCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var category = new Models.OrgCategory
                    {
                        CategoryName = model.CategoryName,
                        Description = model.Description,
                        IsActive = model.IsActive,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.OrgCategories.Add(category);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Category '{model.CategoryName}' has been created successfully.";
                    return RedirectToAction(nameof(Categories));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while creating the category. Please try again.";
                    // Log the exception here if you have logging configured
                }
            }

            return View(model);
        }

        // GET: Organization/EditCategory/5
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.OrgCategories
                .FirstOrDefaultAsync(c => c.OrgCategoryId == id);

            if (category == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction(nameof(Categories));
            }

            var model = new OrgCategoryViewModel
            {
                OrgCategoryId = category.OrgCategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description ?? "",
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return View(model);
        }

        // POST: Organization/EditCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, OrgCategoryViewModel model)
        {
            if (id != model.OrgCategoryId)
            {
                TempData["Error"] = "Invalid category ID.";
                return RedirectToAction(nameof(Categories));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _context.OrgCategories.FindAsync(id);
                    if (category == null)
                    {
                        TempData["Error"] = "Category not found.";
                        return RedirectToAction(nameof(Categories));
                    }

                    category.CategoryName = model.CategoryName;
                    category.Description = model.Description;
                    category.IsActive = model.IsActive;
                    category.UpdatedAt = DateTime.UtcNow;

                    _context.OrgCategories.Update(category);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Category '{model.CategoryName}' has been updated successfully.";
                    return RedirectToAction(nameof(Categories));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while updating the category. Please try again.";
                    // Log the exception here if you have logging configured
                }
            }

            return View(model);
        }

        // POST: Organization/DeleteCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.OrgCategories
                    .Include(c => c.Stations)
                    .FirstOrDefaultAsync(c => c.OrgCategoryId == id);

                if (category == null)
                {
                    TempData["Error"] = "Category not found.";
                    return RedirectToAction(nameof(Categories));
                }

                // Check if category has stations
                if (category.Stations.Any())
                {
                    TempData["Error"] = $"Cannot delete category '{category.CategoryName}' because it has {category.Stations.Count} station(s) assigned to it.";
                    return RedirectToAction(nameof(Categories));
                }

                _context.OrgCategories.Remove(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Category '{category.CategoryName}' has been deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the category. Please try again.";
                // Log the exception here if you have logging configured
            }

            return RedirectToAction(nameof(Categories));
        }
    }
}
