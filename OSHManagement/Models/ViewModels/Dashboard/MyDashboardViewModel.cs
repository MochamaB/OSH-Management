namespace OSHManagement.Models.ViewModels.Dashboard
{
    /// <summary>
    /// ViewModel for My Dashboard
    /// Contains RAW DATA ONLY - No pre-built components
    /// Components are built in the view using extension methods
    /// Uses ACTUAL database tables: Employee, ControlAction, Incident, Hazard, TeamMember
    /// </summary>
    public class MyDashboardViewModel
    {
        // User Info (using EmployeeViewModel)
        public string PayrollNo { get; set; } = string.Empty;
        public EmployeeViewModel Employee { get; set; } = new EmployeeViewModel();

        // Action Metrics (RAW DATA - from ControlAction table)
        public List<ActionItemDto> MyActions { get; set; } = new List<ActionItemDto>();
        public int OverdueActionsCount { get; set; }

        // Team Metrics (RAW DATA - from TeamMember table)
        public List<TeamMembershipDto> MyTeams { get; set; } = new List<TeamMembershipDto>();

        // Incident/Hazard Metrics (RAW DATA - from Incident/Hazard tables)
        public List<IncidentDto> MyIncidents { get; set; } = new List<IncidentDto>();
        public List<HazardDto> MyHazards { get; set; } = new List<HazardDto>();
    }

    #region DTOs (Data Transfer Objects)

    // Using EmployeeViewModel instead of custom UserProfileDto

    // Training/Certification DTOs removed - no Training tables in database

    /// <summary>
    /// Action item details (from ControlAction table)
    /// </summary>
    public class ActionItemDto
    {
        public int ActionId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int IncidentId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Team membership details (from TeamMember table)
    /// </summary>
    public class TeamMembershipDto
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string TeamType { get; set; } = string.Empty;
    }

    // MeetingDto removed - no Meetings table in database

    /// <summary>
    /// Incident details (from Incident table)
    /// </summary>
    public class IncidentDto
    {
        public int IncidentId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime IncidentDate { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    /// <summary>
    /// Hazard details (from Hazard table)
    /// </summary>
    public class HazardDto
    {
        public int HazardId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime IdentifiedDate { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    #endregion
}
