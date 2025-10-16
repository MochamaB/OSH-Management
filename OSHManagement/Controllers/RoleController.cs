using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Models.Authorization;
using OSHManagement.Models.ViewModels;
using OSHManagement.Services;

namespace OSHManagement.Controllers
{
    [Authorize]
    public class RoleController : ScopedController
    {
        private readonly IEmployeeService _employeeService;
        private readonly IScopeFilterService _scopeFilterService;

        public RoleController(
            OshDbContext context,
            IScopeFilterService scopeFilter,
            IEmployeeService employeeService,
            ILogger<RoleController> logger)
            : base(context, scopeFilter, logger)
        {
            _employeeService = employeeService;
            _scopeFilterService = scopeFilter;
        }

        // GET: Role/Index
        public async Task<IActionResult> Index()
        {
            // Use AsSplitQuery to avoid cartesian explosion warning
            // This will execute separate queries for each collection
            var roles = await _context.Roles
                .AsSplitQuery()
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .Include(r => r.EmployeeRoles.Where(er => er.IsActive))
                .OrderBy(r => r.ScopeLevel)
                    .ThenBy(r => r.RoleName)
                .ToListAsync();

            // Map to ViewModels
            var viewModels = roles.Select(r => new RoleViewModel
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Description = r.Description,
                ScopeLevel = r.ScopeLevel,
                ScopeLevelName = r.ScopeLevel.ToString(),
                IsSystemRole = r.IsSystemRole,
                AllowCrossDepartmentAccess = r.AllowCrossDepartmentAccess,
                AllowCrossStationAccess = r.AllowCrossStationAccess,
                IsActive = r.IsActive,
                PermissionsCount = r.RolePermissions.Count,
                EmployeesCount = r.EmployeeRoles.Count(er => er.IsActive),
                PermissionNames = r.RolePermissions
                    .Select(rp => rp.Permission.PermissionName)
                    .ToList(),
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList();

            return View(viewModels);
        }

        // GET: Role/Create
        public async Task<IActionResult> Create()
        {
            // Fetch all active permissions grouped by module
            var permissions = await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Module)
                    .ThenBy(p => p.Action)
                .Select(p => new PermissionViewModel
                {
                    PermissionId = p.PermissionId,
                    PermissionName = p.PermissionName,
                    Description = p.Description,
                    Module = p.Module,
                    Action = p.Action,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            // Group permissions by module for accordion display
            var permissionsByModule = permissions
                .GroupBy(p => p.Module)
                .OrderBy(g => g.Key)
                .ToList();

            ViewBag.PermissionsByModule = permissionsByModule;
            ViewBag.AllPermissions = permissions;

            // Fetch employees for role assignment (with scope filtering)
            var employees = await _employeeService.GetEmployeesForRoleAssignmentAsync(CurrentScope);

            // Group employees by Station for accordion display
            var employeesByStation = employees
                .GroupBy(e => new { e.StationId, e.StationName })
                .OrderBy(g => g.Key.StationName)
                .ToList();

            ViewBag.EmployeesByStation = employeesByStation;
            ViewBag.AllEmployees = employees;

            return View();
        }

        // POST: Role/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if role name already exists
                    var existingRole = await _context.Roles
                        .AnyAsync(r => r.RoleName == model.RoleName);

                    if (existingRole)
                    {
                        ModelState.AddModelError("RoleName", "A role with this name already exists.");
                        return View(model);
                    }

                    // Create role entity
                    var role = new Role
                    {
                        RoleName = model.RoleName.Trim(),
                        Description = model.Description?.Trim(),
                        ScopeLevel = model.ScopeLevel,
                        IsSystemRole = model.IsSystemRole,
                        AllowCrossDepartmentAccess = model.AllowCrossDepartmentAccess,
                        AllowCrossStationAccess = model.AllowCrossStationAccess,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Roles.Add(role);
                    await _context.SaveChangesAsync();

                    // Add permissions if selected
                    if (model.SelectedPermissionIds != null && model.SelectedPermissionIds.Any())
                    {
                        foreach (var permissionId in model.SelectedPermissionIds)
                        {
                            var rolePermission = new RolePermission
                            {
                                RoleId = role.RoleId,
                                PermissionId = permissionId,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.RolePermissions.Add(rolePermission);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Add employee role assignments if selected
                    if (model.SelectedEmployeeIds != null && model.SelectedEmployeeIds.Any())
                    {
                        foreach (var employeeId in model.SelectedEmployeeIds)
                        {
                            var employeeRole = new EmployeeRole
                            {
                                EmployeeId = employeeId,
                                RoleId = role.RoleId,
                                AssignedAt = DateTime.UtcNow,
                                AssignedBy = User.Identity?.Name,
                                IsActive = true
                            };
                            _context.EmployeeRoles.Add(employeeRole);
                        }
                        await _context.SaveChangesAsync();
                    }

                    var successMessage = $"Role '{model.RoleName}' has been created successfully";
                    if (model.SelectedPermissionIds?.Any() == true)
                    {
                        successMessage += $" with {model.SelectedPermissionIds.Count} permission(s)";
                    }
                    if (model.SelectedEmployeeIds?.Any() == true)
                    {
                        successMessage += $" and assigned to {model.SelectedEmployeeIds.Count} employee(s)";
                    }
                    successMessage += ".";

                    TempData["Success"] = successMessage;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating role");
                    TempData["Error"] = "An error occurred while creating the role. Please try again.";
                }
            }

            return View(model);
        }

        // GET: Role/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleId == id);

            if (role == null)
            {
                TempData["Error"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }

            // Map to EditRoleViewModel
            var model = new EditRoleViewModel
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description,
                ScopeLevel = role.ScopeLevel,
                IsSystemRole = role.IsSystemRole,
                AllowCrossDepartmentAccess = role.AllowCrossDepartmentAccess,
                AllowCrossStationAccess = role.AllowCrossStationAccess,
                IsActive = role.IsActive,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            };

            // Fetch all permissions grouped by module
            var permissions = await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Module)
                    .ThenBy(p => p.Action)
                .Select(p => new PermissionViewModel
                {
                    PermissionId = p.PermissionId,
                    PermissionName = p.PermissionName,
                    Description = p.Description,
                    Module = p.Module,
                    Action = p.Action,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            var permissionsByModule = permissions
                .GroupBy(p => p.Module)
                .OrderBy(g => g.Key)
                .ToList();

            ViewBag.PermissionsByModule = permissionsByModule;

            // Get existing role permissions
            var existingPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            ViewBag.SelectedPermissions = existingPermissions;
            model.SelectedPermissionIds = existingPermissions;

            // Fetch employees for role assignment (with scope filtering)
            var employees = await _employeeService.GetEmployeesForRoleAssignmentAsync(CurrentScope);

            var employeesByStation = employees
                .GroupBy(e => new { e.StationId, e.StationName })
                .OrderBy(g => g.Key.StationName)
                .ToList();

            ViewBag.EmployeesByStation = employeesByStation;

            // Get employees with this role
            var existingEmployees = await _context.EmployeeRoles
                .Where(er => er.RoleId == id && er.IsActive)
                .Select(er => er.EmployeeId)
                .ToListAsync();

            ViewBag.SelectedEmployees = existingEmployees;
            model.SelectedEmployeeIds = existingEmployees;

            return View(model);
        }

        // POST: Role/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditRoleViewModel model)
        {
            if (id != model.RoleId)
            {
                TempData["Error"] = "Invalid role ID.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if role name already exists (excluding current role)
                    var existingRole = await _context.Roles
                        .AnyAsync(r => r.RoleName == model.RoleName && r.RoleId != id);

                    if (existingRole)
                    {
                        ModelState.AddModelError("RoleName", "A role with this name already exists.");
                        await LoadEditViewData(id, model);
                        return View(model);
                    }

                    var role = await _context.Roles.FindAsync(id);
                    if (role == null)
                    {
                        TempData["Error"] = "Role not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Update role properties
                    role.RoleName = model.RoleName.Trim();
                    role.Description = model.Description?.Trim();
                    role.ScopeLevel = model.ScopeLevel;
                    role.IsSystemRole = model.IsSystemRole;
                    role.AllowCrossDepartmentAccess = model.AllowCrossDepartmentAccess;
                    role.AllowCrossStationAccess = model.AllowCrossStationAccess;
                    role.IsActive = model.IsActive;
                    role.UpdatedAt = DateTime.UtcNow;

                    _context.Roles.Update(role);
                    await _context.SaveChangesAsync();

                    // Update permissions
                    var existingPermissions = await _context.RolePermissions
                        .Where(rp => rp.RoleId == id)
                        .ToListAsync();

                    _context.RolePermissions.RemoveRange(existingPermissions);
                    await _context.SaveChangesAsync();

                    if (model.SelectedPermissionIds != null && model.SelectedPermissionIds.Any())
                    {
                        foreach (var permissionId in model.SelectedPermissionIds)
                        {
                            _context.RolePermissions.Add(new RolePermission
                            {
                                RoleId = role.RoleId,
                                PermissionId = permissionId,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Update employee assignments
                    var existingEmployeeRoles = await _context.EmployeeRoles
                        .Where(er => er.RoleId == id)
                        .ToListAsync();

                    foreach (var er in existingEmployeeRoles)
                    {
                        er.IsActive = false;
                        er.RevokedAt = DateTime.UtcNow;
                    }
                    await _context.SaveChangesAsync();

                    if (model.SelectedEmployeeIds != null && model.SelectedEmployeeIds.Any())
                    {
                        foreach (var employeeId in model.SelectedEmployeeIds)
                        {
                            var existing = existingEmployeeRoles.FirstOrDefault(er => er.EmployeeId == employeeId);
                            if (existing != null)
                            {
                                existing.IsActive = true;
                                existing.RevokedAt = null;
                            }
                            else
                            {
                                _context.EmployeeRoles.Add(new EmployeeRole
                                {
                                    EmployeeId = employeeId,
                                    RoleId = role.RoleId,
                                    AssignedAt = DateTime.UtcNow,
                                    AssignedBy = User.Identity?.Name,
                                    IsActive = true
                                });
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    TempData["Success"] = $"Role '{model.RoleName}' has been updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating role");
                    TempData["Error"] = "An error occurred while updating the role. Please try again.";
                }
            }

            await LoadEditViewData(id, model);
            return View(model);
        }

        private async Task LoadEditViewData(int roleId, EditRoleViewModel model)
        {
            var permissions = await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Module).ThenBy(p => p.Action)
                .Select(p => new PermissionViewModel
                {
                    PermissionId = p.PermissionId,
                    PermissionName = p.PermissionName,
                    Description = p.Description,
                    Module = p.Module,
                    Action = p.Action,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            ViewBag.PermissionsByModule = permissions.GroupBy(p => p.Module).OrderBy(g => g.Key).ToList();
            ViewBag.SelectedPermissions = model.SelectedPermissionIds;

            var employees = await _employeeService.GetEmployeesForRoleAssignmentAsync(CurrentScope);
            ViewBag.EmployeesByStation = employees.GroupBy(e => new { e.StationId, e.StationName }).OrderBy(g => g.Key.StationName).ToList();
            ViewBag.SelectedEmployees = model.SelectedEmployeeIds;
        }
    }
}
