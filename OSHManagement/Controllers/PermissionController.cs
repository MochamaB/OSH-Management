using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models.ViewModels;

namespace OSHManagement.Controllers
{
    [Authorize]
    public class PermissionController : Controller
    {
        private readonly OshDbContext _context;
        private readonly ILogger<PermissionController> _logger;

        public PermissionController(
            OshDbContext context,
            ILogger<PermissionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Permission/Index
        public async Task<IActionResult> Index()
        {
            var permissions = await _context.Permissions
                .AsSplitQuery()  // FIX: Prevent cartesian explosion
                .Include(p => p.RolePermissions)
                    .ThenInclude(rp => rp.Role)
                .OrderBy(p => p.Module)
                    .ThenBy(p => p.Action)
                .ToListAsync();

            // Map to ViewModels
            var viewModels = permissions.Select(p => new PermissionViewModel
            {
                PermissionId = p.PermissionId,
                PermissionName = p.PermissionName,
                Description = p.Description,
                Module = p.Module,
                Action = p.Action,
                IsActive = p.IsActive,
                AssignedRolesCount = p.RolePermissions.Count,
                RoleNames = p.RolePermissions
                    .Select(rp => rp.Role.RoleName)
                    .ToList(),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();

            return View(viewModels);
        }
    }
}
