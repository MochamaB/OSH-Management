using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Models.Enums;
using OSHManagement.Services.Notifications.Channels;
using OSHManagement.Services.Notifications.DTOs;

namespace OSHManagement.Services.Notifications
{
    /// <summary>
    /// Main notification event publisher service
    /// Receives events from controllers and creates notifications
    /// </summary>
    public class NotificationEventPublisher : INotificationEventPublisher
    {
        private readonly OshDbContext _context;
        private readonly INotificationTemplateService _templateService;
        private readonly IEnumerable<INotificationChannelService> _channelServices;
        private readonly ILogger<NotificationEventPublisher> _logger;

        public NotificationEventPublisher(
            OshDbContext context,
            INotificationTemplateService templateService,
            IEnumerable<INotificationChannelService> channelServices,
            ILogger<NotificationEventPublisher> logger)
        {
            _context = context;
            _templateService = templateService;
            _channelServices = channelServices;
            _logger = logger;
        }

        /// <summary>
        /// Publish a notification event
        /// </summary>
        public async Task PublishAsync(NotificationEvent notificationEvent)
        {
            try
            {
                _logger.LogInformation("Publishing notification event: {EventType}", notificationEvent.EventType);

                // Get template for InApp channel (primary)
                var template = await _templateService.GetTemplateAsync(
                    notificationEvent.EventType,
                    NotificationChannel.InApp);

                if (template == null)
                {
                    _logger.LogWarning("No template found for event '{EventType}'. Skipping notification.", 
                        notificationEvent.EventType);
                    return;
                }

                // Render title and message from template
                var title = !string.IsNullOrEmpty(template.SubjectTemplate)
                    ? _templateService.RenderSubject(template, notificationEvent.Data)
                    : template.TemplateName;

                var message = _templateService.RenderTemplate(template, notificationEvent.Data);

                // Create notifications list
                var notifications = new List<Notification>();

                // Add direct employee recipients
                foreach (var employeeId in notificationEvent.RecipientEmployeeIds)
                {
                    notifications.Add(CreateNotification(
                        "Employee",
                        employeeId,
                        title,
                        message,
                        notificationEvent));
                }

                // Add role-based recipients
                foreach (var roleId in notificationEvent.RecipientRoleIds)
                {
                    notifications.Add(CreateNotification(
                        "Role",
                        roleId,
                        title,
                        message,
                        notificationEvent));
                }

                // Add station-based recipients
                foreach (var stationId in notificationEvent.RecipientStationIds)
                {
                    notifications.Add(CreateNotification(
                        "Station",
                        stationId,
                        title,
                        message,
                        notificationEvent));
                }

                // Add department-based recipients
                foreach (var departmentId in notificationEvent.RecipientDepartmentIds)
                {
                    notifications.Add(CreateNotification(
                        "Department",
                        departmentId,
                        title,
                        message,
                        notificationEvent));
                }

                // Add team-based recipients
                foreach (var teamId in notificationEvent.RecipientTeamIds)
                {
                    notifications.Add(CreateNotification(
                        "Team",
                        teamId,
                        title,
                        message,
                        notificationEvent));
                }

                if (notifications.Count == 0)
                {
                    _logger.LogWarning("No recipients specified for event '{EventType}'", 
                        notificationEvent.EventType);
                    return;
                }

                // Save all notifications to database
                await _context.Notifications.AddRangeAsync(notifications);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created {Count} notifications for event '{EventType}'",
                    notifications.Count, notificationEvent.EventType);

                // TODO Phase 2: Queue email/SMS deliveries based on user preferences
                // TODO Phase 3: Trigger SignalR push for real-time updates
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing notification event '{EventType}'", 
                    notificationEvent.EventType);
            }
        }

        /// <summary>
        /// Create a notification object
        /// </summary>
        private Notification CreateNotification(
            string recipientType,
            int recipientId,
            string title,
            string message,
            NotificationEvent notificationEvent)
        {
            return new Notification
            {
                RecipientType = recipientType,
                RecipientId = recipientId,
                Title = title,
                Message = message,
                NotificationType = MapPriorityToType(notificationEvent.Priority),
                Priority = notificationEvent.Priority.ToString(),
                Category = notificationEvent.Category,
                ActionUrl = notificationEvent.ActionUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Map priority to notification type for UI styling
        /// </summary>
        private string MapPriorityToType(NotificationPriority priority)
        {
            return priority switch
            {
                NotificationPriority.Urgent => "ActionRequired",
                NotificationPriority.High => "Warning",
                NotificationPriority.Normal => "Info",
                NotificationPriority.Low => "Info",
                _ => "Info"
            };
        }
    }
}
