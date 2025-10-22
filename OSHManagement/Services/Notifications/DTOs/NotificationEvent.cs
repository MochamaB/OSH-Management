using OSHManagement.Models.Enums;

namespace OSHManagement.Services.Notifications.DTOs
{
    /// <summary>
    /// Event-driven notification request
    /// Published by controllers when events occur (EmployeeCreated, TeamMemberAdded, etc.)
    /// </summary>
    public class NotificationEvent
    {
        /// <summary>
        /// Event type identifier (e.g., "EmployeeCreated", "TeamMemberAdded")
        /// Maps to template name in NotificationTemplates table
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Category for grouping (e.g., "Employee", "Team", "Incident")
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Template data for placeholder replacement
        /// Example: { "EmployeeName": "John Doe", "StationName": "Boito" }
        /// </summary>
        public Dictionary<string, string> Data { get; set; } = new();

        /// <summary>
        /// Direct employee recipients (notify specific employees)
        /// </summary>
        public List<int> RecipientEmployeeIds { get; set; } = new();

        /// <summary>
        /// Role-based recipients (notify all users with these roles)
        /// </summary>
        public List<int> RecipientRoleIds { get; set; } = new();

        /// <summary>
        /// Station-based recipients (notify all users in these stations)
        /// </summary>
        public List<int> RecipientStationIds { get; set; } = new();

        /// <summary>
        /// Department-based recipients (notify all users in these departments)
        /// </summary>
        public List<int> RecipientDepartmentIds { get; set; } = new();

        /// <summary>
        /// Team-based recipients (notify all members of these teams)
        /// </summary>
        public List<int> RecipientTeamIds { get; set; } = new();

        /// <summary>
        /// Notification priority
        /// </summary>
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        /// <summary>
        /// Optional action URL to navigate to related entity
        /// </summary>
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Delivery channels (leave empty to use user preferences)
        /// </summary>
        public List<NotificationChannel> Channels { get; set; } = new();
    }
}
