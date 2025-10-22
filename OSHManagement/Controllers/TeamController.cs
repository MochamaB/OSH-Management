using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Models.Enums;
using OSHManagement.Models.ViewModels;
using OSHManagement.Services;
using OSHManagement.Services.Notifications;

namespace OSHManagement.Controllers
{
    [Authorize]
    public class TeamController : ScopedController
    {
        private readonly IScopeFilterService _scopeFilterService;
        private readonly IEmployeeService _employeeService;
        private readonly ITeamNotificationService _teamNotifications;

        public TeamController(
            OshDbContext context,
            IScopeFilterService scopeFilter,
            IEmployeeService employeeService,
            ITeamNotificationService teamNotifications,
            ILogger<TeamController> logger)
            : base(context, scopeFilter, logger)
        {
            _scopeFilterService = scopeFilter;
            _employeeService = employeeService;
            _teamNotifications = teamNotifications;
        }

        // GET: Team/Index
        public async Task<IActionResult> Index(string? search, string? teamType, int? stationId, string? status)
        {
            // Start with base query
            var query = _context.Teams.AsQueryable();

            // ⚠️ CRITICAL: Apply scope FIRST (security takes precedence)
            query = _scopeFilterService.ApplyScope(query, CurrentScope);

            // THEN apply includes (only for scoped data)
            query = query
                .Include(t => t.Station)
                .Include(t => t.TeamMembers.Where(tm => tm.IsActive));

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(t =>
                    t.TeamName.ToLower().Contains(search) ||
                    (t.TeamDescription != null && t.TeamDescription.ToLower().Contains(search)) ||
                    t.TeamMembers.Any(tm => tm.EmployeePayroll.ToLower().Contains(search))
                );
            }

            // Apply team type filter
            if (!string.IsNullOrWhiteSpace(teamType))
            {
                query = query.Where(t => t.TeamType == teamType);
            }

            // Apply station filter
            if (stationId.HasValue && stationId.Value > 0)
            {
                query = query.Where(t => t.StationId == stationId.Value);
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.TeamStatus == status);
            }

            // Order by creation date (newest first)
            query = query.OrderByDescending(t => t.CreatedAt);

            // Execute query and map to ViewModels
            var teams = await query.ToListAsync();

            var viewModels = teams.Select(t => new TeamViewModel
            {
                TeamId = t.TeamId,
                TeamName = t.TeamName,
                TeamType = t.TeamType,
                TeamDescription = t.TeamDescription,
                StationName = t.Station.StationName,
                StationId = t.StationId,
                TeamStatus = t.TeamStatus,
                FormationDate = t.FormationDate,
                DisbandDate = t.DisbandDate,
                ActiveMemberCount = t.TeamMembers.Count(tm => tm.IsActive),
                MaxMemberCount = t.MaxMemberCount,
                RequiredMemberCount = t.RequiredMemberCount,
                RequiresSectionRepresentation = t.RequiresSectionRepresentation,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            }).ToList();

