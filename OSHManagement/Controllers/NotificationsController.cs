using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Services;
using OSHManagement.Services.Notifications;
using OSHManagement.Services.Notifications.DTOs;

namespace OSHManagement.Controllers
{
    /// <summary>
    /// Notifications Controller
    /// Handles all notification-related views and API endpoints
    /// Uses recipient resolution logic instead of scope filtering
    /// </summary>
    [Authorize]
    public class NotificationsController : ScopedController
    {
        private readonly INotificationService _notificationService;
        private readonly INotificationTemplateService _templateService;
        private readonly INotificationPreferenceService _preferenceService;
        private readonly IChannelConfigService _channelConfigService;

        public NotificationsController(
            OshDbContext context,
            IScopeFilterService scopeFilter,
            ILogger<NotificationsController> logger,
            INotificationService notificationService,
            INotificationTemplateService templateService,
            INotificationPreferenceService preferenceService,
            IChannelConfigService channelConfigService)
            : base(context, scopeFilter, logger)
        {
            _notificationService = notificationService;
            _templateService = templateService;
            _preferenceService = preferenceService;
            _channelConfigService = channelConfigService;
        }

        #region User-Facing Pages (Recipient Resolution)

        /// <summary>
        /// 1. Notification Dashboard - Statistics and analytics
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var stats = await _notificationService.GetStatisticsAsync(CurrentUserId);
                return View(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notification dashboard");
                TempData["ErrorMessage"] = "Error loading notification dashboard.";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// 2. All Notifications - Complete list with pagination
        /// </summary>
        public async Task<IActionResult> Index(
            string? search,
            string? category,
            string? priority,
            string? notificationType,
            string? isRead,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                // Build filters
                var filters = new NotificationFilters
                {
                    Category = category,
                    Priority = priority,
                    NotificationType = notificationType,
                    SearchTerm = search
                };

                // Handle IsRead filter
                if (!string.IsNullOrEmpty(isRead))
                {
                    filters.IsRead = isRead.ToLower() == "true";
                }

                // Get all notifications for user
                var allNotifications = await _notificationService.GetUserNotificationsAsync(CurrentUserId, filters);
                
                // Calculate statistics
                var totalCount = allNotifications.Count;
                var unreadCount = allNotifications.Count(n => !n.IsRead);
                var todayCount = allNotifications.Count(n => n.CreatedAt.Date == DateTime.UtcNow.Date);
                var weekCount = allNotifications.Count(n => n.CreatedAt >= DateTime.UtcNow.AddDays(-7));

                // Apply pagination
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                var notifications = allNotifications
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Pass data to view
                ViewBag.TotalNotifications = totalCount;
                ViewBag.UnreadCount = unreadCount;
                ViewBag.TodayCount = todayCount;
                ViewBag.WeekCount = weekCount;
                
                ViewBag.CurrentSearch = search;
                ViewBag.CurrentCategory = category;
                ViewBag.CurrentPriority = priority;
                ViewBag.CurrentType = notificationType;
                ViewBag.CurrentStatus = isRead;
                
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalCount;
                
                return View(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notifications");
                TempData["ErrorMessage"] = "Error loading notifications.";
                
                // Return empty view with defaults
                ViewBag.TotalNotifications = 0;
                ViewBag.UnreadCount = 0;
                ViewBag.TodayCount = 0;
                ViewBag.WeekCount = 0;
                ViewBag.CurrentPage = 1;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = 0;
                ViewBag.TotalItems = 0;
                
                return View(new List<Models.Notification>());
            }
        }

        /// <summary>
        /// 3. In-App Notifications - Chat-style view
        /// </summary>
        public async Task<IActionResult> InApp()
        {
            try
            {
                var notifications = await _notificationService.GetUserNotificationsAsync(CurrentUserId);
                return View(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading in-app notifications");
                TempData["ErrorMessage"] = "Error loading in-app notifications.";
                return View(new List<Models.Notification>());
            }
        }

        /// <summary>
        /// 4. Email Notifications - Three-column mail interface
        /// </summary>
        public async Task<IActionResult> Email()
        {
            return View();
        }

        /// <summary>
        /// 5. SMS Notifications - Under development placeholder
        /// </summary>
        public IActionResult SMS()
        {
            return View();
        }

        /// <summary>
        /// 6. WhatsApp Notifications - Coming soon placeholder
        /// </summary>
        public IActionResult WhatsApp()
        {
            return View();
        }

        /// <summary>
        /// 7. Notification Templates - Admin only (uses scope for permissions)
        /// </summary>
        [Authorize(Roles = "Admin,HR Manager")]
        public async Task<IActionResult> Templates()
        {
            try
            {
                var templates = await _context.NotificationTemplates
                    .OrderBy(t => t.Category)
                    .ThenBy(t => t.TemplateName)
                    .ThenBy(t => t.Channel)
                    .ToListAsync();
                
                return View(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notification templates");
                TempData["ErrorMessage"] = "Error loading notification templates.";
                return View(new List<Models.NotificationTemplate>());
            }
        }

        /// <summary>
        /// 8. Notification Preferences - User settings with lazy loading
        /// </summary>
        public IActionResult Preferences()
        {
            return View();
        }

        /// <summary>
        /// 9. Channel Configurations - Admin only (Email/SMS/WhatsApp settings)
        /// Displays all channel configurations grouped by channel (Email, SMS, WhatsApp)
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChannelConfigs()
        {
            try
            {
                // Get all configurations grouped by channel
                var configs = await _channelConfigService.GetAllChannelConfigsAsync();
                
                return View(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading channel configurations");
                TempData["ErrorMessage"] = "Error loading channel configurations.";
                return View(new Dictionary<string, List<Models.NotificationChannelConfig>>());
            }
        }

        #endregion

        #region API Endpoints for AJAX/JavaScript

        /// <summary>
        /// Get recent notifications for bell dropdown (JSON)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRecent(int count = 10)
        {
            try
            {
                var notifications = await _notificationService.GetRecentNotificationsAsync(CurrentUserId, count);
                
                return Json(new
                {
                    success = true,
                    data = notifications.Select(n => new
                    {
                        n.NotificationId,
                        n.Title,
                        n.Message,
                        n.NotificationType,
                        n.Priority,
                        n.Category,
                        n.ActionUrl,
                        n.IsRead,
                        n.CreatedAt,
                        TimeAgo = GetTimeAgo(n.CreatedAt)
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent notifications");
                return Json(new { success = false, message = "Error loading notifications" });
            }
        }

        /// <summary>
        /// Get unread notification count for badge (JSON)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var count = await _notificationService.GetUnreadCountAsync(CurrentUserId);
                return Json(new { success = true, count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return Json(new { success = false, count = 0 });
            }
        }

        /// <summary>
        /// Mark notification as read (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var success = await _notificationService.MarkAsReadAsync(id, CurrentUserId);
                
                if (success)
                {
                    return Json(new { success = true, message = "Notification marked as read" });
                }
                else
                {
                    return Json(new { success = false, message = "Notification not found or access denied" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return Json(new { success = false, message = "Error updating notification" });
            }
        }

        /// <summary>
        /// Mark all notifications as read (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var count = await _notificationService.MarkAllAsReadAsync(CurrentUserId);
                return Json(new { success = true, count, message = $"{count} notifications marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return Json(new { success = false, message = "Error updating notifications" });
            }
        }

        /// <summary>
        /// Delete notification (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _notificationService.DeleteNotificationAsync(id, CurrentUserId);
                
                if (success)
                {
                    return Json(new { success = true, message = "Notification deleted" });
                }
                else
                {
                    return Json(new { success = false, message = "Notification not found or access denied" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                return Json(new { success = false, message = "Error deleting notification" });
            }
        }

        /// <summary>
        /// Bulk delete notifications (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BulkDelete([FromBody] List<int> ids)
        {
            try
            {
                var count = await _notificationService.BulkDeleteAsync(ids, CurrentUserId);
                return Json(new { success = true, count, message = $"{count} notifications deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk deleting notifications");
                return Json(new { success = false, message = "Error deleting notifications" });
            }
        }

        /// <summary>
        /// Get email notifications with delivery status (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetEmailNotifications()
        {
            try
            {
                // Get notifications with Email deliveries
                var notificationsWithDelivery = await _notificationService
                    .GetUserNotificationsByChannelAsync(CurrentUserId, "Email");

                var result = notificationsWithDelivery.Select(nwd => new
                {
                    notificationId = nwd.Notification.NotificationId,
                    deliveryId = nwd.Delivery?.NotificationDeliveryId ?? 0,
                    subject = nwd.Notification.Title,
                    message = nwd.Notification.Message,
                    category = nwd.Notification.Category,
                    priority = nwd.Notification.Priority,
                    recipientType = nwd.Notification.RecipientType,
                    recipientEmail = nwd.Delivery?.RecipientAddress ?? "N/A",
                    deliveryStatus = nwd.Delivery?.Status ?? "Not Sent",
                    isRead = nwd.Notification.IsRead,
                    createdAt = nwd.Notification.CreatedAt,
                    sentAt = nwd.Delivery?.SentAt,
                    actionUrl = nwd.Notification.ActionUrl,
                    retryCount = nwd.Delivery?.RetryCount ?? 0,
                    errorMessage = nwd.Delivery?.ErrorMessage
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email notifications");
                return Json(new { success = false, message = "Error loading email notifications" });
            }
        }

        /// <summary>
        /// Get email notification details (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetEmailDetails(int notificationId, int deliveryId)
        {
            try
            {
                // Verify user is recipient
                var isRecipient = await _notificationService.IsRecipientAsync(notificationId, CurrentUserId);
                if (!isRecipient)
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

                if (notification == null)
                {
                    return Json(new { success = false, message = "Notification not found" });
                }

                Models.NotificationDelivery? delivery = null;
                if (deliveryId > 0)
                {
                    delivery = await _context.NotificationDeliveries
                        .FirstOrDefaultAsync(nd => nd.NotificationDeliveryId == deliveryId);
                }

                var result = new
                {
                    notificationId = notification.NotificationId,
                    deliveryId = delivery?.NotificationDeliveryId ?? 0,
                    subject = notification.Title,
                    message = notification.Message,
                    category = notification.Category,
                    priority = notification.Priority,
                    recipientType = notification.RecipientType,
                    recipientEmail = delivery?.RecipientAddress ?? "N/A",
                    deliveryStatus = delivery?.Status ?? "Not Sent",
                    isRead = notification.IsRead,
                    createdAt = notification.CreatedAt,
                    sentAt = delivery?.SentAt,
                    actionUrl = notification.ActionUrl,
                    retryCount = delivery?.RetryCount ?? 0,
                    errorMessage = delivery?.ErrorMessage
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email details");
                return Json(new { success = false, message = "Error loading email details" });
            }
        }

        /// <summary>
        /// Get effective preference for a category (custom or default) (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPreferences(string category)
        {
            try
            {
                var preference = await _preferenceService.GetEffectivePreferenceAsync(CurrentUserId, category);
                
                // Convert TimeSpan to string for JSON
                var result = new
                {
                    preference.PreferenceId,
                    preference.EmployeeId,
                    preference.Category,
                    preference.InAppEnabled,
                    preference.EmailEnabled,
                    preference.SmsEnabled,
                    preference.WhatsAppEnabled,
                    preference.MinPriority,
                    QuietHoursStart = preference.QuietHoursStart?.ToString(@"hh\:mm"),
                    QuietHoursEnd = preference.QuietHoursEnd?.ToString(@"hh\:mm"),
                    preference.DigestFrequency,
                    preference.CreatedAt,
                    preference.UpdatedAt
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting preferences for category {Category}", category);
                return Json(new { success = false, message = "Error loading preferences" });
            }
        }

        /// <summary>
        /// Save user's custom preference (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SavePreference([FromBody] Models.NotificationPreference preference)
        {
            try
            {
                // Set employee ID from current user
                preference.EmployeeId = CurrentUserId;

                // Parse quiet hours from strings if provided
                // (JavaScript sends as strings like "22:00")

                var success = await _preferenceService.SavePreferenceAsync(preference);

                if (success)
                {
                    return Json(new { success = true, message = "Preferences saved successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to save preferences" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving preference");
                return Json(new { success = false, message = "Error saving preferences" });
            }
        }

        /// <summary>
        /// Reset preference to system default (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ResetPreference(string category)
        {
            try
            {
                var success = await _preferenceService.ResetToDefaultAsync(CurrentUserId, category);

                if (success)
                {
                    return Json(new { success = true, message = "Preferences reset to default" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to reset preferences" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting preference");
                return Json(new { success = false, message = "Error resetting preferences" });
            }
        }

        /// <summary>
        /// Edit channel configuration (GET)
        /// Admin only - edit Email/SMS/WhatsApp configurations
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EditChannelConfig(string channel)
        {
            try
            {
                if (string.IsNullOrEmpty(channel))
                {
                    TempData["ErrorMessage"] = "Channel parameter is required.";
                    return RedirectToAction("ChannelConfigs");
                }

                // Get all configurations for this channel
                var configs = await _channelConfigService.GetChannelConfigRecordsAsync(channel);

                if (!configs.Any())
                {
                    TempData["ErrorMessage"] = $"No configurations found for channel: {channel}";
                    return RedirectToAction("ChannelConfigs");
                }

                ViewBag.Channel = channel;
                return View(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit channel config page for {Channel}", channel);
                TempData["ErrorMessage"] = "Error loading channel configuration.";
                return RedirectToAction("ChannelConfigs");
            }
        }

        /// <summary>
        /// Save channel configuration (POST)
        /// Admin only - save all configurations for a channel
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveChannelConfig(string channel, Dictionary<string, string> configs)
        {
            try
            {
                if (string.IsNullOrEmpty(channel))
                {
                    TempData["ErrorMessage"] = "Channel parameter is required.";
                    return RedirectToAction("ChannelConfigs");
                }

                // Handle checkbox fields (EnableSsl) - if not in dictionary, it's unchecked
                var allConfigKeys = await _context.NotificationChannelConfigs
                    .Where(c => c.Channel == channel)
                    .Select(c => c.ConfigKey)
                    .ToListAsync();

                foreach (var key in allConfigKeys)
                {
                    if (key.Equals("EnableSsl", StringComparison.OrdinalIgnoreCase) && !configs.ContainsKey(key))
                    {
                        configs[key] = "false";
                    }
                }

                var success = await _channelConfigService.SaveChannelConfigsAsync(channel, configs);

                if (success)
                {
                    TempData["SuccessMessage"] = $"{channel} configuration saved successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to save configuration. Please try again.";
                }

                return RedirectToAction("EditChannelConfig", new { channel });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving channel config for {Channel}", channel);
                TempData["ErrorMessage"] = "An error occurred while saving configuration.";
                return RedirectToAction("EditChannelConfig", new { channel });
            }
        }

        /// <summary>
        /// Test channel configuration (AJAX)
        /// Admin only - test connection with current settings
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> TestChannelConfig(string channel)
        {
            try
            {
                if (string.IsNullOrEmpty(channel))
                {
                    return Json(new { success = false, message = "Channel parameter is required" });
                }

                var (success, message) = await _channelConfigService.TestChannelConnectionAsync(channel);

                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing channel config for {Channel}", channel);
                return Json(new { success = false, message = "An error occurred while testing the connection" });
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Convert datetime to "time ago" string
        /// </summary>
        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)}w ago";
            
            return dateTime.ToString("MMM dd, yyyy");
        }

        #endregion
    }
}
