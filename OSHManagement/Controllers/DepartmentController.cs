using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models.ViewModels;
using OSHManagement.Services;

namespace OSHManagement.Controllers
{
    [Authorize]
    public class DepartmentController : ScopedController
    {
        private readonly IOrganizationalHierarchyService _orgHierarchyService;
        private readonly IEmployeeService _employeeService;
        private readonly IScopeFilterService _scopeFilterService;

        public DepartmentController(
            OshDbContext context,
            IScopeFilterService scopeFilter,
            IOrganizationalHierarchyService orgHierarchyService,
            IEmployeeService employeeService,
            ILogger<DepartmentController> logger)
            : base(context, scopeFilter, logger)
        {
            _orgHierarchyService = orgHierarchyService;
            _employeeService = employeeService;
            _scopeFilterService = scopeFilter;
        }

        // GET: Department/Index
        public async Task<IActionResult> Index(string? search, string? status, int? stationId, int page = 1)
        {
            const int pageSize = 10; // Items per page
            
            // Start with base query
            var query = _context.Departments.AsQueryable();

            // ⚠️ CRITICAL: Apply scope FIRST (security takes precedence)
            // Organization: All departments
            // Station: Departments in their station
            // Department: ONLY their department (Principle of Least Privilege)
            query = _scopeFilterService.ApplyScope(query, CurrentScope);

            // THEN apply includes (only for scoped data)
            query = query
                .Include(d => d.Station)
                .Include(d => d.ParentDepartment)
                .Include(d => d.Employees);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(d =>
                    d.DepartmentCode.ToLower().Contains(search) ||
                    d.DepartmentName.ToLower().Contains(search) ||
                    d.Station.StationName.ToLower().Contains(search)
                );
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(d => d.IsActive);
                }
                else if (status.Equals("inactive", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(d => !d.IsActive);
                }
            }

            // Apply station filter
            if (stationId.HasValue && stationId.Value > 0)
            {
                query = query.Where(d => d.StationId == stationId.Value);
            }

            // Get total count before pagination (for statistics)
            var totalItems = await query.CountAsync();
            
            // Calculate statistics from all items (not just current page)
            var allDepartments = await query
                .Select(d => new { d.IsActive, EmployeeCount = d.Employees.Count })
                .ToListAsync();
            
            ViewBag.TotalDepartments = totalItems;
            ViewBag.ActiveDepartments = allDepartments.Count(d => d.IsActive);
            ViewBag.InactiveDepartments = allDepartments.Count(d => !d.IsActive);
            ViewBag.TotalEmployees = allDepartments.Sum(d => d.EmployeeCount);
            
            // Execute query with pagination
            var departments = await query
                .OrderBy(d => d.DepartmentCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DepartmentViewModel
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentCode = d.DepartmentCode,
                    DepartmentName = d.DepartmentName,
                    StationId = d.StationId,
                    StationName = d.Station.StationName,
                    ParentDepartmentId = d.ParentDepartmentId,
                    ParentDepartmentName = d.ParentDepartment != null ? d.ParentDepartment.DepartmentName : null,
                    DepartmentHeadPayroll = d.DepartmentHeadPayroll,
                    DepartmentHeadUsername = d.DepartmentHeadPayroll != null 
                        ? d.Employees.Where(e => e.PayrollNo == d.DepartmentHeadPayroll).Select(e => e.Username).FirstOrDefault()
                        : null,
                    DepartmentHeadFullName = d.DepartmentHeadPayroll != null
                        ? d.Employees.Where(e => e.PayrollNo == d.DepartmentHeadPayroll).Select(e => e.FirstName + " " + e.LastName).FirstOrDefault()
                        : null,
                    IsActive = d.IsActive,
                    EmployeeCount = d.Employees.Count,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                })
                .ToListAsync();
            
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

