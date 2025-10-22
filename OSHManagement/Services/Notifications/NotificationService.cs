using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Services.Notifications.DTOs;

namespace OSHManagement.Services.Notifications
{
    /// <summary>
    /// Core notification service with recipient resolution logic
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly OshDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            OshDbContext context,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get notifications for a user using recipient resolution logic
        /// </summary>
        public async Task<List<Notification>> GetUserNotificationsAsync(int employeeId, NotificationFilters? filters = null)
        {
            try
            {
                // Get employee context (roles, station, department, teams)
                var employee = await _context.Employees
                    .Include(e => e.EmployeeRoles)
                    .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

                if (employee == null)
                {
                    _logger.LogWarning("Employee {EmployeeId} not found", employeeId);
                    return new List<Notification>();
                }

                // Get user's role IDs
                var userRoleIds = employee.EmployeeRoles
                    .Select(er => er.RoleId)
                    .ToList();

                // Get user's team IDs
                var userTeamIds = await _context.TeamMembers
                    .Where(tm => tm.EmployeePayroll == employee.PayrollNo && tm.IsActive)
                    .Select(tm => tm.TeamId)
                    .ToListAsync();

                // Build query with recipient resolution
                var query = _context.Notifications.AsQueryable();

                // RECIPIENT RESOLUTION LOGIC
                query = query.Where(n =>
                    // 1. Direct employee notification
                    (n.RecipientType == "Employee" && n.RecipientId == employeeId) ||

                    // 2. Role-based notification (user has this role)
                    (n.RecipientType == "Role" && userRoleIds.Contains(n.RecipientId)) ||

                    // 3. Station-based notification (user works in this station)
                    (n.RecipientType == "Station" && n.RecipientId == employee.StationId) ||

                    // 4. Department-based notification (user works in this department)
                    (n.RecipientType == "Department" && employee.DepartmentId.HasValue &&
                     n.RecipientId == employee.DepartmentId.Value) ||

                    // 5. Team-based notification (user is member of this team)
                    (n.RecipientType == "Team" && userTeamIds.Contains(n.RecipientId))
                );

                // Apply filters if provided
                if (filters != null)
                {
                    if (!string.IsNullOrEmpty(filters.Category))
                        query = query.Where(n => n.Category == filters.Category);

                    if (!string.IsNullOrEmpty(filters.Priority))
                        query = query.Where(n => n.Priority == filters.Priority);

                    if (!string.IsNullOrEmpty(filters.NotificationType))
                        query = query.Where(n => n.NotificationType == filters.NotificationType);

                    if (filters.IsRead.HasValue)
                        query = query.Where(n => n.IsRead == filters.IsRead.Value);

                    if (filters.FromDate.HasValue)
                        query = query.Where(n => n.CreatedAt >= filters.FromDate.Value);

                    if (filters.ToDate.HasValue)
                        query = query.Where(n => n.CreatedAt <= filters.ToDate.Value);

                    if (!string.IsNullOrEmpty(filters.SearchTerm))
                    {
                        var searchTerm = filters.SearchTerm.ToLower();
                        query = query.Where(n =>
                            n.Title.ToLower().Contains(searchTerm) ||
                            n.Message.ToLower().Contains(searchTerm));
                    }
                }

                return await query
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications for employee {EmployeeId}", employeeId);
                return new List<Notification>();
            }
        }

        /// <summary>
        /// Get notifications by channel with delivery information
        /// </summary>
        public async Task<List<NotificationWithDelivery>> GetUserNotificationsByChannelAsync(int employeeId, string channel)
        {
            try
            {
                // Get base notifications using recipient resolution
                var notifications = await GetUserNotificationsAsync(employeeId);

                // Get deliveries for this channel
                var notificationIds = notifications.Select(n => n.NotificationId).ToList();

                var deliveries = await _context.NotificationDeliveries
                    .Where(nd => notificationIds.Contains(nd.NotificationId) && nd.Channel == channel)
                    .ToListAsync();

                // Combine notifications with delivery status
                var result = notifications.Select(n => new NotificationWithDelivery
                {
                    Notification = n,
                    Delivery = deliveries.FirstOrDefault(d => d.NotificationId == n.NotificationId)
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving {Channel} notifications for employee {EmployeeId}",
                    channel, employeeId);
                return new List<NotificationWithDelivery>();
            }
        }

        /// <summary>
        /// Get unread notification count
        /// </summary>
        public async Task<int> GetUnreadCountAsync(int employeeId)
        {
            try
            {
                var filters = new NotificationFilters { IsRead = false };
                var notifications = await GetUserNotificationsAsync(employeeId, filters);
                return notifications.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for employee {EmployeeId}", employeeId);
                return 0;
            }
        }

        /// <summary>
        /// Get recent notifications for bell dropdown
        /// </summary>
        public async Task<List<Notification>> GetRecentNotificationsAsync(int employeeId, int count = 10)
        {
            try
            {
                var filters = new NotificationFilters { IsRead = false };
                var notifications = await GetUserNotificationsAsync(employeeId, filters);
                return notifications.Take(count).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent notifications for employee {EmployeeId}", employeeId);
                return new List<Notification>();
            }
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        public async Task<bool> MarkAsReadAsync(int notificationId, int employeeId)
        {
            try
            {
                // Verify user is recipient before marking as read
                if (!await IsRecipientAsync(notificationId, employeeId))
                {
                    _logger.LogWarning("Employee {EmployeeId} attempted to mark notification {NotificationId} as read but is not a recipient",
                        employeeId, notificationId);
                    return false;
                }

                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

                if (notification == null)
                    return false;

                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Notification {NotificationId} marked as read by employee {EmployeeId}",
                    notificationId, employeeId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                return false;
            }
        }

        /// <summary>
        /// Mark all user notifications as read
        /// </summary>
        public async Task<int> MarkAllAsReadAsync(int employeeId)
        {
            try
            {
                var unreadNotifications = await GetUserNotificationsAsync(
                    employeeId,
                    new NotificationFilters { IsRead = false });

                var count = 0;
                var now = DateTime.UtcNow;

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = now;
                    count++;
                }

                if (count > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Marked {Count} notifications as read for employee {EmployeeId}",
                        count, employeeId);
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for employee {EmployeeId}", employeeId);
                return 0;
            }
        }

        /// <summary>
        /// Delete notification (only if user is recipient)
        /// </summary>
        public async Task<bool> DeleteNotificationAsync(int notificationId, int employeeId)
        {
            try
            {
                // Verify user is recipient before deleting
                if (!await IsRecipientAsync(notificationId, employeeId))
                {
                    _logger.LogWarning("Employee {EmployeeId} attempted to delete notification {NotificationId} but is not a recipient",
                        employeeId, notificationId);
                    return false;
                }

                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

                if (notification == null)
                    return false;

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Notification {NotificationId} deleted by employee {EmployeeId}",
                    notificationId, employeeId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId}", notificationId);
                return false;
            }
        }

