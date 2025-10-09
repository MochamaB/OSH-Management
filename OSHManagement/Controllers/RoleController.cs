using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
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
    }
}
