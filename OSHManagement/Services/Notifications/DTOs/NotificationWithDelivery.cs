using OSHManagement.Models;

namespace OSHManagement.Services.Notifications.DTOs
{
    /// <summary>
    /// Notification with delivery status information
    /// Used for channel-specific views (Email, SMS, WhatsApp)
    /// </summary>
    public class NotificationWithDelivery
    {
        public Notification Notification { get; set; } = null!;
        public NotificationDelivery? Delivery { get; set; }
    }
}
