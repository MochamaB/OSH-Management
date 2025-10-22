using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Models.ViewModels;
using OSHManagement.Services;

namespace OSHManagement.Controllers
{
    public class TeamTypeDefinitionController : Controller
    {
        private readonly OshDbContext _context;
        private readonly ILogger<TeamTypeDefinitionController> _logger;
        private readonly IScopeFilterService _scopeFilterService;

        public TeamTypeDefinitionController(
            OshDbContext context,
            ILogger<TeamTypeDefinitionController> logger,
            IScopeFilterService scopeFilterService)
        {
            _context = context;
            _logger = logger;
            _scopeFilterService = scopeFilterService;
        }

        // GET: TeamTypeDefinition
        public async Task<IActionResult> Index()
        {
            var teamTypes = await _context.TeamTypeDefinitions
                .Include(ttd => ttd.TeamRoleDefinitions)
                .Include(ttd => ttd.Teams)
                .OrderBy(ttd => ttd.IsSystemType ? 0 : 1)
                .ThenBy(ttd => ttd.TypeName)
                .Select(ttd => new TeamTypeDefinitionViewModel
                {
                    TeamTypeDefinitionId = ttd.TeamTypeDefinitionId,
                    TypeName = ttd.TypeName,
                    TypeCode = ttd.TypeCode,
                    Description = ttd.Description,
                    MaxTeamsPerStation = ttd.MaxTeamsPerStation,
                    RequiresStatutoryCompliance = ttd.RequiresStatutoryCompliance,
                    RequiredEmployeeRepRatioPercent = ttd.RequiredEmployeeRepRatio.HasValue ? ttd.RequiredEmployeeRepRatio * 100 : null,
                    RequiredEmployerRepRatioPercent = ttd.RequiredEmployerRepRatio.HasValue ? ttd.RequiredEmployerRepRatio * 100 : null,
                    MinFemaleRatioPercent = ttd.MinFemaleRatio.HasValue ? ttd.MinFemaleRatio * 100 : null,
                    MinMaleRatioPercent = ttd.MinMaleRatio.HasValue ? ttd.MinMaleRatio * 100 : null,
                    MinMeetingsPerYear = ttd.MinMeetingsPerYear,
                    QuorumPercentageValue = ttd.QuorumPercentage.HasValue ? ttd.QuorumPercentage * 100 : null,
                    MinMemberCount = ttd.MinMemberCount,
                    MaxMemberCount = ttd.MaxMemberCount,
                    RequiresSectionRepresentation = ttd.RequiresSectionRepresentation,
                    DefaultTermMonths = ttd.DefaultTermMonths,
                    MaxConsecutiveTerms = ttd.MaxConsecutiveTerms,
                    IsActive = ttd.IsActive,
                    IsSystemType = ttd.IsSystemType,
                    RoleCount = ttd.TeamRoleDefinitions.Count(trd => trd.IsActive),
                    TeamCount = ttd.Teams.Count(t => t.TeamStatus == "Active"),
                    CreatedAt = ttd.CreatedAt,
                    UpdatedAt = ttd.UpdatedAt
                })
                .ToListAsync();

            return View(teamTypes);
        }

        // GET: TeamTypeDefinition/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var teamType = await _context.TeamTypeDefinitions
                .Include(ttd => ttd.TeamRoleDefinitions.Where(trd => trd.IsActive))
                .Include(ttd => ttd.Teams.Where(t => t.TeamStatus == "Active"))
                    .ThenInclude(t => t.Station)
                .FirstOrDefaultAsync(ttd => ttd.TeamTypeDefinitionId == id);

            if (teamType == null)
            {
                TempData["Error"] = "Team type definition not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new TeamTypeDefinitionViewModel
            {
                TeamTypeDefinitionId = teamType.TeamTypeDefinitionId,
                TypeName = teamType.TypeName,
                TypeCode = teamType.TypeCode,
                Description = teamType.Description,
                MaxTeamsPerStation = teamType.MaxTeamsPerStation,
                RequiresStatutoryCompliance = teamType.RequiresStatutoryCompliance,
                RequiredEmployeeRepRatioPercent = teamType.RequiredEmployeeRepRatio.HasValue ? teamType.RequiredEmployeeRepRatio * 100 : null,
                RequiredEmployerRepRatioPercent = teamType.RequiredEmployerRepRatio.HasValue ? teamType.RequiredEmployerRepRatio * 100 : null,
                MinFemaleRatioPercent = teamType.MinFemaleRatio.HasValue ? teamType.MinFemaleRatio * 100 : null,
                MinMaleRatioPercent = teamType.MinMaleRatio.HasValue ? teamType.MinMaleRatio * 100 : null,
                MinMeetingsPerYear = teamType.MinMeetingsPerYear,
                QuorumPercentageValue = teamType.QuorumPercentage.HasValue ? teamType.QuorumPercentage * 100 : null,
                MinMemberCount = teamType.MinMemberCount,
                MaxMemberCount = teamType.MaxMemberCount,
                RequiresSectionRepresentation = teamType.RequiresSectionRepresentation,
                DefaultTermMonths = teamType.DefaultTermMonths,
                MaxConsecutiveTerms = teamType.MaxConsecutiveTerms,
                IsActive = teamType.IsActive,
                IsSystemType = teamType.IsSystemType,
                RoleCount = teamType.TeamRoleDefinitions.Count,
                TeamCount = teamType.Teams.Count,
                CreatedAt = teamType.CreatedAt,
                UpdatedAt = teamType.UpdatedAt
            };

            ViewBag.Roles = teamType.TeamRoleDefinitions.OrderBy(r => r.DisplayOrder).ToList();
            ViewBag.Teams = teamType.Teams.ToList();

            return View(viewModel);
        }

        // POST: TeamTypeDefinition/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var teamType = await _context.TeamTypeDefinitions
                    .FirstOrDefaultAsync(ttd => ttd.TeamTypeDefinitionId == id);

                if (teamType == null)
                {
                    return Json(new { success = false, message = "Team type not found." });
                }

                if (teamType.IsSystemType)
                {
                    return Json(new { success = false, message = "Cannot deactivate system team types." });
                }

                teamType.IsActive = !teamType.IsActive;
                teamType.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Team type '{teamType.TypeName}' {(teamType.IsActive ? "activated" : "deactivated")} successfully.",
                    isActive = teamType.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling team type status");
                return Json(new { success = false, message = "An error occurred while updating the team type status." });
            }
        }
    }
}
