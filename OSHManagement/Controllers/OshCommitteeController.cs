using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Models.ViewModels;
using OSHManagement.Services;

namespace OSHManagement.Controllers
{
    [Authorize]
    public class OshCommitteeController : ScopedController
    {
        public OshCommitteeController(
            OshDbContext context,
            IScopeFilterService scopeFilter,
            ILogger<OshCommitteeController> logger)
            : base(context, scopeFilter, logger)
        {
        }

        /// <summary>
        /// Main index - Lists all OSH Committees within user's scope
        /// </summary>
        public async Task<IActionResult> Index(string? filter)
        {
            // Build base query
            var query = _context.Teams.AsQueryable();

            // Apply scope filtering FIRST
            query = ApplyScope(query);

            // Add includes after scope
            query = query
                .Include(t => t.Station)
                    .ThenInclude(s => s.OrgCategory)
                .Include(t => t.TeamTypeDefinition)
                .Include(t => t.OshCommitteeConfig)
                .Include(t => t.TeamMembers);

            // Load data
            var allTeams = await query.ToListAsync();

            // Filter for OSH Committees in memory (handles both old TeamType and new TeamTypeDefinition)
            var committees = allTeams
                .Where(t => t.TeamType == "OSH_Committee" || 
                           (t.TeamTypeDefinition != null && 
                            (t.TeamTypeDefinition.TypeName == "OSH_Committee" || 
                             t.TeamTypeDefinition.TypeCode == "OSH_COMM")))
                .ToList();

            // Apply additional filters
            if (!string.IsNullOrEmpty(filter))
            {
                if (filter == "pending")
                {
                    committees = committees.Where(t => t.OshCommitteeConfig == null).ToList();
                }
                else if (filter == "active")
                {
                    committees = committees.Where(t => t.OshCommitteeConfig != null).ToList();
                }
            }

            // Calculate summary metrics
            var totalCommittees = committees.Count;
            var activeCommittees = committees.Count(t => t.OshCommitteeConfig != null);
            var formingCommittees = committees.Count(t => t.OshCommitteeConfig == null);
            var complianceRate = totalCommittees > 0 
                ? Math.Round((decimal)activeCommittees / totalCommittees * 100, 1) 
                : 0;

            ViewBag.TotalCommittees = totalCommittees;
            ViewBag.ActiveCommittees = activeCommittees;
            ViewBag.FormingCommittees = formingCommittees;
            ViewBag.ComplianceRate = complianceRate;
            ViewBag.CurrentFilter = filter;

            return View(committees);
        }

        /// <summary>
        /// Committee Details Dashboard (specific committee)
        /// </summary>
        public async Task<IActionResult> Details(int teamId)
        {
            var query = _context.Teams
                .Include(t => t.Station)
                    .ThenInclude(s => s.OrgCategory)
                .Include(t => t.TeamTypeDefinition)
                .Include(t => t.OshCommitteeConfig)
                .Include(t => t.TeamMembers)
                    .ThenInclude(m => m.Employee)
                .Where(t => t.TeamId == teamId);

            // Apply scope filtering
            var scopedQuery = ApplyScope(query);
            var team = await scopedQuery.FirstOrDefaultAsync();

            if (team == null)
            {
                return NotFound();
            }

            // Build view model
            var viewModel = new OshCommitteeViewModel
            {
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                StationName = $"{team.Station.StationName} ({team.Station.OrgCategory?.CategoryName ?? "N/A"})",
                TeamStatus = team.TeamStatus,
                FormationDate = team.FormationDate,
                IsActivated = team.OshCommitteeConfig != null,
                Config = team.OshCommitteeConfig != null ? new OshCommitteeConfigViewModel
                {
                    IsCommitteeTrained = team.OshCommitteeConfig.IsCommitteeTrained,
                    TrainingDate = team.OshCommitteeConfig.TrainingDate,
                    HasMeetingSchedule = team.OshCommitteeConfig.HasMeetingSchedule,
                    InspectionFrequency = team.OshCommitteeConfig.InspectionFrequency,
                    LastInspectionDate = team.OshCommitteeConfig.LastInspectionDate,
                    NextInspectionDate = team.OshCommitteeConfig.NextInspectionDate
                } : null,
                Metrics = await CalculateCommitteeMetrics(teamId)
            };

            return View(viewModel);
        }

        /// <summary>
        /// Calculate committee metrics for dashboard
        /// </summary>
        private async Task<CommitteeMetrics> CalculateCommitteeMetrics(int teamId)
        {
            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                .Include(t => t.OshCommitteeConfig)
                .FirstOrDefaultAsync(t => t.TeamId == teamId);

            if (team == null)
            {
                return new CommitteeMetrics();
            }

            // Get issue counts
            var openIssues = await _context.CommitteeIssues
                .Where(i => i.TeamId == teamId && i.IssueStatus == "Open")
                .CountAsync();

            // Get recommendation counts
            var pendingRecommendations = await _context.CommitteeRecommendations
                .Where(r => r.TeamId == teamId && r.ImplementationStatus == "Pending")
                .CountAsync();

            // Get action counts
            var activeActions = await _context.CommitteeActions
                .Where(a => a.TeamId == teamId && 
                           (a.ActionStatus == "Pending" || a.ActionStatus == "In Progress"))
                .CountAsync();

            var overdueActions = await _context.CommitteeActions
                .Where(a => a.TeamId == teamId && 
                           a.DueDate.HasValue && 
                           a.DueDate.Value < DateTime.UtcNow &&
                           !a.CompletionDate.HasValue)
                .CountAsync();

            return new CommitteeMetrics
            {
                TotalMembers = team.TeamMembers.Count,
                OpenIssues = openIssues,
                PendingRecommendations = pendingRecommendations,
                ActiveActions = activeActions,
                OverdueActions = overdueActions,
                NextInspectionDate = team.OshCommitteeConfig?.NextInspectionDate
            };
        }
    }
}
