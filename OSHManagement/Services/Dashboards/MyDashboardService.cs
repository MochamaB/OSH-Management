using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models.ViewModels;
using OSHManagement.Models.ViewModels.Dashboard;

namespace OSHManagement.Services.Dashboards
{
    /// <summary>
    /// Service for My Dashboard - Personal employee OSH information
    /// Handles all data queries for the dashboard
    /// </summary>
    public class MyDashboardService
    {
        private readonly OshDbContext _context;

        public MyDashboardService(OshDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Main method - orchestrates all queries and returns populated ViewModel
        /// Uses ONLY actual database tables: Employee, ControlAction, Incident, Hazard, TeamMember
        /// </summary>
        public async Task<MyDashboardViewModel> GetDashboardDataAsync(string payrollNo)
        {
            var viewModel = new MyDashboardViewModel
            {
                PayrollNo = payrollNo,
                Employee = await GetEmployeeAsync(payrollNo),
                MyActions = await GetMyActionsAsync(payrollNo),
                OverdueActionsCount = await GetOverdueActionsCountAsync(payrollNo),
                MyTeams = await GetMyTeamsAsync(payrollNo),
                MyIncidents = await GetMyIncidentsAsync(payrollNo),
                MyHazards = await GetMyHazardsAsync(payrollNo)
            };

            return viewModel;
        }

        #region Employee Profile

        private async Task<EmployeeViewModel> GetEmployeeAsync(string payrollNo)
        {
            var employee = await _context.Employees
                .Where(e => e.PayrollNo == payrollNo)
                .Select(e => new EmployeeViewModel
                {
                    EmployeeId = e.EmployeeId,
                    PayrollNo = e.PayrollNo,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    EmailAddress = e.EmailAddress,
                    PhoneNo = e.PhoneNo,
                    StationId = e.StationId,
                    StationName = e.Station != null ? e.Station.StationName : "N/A",
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department != null ? e.Department.DepartmentName : null,
                    EmployeeType = e.EmployeeType,
                    Designation = e.Designation,
                    EmploymentStatus = e.EmploymentStatus,
                    HireDate = e.HireDate,
                    ServiceYears = e.ServiceYears,
                    HodPayroll = e.HodPayroll,
                    SupervisorPayroll = e.SupervisorPayroll,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return employee ?? new EmployeeViewModel();
        }

        #endregion

        // Training queries removed - no Training/Certification tables in database

        #region Action Queries

        private async Task<List<ActionItemDto>> GetMyActionsAsync(string payrollNo)
        {
            return await _context.ControlActions
                .Where(a => a.AssignedToPayroll == payrollNo && a.ActionStatus != "Completed")
                .OrderBy(a => a.TargetCompletionDate)
                .Take(10)
                .Select(a => new ActionItemDto
                {
                    ActionId = a.ActionId,
                    Description = a.ActionDescription,
                    TargetDate = a.TargetCompletionDate,
                    Status = a.ActionStatus,
                    IncidentId = a.IncidentId,
                    Category = a.ActionCategory,
                    ActionType = a.ActionType
                })
                .ToListAsync();
        }

        private async Task<int> GetOverdueActionsCountAsync(string payrollNo)
        {
            var today = DateTime.Now;
            return await _context.ControlActions
                .CountAsync(a => a.AssignedToPayroll == payrollNo &&
                               a.TargetCompletionDate.HasValue &&
                               a.TargetCompletionDate.Value < today &&
                               a.ActionStatus != "Completed");
        }

        #endregion

        #region Team Queries

        private async Task<List<TeamMembershipDto>> GetMyTeamsAsync(string payrollNo)
        {
            return await _context.TeamMembers
                .Where(tm => tm.EmployeePayroll == payrollNo && tm.IsActive)
                .Select(tm => new TeamMembershipDto
                {
                    TeamId = tm.TeamId,
                    TeamName = tm.Team.TeamName,
                    RoleName = tm.TeamRoleDefinition != null ? tm.TeamRoleDefinition.RoleName : "Member",
                    AppointmentDate = tm.AppointmentDate,
                    TeamType = tm.Team.TeamType ?? "Committee"
                })
                .ToListAsync();
        }

        // No Meetings table in database - removed query

        #endregion

        #region Incident/Hazard Queries

        private async Task<List<IncidentDto>> GetMyIncidentsAsync(string payrollNo)
        {
            return await _context.Incidents
                .Where(i => i.ReportedByPayroll == payrollNo)
                .OrderByDescending(i => i.IncidentDate)
                .Take(5)
                .Select(i => new IncidentDto
                {
                    IncidentId = i.IncidentId,
                    Description = i.IncidentDescription.Length > 100 
                        ? i.IncidentDescription.Substring(0, 100) + "..." 
                        : i.IncidentDescription,
                    IncidentDate = i.IncidentDate,
                    Severity = i.IncidentSeverity,
                    Status = i.IncidentStatus,
                    Location = i.LocationDescription
                })
                .ToListAsync();
        }

        private async Task<List<HazardDto>> GetMyHazardsAsync(string payrollNo)
        {
            // Get hazards from teams where user is a member
            var userTeamIds = await _context.TeamMembers
                .Where(tm => tm.EmployeePayroll == payrollNo && tm.IsActive)
                .Select(tm => tm.TeamId)
                .ToListAsync();

            return await _context.Hazards
                .Where(h => userTeamIds.Contains(h.TeamId))
                .OrderByDescending(h => h.IdentifiedDate)
                .Take(5)
                .Select(h => new HazardDto
                {
                    HazardId = h.HazardId,
                    Description = h.HazardDescription.Length > 100 
                        ? h.HazardDescription.Substring(0, 100) + "..." 
                        : h.HazardDescription,
                    IdentifiedDate = h.IdentifiedDate,
                    RiskLevel = h.PriorityLevel,
                    Category = h.HazardCategory
                })
                .ToListAsync();
        }

        #endregion
    }
}
