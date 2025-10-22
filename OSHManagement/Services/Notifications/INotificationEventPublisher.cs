using OSHManagement.Services.Notifications.DTOs;

namespace OSHManagement.Services.Notifications
{
    /// <summary>
    /// Publishes notification events from controllers
    /// </summary>
    public interface INotificationEventPublisher
    {
        /// <summary>
        /// Publish a notification event (creates notifications and queues deliveries)
        /// </summary>
        Task PublishAsync(NotificationEvent notificationEvent);
    }
}
