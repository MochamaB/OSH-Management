using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Models.Authorization;
using OSHManagement.Models.ViewModels;

namespace OSHManagement.Controllers
{
    [Authorize]
    public class RoleController : Controller
    {
        private readonly OshDbContext _context;
        private readonly ILogger<RoleController> _logger;

        public RoleController(
            OshDbContext context,
            ILogger<RoleController> logger)
        {
            _context = context;
            _logger = logger;
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
        public IActionResult Create()
        {
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

                    TempData["Success"] = $"Role '{model.RoleName}' has been created successfully.";
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
    }
}
