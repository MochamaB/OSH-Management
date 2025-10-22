namespace OSHManagement.Services.Notifications.DTOs
{
    /// <summary>
    /// Filter parameters for querying notifications
    /// </summary>
    public class NotificationFilters
    {
        /// <summary>
        /// Filter by category (Employee, Team, Incident, etc.)
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Filter by priority (Low, Normal, High, Urgent)
        /// </summary>
        public string? Priority { get; set; }

        /// <summary>
        /// Filter by notification type (Info, Success, Warning, Error, ActionRequired)
        /// </summary>
        public string? NotificationType { get; set; }

        /// <summary>
        /// Filter by channel (InApp, Email, SMS, WhatsApp)
        /// </summary>
        public string? Channel { get; set; }

        /// <summary>
        /// Filter by read status (null = all, true = read only, false = unread only)
        /// </summary>
        public bool? IsRead { get; set; }

        /// <summary>
        /// Filter from date (inclusive)
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Filter to date (inclusive)
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Search in title and message
        /// </summary>
        public string? SearchTerm { get; set; }
    }
}