        /// <summary>
        /// Bulk delete notifications
        /// </summary>
        public async Task<int> BulkDeleteAsync(List<int> notificationIds, int employeeId)
        {
            try
            {
                var count = 0;

                foreach (var notificationId in notificationIds)
                {
                    if (await DeleteNotificationAsync(notificationId, employeeId))
                        count++;
                }

                _logger.LogInformation("Bulk deleted {Count} notifications for employee {EmployeeId}",
                    count, employeeId);

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk deleting notifications for employee {EmployeeId}", employeeId);
                return 0;
            }
        }

        /// <summary>
        /// Get notification statistics for dashboard
        /// </summary>
        public async Task<NotificationStatistics> GetStatisticsAsync(int employeeId)
        {
            try
            {
                var notifications = await GetUserNotificationsAsync(employeeId);

                var stats = new NotificationStatistics
                {
                    TotalNotifications = notifications.Count,
                    UnreadNotifications = notifications.Count(n => !n.IsRead),
                    ReadNotifications = notifications.Count(n => n.IsRead),

                    // By Priority
                    UrgentCount = notifications.Count(n => n.Priority == "Urgent"),
                    HighCount = notifications.Count(n => n.Priority == "High"),
                    NormalCount = notifications.Count(n => n.Priority == "Normal"),
                    LowCount = notifications.Count(n => n.Priority == "Low"),

                    // By Type
                    InfoCount = notifications.Count(n => n.NotificationType == "Info"),
                    SuccessCount = notifications.Count(n => n.NotificationType == "Success"),
                    WarningCount = notifications.Count(n => n.NotificationType == "Warning"),
                    ErrorCount = notifications.Count(n => n.NotificationType == "Error"),
                    ActionRequiredCount = notifications.Count(n => n.NotificationType == "ActionRequired"),

                    // Time-based
                    TodayCount = notifications.Count(n => n.CreatedAt.Date == DateTime.UtcNow.Date),
                    ThisWeekCount = notifications.Count(n => n.CreatedAt >= DateTime.UtcNow.AddDays(-7)),
                    ThisMonthCount = notifications.Count(n => n.CreatedAt >= DateTime.UtcNow.AddDays(-30)),

                    // By Category
                    ByCategory = notifications
                        .Where(n => !string.IsNullOrEmpty(n.Category))
                        .GroupBy(n => n.Category!)
                        .ToDictionary(g => g.Key, g => g.Count()),

                    // Recent categories
                    RecentCategories = notifications
                        .Where(n => !string.IsNullOrEmpty(n.Category))
                        .Select(n => n.Category!)
                        .Distinct()
                        .Take(5)
                        .ToList()
                };

                // Get delivery counts
                var notificationIds = notifications.Select(n => n.NotificationId).ToList();
                var deliveries = await _context.NotificationDeliveries
                    .Where(nd => notificationIds.Contains(nd.NotificationId))
                    .ToListAsync();

                stats.InAppCount = notifications.Count; // All notifications are in-app
                stats.EmailSentCount = deliveries.Count(d => d.Channel == "Email" && d.Status == "Sent");
                stats.SmsSentCount = deliveries.Count(d => d.Channel == "SMS" && d.Status == "Sent");
                stats.WhatsAppSentCount = deliveries.Count(d => d.Channel == "WhatsApp" && d.Status == "Sent");

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for employee {EmployeeId}", employeeId);
                return new NotificationStatistics();
            }
        }

        /// <summary>
        /// Check if employee is a valid recipient for a notification
        /// </summary>
        public async Task<bool> IsRecipientAsync(int notificationId, int employeeId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

                if (notification == null)
                    return false;

                var employee = await _context.Employees
                    .Include(e => e.EmployeeRoles)
                    .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

                if (employee == null)
                    return false;

                // Check based on recipient type
                switch (notification.RecipientType)
                {
                    case "Employee":
                        return notification.RecipientId == employeeId;

                    case "Role":
                        return employee.EmployeeRoles.Any(er => er.RoleId == notification.RecipientId);

                    case "Station":
                        return employee.StationId == notification.RecipientId;

                    case "Department":
                        return employee.DepartmentId == notification.RecipientId;

                    case "Team":
                        var isMember = await _context.TeamMembers
                            .AnyAsync(tm => tm.TeamId == notification.RecipientId &&
                                          tm.EmployeePayroll == employee.PayrollNo &&
                                          tm.IsActive);
                        return isMember;

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if employee {EmployeeId} is recipient of notification {NotificationId}",
                    employeeId, notificationId);
                return false;
            }
        }
    }
}
