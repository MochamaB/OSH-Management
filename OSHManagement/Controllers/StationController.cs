using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models.ViewModels;

namespace OSHManagement.Controllers
{
    [Authorize]
    public class StationController : Controller
    {
        private readonly OshDbContext _context;

        public StationController(OshDbContext context)
        {
            _context = context;
        }

        // GET: Station/Index
        public async Task<IActionResult> Index(string? search, string? status, int? categoryId)
        {
            // Start with base query
            var query = _context.Stations
                .Include(s => s.OrgCategory)
                .Include(s => s.ParentStation)
                .Include(s => s.Departments)
                .Include(s => s.Sections)
                .Include(s => s.Teams)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(s =>
                    s.StationCode.ToLower().Contains(search) ||
                    s.StationName.ToLower().Contains(search) ||
                    (s.LegacyStationMapping != null && s.LegacyStationMapping.ToLower().Contains(search))
                );
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(s => s.IsActive);
                }
                else if (status.Equals("inactive", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(s => !s.IsActive);
                }
            }

            // Apply category filter
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(s => s.OrgCategoryId == categoryId.Value);
            }

            // Execute query and project to ViewModel
            var stations = await query
                .OrderBy(s => s.StationCode)
                .Select(s => new StationViewModel
                {
                    StationId = s.StationId,
                    StationCode = s.StationCode,
                    StationName = s.StationName,
                    OrgCategoryId = s.OrgCategoryId,
                    CategoryName = s.OrgCategory.CategoryName,
                    ParentStationId = s.ParentStationId,
                    ParentStationName = s.ParentStation != null ? s.ParentStation.StationName : null,
                    LegacyStationMapping = s.LegacyStationMapping,
                    IsActive = s.IsActive,
                    DepartmentCount = s.Departments.Count,
                    SectionCount = s.Sections.Count,
                    TeamCount = s.Teams.Count,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToListAsync();

            // Calculate statistics
            ViewBag.TotalStations = stations.Count;
            ViewBag.ActiveStations = stations.Count(s => s.IsActive);
            ViewBag.InactiveStations = stations.Count(s => !s.IsActive);
            ViewBag.TotalDepartments = stations.Sum(s => s.DepartmentCount);

            // Get categories for filter dropdown
            var categories = await _context.OrgCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .Select(c => new { c.OrgCategoryId, c.CategoryName })
                .ToListAsync();

            ViewBag.Categories = categories;

            // Pass filter values to view for maintaining state
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentCategoryId = categoryId;

            return View(stations);
        }

        // GET: Station/Create
        public async Task<IActionResult> Create()
        {
            // Get categories for dropdown
            var categories = await _context.OrgCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .Select(c => new { c.OrgCategoryId, c.CategoryName })
                .ToListAsync();

            ViewBag.Categories = categories;

            // Get parent stations for dropdown
            var parentStations = await _context.Stations
                .Where(s => s.IsActive)
                .OrderBy(s => s.StationName)
                .Select(s => new { s.StationId, s.StationName })
                .ToListAsync();

            ViewBag.ParentStations = parentStations;

            return View();
        }

        // POST: Station/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StationViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Validate parent station is in the same category
                    if (model.ParentStationId.HasValue)
                    {
                        var parentStation = await _context.Stations
                            .FirstOrDefaultAsync(s => s.StationId == model.ParentStationId.Value);

                        if (parentStation != null && parentStation.OrgCategoryId != model.OrgCategoryId)
                        {
                            ModelState.AddModelError("ParentStationId", 
                                "Parent station must be in the same organization category.");
                            await PopulateDropdowns();
                            return View(model);
                        }
                    }

                    // Generate a unique station code
                    var maxStationCode = await _context.Stations
                        .Select(s => s.StationCode)
                        .Where(code => code != null)
                        .ToListAsync();
                    
                    // Find the highest numeric code
                    int nextCode = 1;
                    var numericCodes = maxStationCode
                        .Where(code => int.TryParse(code, out _))
                        .Select(code => int.Parse(code))
                        .ToList();
                    
                    if (numericCodes.Any())
                    {
                        nextCode = numericCodes.Max() + 1;
                    }
                    
                    // Create station with unique code
                    var station = new Models.Station
                    {
                        StationCode = nextCode.ToString(),
                        StationName = model.StationName,
                        OrgCategoryId = model.OrgCategoryId,
                        ParentStationId = model.ParentStationId,
                        IsActive = model.IsActive,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Stations.Add(station);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Station '{model.StationName}' has been created successfully with code {station.StationCode}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while creating the station. Please try again.";
                    // Log the exception here if you have logging configured
                }
            }

