using OSHManagement.Models;
using OSHManagement.Models.Enums;

namespace OSHManagement.Services.Notifications.Channels
{
    /// <summary>
    /// Interface for channel-specific notification delivery services
    /// </summary>
    public interface INotificationChannelService
    {
        /// <summary>
        /// The channel this service handles
        /// </summary>
        NotificationChannel Channel { get; }

        /// <summary>
        /// Send a notification via this channel
        /// </summary>
        Task<bool> SendAsync(Notification notification, string? recipientAddress = null);

        /// <summary>
        /// Check if this channel is enabled globally
        /// </summary>
        Task<bool> IsEnabledAsync();
    }
}