            return View(viewModels);
        }

        // GET: Team/Create
        public async Task<IActionResult> Create()
        {
            // Fetch stations for dropdown (scope-filtered)
            var stationsQuery = _context.Stations.AsQueryable();
            stationsQuery = _scopeFilterService.ApplyScope(stationsQuery, CurrentScope);

            var stations = await stationsQuery
                .OrderBy(s => s.StationName)
                .Select(s => new { s.StationId, s.StationName })
                .ToListAsync();

            ViewBag.Stations = stations;

            // Fetch sections for member section assignment (scope-filtered)
            var sectionsQuery = _context.Sections.AsQueryable();
            sectionsQuery = _scopeFilterService.ApplyScope(sectionsQuery, CurrentScope);

            var sections = await sectionsQuery
                .OrderBy(s => s.SectionName)
                .Select(s => new { s.SectionId, s.SectionName, s.StationId })
                .ToListAsync();

            ViewBag.Sections = sections;

            // Don't load employees initially - they will be loaded via AJAX based on selected station
            ViewBag.EmployeesByDepartment = new List<object>();
            ViewBag.AllEmployees = new List<object>();

            // Team type options from enum (dictionary for proper value/text mapping)
            ViewBag.TeamTypes = TeamEnumExtensions.GetTeamTypeDictionary();

            // Load all team role definitions grouped by team type
            ViewBag.TeamRoleDefinitions = await _context.TeamRoleDefinitions
                .Where(trd => trd.IsActive)
                .OrderBy(trd => trd.TeamType)
                .ThenBy(trd => trd.DisplayOrder)
                .Select(trd => new
                {
                    trd.TeamRoleDefinitionId,
                    trd.TeamType,
                    trd.RoleName,
                    trd.Description,
                    trd.RequiresVotingRights,
                    trd.MaxOccurrences,
                    trd.MinOccurrences,
                    trd.RequiredQualifications
                })
                .ToListAsync();

            // Frequency options from enum
            ViewBag.FrequencyOptions = TeamEnumExtensions.GetFrequencyValues();

            // Education level options from enum
            ViewBag.EducationLevels = TeamEnumExtensions.GetEducationLevelValues();

            return View();
        }

        // GET: Team/GetEmployeesByStation/5
        [HttpGet]
        public async Task<IActionResult> GetEmployeesByStation(int stationId)
        {
            // Get all employees for the selected station (scope-filtered)
            var employees = await _employeeService.GetEmployeesForRoleAssignmentAsync(CurrentScope);

            // Filter by station
            var stationEmployees = employees
                .Where(e => e.StationId == stationId)
                .ToList();

            // Group by Department for accordion display
            var employeesByDepartment = stationEmployees
                .GroupBy(e => new { e.DepartmentId, e.DepartmentName, e.StationId, e.StationName })
                .OrderBy(g => g.Key.DepartmentName);

            ViewBag.EmployeesByDepartment = employeesByDepartment;

            // Return the partial view
            return PartialView("_MemberSelectionAccordion");
        }

        // POST: Team/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTeamViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Validate station is within user's scope
                    var stationsQuery = _context.Stations.AsQueryable();
                    stationsQuery = _scopeFilterService.ApplyScope(stationsQuery, CurrentScope);
                    var stationExists = await stationsQuery.AnyAsync(s => s.StationId == model.StationId);

                    if (!stationExists)
                    {
                        ModelState.AddModelError("StationId", "You do not have permission to create teams for this station.");
                        await LoadCreateViewData();
                        return View(model);
                    }

                    // Check if team name already exists in the station
                    var existingTeam = await _context.Teams
                        .AnyAsync(t => t.TeamName == model.TeamName && t.StationId == model.StationId);

                    if (existingTeam)
                    {
                        ModelState.AddModelError("TeamName", "A team with this name already exists at this station.");
                        await LoadCreateViewData();
                        return View(model);
                    }

                    // Create team entity
                    var team = new Team
                    {
                        TeamName = model.TeamName.Trim(),
                        TeamType = model.TeamType,
                        TeamDescription = model.TeamDescription?.Trim(),
                        StationId = model.StationId,
                        FormationDate = model.FormationDate,
                        TeamStatus = model.SaveAsDraft ? "Inactive" : "Active",
                        RequiredMemberCount = model.RequiredMemberCount,
                        MaxMemberCount = model.MaxMemberCount,
                        RequiresSectionRepresentation = model.RequiresSectionRepresentation,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Teams.Add(team);
                    await _context.SaveChangesAsync();

                    // Create type-specific configuration based on team type
                    switch (model.TeamType)
                    {
                        case "OSH Committee":
                            var oshConfig = new OshCommitteeConfig
                            {
                                TeamId = team.TeamId,
                                IsCommitteeTrained = model.IsCommitteeTrained,
                                TrainingDate = model.TrainingDate,
                                HasMeetingSchedule = model.HasMeetingSchedule,
                                InspectionFrequency = model.InspectionFrequency,
                                NextInspectionDate = model.NextInspectionDate,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.OshCommitteeConfigs.Add(oshConfig);
                            break;

                        case "Risk Assessment":
                            var riskConfig = new RiskAssessmentConfig
                            {
                                TeamId = team.TeamId,
                                AssessmentFrequency = model.AssessmentFrequency,
                                TeamQualifications = model.TeamQualifications,
                                NextAssessmentDate = model.NextAssessmentDate,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.RiskAssessmentConfigs.Add(riskConfig);
                            break;

                        case "Investigation":
                            var investigationConfig = new IncidentInvestigationConfig
                            {
                                TeamId = team.TeamId,
                                InvestigationScope = model.InvestigationScope,
                                TeamExpertise = model.TeamExpertise,
                                ResponseTimeHours = model.ResponseTimeHours,
                                EscalationThreshold = model.EscalationThreshold,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.IncidentInvestigationConfigs.Add(investigationConfig);
                            break;
                    }

                    await _context.SaveChangesAsync();

                    // Add team members if selected
                    if (model.SelectedMemberPayrolls != null && model.SelectedMemberPayrolls.Any())
                    {
                        // Validate all employees are within scope
                        var employeesQuery = _context.Employees
                            .Where(e => model.SelectedMemberPayrolls.Contains(e.PayrollNo));
                        employeesQuery = _scopeFilterService.ApplyScope(employeesQuery, CurrentScope);

                        var validEmployeePayrolls = await employeesQuery
                            .Select(e => e.PayrollNo)
                            .ToListAsync();

                        foreach (var payroll in model.SelectedMemberPayrolls)
                        {
                            if (!validEmployeePayrolls.Contains(payroll))
                            {
                                _logger.LogWarning($"User attempted to add employee {payroll} outside their scope to team {team.TeamId}");
                                continue; // Skip employees outside scope
                            }

                            // Find member details from model.Members
                            var memberDto = model.Members.FirstOrDefault(m => m.EmployeePayroll == payroll);
                            if (memberDto == null)
                            {
                                // If no details provided, skip this member or find a default role
                                // Get a generic role for this team type
                                /*
                                var defaultRole = await _context.TeamRoleDefinitions
                                    .Where(trd => trd.TeamType == model.TeamType && trd.IsActive)
                                    .OrderBy(trd => trd.DisplayOrder)
                                    .FirstOrDefaultAsync();
                                

                                if (defaultRole == null)
                                {
                                    ModelState.AddModelError("", $"No role definitions found for team type '{model.TeamType}'. Please contact administrator.");
                                    await LoadCreateViewData();
                                    return View(model);
                                }

                                memberDto = new AddTeamMemberDto
                                {
                                    EmployeePayroll = payroll,
                                    TeamRoleDefinitionId = defaultRole.TeamRoleDefinitionId,
                                    AppointmentDate = DateTime.Today,
                                    IsVotingMember = defaultRole.RequiresVotingRights
                                };
                                */
                            }

                                
                            var teamMember = new TeamMember
                            {
                                TeamId = team.TeamId,
                                EmployeePayroll = memberDto.EmployeePayroll,
                                TeamRoleDefinitionId = memberDto.TeamRoleDefinitionId,
                                SectionId = memberDto.SectionId,
                                EducationLevel = memberDto.EducationLevel,
                                RelevantExperience = memberDto.RelevantExperience,
                                AppointmentDate = memberDto.AppointmentDate,
                                IsVotingMember = memberDto.IsVotingMember,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            };

                            _context.TeamMembers.Add(teamMember);
                        }

                        await _context.SaveChangesAsync();
                    }

                    var successMessage = $"Team '{model.TeamName}' has been created successfully";
                    if (model.SelectedMemberPayrolls?.Any() == true)
                    {
                        successMessage += $" with {model.SelectedMemberPayrolls.Count} member(s)";
                    }
                    if (model.SaveAsDraft)
                    {
                        successMessage += " as a draft (Inactive)";
                    }
                    successMessage += ".";

                    TempData["Success"] = successMessage;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating team");
                    TempData["Error"] = "An error occurred while creating the team. Please try again.";
                }
            }

            await LoadCreateViewData();
            return View(model);
        }

        // GET: Team/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Fetch team with related data (apply scope filter first)
            var teamsQuery = _context.Teams.AsQueryable();
            teamsQuery = _scopeFilterService.ApplyScope(teamsQuery, CurrentScope);

            var team = await teamsQuery
                .Include(t => t.Station)
                .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.Employee)
                .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.TeamRoleDefinition)
                .Include(t => t.OshCommitteeConfig)
                .Include(t => t.RiskAssessmentConfig)
                .Include(t => t.IncidentInvestigationConfig)
                .FirstOrDefaultAsync(t => t.TeamId == id);

            if (team == null)
            {
                TempData["Error"] = "Team not found or you don't have permission to edit it.";
                return RedirectToAction(nameof(Index));
            }

            // Map to EditTeamViewModel
            var model = new EditTeamViewModel
            {
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                TeamType = team.TeamType,
                TeamDescription = team.TeamDescription,
                StationId = team.StationId,
                TeamStatus = team.TeamStatus,
                FormationDate = team.FormationDate,
                RequiredMemberCount = team.RequiredMemberCount,
                MaxMemberCount = team.MaxMemberCount,
                RequiresSectionRepresentation = team.RequiresSectionRepresentation,
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt
            };

            // Load ViewBag data
            await LoadEditViewData(team.StationId);

            // Pass team members for member management (only active)
            ViewBag.CurrentMembers = team.TeamMembers
                .Where(tm => tm.IsActive)
                .Select(tm => new
                {
                    tm.MemberId,
                    tm.EmployeePayroll,
                    EmployeeName = $"{tm.Employee.FirstName} {tm.Employee.LastName}",
                    tm.TeamRoleDefinitionId,
                    MemberRole = tm.TeamRoleDefinition != null ? tm.TeamRoleDefinition.RoleName : "No Role Assigned",
                    tm.SectionId,
                    tm.EducationLevel,
                    tm.RelevantExperience,
                    tm.AppointmentDate,
                    tm.IsVotingMember
                }).ToList();

            // Load role definitions for this team type
            var roleDefinitions = await _context.TeamRoleDefinitions
                .Where(trd => trd.TeamType == team.TeamType && trd.IsActive)
                .OrderBy(trd => trd.DisplayOrder)
                .ToListAsync();

            // Calculate current counts in memory (after materializing the query)
            var roleDefinitionsWithCounts = roleDefinitions.Select(trd => new
            {
                trd.TeamRoleDefinitionId,
                trd.RoleName,
                trd.Description,
                trd.MinOccurrences,
                trd.MaxOccurrences,
                trd.RequiresVotingRights,
                trd.RequiredQualifications,
                CurrentCount = team.TeamMembers.Count(tm => tm.TeamRoleDefinitionId == trd.TeamRoleDefinitionId && tm.IsActive)
            }).ToList();

            ViewBag.RoleDefinitions = roleDefinitionsWithCounts;

            // Calculate role-based minimums and compliance
            var calculatedMinimum = roleDefinitions
                .Where(r => r.MinOccurrences.HasValue)
                .Sum(r => r.MinOccurrences.Value);

            ViewBag.CalculatedMinimum = calculatedMinimum;

            // Check section representation
            if (team.RequiresSectionRepresentation)
            {
                var sectionCount = team.TeamMembers
                    .Where(tm => tm.IsActive && tm.SectionId != null)
                    .Select(tm => tm.SectionId)
                    .Distinct()
                    .Count();

                ViewBag.SectionCount = sectionCount;
            }

            // Pass type-specific config
            if (team.TeamType == "OSH Committee" && team.OshCommitteeConfig != null)
            {
                ViewBag.OshCommitteeConfig = team.OshCommitteeConfig;
            }
            else if (team.TeamType == "Risk Assessment" && team.RiskAssessmentConfig != null)
            {
                ViewBag.RiskAssessmentConfig = team.RiskAssessmentConfig;
            }
            else if (team.TeamType == "Investigation" && team.IncidentInvestigationConfig != null)
            {
                ViewBag.IncidentInvestigationConfig = team.IncidentInvestigationConfig;
            }

            return View(model);
        }

        // POST: Team/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditTeamViewModel model, IFormCollection form)
        {
            if (id != model.TeamId)
            {
                TempData["Error"] = "Invalid team ID.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch team with scope validation
                    var teamsQuery = _context.Teams.AsQueryable();
                    teamsQuery = _scopeFilterService.ApplyScope(teamsQuery, CurrentScope);

                    var team = await teamsQuery
                        .Include(t => t.OshCommitteeConfig)
                        .Include(t => t.RiskAssessmentConfig)
                        .Include(t => t.IncidentInvestigationConfig)
                        .FirstOrDefaultAsync(t => t.TeamId == id);

                    if (team == null)
                    {
                        TempData["Error"] = "Team not found or you don't have permission to edit it.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Check if team name already exists in the station (excluding current team)
                    var existingTeam = await _context.Teams
                        .AnyAsync(t => t.TeamName == model.TeamName && t.StationId == model.StationId && t.TeamId != id);

                    if (existingTeam)
                    {
                        ModelState.AddModelError("TeamName", "A team with this name already exists at this station.");
                        await LoadEditViewData(model.StationId);
                        return View(model);
                    }

                    // Update team properties
                    team.TeamName = model.TeamName.Trim();
                    team.TeamDescription = model.TeamDescription?.Trim();
                    team.TeamStatus = model.TeamStatus;
                    team.FormationDate = model.FormationDate;
                    team.RequiredMemberCount = model.RequiredMemberCount;
                    team.MaxMemberCount = model.MaxMemberCount;
                    team.RequiresSectionRepresentation = model.RequiresSectionRepresentation;
                    team.UpdatedAt = DateTime.UtcNow;

                    // Handle type-specific configs
                    if (team.TeamType == "OSH Committee")
                    {
                        await UpdateOshCommitteeConfig(team, form);
                    }
                    else if (team.TeamType == "Risk Assessment")
                    {
                        await UpdateRiskAssessmentConfig(team, form);
                    }
                    else if (team.TeamType == "Investigation")
                    {
                        await UpdateInvestigationConfig(team, form);
                    }

                    _context.Teams.Update(team);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Team '{model.TeamName}' has been updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating team");
                    TempData["Error"] = "An error occurred while updating the team. Please try again.";
                }
            }

            await LoadEditViewData(model.StationId);
            return View(model);
        }

        private async Task UpdateOshCommitteeConfig(Team team, IFormCollection form)
        {
            var configId = int.TryParse(form["OshConfig.ConfigId"], out var id) ? id : 0;

            if (configId == 0)
            {
                // Create new config
                team.OshCommitteeConfig = new OshCommitteeConfig
                {
                    TeamId = team.TeamId,
                    IsCommitteeTrained = form["OshConfig.IsCommitteeTrained"].Contains("true"),
                    TrainingDate = DateTime.TryParse(form["OshConfig.TrainingDate"], out var td) ? td : (DateTime?)null,
                    HasMeetingSchedule = form["OshConfig.HasMeetingSchedule"].Contains("true"),
                    InspectionFrequency = form["OshConfig.InspectionFrequency"].ToString(),
                    LastInspectionDate = DateTime.TryParse(form["OshConfig.LastInspectionDate"], out var lid) ? lid : (DateTime?)null,
                    NextInspectionDate = DateTime.TryParse(form["OshConfig.NextInspectionDate"], out var nid) ? nid : (DateTime?)null,
                    CreatedAt = DateTime.UtcNow
                };
                _context.OshCommitteeConfigs.Add(team.OshCommitteeConfig);
            }
            else if (team.OshCommitteeConfig != null)
            {
                // Update existing config
                team.OshCommitteeConfig.IsCommitteeTrained = form["OshConfig.IsCommitteeTrained"].Contains("true");
                team.OshCommitteeConfig.TrainingDate = DateTime.TryParse(form["OshConfig.TrainingDate"], out var td) ? td : (DateTime?)null;
                team.OshCommitteeConfig.HasMeetingSchedule = form["OshConfig.HasMeetingSchedule"].Contains("true");
                team.OshCommitteeConfig.InspectionFrequency = form["OshConfig.InspectionFrequency"].ToString();
                team.OshCommitteeConfig.LastInspectionDate = DateTime.TryParse(form["OshConfig.LastInspectionDate"], out var lid) ? lid : (DateTime?)null;
                team.OshCommitteeConfig.NextInspectionDate = DateTime.TryParse(form["OshConfig.NextInspectionDate"], out var nid) ? nid : (DateTime?)null;
                team.OshCommitteeConfig.UpdatedAt = DateTime.UtcNow;
                _context.OshCommitteeConfigs.Update(team.OshCommitteeConfig);
            }
        }

        private async Task UpdateRiskAssessmentConfig(Team team, IFormCollection form)
        {
            var configId = int.TryParse(form["RiskConfig.ConfigId"], out var id) ? id : 0;

            if (configId == 0)
            {
                // Create new config
                team.RiskAssessmentConfig = new RiskAssessmentConfig
                {
                    TeamId = team.TeamId,
                    AssessmentFrequency = form["RiskConfig.AssessmentFrequency"].ToString(),
                    LastAssessmentDate = DateTime.TryParse(form["RiskConfig.LastAssessmentDate"], out var lad) ? lad : (DateTime?)null,
                    NextAssessmentDate = DateTime.TryParse(form["RiskConfig.NextAssessmentDate"], out var nad) ? nad : (DateTime?)null,
                    TeamQualifications = form["RiskConfig.TeamQualifications"].ToString(),
                    CreatedAt = DateTime.UtcNow
                };
                _context.RiskAssessmentConfigs.Add(team.RiskAssessmentConfig);
            }
            else if (team.RiskAssessmentConfig != null)
            {
                // Update existing config
                team.RiskAssessmentConfig.AssessmentFrequency = form["RiskConfig.AssessmentFrequency"].ToString();
                team.RiskAssessmentConfig.LastAssessmentDate = DateTime.TryParse(form["RiskConfig.LastAssessmentDate"], out var lad) ? lad : (DateTime?)null;
                team.RiskAssessmentConfig.NextAssessmentDate = DateTime.TryParse(form["RiskConfig.NextAssessmentDate"], out var nad) ? nad : (DateTime?)null;
                team.RiskAssessmentConfig.TeamQualifications = form["RiskConfig.TeamQualifications"].ToString();
                team.RiskAssessmentConfig.UpdatedAt = DateTime.UtcNow;
                _context.RiskAssessmentConfigs.Update(team.RiskAssessmentConfig);
            }
        }

        private async Task UpdateInvestigationConfig(Team team, IFormCollection form)
        {
            var configId = int.TryParse(form["InvestigationConfig.ConfigId"], out var id) ? id : 0;

            if (configId == 0)
            {
                // Create new config
                team.IncidentInvestigationConfig = new IncidentInvestigationConfig
                {
                    TeamId = team.TeamId,
                    ResponseTimeHours = int.TryParse(form["InvestigationConfig.ResponseTimeHours"], out var rth) ? rth : 24,
                    EscalationThreshold = form["InvestigationConfig.EscalationThreshold"].ToString(),
                    InvestigationScope = form["InvestigationConfig.InvestigationScope"].ToString(),
                    TeamExpertise = form["InvestigationConfig.TeamExpertise"].ToString(),
                    CreatedAt = DateTime.UtcNow
                };
                _context.IncidentInvestigationConfigs.Add(team.IncidentInvestigationConfig);
            }
            else if (team.IncidentInvestigationConfig != null)
            {
                // Update existing config
                team.IncidentInvestigationConfig.ResponseTimeHours = int.TryParse(form["InvestigationConfig.ResponseTimeHours"], out var rth) ? rth : 24;
                team.IncidentInvestigationConfig.EscalationThreshold = form["InvestigationConfig.EscalationThreshold"].ToString();
                team.IncidentInvestigationConfig.InvestigationScope = form["InvestigationConfig.InvestigationScope"].ToString();
                team.IncidentInvestigationConfig.TeamExpertise = form["InvestigationConfig.TeamExpertise"].ToString();
                team.IncidentInvestigationConfig.UpdatedAt = DateTime.UtcNow;
                _context.IncidentInvestigationConfigs.Update(team.IncidentInvestigationConfig);
            }
        }

        private async Task LoadCreateViewData()
        {
            // Fetch stations (scope-filtered)
            var stationsQuery = _context.Stations.AsQueryable();
            stationsQuery = _scopeFilterService.ApplyScope(stationsQuery, CurrentScope);

            var stations = await stationsQuery
                .OrderBy(s => s.StationName)
                .Select(s => new { s.StationId, s.StationName })
                .ToListAsync();

            ViewBag.Stations = stations;

            // Fetch sections (scope-filtered)
            var sectionsQuery = _context.Sections.AsQueryable();
            sectionsQuery = _scopeFilterService.ApplyScope(sectionsQuery, CurrentScope);

            var sections = await sectionsQuery
                .OrderBy(s => s.SectionName)
                .Select(s => new { s.SectionId, s.SectionName, s.StationId })
                .ToListAsync();

            ViewBag.Sections = sections;

            // Fetch employees (scope-filtered)
            var employees = await _employeeService.GetEmployeesForRoleAssignmentAsync(CurrentScope);

            var employeesByDepartment = employees
                .GroupBy(e => new { e.DepartmentId, e.DepartmentName, e.StationId, e.StationName })
                .OrderBy(g => g.Key.StationName)
                    .ThenBy(g => g.Key.DepartmentName)
                .ToList();

            ViewBag.EmployeesByDepartment = employeesByDepartment;
            ViewBag.AllEmployees = employees;

            ViewBag.TeamTypes = TeamEnumExtensions.GetTeamTypeDictionary();
            ViewBag.MemberRoles = TeamEnumExtensions.GetMemberRoleValues();
            ViewBag.FrequencyOptions = TeamEnumExtensions.GetFrequencyValues();
            ViewBag.EducationLevels = TeamEnumExtensions.GetEducationLevelValues();
        }

        private async Task LoadEditViewData(int stationId)
        {
            // Fetch stations (scope-filtered)
            var stationsQuery = _context.Stations.AsQueryable();
            stationsQuery = _scopeFilterService.ApplyScope(stationsQuery, CurrentScope);

            var stations = await stationsQuery
                .OrderBy(s => s.StationName)
                .Select(s => new { s.StationId, s.StationName })
                .ToListAsync();

            ViewBag.Stations = stations;

            // Fetch sections for the team's station (scope-filtered)
            var sectionsQuery = _context.Sections.Where(s => s.StationId == stationId).AsQueryable();
            sectionsQuery = _scopeFilterService.ApplyScope(sectionsQuery, CurrentScope);

            var sections = await sectionsQuery
                .OrderBy(s => s.SectionName)
                .Select(s => new { s.SectionId, s.SectionName, s.StationId })
                .ToListAsync();

            ViewBag.Sections = sections;

            // Fetch employees for the station (scope-filtered)
            var employees = await _employeeService.GetEmployeesForRoleAssignmentAsync(CurrentScope);

            var stationEmployees = employees
                .Where(e => e.StationId == stationId)
                .ToList();

            var employeesByDepartment = stationEmployees
                .GroupBy(e => new { e.DepartmentId, e.DepartmentName, e.StationId, e.StationName })
                .OrderBy(g => g.Key.DepartmentName)
                .ToList();

            ViewBag.EmployeesByDepartment = employeesByDepartment;
            ViewBag.AllEmployees = stationEmployees;

            ViewBag.TeamTypes = TeamEnumExtensions.GetTeamTypeDictionary();

            // Load team role definitions
            ViewBag.TeamRoleDefinitions = await _context.TeamRoleDefinitions
                .Where(trd => trd.IsActive)
                .OrderBy(trd => trd.TeamType)
                .ThenBy(trd => trd.DisplayOrder)
                .Select(trd => new
                {
                    trd.TeamRoleDefinitionId,
                    trd.TeamType,
                    trd.RoleName,
                    trd.Description,
                    trd.RequiresVotingRights
                })
                .ToListAsync();

            ViewBag.FrequencyOptions = TeamEnumExtensions.GetFrequencyValues();
            ViewBag.EducationLevels = TeamEnumExtensions.GetEducationLevelValues();
        }

        // ========== MEMBER MANAGEMENT ENDPOINTS ==========

        // GET: Team/GetAvailableRoles
        [HttpGet]
        public async Task<IActionResult> GetAvailableRoles(int teamId, string teamType)
        {
            try
            {
                // Get current member counts
                var memberCounts = await _context.TeamMembers
                    .Where(tm => tm.TeamId == teamId && tm.IsActive)
                    .GroupBy(tm => tm.TeamRoleDefinitionId)
                    .Select(g => new { RoleId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.RoleId, x => x.Count);

                // Get role definitions for this team type
                var roles = await _context.TeamRoleDefinitions
                    .Where(trd => trd.TeamType == teamType && trd.IsActive)
                    .OrderBy(trd => trd.DisplayOrder)
                    .Select(trd => new
                    {
                        trd.TeamRoleDefinitionId,
                        trd.RoleName,
                        trd.Description,
                        trd.MinOccurrences,
                        trd.MaxOccurrences,
                        trd.RequiresVotingRights,
                        trd.RequiredQualifications,
                        CurrentCount = memberCounts.ContainsKey(trd.TeamRoleDefinitionId) ? memberCounts[trd.TeamRoleDefinitionId] : 0
                    })
                    .ToListAsync();

                return Json(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading available roles");
                return StatusCode(500, new { message = "Error loading roles" });
            }
        }

        // GET: Team/SearchEmployees
        [HttpGet]
        public async Task<IActionResult> SearchEmployees(int stationId, string? searchTerm, int teamId)
        {
            try
            {
                // Get employees already in this team
                var existingMembers = await _context.TeamMembers
                    .Where(tm => tm.TeamId == teamId && tm.IsActive)
                    .Select(tm => tm.EmployeePayroll)
                    .ToListAsync();

                // Build query for employees at this station
                var query = _context.Employees
                    .Where(e => e.StationId == stationId);

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(e =>
                        e.FirstName.Contains(searchTerm) ||
                        e.LastName.Contains(searchTerm) ||
                        e.PayrollNo.Contains(searchTerm));
                }

                var employees = await query
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .Select(e => new
                    {
                        e.PayrollNo,
                        FullName = e.FirstName + " " + e.LastName,
                        e.Designation,
                        DepartmentName = e.Department != null ? e.Department.DepartmentName : null
                    })
                    .ToListAsync();

                return Json(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching employees");
                return StatusCode(500, new { message = "Error searching employees" });
            }
        }

        // GET: Team/GetTeamMembers
        [HttpGet]
        public async Task<IActionResult> GetTeamMembers(int teamId)
        {
            try
            {
                var members = await _context.TeamMembers
                    .Where(tm => tm.TeamId == teamId && tm.IsActive)
                    .Select(tm => new
                    {
                        tm.MemberId,
                        tm.EmployeePayroll
                    })
                    .ToListAsync();

                return Json(members);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading team members");
                return StatusCode(500, new { message = "Error loading team members" });
            }
        }

        // GET: Team/GetSectionsByStation
        [HttpGet]
        public async Task<IActionResult> GetSectionsByStation(int stationId)
        {
            try
            {
                var sections = await _context.Sections
                    .Where(s => s.StationId == stationId)
                    .OrderBy(s => s.SectionName)
                    .Select(s => new { s.SectionId, s.SectionName })
                    .ToListAsync();

                return Json(sections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sections");
                return StatusCode(500, new { message = "Error loading sections" });
            }
        }

        // POST: Team/AddMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember([FromBody] AddTeamMemberDto dto)
        {
            try
            {
                // Validate team exists
                var team = await _context.Teams
                    .Include(t => t.TeamMembers.Where(tm => tm.IsActive))
                    .FirstOrDefaultAsync(t => t.TeamId == dto.TeamId);

                if (team == null)
                {
                    return Json(new { success = false, message = "Team not found" });
                }

                // Validate employee exists
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.PayrollNo == dto.EmployeePayroll);

                if (employee == null)
                {
                    return Json(new { success = false, message = "Employee not found" });
                }

                // Check if employee is already in team
                var exists = await _context.TeamMembers
                    .AnyAsync(tm => tm.TeamId == dto.TeamId &&
                                   tm.EmployeePayroll == dto.EmployeePayroll &&
                                   tm.IsActive);

                if (exists)
                {
                    return Json(new { success = false, message = "Employee is already a member of this team" });
                }

                // Validate role capacity
                var roleDefinition = await _context.TeamRoleDefinitions
                    .FirstOrDefaultAsync(trd => trd.TeamRoleDefinitionId == dto.TeamRoleDefinitionId);

                if (roleDefinition == null)
                {
                    return Json(new { success = false, message = "Invalid role selected" });
                }

                if (roleDefinition.MaxOccurrences.HasValue)
                {
                    var currentCount = team.TeamMembers.Count(tm => tm.TeamRoleDefinitionId == dto.TeamRoleDefinitionId);
                    if (currentCount >= roleDefinition.MaxOccurrences.Value)
                    {
                        return Json(new { success = false, message = $"Role '{roleDefinition.RoleName}' is at maximum capacity ({roleDefinition.MaxOccurrences})" });
                    }
                }

                // Validate team capacity
                if (team.MaxMemberCount.HasValue)
                {
                    var currentTeamSize = team.TeamMembers.Count;
                    if (currentTeamSize >= team.MaxMemberCount.Value)
                    {
                        return Json(new { success = false, message = $"Team is at maximum capacity ({team.MaxMemberCount} members)" });
                    }
                }

                // Create new team member
                var newMember = new TeamMember
                {
                    TeamId = dto.TeamId,
                    EmployeePayroll = dto.EmployeePayroll,
                    TeamRoleDefinitionId = dto.TeamRoleDefinitionId,
                    SectionId = dto.SectionId,
                    AppointmentDate = dto.AppointmentDate,
                    IsVotingMember = dto.IsVotingMember,
                    EducationLevel = dto.EducationLevel,
                    RelevantExperience = dto.RelevantExperience,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TeamMembers.Add(newMember);
                await _context.SaveChangesAsync();

                // ✅ Send notification to new member and existing team members
                await _teamNotifications.NotifyMemberAddedAsync(
                    team,
                    employee,
                    roleDefinition.RoleName,
                    User.Identity?.Name ?? "System"
                );

                return Json(new { success = true, message = $"{employee.FirstName} {employee.LastName} added to team as {roleDefinition.RoleName}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding team member");
                return Json(new { success = false, message = "An error occurred while adding the member" });
            }
        }

        // GET: Team/GetMemberDetails
        [HttpGet]
        public async Task<IActionResult> GetMemberDetails(int memberId)
        {
            try
            {
                var member = await _context.TeamMembers
                    .Include(tm => tm.Employee)
                    .Where(tm => tm.MemberId == memberId)
                    .Select(tm => new
                    {
                        tm.MemberId,
                        tm.TeamRoleDefinitionId,
                        tm.EmployeePayroll,
                        EmployeeName = tm.Employee.FirstName + " " + tm.Employee.LastName,
                        tm.AppointmentDate,
                        tm.SectionId,
                        tm.IsVotingMember,
                        tm.EducationLevel,
                        tm.RelevantExperience
                    })
                    .FirstOrDefaultAsync();

                if (member == null)
                {
                    return NotFound(new { message = "Member not found" });
                }

                return Json(member);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading member details");
                return StatusCode(500, new { message = "Error loading member details" });
            }
        }

        // POST: Team/UpdateMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMember([FromBody] AddTeamMemberDto dto)
        {
            try
            {
                var member = await _context.TeamMembers
                    .Include(tm => tm.Employee)
                    .Include(tm => tm.Team)
                    .FirstOrDefaultAsync(tm => tm.MemberId == dto.MemberId);

                if (member == null)
                {
                    return Json(new { success = false, message = "Member not found" });
                }

                // If role changed, validate new role capacity
                if (member.TeamRoleDefinitionId != dto.TeamRoleDefinitionId)
                {
                    var roleDefinition = await _context.TeamRoleDefinitions
                        .FirstOrDefaultAsync(trd => trd.TeamRoleDefinitionId == dto.TeamRoleDefinitionId);

                    if (roleDefinition != null && roleDefinition.MaxOccurrences.HasValue)
                    {
                        var currentCount = await _context.TeamMembers
                            .CountAsync(tm => tm.TeamId == member.TeamId &&
                                            tm.TeamRoleDefinitionId == dto.TeamRoleDefinitionId &&
                                            tm.IsActive &&
                                            tm.MemberId != dto.MemberId);

                        if (currentCount >= roleDefinition.MaxOccurrences.Value)
                        {
                            return Json(new { success = false, message = $"Role '{roleDefinition.RoleName}' is at maximum capacity" });
                        }
                    }
                }

                // Update member details
                member.TeamRoleDefinitionId = dto.TeamRoleDefinitionId;
                member.SectionId = dto.SectionId;
                member.AppointmentDate = dto.AppointmentDate;
                member.IsVotingMember = dto.IsVotingMember;
                member.EducationLevel = dto.EducationLevel;
                member.RelevantExperience = dto.RelevantExperience;
                member.UpdatedAt = DateTime.UtcNow;

                _context.TeamMembers.Update(member);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Member updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating team member");
                return Json(new { success = false, message = "An error occurred while updating the member" });
            }
        }

        // POST: Team/RemoveMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int memberId)
        {
            try
            {
                var member = await _context.TeamMembers
                    .Include(tm => tm.Employee)
                    .Include(tm => tm.TeamRoleDefinition)
                    .Include(tm => tm.Team)
                    .FirstOrDefaultAsync(tm => tm.MemberId == memberId);

                if (member == null)
                {
                    return Json(new { success = false, message = "Member not found" });
                }

                // Check if removing this member would violate minimum requirements
                if (member.TeamRoleDefinition != null && member.TeamRoleDefinition.MinOccurrences.HasValue)
                {
                    var currentCount = await _context.TeamMembers
                        .CountAsync(tm => tm.TeamId == member.TeamId &&
                                        tm.TeamRoleDefinitionId == member.TeamRoleDefinitionId &&
                                        tm.IsActive);

                    if (currentCount <= member.TeamRoleDefinition.MinOccurrences.Value)
                    {
                        return Json(new {
                            success = false,
                            message = $"Cannot remove member. Role '{member.TeamRoleDefinition.RoleName}' requires minimum {member.TeamRoleDefinition.MinOccurrences} member(s)"
                        });
                    }
                }

                // Soft delete (set IsActive = false)
                member.IsActive = false;
                member.DepartureDate = DateTime.UtcNow;
                member.UpdatedAt = DateTime.UtcNow;

                _context.TeamMembers.Update(member);
                await _context.SaveChangesAsync();

                // ✅ Send notification to removed member and remaining team members
                await _teamNotifications.NotifyMemberRemovedAsync(
                    member.Team,
                    member.Employee,
                    "Removed by admin",
                    User.Identity?.Name ?? "System"
                );

                return Json(new { success = true, message = $"{member.Employee.FirstName} {member.Employee.LastName} removed from team" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing team member");
                return Json(new { success = false, message = "An error occurred while removing the member" });
            }
        }
    }
}
