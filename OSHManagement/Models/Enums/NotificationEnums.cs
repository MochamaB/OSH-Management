namespace OSHManagement.Models.Enums
{
    /// <summary>
    /// Priority levels for notifications
    /// </summary>
    public enum NotificationPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4
    }

    /// <summary>
    /// Available notification channels
    /// </summary>
    public enum NotificationChannel
    {
        InApp = 1,
        Email = 2,
        SMS = 3,
        WhatsApp = 4
    }

    /// <summary>
    /// Types of notifications for UI styling
    /// </summary>
    public enum NotificationType
    {
        Info = 1,
        Success = 2,
        Warning = 3,
        Error = 4,
        ActionRequired = 5
    }

    /// <summary>
    /// Recipient types using type-discriminator pattern
    /// </summary>
    public enum RecipientType
    {
        Employee = 1,
        Role = 2,
        Station = 3,
        Department = 4,
        Team = 5
    }

    /// <summary>
    /// Delivery status for multi-channel notifications
    /// </summary>
    public enum DeliveryStatus
    {
        Pending = 1,
        Sending = 2,
        Sent = 3,
        Delivered = 4,
        Failed = 5,
        Bounced = 6
    }

    /// <summary>
    /// Digest frequency for notification preferences
    /// </summary>
    public enum DigestFrequency
    {
        Instant = 1,
        Hourly = 2,
        Daily = 3,
        Weekly = 4
    }
}
