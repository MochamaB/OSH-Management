using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models.ViewModels;
using OSHManagement.Services;

namespace OSHManagement.Controllers
{
    [Authorize]
    public class SectionController : ScopedController
    {
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationalHierarchyService _orgHierarchyService;
        private readonly IEmployeeService _employeeService;
        private readonly IScopeFilterService _scopeFilterService;

        public SectionController(
            OshDbContext context,
            IScopeFilterService scopeFilter,
            IOrganizationService organizationService,
            IOrganizationalHierarchyService orgHierarchyService,
            IEmployeeService employeeService,
            ILogger<SectionController> logger)
            : base(context, scopeFilter, logger)
        {
            _organizationService = organizationService;
            _orgHierarchyService = orgHierarchyService;
            _employeeService = employeeService;
            _scopeFilterService = scopeFilter;
        }

        // GET: Section/Index
        public async Task<IActionResult> Index(string? search, string? status, int? stationId, int page = 1)
        {
            const int pageSize = 15; // Items per page

            // Start with base query
            var query = _context.Sections.AsQueryable();

            // ⚠️ CRITICAL: Apply scope FIRST (security takes precedence)
            // Users can only see sections in their station
            query = _scopeFilterService.ApplyScope(query, CurrentScope);

            // THEN apply includes (only for scoped data)
            query = query
                .Include(s => s.Station)
                .Include(s => s.TeamMembers);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(s =>
                    s.SectionName.ToLower().Contains(search) ||
                    s.Station.StationName.ToLower().Contains(search)
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

            // Apply station filter
            if (stationId.HasValue && stationId.Value > 0)
            {
                query = query.Where(s => s.StationId == stationId.Value);
            }

            // Get total count before pagination
            var totalItems = await query.CountAsync();

            // Calculate statistics
            var allSections = await query
                .Select(s => new { s.IsActive, TeamMemberCount = s.TeamMembers.Count })
                .ToListAsync();

            ViewBag.TotalSections = totalItems;
            ViewBag.ActiveSections = allSections.Count(s => s.IsActive);
            ViewBag.InactiveSections = allSections.Count(s => !s.IsActive);
            ViewBag.TotalTeamMembers = allSections.Sum(s => s.TeamMemberCount);

            // Execute query with pagination
            var sections = await query
                .OrderBy(s => s.Station.StationName).ThenBy(s => s.SectionName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SectionViewModel
                {
                    SectionId = s.SectionId,
                    SectionName = s.SectionName,
                    StationId = s.StationId,
                    StationName = s.Station.StationName,
                    SectionSupervisorPayroll = s.SectionSupervisorPayroll,
                    IsActive = s.IsActive,
                    TeamMemberCount = s.TeamMembers.Count,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToListAsync();

            // ✅ Get supervisor names using service (with scope)
            var supervisorPayrolls = sections
                .Where(s => !string.IsNullOrEmpty(s.SectionSupervisorPayroll))
                .Select(s => s.SectionSupervisorPayroll!)
                .Distinct()
                .ToList();

            if (supervisorPayrolls.Any())
            {
                var supervisorNames = await _employeeService.GetEmployeeNamesByPayrollsAsync(
                    supervisorPayrolls, 
                    CurrentScope);

                // Populate supervisor names
                foreach (var section in sections)
                {
                    if (!string.IsNullOrEmpty(section.SectionSupervisorPayroll) && 
                        supervisorNames.ContainsKey(section.SectionSupervisorPayroll))
                    {
                        section.SupervisorFullName = supervisorNames[section.SectionSupervisorPayroll];
                    }
                }
            }

            // Pass pagination info to view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            // ✅ Use service for stations dropdown (with scope)
            ViewBag.Stations = await _orgHierarchyService.GetActiveStationsAsync(CurrentScope);

            // Pass filter values to view for maintaining state
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentStationId = stationId;

            return View(sections);
        }

        // GET: Section/TestBackgroundCard (For testing BackgroundFillCard component)
        public IActionResult TestBackgroundCard()
        {
            return View();
        }

        // GET: Section/Create
        public async Task<IActionResult> Create()
        {
            // ✅ Use services for dropdowns (with scope)
            ViewBag.Stations = await _orgHierarchyService.GetActiveStationsAsync(CurrentScope);
            ViewBag.Employees = await _employeeService.GetSupervisorCandidatesAsync(CurrentScope);
            
            return View();
        }

        // POST: Section/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SectionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if section name already exists in this station
                    var existingSection = await _context.Sections
                        .AnyAsync(s => s.SectionName == model.SectionName && 
                                      s.StationId == model.StationId && 
                                      s.IsActive);
                    
                    if (existingSection)
                    {
                        ModelState.AddModelError("SectionName", "A section with this name already exists in the selected station.");
                        
                        // Reload dropdowns
                        await LoadDropdowns();
                        return View(model);
                    }

                    var section = new Models.Section
                    {
                        SectionName = model.SectionName.Trim(),
                        StationId = model.StationId,
                        SectionSupervisorPayroll = model.SectionSupervisorPayroll,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _context.Sections.Add(section);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Section '{model.SectionName}' has been created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while creating the section. Please try again.";
                    // Log the exception if you have logging configured
                }
            }
            
            // Reload dropdowns on error
            await LoadDropdowns();
            return View(model);
        }

        // GET: Section/BulkCreate
        public async Task<IActionResult> BulkCreate()
        {
            // ✅ Use services for dropdowns (with scope)
            ViewBag.Categories = await _organizationService.GetActiveCategoriesAsync();
            ViewBag.Stations = await _orgHierarchyService.GetActiveStationsAsync(CurrentScope);
            ViewBag.Employees = await _employeeService.GetSupervisorCandidatesAsync(CurrentScope);
            
            return View();
        }

        // POST: Section/BulkCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(int stationId, List<SectionBulkItem> Items)
        {
            if (ModelState.IsValid && Items != null && Items.Any())
            {
                try
                {
                    // Validate station exists
                    var stationExists = await _context.Stations.AnyAsync(s => s.StationId == stationId && s.IsActive);
                    if (!stationExists)
                    {
                        TempData["Error"] = "Invalid station selected.";
                        return RedirectToAction(nameof(BulkCreate));
                    }

                    // Create all sections in a transaction
                    foreach (var item in Items)
                    {
                        // Skip empty rows
                        if (string.IsNullOrWhiteSpace(item.SectionName))
                            continue;

                        var section = new Models.Section
                        {
                            SectionName = item.SectionName.Trim(),
                            StationId = stationId,
                            SectionSupervisorPayroll = item.SupervisorPayroll,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        _context.Sections.Add(section);
                    }
                    
                    await _context.SaveChangesAsync();
                    
                    var createdCount = Items.Count(i => !string.IsNullOrWhiteSpace(i.SectionName));
                    TempData["Success"] = $"{createdCount} section(s) created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while creating sections. Please try again.";
                    // Log the exception here if you have logging configured
                }
            }
            
            // ✅ Reload data on error using services
            ViewBag.Stations = await _orgHierarchyService.GetActiveStationsAsync(CurrentScope);
            ViewBag.Employees = await _employeeService.GetSupervisorCandidatesAsync(CurrentScope);
            
            return View();
        }

        // Helper method to load dropdowns
        private async Task LoadDropdowns()
        {
            // ✅ Use services for dropdowns (with scope awareness)
            ViewBag.Stations = await _orgHierarchyService.GetActiveStationsAsync(CurrentScope);
            ViewBag.Employees = await _employeeService.GetSupervisorCandidatesAsync(CurrentScope);
        }

        /// <summary>
        /// AJAX endpoint to get employees by station
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetEmployeesByStation(int stationId)
        {
            try
            {
                var employees = await _employeeService.GetEmployeesByStationAsync(stationId, CurrentScope);
                return Json(new { success = true, data = employees });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employees for station {StationId}", stationId);
                return Json(new { success = false, message = "Failed to load employees" });
            }
        }
    }

    // Helper class for bulk section creation
    public class SectionBulkItem
    {
        public string SectionName { get; set; } = "";
        public string? SupervisorPayroll { get; set; }
    }
}
