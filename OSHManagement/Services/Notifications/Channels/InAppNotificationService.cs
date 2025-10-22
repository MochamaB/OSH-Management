using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Models.Enums;

namespace OSHManagement.Services.Notifications.Channels
{
    /// <summary>
    /// In-app notification service (stores notifications in database)
    /// </summary>
    public class InAppNotificationService : INotificationChannelService
    {
        private readonly OshDbContext _context;
        private readonly ILogger<InAppNotificationService> _logger;

        public NotificationChannel Channel => NotificationChannel.InApp;

        public InAppNotificationService(
            OshDbContext context,
            ILogger<InAppNotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Store notification in database (already done by NotificationEventPublisher)
        /// This method is a no-op for InApp since the notification is already in DB
        /// </summary>
        public async Task<bool> SendAsync(Notification notification, string? recipientAddress = null)
        {
            try
            {
                // For InApp notifications, the notification is already in the database
                // This method exists for interface consistency but doesn't need to do anything
                
                _logger.LogInformation("InApp notification {NotificationId} available for recipient {RecipientType}:{RecipientId}",
                    notification.NotificationId, notification.RecipientType, notification.RecipientId);

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing InApp notification {NotificationId}", 
                    notification.NotificationId);
                return false;
            }
        }

        /// <summary>
        /// InApp notifications are always enabled
        /// </summary>
        public Task<bool> IsEnabledAsync()
        {
            return Task.FromResult(true);
        }
    }
}
