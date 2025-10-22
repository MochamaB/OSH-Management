namespace OSHManagement.Services.Notifications.DTOs
{
    /// <summary>
    /// Notification statistics for dashboard
    /// </summary>
    public class NotificationStatistics
    {
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public int ReadNotifications { get; set; }
        
        // By Priority
        public int UrgentCount { get; set; }
        public int HighCount { get; set; }
        public int NormalCount { get; set; }
        public int LowCount { get; set; }
        
        // By Category
        public Dictionary<string, int> ByCategory { get; set; } = new();
        
        // By Type
        public int InfoCount { get; set; }
        public int SuccessCount { get; set; }
        public int WarningCount { get; set; }
        public int ErrorCount { get; set; }
        public int ActionRequiredCount { get; set; }
        
        // By Channel (with deliveries)
        public int InAppCount { get; set; }
        public int EmailSentCount { get; set; }
        public int SmsSentCount { get; set; }
        public int WhatsAppSentCount { get; set; }
        
        // Time-based
        public int TodayCount { get; set; }
        public int ThisWeekCount { get; set; }
        public int ThisMonthCount { get; set; }
        
        // Recent notifications for quick view
        public List<string> RecentCategories { get; set; } = new();
    }
}
