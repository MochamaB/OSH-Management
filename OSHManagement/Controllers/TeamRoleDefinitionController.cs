using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Models.ViewModels;
using OSHManagement.Services;


namespace OSHManagement.Controllers
{
    public class TeamRoleDefinitionController : Controller
    {
        private readonly OshDbContext _context;
        private readonly ILogger<TeamRoleDefinitionController> _logger;
        private readonly IScopeFilterService _scopeFilterService;

        public TeamRoleDefinitionController(
            OshDbContext context,
            ILogger<TeamRoleDefinitionController> logger,
            IScopeFilterService scopeFilterService)
        {
            _context = context;
            _logger = logger;
            _scopeFilterService = scopeFilterService;
        }

        // GET: TeamRoleDefinition
        public async Task<IActionResult> Index(string? teamType = null)
        {
            var query = _context.TeamRoleDefinitions.AsQueryable();

            // Filter by team type if specified
            if (!string.IsNullOrEmpty(teamType))
            {
                query = query.Where(trd => trd.TeamType == teamType);
                ViewBag.SelectedTeamType = teamType;
            }

            var roleDefinitions = await query
                .OrderBy(trd => trd.TeamType)
                .ThenBy(trd => trd.DisplayOrder)
                .Select(trd => new TeamRoleDefinitionViewModel
                {
                    TeamRoleDefinitionId = trd.TeamRoleDefinitionId,
                    TeamType = trd.TeamType,
                    RoleName = trd.RoleName,
                    Description = trd.Description,
                    RequiresVotingRights = trd.RequiresVotingRights,
                    MaxOccurrences = trd.MaxOccurrences,
                    MinOccurrences = trd.MinOccurrences,
                    RequiredQualifications = trd.RequiredQualifications,
                    DisplayOrder = trd.DisplayOrder,
                    IsActive = trd.IsActive,
                    UsageCount = trd.TeamMembers.Count(tm => tm.IsActive)
                })
                .ToListAsync();

            // Get team types for filter dropdown
            ViewBag.TeamTypes = await _context.TeamRoleDefinitions
                .Select(trd => trd.TeamType)
                .Distinct()
                .OrderBy(tt => tt)
                .ToListAsync();

            return View(roleDefinitions);
        }

        // GET: TeamRoleDefinition/Create
        public IActionResult Create()
        {
            ViewBag.TeamTypes = new List<string> { "OshCommittee", "RiskAssessment", "Investigation", "Custom" };
            return View();
        }

        // POST: TeamRoleDefinition/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeamRoleDefinitionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate role name in same team type
                    var exists = await _context.TeamRoleDefinitions
                        .AnyAsync(trd => trd.TeamType == model.TeamType && trd.RoleName == model.RoleName);

                    if (exists)
                    {
                        ModelState.AddModelError("RoleName", $"A role with the name '{model.RoleName}' already exists for {model.TeamType}.");
                        ViewBag.TeamTypes = new List<string> { "OshCommittee", "RiskAssessment", "Investigation", "Custom" };
                        return View(model);
                    }

                    var roleDefinition = new TeamRoleDefinition
                    {
                        TeamType = model.TeamType,
                        RoleName = model.RoleName.Trim(),
                        Description = model.Description?.Trim(),
                        RequiresVotingRights = model.RequiresVotingRights,
                        MaxOccurrences = model.MaxOccurrences,
                        MinOccurrences = model.MinOccurrences,
                        RequiredQualifications = model.RequiredQualifications?.Trim(),
                        DisplayOrder = model.DisplayOrder,
                        IsActive = model.IsActive,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.TeamRoleDefinitions.Add(roleDefinition);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Role '{model.RoleName}' created successfully for {model.TeamType}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating team role definition");
                    TempData["Error"] = "An error occurred while creating the role definition.";
                }
            }

