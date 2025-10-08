using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models.ViewModels;

namespace OSHManagement.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly OshDbContext _context;

        public EmployeeController(OshDbContext context)
        {
            _context = context;
        }

        // GET: Employee/Index
        public async Task<IActionResult> Index(string? search, int? categoryId, int? stationId, int? departmentId, int? roleId, string? employeeType, int page = 1)
        {
            const int pageSize = 15; // Items per page

            // Start with base query - only active employees
            var query = _context.Employees
                .Where(e => e.EmploymentStatus == "Active")
                .Include(e => e.Station)
                    .ThenInclude(s => s.OrgCategory)
                .Include(e => e.Department)
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(e =>
                    e.PayrollNo.ToLower().Contains(search) ||
                    e.FirstName.ToLower().Contains(search) ||
                    e.LastName.ToLower().Contains(search) ||
                    (e.EmailAddress != null && e.EmailAddress.ToLower().Contains(search)) ||
                    (e.Designation != null && e.Designation.ToLower().Contains(search))
                );
            }

            // Apply category filter (filters via station)
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(e => e.Station.OrgCategoryId == categoryId.Value);
            }

            // Apply station filter
            if (stationId.HasValue && stationId.Value > 0)
            {
                query = query.Where(e => e.StationId == stationId.Value);
            }

            // Apply department filter
            if (departmentId.HasValue && departmentId.Value > 0)
            {
                query = query.Where(e => e.DepartmentId == departmentId.Value);
            }

            // Apply role filter
            if (roleId.HasValue && roleId.Value > 0)
            {
                query = query.Where(e => e.EmployeeRoles.Any(er => er.RoleId == roleId.Value && er.IsActive));
            }

            // Apply employee type filter
            if (!string.IsNullOrWhiteSpace(employeeType))
            {
                query = query.Where(e => e.EmployeeType == employeeType);
            }

            // Get total count before pagination
            var totalItems = await query.CountAsync();

            // Calculate statistics
            var allEmployees = await query
                .Select(e => new { e.EmployeeType })
                .ToListAsync();

            ViewBag.TotalEmployees = totalItems;
            ViewBag.FactoryEmployees = allEmployees.Count(e => e.EmployeeType == "Factory");
            ViewBag.OutsourcedEmployees = allEmployees.Count(e => e.EmployeeType == "Outsourced");
            ViewBag.CasualEmployees = allEmployees.Count(e => e.EmployeeType == "Casual");

            // Execute query with pagination
            var employees = await query
                .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EmployeeViewModel
                {
                    EmployeeId = e.EmployeeId,
                    PayrollNo = e.PayrollNo,
                    RollNo = e.RollNo,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    EmailAddress = e.EmailAddress,
                    PhoneNo = e.PhoneNo,
                    StationId = e.StationId,
                    StationName = e.Station != null ? e.Station.StationName : "",
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department != null ? e.Department.DepartmentName : null,
                    Username = e.Username,
                    EmploymentStatus = e.EmploymentStatus,
                    EmployeeType = e.EmployeeType,
                    Designation = e.Designation,
                    HireDate = e.HireDate,
                    ServiceYears = e.ServiceYears,
                    ContractEndDate = e.ContractEndDate,
                    HodPayroll = e.HodPayroll,
                    SupervisorPayroll = e.SupervisorPayroll,
                    RoleNames = e.EmployeeRoles.Where(er => er.IsActive).Select(er => er.Role.RoleName).ToList(),
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })
                .ToListAsync();

            // Get HOD and Supervisor names
            var hodPayrolls = employees.Where(e => !string.IsNullOrEmpty(e.HodPayroll)).Select(e => e.HodPayroll).Distinct().ToList();
            var supervisorPayrolls = employees.Where(e => !string.IsNullOrEmpty(e.SupervisorPayroll)).Select(e => e.SupervisorPayroll).Distinct().ToList();
            
            var hodNames = await _context.Employees
                .Where(e => hodPayrolls.Contains(e.PayrollNo))
                .Select(e => new { e.PayrollNo, FullName = e.FirstName + " " + e.LastName })
                .ToDictionaryAsync(e => e.PayrollNo, e => e.FullName);

            var supervisorNames = await _context.Employees
                .Where(e => supervisorPayrolls.Contains(e.PayrollNo))
                .Select(e => new { e.PayrollNo, FullName = e.FirstName + " " + e.LastName })
                .ToDictionaryAsync(e => e.PayrollNo, e => e.FullName);

            // Get avatar URLs from MediaAssociation
            var employeeIds = employees.Select(e => e.EmployeeId).ToList();
            var avatars = await _context.MediaAssociations
                .Where(ma => ma.AssociatedTable == "Employees" && 
                            employeeIds.Contains(ma.AssociatedRecordId) && 
                            ma.AssociationType == "Avatar" &&
                            ma.IsPrimary)
                .Include(ma => ma.Media)
                .Select(ma => new { 
                    EmployeeId = ma.AssociatedRecordId, 
                    AvatarUrl = ma.Media.FilePath 
                })
                .ToDictionaryAsync(a => a.EmployeeId, a => a.AvatarUrl);

            // Populate HOD, Supervisor names and avatars
            foreach (var employee in employees)
            {
                if (!string.IsNullOrEmpty(employee.HodPayroll) && hodNames.ContainsKey(employee.HodPayroll))
                {
                    employee.HodFullName = hodNames[employee.HodPayroll];
                }

                if (!string.IsNullOrEmpty(employee.SupervisorPayroll) && supervisorNames.ContainsKey(employee.SupervisorPayroll))
                {
                    employee.SupervisorFullName = supervisorNames[employee.SupervisorPayroll];
                }

                if (avatars.ContainsKey(employee.EmployeeId))
                {
                    employee.AvatarUrl = avatars[employee.EmployeeId];
                }
            }

            // Pass pagination info to view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            // Get filter dropdown data
            var categories = await _context.OrgCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .Select(c => new { c.OrgCategoryId, c.CategoryName })
                .ToListAsync();

            var stations = await _context.Stations
                .Where(s => s.IsActive)
                .OrderBy(s => s.StationName)
                .Select(s => new { s.StationId, s.StationName, s.OrgCategoryId })
                .ToListAsync();

            var departments = await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.DepartmentName)
                .Select(d => new { d.DepartmentId, d.DepartmentName })
                .ToListAsync();

            var roles = await _context.Roles
                .Where(r => r.IsActive)
                .OrderBy(r => r.RoleName)
                .Select(r => new { r.RoleId, r.RoleName })
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.Stations = stations;
            ViewBag.Departments = departments;
            ViewBag.Roles = roles;

            // Pass filter values to view for maintaining state
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.CurrentStationId = stationId;
            ViewBag.CurrentDepartmentId = departmentId;
            ViewBag.CurrentRoleId = roleId;
            ViewBag.CurrentEmployeeType = employeeType;

            return View(employees);
        }

        // GET: Employee/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel model, List<int>? SelectedRoles)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Create new employee
                    var employee = new Models.Employee
                    {
                        PayrollNo = model.PayrollNo,
                        RollNo = model.RollNo,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        EmailAddress = model.EmailAddress,
                        PhoneNo = model.PhoneNo,
                        StationId = model.StationId,
                        DepartmentId = model.DepartmentId,
                        Username = model.Username,
                        EmploymentStatus = model.EmploymentStatus ?? "Active",
                        EmployeeType = model.EmployeeType,
                        Designation = model.Designation,
                        HireDate = DateTime.UtcNow, // Auto-set to current date
                        ServiceYears = null, // Will be calculated later
                        ContractEndDate = model.ContractEndDate,
                        HodPayroll = model.HodPayroll,
                        SupervisorPayroll = model.SupervisorPayroll,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Employees.Add(employee);
                    await _context.SaveChangesAsync();

                    // Assign roles if selected
                    if (SelectedRoles != null && SelectedRoles.Any())
                    {
                        foreach (var roleId in SelectedRoles)
                        {
                            var employeeRole = new Models.EmployeeRole
                            {
                                EmployeeId = employee.EmployeeId,
                                RoleId = roleId,
                                IsActive = true,
                                AssignedAt = DateTime.UtcNow
                            };

                            _context.EmployeeRoles.Add(employeeRole);
                        }

                        await _context.SaveChangesAsync();
                    }

                    TempData["Success"] = $"Employee '{model.FirstName} {model.LastName}' has been created successfully with payroll number {model.PayrollNo}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while creating the employee. Please try again.";
                    // Log the exception here if you have logging configured
                }
            }

            await PopulateDropdowns();
            return View(model);
        }

        // GET: Employee/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Station)
                    .ThenInclude(s => s.OrgCategory)
                .Include(e => e.Department)
                .Include(e => e.EmployeeRoles.Where(er => er.IsActive))
                    .ThenInclude(er => er.Role)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                TempData["Error"] = "Employee not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new EmployeeViewModel
            {
                EmployeeId = employee.EmployeeId,
                PayrollNo = employee.PayrollNo,
                RollNo = employee.RollNo,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                EmailAddress = employee.EmailAddress,
                PhoneNo = employee.PhoneNo,
                StationId = employee.StationId,
                StationName = employee.Station?.StationName ?? "",
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.Department?.DepartmentName,
                Username = employee.Username,
                EmploymentStatus = employee.EmploymentStatus,
                EmployeeType = employee.EmployeeType,
                Designation = employee.Designation,
                HireDate = employee.HireDate,
                ServiceYears = employee.ServiceYears,
                ContractEndDate = employee.ContractEndDate,
                HodPayroll = employee.HodPayroll,
                SupervisorPayroll = employee.SupervisorPayroll,
                RoleNames = employee.EmployeeRoles.Where(er => er.IsActive).Select(er => er.Role.RoleName).ToList(),
                CreatedAt = employee.CreatedAt,
                UpdatedAt = employee.UpdatedAt
            };

            // Get HOD and Supervisor names
            if (!string.IsNullOrEmpty(employee.HodPayroll))
            {
                var hod = await _context.Employees
                    .FirstOrDefaultAsync(e => e.PayrollNo == employee.HodPayroll);
                if (hod != null)
                {
                    model.HodFullName = $"{hod.FirstName} {hod.LastName}";
                }
            }

            if (!string.IsNullOrEmpty(employee.SupervisorPayroll))
            {
                var supervisor = await _context.Employees
                    .FirstOrDefaultAsync(e => e.PayrollNo == employee.SupervisorPayroll);
                if (supervisor != null)
                {
                    model.SupervisorFullName = $"{supervisor.FirstName} {supervisor.LastName}";
                }
            }

            await PopulateDropdowns();
            return View(model);
        }

        // POST: Employee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel model, List<int>? SelectedRoles)
        {
            if (id != model.EmployeeId)
            {
                TempData["Error"] = "Invalid employee ID.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var employee = await _context.Employees
                        .Include(e => e.EmployeeRoles)
                        .FirstOrDefaultAsync(e => e.EmployeeId == id);

                    if (employee == null)
                    {
                        TempData["Error"] = "Employee not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Update employee properties
                    employee.PayrollNo = model.PayrollNo;
                    employee.RollNo = model.RollNo;
                    employee.FirstName = model.FirstName;
                    employee.LastName = model.LastName;
                    employee.EmailAddress = model.EmailAddress;
                    employee.PhoneNo = model.PhoneNo;
                    employee.StationId = model.StationId;
                    employee.DepartmentId = model.DepartmentId;
                    employee.Username = model.Username;
                    employee.EmploymentStatus = model.EmploymentStatus ?? "Active";
                    employee.EmployeeType = model.EmployeeType;
                    employee.Designation = model.Designation;
                    employee.ContractEndDate = model.ContractEndDate;
                    employee.HodPayroll = model.HodPayroll;
                    employee.SupervisorPayroll = model.SupervisorPayroll;
                    employee.UpdatedAt = DateTime.UtcNow;

                    // Update roles
                    // Deactivate all existing roles
                    foreach (var existingRole in employee.EmployeeRoles.Where(er => er.IsActive))
                    {
                        existingRole.IsActive = false;
                        existingRole.AssignedAt = DateTime.UtcNow;
                    }

                    // Add/activate selected roles
                    if (SelectedRoles != null && SelectedRoles.Any())
                    {
                        foreach (var roleId in SelectedRoles)
                        {
                            var existingEmployeeRole = employee.EmployeeRoles
                                .FirstOrDefault(er => er.RoleId == roleId);

                            if (existingEmployeeRole != null)
                            {
                                // Reactivate existing role
                                existingEmployeeRole.IsActive = true;
                                existingEmployeeRole.AssignedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                // Create new role assignment
                                var employeeRole = new Models.EmployeeRole
                                {
                                    EmployeeId = employee.EmployeeId,
                                    RoleId = roleId,
                                    IsActive = true,
                                    AssignedAt = DateTime.UtcNow
                                };

                                _context.EmployeeRoles.Add(employeeRole);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Employee '{model.FirstName} {model.LastName}' has been updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await EmployeeExists(model.EmployeeId))
                    {
                        TempData["Error"] = "Employee not found.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while updating the employee. Please try again.";
                    // Log the exception here if you have logging configured
                }
            }

            await PopulateDropdowns();
            return View(model);
        }

        private async Task<bool> EmployeeExists(int id)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeId == id);
        }

        // AJAX validation methods (lightweight checks on blur)
        [HttpGet]
        public async Task<IActionResult> CheckPayrollNoUnique(string payrollNo, int employeeId = 0)
        {
            if (string.IsNullOrWhiteSpace(payrollNo))
                return Json(new { isUnique = true });

            var exists = await _context.Employees
                .AnyAsync(e => e.PayrollNo == payrollNo && e.EmployeeId != employeeId);

            return Json(new { isUnique = !exists });
        }

        [HttpGet]
        public async Task<IActionResult> CheckRollNoUnique(string? rollNo, int employeeId = 0)
        {
            if (string.IsNullOrWhiteSpace(rollNo))
                return Json(new { isUnique = true });

            var exists = await _context.Employees
                .AnyAsync(e => e.RollNo == rollNo && e.EmployeeId != employeeId);

            return Json(new { isUnique = !exists });
        }

        [HttpGet]
        public async Task<IActionResult> CheckEmailUnique(string? emailAddress, int employeeId = 0)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                return Json(new { isUnique = true });

            var exists = await _context.Employees
                .AnyAsync(e => e.EmailAddress == emailAddress && e.EmployeeId != employeeId);

            return Json(new { isUnique = !exists });
        }

        [HttpGet]
        public async Task<IActionResult> CheckPhoneUnique(string? phoneNo, int employeeId = 0)
        {
            if (string.IsNullOrWhiteSpace(phoneNo))
                return Json(new { isUnique = true });

            var exists = await _context.Employees
                .AnyAsync(e => e.PhoneNo == phoneNo && e.EmployeeId != employeeId);

            return Json(new { isUnique = !exists });
        }

        // Helper method to populate dropdowns
        private async Task PopulateDropdowns()
        {
            var categories = await _context.OrgCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .Select(c => new { c.OrgCategoryId, c.CategoryName })
                .ToListAsync();

            var stations = await _context.Stations
                .Where(s => s.IsActive)
                .OrderBy(s => s.StationName)
                .Select(s => new { s.StationId, s.StationName, s.OrgCategoryId })
                .ToListAsync();

            var departments = await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.DepartmentName)
                .Select(d => new { d.DepartmentId, d.DepartmentName, d.StationId })
                .ToListAsync();

            var employees = await _context.Employees
                .Where(e => e.EmploymentStatus == "Active")
                .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
                .Select(e => new {
                    e.EmployeeId,
                    e.PayrollNo,
                    FullName = e.FirstName + " " + e.LastName,
                    e.StationId,
                    e.DepartmentId
                })
                .ToListAsync();

            var roles = await _context.Roles
                .Where(r => r.IsActive)
                .OrderBy(r => r.RoleName)
                .Select(r => new { r.RoleId, r.RoleName })
                .ToListAsync();

            ViewBag.OrgCategories = categories;
            ViewBag.Stations = stations;
            ViewBag.Departments = departments;
            ViewBag.Employees = employees;
            ViewBag.Roles = roles;
        }
    }
}