            return View(departments);
        }

        // GET: Department/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        // POST: Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Validate parent department is in the same station
                    if (model.ParentDepartmentId.HasValue)
                    {
                        var parentDepartment = await _context.Departments
                            .FirstOrDefaultAsync(d => d.DepartmentId == model.ParentDepartmentId.Value);

                        if (parentDepartment != null && parentDepartment.StationId != model.StationId)
                        {
                            ModelState.AddModelError("ParentDepartmentId",
                                "Parent department must be in the same station.");
                            await PopulateDropdowns();
                            return View(model);
                        }
                    }

                    // Generate a unique department code
                    var maxDepartmentCode = await _context.Departments
                        .Select(d => d.DepartmentCode)
                        .Where(code => code != null)
                        .ToListAsync();

                    // Find the highest numeric code
                    int nextCode = 1;
                    var numericCodes = maxDepartmentCode
                        .Where(code => int.TryParse(code, out _))
                        .Select(code => int.Parse(code))
                        .ToList();

                    if (numericCodes.Any())
                    {
                        nextCode = numericCodes.Max() + 1;
                    }

                    // Create department with unique code
                    var department = new Models.Department
                    {
                        DepartmentCode = nextCode.ToString(),
                        DepartmentName = model.DepartmentName,
                        StationId = model.StationId,
                        ParentDepartmentId = model.ParentDepartmentId,
                        DepartmentHeadPayroll = model.DepartmentHeadPayroll,
                        IsActive = model.IsActive,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Departments.Add(department);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Department '{model.DepartmentName}' has been created successfully with code {department.DepartmentCode}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while creating the department. Please try again.";
                    // Log the exception here if you have logging configured
                }
            }

            await PopulateDropdowns();
            return View(model);
        }

        // GET: Department/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Station)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null)
            {
                TempData["Error"] = "Department not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new DepartmentViewModel
            {
                DepartmentId = department.DepartmentId,
                DepartmentCode = department.DepartmentCode,
                DepartmentName = department.DepartmentName,
                StationId = department.StationId,
                ParentDepartmentId = department.ParentDepartmentId,
                DepartmentHeadPayroll = department.DepartmentHeadPayroll,
                IsActive = department.IsActive,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt
            };

            await PopulateDropdowns();
            return View(model);
        }

        // POST: Department/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentViewModel model)
        {
            if (id != model.DepartmentId)
            {
                TempData["Error"] = "Invalid department ID.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate parent department is in the same station
                    if (model.ParentDepartmentId.HasValue)
                    {
                        var parentDepartment = await _context.Departments
                            .FirstOrDefaultAsync(d => d.DepartmentId == model.ParentDepartmentId.Value);

                        if (parentDepartment != null && parentDepartment.StationId != model.StationId)
                        {
                            ModelState.AddModelError("ParentDepartmentId",
                                "Parent department must be in the same station.");
                            await PopulateDropdowns();
                            return View(model);
                        }

                        // Prevent circular reference
                        if (model.ParentDepartmentId.Value == model.DepartmentId)
                        {
                            ModelState.AddModelError("ParentDepartmentId",
                                "A department cannot be its own parent.");
                            await PopulateDropdowns();
                            return View(model);
                        }
                    }

                    var department = await _context.Departments.FindAsync(id);
                    if (department == null)
                    {
                        TempData["Error"] = "Department not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Update department properties (DepartmentCode is read-only)
                    department.DepartmentName = model.DepartmentName;
                    department.StationId = model.StationId;
                    department.ParentDepartmentId = model.ParentDepartmentId;
                    department.DepartmentHeadPayroll = model.DepartmentHeadPayroll;
                    department.IsActive = model.IsActive;
                    department.UpdatedAt = DateTime.UtcNow;

                    _context.Departments.Update(department);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Department '{model.DepartmentName}' has been updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while updating the department. Please try again.";
                    // Log the exception here if you have logging configured
                }
            }

            await PopulateDropdowns();
            return View(model);
        }

        // POST: Department/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var department = await _context.Departments
                    .Include(d => d.ChildDepartments)
                    .Include(d => d.Employees)
                    .FirstOrDefaultAsync(d => d.DepartmentId == id);

                if (department == null)
                {
                    TempData["Error"] = "Department not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if department has child departments
                if (department.ChildDepartments.Any())
                {
                    TempData["Error"] = $"Cannot delete department '{department.DepartmentName}' because it has {department.ChildDepartments.Count} child department(s) assigned to it.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if department has employees
                if (department.Employees.Any())
                {
                    TempData["Error"] = $"Cannot delete department '{department.DepartmentName}' because it has {department.Employees.Count} employee(s) assigned to it.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Department '{department.DepartmentName}' has been deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the department. Please try again.";
                // Log the exception here if you have logging configured
            }

            return RedirectToAction(nameof(Index));
        }

        // API endpoint to get departments with their stations for cascading dropdown
        [HttpGet]
        public async Task<IActionResult> GetDepartmentsWithStations()
        {
            // ✅ Use service with scope filtering
            var departments = await _orgHierarchyService.GetActiveDepartmentsAsync(CurrentScope);

            return Json(departments.Select(d => new
            {
                departmentId = d.DepartmentId,
                departmentName = d.DepartmentName,
                stationId = d.StationId
            }));
        }

        // Helper method to populate dropdowns
        private async Task PopulateDropdowns()
        {
            // ✅ Use services for dropdowns (with scope awareness)
            ViewBag.Stations = await _orgHierarchyService.GetActiveStationsAsync(CurrentScope);
            
            // 🔒 CRITICAL: Parent departments respects scope
            // Department Head can ONLY see their department (not other departments)
            ViewBag.ParentDepartments = await _orgHierarchyService.GetActiveDepartmentsAsync(CurrentScope);
            
            // ✅ HOD dropdown with scope (only employees in their scope)
            ViewBag.HodCandidates = await _employeeService.GetHodCandidatesAsync(CurrentScope);
        }
    }
}
