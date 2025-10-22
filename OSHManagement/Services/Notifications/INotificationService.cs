using OSHManagement.Models;
using OSHManagement.Services.Notifications.DTOs;

namespace OSHManagement.Services.Notifications
{
    /// <summary>
    /// Service for querying and managing user notifications using recipient resolution
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Get notifications for a specific employee using recipient resolution logic
        /// Resolves based on: Employee, Role, Station, Department, Team membership
        /// </summary>
        Task<List<Notification>> GetUserNotificationsAsync(int employeeId, NotificationFilters? filters = null);

        /// <summary>
        /// Get notifications filtered by channel (InApp, Email, SMS, WhatsApp)
        /// </summary>
        Task<List<NotificationWithDelivery>> GetUserNotificationsByChannelAsync(int employeeId, string channel);

        /// <summary>
        /// Get unread notification count for an employee
        /// </summary>
        Task<int> GetUnreadCountAsync(int employeeId);

        /// <summary>
        /// Get recent notifications for bell dropdown (last 10 unread)
        /// </summary>
        Task<List<Notification>> GetRecentNotificationsAsync(int employeeId, int count = 10);

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        Task<bool> MarkAsReadAsync(int notificationId, int employeeId);

        /// <summary>
        /// Mark all user notifications as read
        /// </summary>
        Task<int> MarkAllAsReadAsync(int employeeId);

        /// <summary>
        /// Delete a notification (only if user is the recipient)
        /// </summary>
        Task<bool> DeleteNotificationAsync(int notificationId, int employeeId);

        /// <summary>
        /// Bulk delete notifications
        /// </summary>
        Task<int> BulkDeleteAsync(List<int> notificationIds, int employeeId);

        /// <summary>
        /// Get notification statistics for dashboard
        /// </summary>
        Task<NotificationStatistics> GetStatisticsAsync(int employeeId);

        /// <summary>
        /// Check if employee is a valid recipient for a notification
        /// </summary>
        Task<bool> IsRecipientAsync(int notificationId, int employeeId);
    }
}