            ViewBag.TeamTypes = new List<string> { "OSH Committee", "Risk Assessment", "Investigation", "Custom" };
            return View(model);
        }

        // GET: TeamRoleDefinition/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var roleDefinition = await _context.TeamRoleDefinitions
                .FirstOrDefaultAsync(trd => trd.TeamRoleDefinitionId == id);

            if (roleDefinition == null)
            {
                TempData["Error"] = "Role definition not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new TeamRoleDefinitionViewModel
            {
                TeamRoleDefinitionId = roleDefinition.TeamRoleDefinitionId,
                TeamType = roleDefinition.TeamType,
                RoleName = roleDefinition.RoleName,
                Description = roleDefinition.Description,
                RequiresVotingRights = roleDefinition.RequiresVotingRights,
                MaxOccurrences = roleDefinition.MaxOccurrences,
                MinOccurrences = roleDefinition.MinOccurrences,
                RequiredQualifications = roleDefinition.RequiredQualifications,
                DisplayOrder = roleDefinition.DisplayOrder,
                IsActive = roleDefinition.IsActive,
                UsageCount = await _context.TeamMembers.CountAsync(tm => tm.TeamRoleDefinitionId == id && tm.IsActive)
            };

            ViewBag.TeamTypes = new List<string> { "OSH Committee", "Risk Assessment", "Investigation", "Custom" };
            return View(model);
        }

        // POST: TeamRoleDefinition/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TeamRoleDefinitionViewModel model)
        {
            if (id != model.TeamRoleDefinitionId)
            {
                TempData["Error"] = "Invalid role definition ID.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var roleDefinition = await _context.TeamRoleDefinitions
                        .FirstOrDefaultAsync(trd => trd.TeamRoleDefinitionId == id);

                    if (roleDefinition == null)
                    {
                        TempData["Error"] = "Role definition not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Check for duplicate if name or team type changed
                    if (roleDefinition.RoleName != model.RoleName || roleDefinition.TeamType != model.TeamType)
                    {
                        var exists = await _context.TeamRoleDefinitions
                            .AnyAsync(trd => trd.TeamType == model.TeamType &&
                                           trd.RoleName == model.RoleName &&
                                           trd.TeamRoleDefinitionId != id);

                        if (exists)
                        {
                            ModelState.AddModelError("RoleName", $"A role with the name '{model.RoleName}' already exists for {model.TeamType}.");
                            ViewBag.TeamTypes = new List<string> { "OSH Committee", "Risk Assessment", "Investigation", "Custom" };
                            return View(model);
                        }
                    }

                    roleDefinition.TeamType = model.TeamType;
                    roleDefinition.RoleName = model.RoleName.Trim();
                    roleDefinition.Description = model.Description?.Trim();
                    roleDefinition.RequiresVotingRights = model.RequiresVotingRights;
                    roleDefinition.MaxOccurrences = model.MaxOccurrences;
                    roleDefinition.MinOccurrences = model.MinOccurrences;
                    roleDefinition.RequiredQualifications = model.RequiredQualifications?.Trim();
                    roleDefinition.DisplayOrder = model.DisplayOrder;
                    roleDefinition.IsActive = model.IsActive;
                    roleDefinition.UpdatedAt = DateTime.UtcNow;

                    _context.TeamRoleDefinitions.Update(roleDefinition);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Role '{model.RoleName}' updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating team role definition");
                    TempData["Error"] = "An error occurred while updating the role definition.";
                }
            }

            ViewBag.TeamTypes = new List<string> { "OSH Committee", "Risk Assessment", "Investigation", "Custom" };
            return View(model);
        }

        // POST: TeamRoleDefinition/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var roleDefinition = await _context.TeamRoleDefinitions
                    .Include(trd => trd.TeamMembers)
                    .FirstOrDefaultAsync(trd => trd.TeamRoleDefinitionId == id);

                if (roleDefinition == null)
                {
                    TempData["Error"] = "Role definition not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if role is in use
                var usageCount = roleDefinition.TeamMembers.Count(tm => tm.IsActive);
                if (usageCount > 0)
                {
                    TempData["Error"] = $"Cannot delete role '{roleDefinition.RoleName}' because it is assigned to {usageCount} active team member(s). Please reassign or remove those members first.";
                    return RedirectToAction(nameof(Index));
                }

                _context.TeamRoleDefinitions.Remove(roleDefinition);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Role '{roleDefinition.RoleName}' deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting team role definition");
                TempData["Error"] = "An error occurred while deleting the role definition.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: TeamRoleDefinition/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var roleDefinition = await _context.TeamRoleDefinitions
                    .FirstOrDefaultAsync(trd => trd.TeamRoleDefinitionId == id);

                if (roleDefinition == null)
                {
                    return Json(new { success = false, message = "Role definition not found." });
                }

                roleDefinition.IsActive = !roleDefinition.IsActive;
                roleDefinition.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Json(new {
                    success = true,
                    message = $"Role '{roleDefinition.RoleName}' {(roleDefinition.IsActive ? "activated" : "deactivated")} successfully.",
                    isActive = roleDefinition.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling role definition status");
                return Json(new { success = false, message = "An error occurred while updating the role status." });
            }
        }
    }
}