            await PopulateDropdowns();
            return View(model);
        }

        // GET: Station/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var station = await _context.Stations
                .Include(s => s.OrgCategory)
                .FirstOrDefaultAsync(s => s.StationId == id);

            if (station == null)
            {
                TempData["Error"] = "Station not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new StationViewModel
            {
                StationId = station.StationId,
                StationCode = station.StationCode,
                StationName = station.StationName,
                OrgCategoryId = station.OrgCategoryId,
                ParentStationId = station.ParentStationId,
                IsActive = station.IsActive,
                CreatedAt = station.CreatedAt,
                UpdatedAt = station.UpdatedAt
            };

            // Populate dropdowns
            await PopulateDropdowns();

            return View(model);
        }

        // POST: Station/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StationViewModel model)
        {
            if (id != model.StationId)
            {
                TempData["Error"] = "Invalid station ID.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate parent station is in the same category
                    if (model.ParentStationId.HasValue)
                    {
                        var parentStation = await _context.Stations
                            .FirstOrDefaultAsync(s => s.StationId == model.ParentStationId.Value);

                        if (parentStation != null && parentStation.OrgCategoryId != model.OrgCategoryId)
                        {
                            ModelState.AddModelError("ParentStationId",
                                "Parent station must be in the same organization category.");
                            await PopulateDropdowns();
                            return View(model);
                        }

                        // Prevent circular reference (station cannot be its own parent)
                        if (model.ParentStationId.Value == model.StationId)
                        {
                            ModelState.AddModelError("ParentStationId",
                                "A station cannot be its own parent.");
                            await PopulateDropdowns();
                            return View(model);
                        }
                    }

                    var station = await _context.Stations.FindAsync(id);
                    if (station == null)
                    {
                        TempData["Error"] = "Station not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Update station properties (StationCode is read-only, not updated)
                    station.StationName = model.StationName;
                    station.OrgCategoryId = model.OrgCategoryId;
                    station.ParentStationId = model.ParentStationId;
                    station.IsActive = model.IsActive;
                    station.UpdatedAt = DateTime.UtcNow;

                    _context.Stations.Update(station);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Station '{model.StationName}' has been updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while updating the station. Please try again.";
                    // Log the exception here if you have logging configured
                }
            }

            await PopulateDropdowns();
            return View(model);
        }

        // POST: Station/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var station = await _context.Stations
                    .Include(s => s.ChildStations)
                    .Include(s => s.Departments)
                    .Include(s => s.Sections)
                    .Include(s => s.Teams)
                    .FirstOrDefaultAsync(s => s.StationId == id);

                if (station == null)
                {
                    TempData["Error"] = "Station not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if station has child stations
                if (station.ChildStations.Any())
                {
                    TempData["Error"] = $"Cannot delete station '{station.StationName}' because it has {station.ChildStations.Count} child station(s) assigned to it.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if station has departments
                if (station.Departments.Any())
                {
                    TempData["Error"] = $"Cannot delete station '{station.StationName}' because it has {station.Departments.Count} department(s) assigned to it.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if station has sections
                if (station.Sections.Any())
                {
                    TempData["Error"] = $"Cannot delete station '{station.StationName}' because it has {station.Sections.Count} section(s) assigned to it.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if station has teams
                if (station.Teams.Any())
                {
                    TempData["Error"] = $"Cannot delete station '{station.StationName}' because it has {station.Teams.Count} team(s) assigned to it.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Stations.Remove(station);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Station '{station.StationName}' has been deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the station. Please try again.";
                // Log the exception here if you have logging configured
            }

            return RedirectToAction(nameof(Index));
        }

        // API endpoint to get stations with their categories for cascading dropdown
        [HttpGet]
        public async Task<IActionResult> GetStationsWithCategories()
        {
            var stations = await _context.Stations
                .Where(s => s.IsActive)
                .OrderBy(s => s.StationName)
                .Select(s => new
                {
                    stationId = s.StationId,
                    stationName = s.StationName,
                    orgCategoryId = s.OrgCategoryId
                })
                .ToListAsync();

            return Json(stations);
        }

        // Helper method to populate dropdowns
        private async Task PopulateDropdowns()
        {
            var categories = await _context.OrgCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .Select(c => new { c.OrgCategoryId, c.CategoryName })
                .ToListAsync();

            ViewBag.Categories = categories;

            var parentStations = await _context.Stations
                .Where(s => s.IsActive)
                .OrderBy(s => s.StationName)
                .Select(s => new { s.StationId, s.StationName })
                .ToListAsync();

            ViewBag.ParentStations = parentStations;
        }
    }
}
