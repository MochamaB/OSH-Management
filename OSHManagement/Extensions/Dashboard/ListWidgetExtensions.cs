using OSHManagement.Models.ViewModels;
using OSHManagement.Models.ViewModels.Dashboard;

namespace OSHManagement.Extensions.Dashboard
{
    /// <summary>
    /// Extension methods for building List Widget components
    /// ALL LOGIC - NO RENDERING
    /// </summary>
    public static class ListWidgetExtensions
    {
        /// <summary>
        /// Build a standard list widget
        /// </summary>
        public static ListWidgetViewModel BuildListWidget(
            string title,
            List<ListItemViewModel> items,
            string? viewAllUrl = null,
            string? viewAllText = "View All",
            bool showIcons = true,
            bool showTimestamps = true,
            int maxItems = 10)
        {
            return new ListWidgetViewModel
            {
                Title = title,
                Items = items,
                ViewAllUrl = viewAllUrl,
                ViewAllText = viewAllText,
                ShowIcons = showIcons,
                ShowTimestamps = showTimestamps,
                MaxItems = maxItems,
                WidgetType = ListWidgetType.Standard
            };
        }

        /// <summary>
        /// Build a timeline-style list widget
        /// </summary>
        public static ListWidgetViewModel BuildTimelineListWidget(
            string title,
            List<ListItemViewModel> items,
            string? viewAllUrl = null,
            int maxItems = 10)
        {
            return new ListWidgetViewModel
            {
                Title = title,
                Items = items,
                ViewAllUrl = viewAllUrl,
                ShowIcons = true,
                ShowTimestamps = true,
                MaxItems = maxItems,
                WidgetType = ListWidgetType.WithTimeline
            };
        }

        /// <summary>
        /// Build a compact list widget (minimal spacing)
        /// </summary>
        public static ListWidgetViewModel BuildCompactListWidget(
            string title,
            List<ListItemViewModel> items,
            string? viewAllUrl = null,
            int maxItems = 15)
        {
            return new ListWidgetViewModel
            {
                Title = title,
                Items = items,
                ViewAllUrl = viewAllUrl,
                ShowIcons = false,
                ShowTimestamps = false,
                ShowSubtitles = false,
                MaxItems = maxItems,
                WidgetType = ListWidgetType.Compact
            };
        }

        /// <summary>
        /// Build a notification-style list widget
        /// </summary>
        public static ListWidgetViewModel BuildNotificationListWidget(
            string title,
            List<ListItemViewModel> items,
            string? viewAllUrl = null,
            int maxItems = 10)
        {
            return new ListWidgetViewModel
            {
                Title = title,
                Items = items,
                ViewAllUrl = viewAllUrl,
                ShowIcons = true,
                ShowTimestamps = true,
                ShowBadges = true,
                MaxItems = maxItems,
                WidgetType = ListWidgetType.Notification
            };
        }

        /// <summary>
        /// Build a single list item
        /// </summary>
        public static ListItemViewModel BuildListItem(
            string title,
            string? subtitle = null,
            string? icon = null,
            string? iconColor = "primary",
            string? badge = null,
            string? badgeColor = null,
            string? timestamp = null,
            string? linkUrl = null)
        {
            return new ListItemViewModel
            {
                Title = title,
                Subtitle = subtitle,
                Icon = icon,
                IconColor = iconColor,
                Badge = badge,
                BadgeColor = badgeColor,
                Timestamp = timestamp,
                LinkUrl = linkUrl
            };
        }

        /// <summary>
        /// Build list items from simple data
        /// </summary>
        public static List<ListItemViewModel> BuildListItems(
            List<string> titles,
            List<string>? subtitles = null,
            List<string>? icons = null,
            List<string>? badges = null,
            List<string>? timestamps = null,
            string defaultIcon = "ri-file-list-line")
        {
            var items = new List<ListItemViewModel>();

            for (int i = 0; i < titles.Count; i++)
            {
                items.Add(new ListItemViewModel
                {
                    Title = titles[i],
                    Subtitle = subtitles != null && i < subtitles.Count ? subtitles[i] : null,
                    Icon = icons != null && i < icons.Count ? icons[i] : defaultIcon,
                    Badge = badges != null && i < badges.Count ? badges[i] : null,
                    Timestamp = timestamps != null && i < timestamps.Count ? timestamps[i] : null
                });
            }

            return items;
        }

        /// <summary>
        /// Get "time ago" formatted string
        /// </summary>
        public static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalSeconds < 60)
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

        /// <summary>
        /// Get appropriate icon based on item type
        /// </summary>
        public static string GetIconForItemType(string itemType)
        {
            return itemType.ToLower() switch
            {
                "incident" => "ri-alert-line",
                "action" => "ri-todo-line",
                "training" => "ri-book-open-line",
                "inspection" => "ri-search-eye-line",
                "audit" => "ri-file-list-3-line",
                "hazard" => "ri-error-warning-line",
                "meeting" => "ri-team-line",
                "document" => "ri-file-text-line",
                "notification" => "ri-notification-3-line",
                "task" => "ri-task-line",
                "comment" => "ri-message-3-line",
                "approval" => "ri-checkbox-circle-line",
                "ppe" => "ri-shield-user-line",
                "equipment" => "ri-tools-line",
                "employee" => "ri-user-line",
                _ => "ri-file-list-line"
            };
        }

        /// <summary>
        /// Get color based on status
        /// </summary>
        public static string GetColorForStatus(string status)
        {
            return status.ToLower() switch
            {
                "open" or "pending" or "in progress" => "warning",
                "closed" or "completed" or "resolved" => "success",
                "overdue" or "critical" or "urgent" => "danger",
                "cancelled" or "rejected" => "secondary",
                "approved" => "success",
                "draft" => "info",
                _ => "primary"
            };
        }

        /// <summary>
        /// Build recent incidents list widget (common use case)
        /// </summary>
        public static ListWidgetViewModel BuildRecentIncidentsWidget(
            List<(string Title, string Location, string Severity, DateTime Date, int Id)> incidents,
            int maxItems = 5)
        {
            var items = incidents.Take(maxItems).Select(i => new ListItemViewModel
            {
                Title = i.Title,
                Subtitle = i.Location,
                Icon = "ri-alert-line",
                IconColor = GetColorForSeverity(i.Severity),
                Badge = i.Severity,
                BadgeColor = GetColorForSeverity(i.Severity),
                Timestamp = GetTimeAgo(i.Date),
                LinkUrl = $"/Incident/Details/{i.Id}"
            }).ToList();

            return BuildListWidget(
                title: "Recent Incidents",
                items: items,
                viewAllUrl: "/Incident/Index",
                maxItems: maxItems
            );
        }

        /// <summary>
        /// Build recent actions list widget (common use case)
        /// </summary>
        public static ListWidgetViewModel BuildRecentActionsWidget(
            List<(string Title, string AssignedTo, string Status, DateTime DueDate, int Id)> actions,
            int maxItems = 5)
        {
            var items = actions.Take(maxItems).Select(a => new ListItemViewModel
            {
                Title = a.Title,
                Subtitle = $"Assigned to: {a.AssignedTo}",
                Icon = "ri-todo-line",
                IconColor = GetColorForStatus(a.Status),
                Badge = a.Status,
                BadgeColor = GetColorForStatus(a.Status),
                Timestamp = $"Due {GetTimeAgo(a.DueDate)}",
                LinkUrl = $"/Action/Details/{a.Id}"
            }).ToList();

            return BuildListWidget(
                title: "Recent Actions",
                items: items,
                viewAllUrl: "/Action/Index",
                maxItems: maxItems
            );
        }

        /// <summary>
        /// Build notifications list widget (common use case)
        /// </summary>
        public static ListWidgetViewModel BuildNotificationsWidget(
            List<(string Title, string Message, DateTime Date, bool IsRead, int Id)> notifications,
            int maxItems = 10)
        {
            var items = notifications.Take(maxItems).Select(n => new ListItemViewModel
            {
                Title = n.Title,
                Subtitle = n.Message,
                Icon = "ri-notification-3-line",
                IconColor = n.IsRead ? "secondary" : "primary",
                Timestamp = GetTimeAgo(n.Date),
                IsRead = n.IsRead,
                LinkUrl = $"/Notification/Details/{n.Id}"
            }).ToList();

            return BuildNotificationListWidget(
                title: "Notifications",
                items: items,
                viewAllUrl: "/Notification/Index",
                maxItems: maxItems
            );
        }

        /// <summary>
        /// Build activity timeline widget (common use case)
        /// </summary>
        public static ListWidgetViewModel BuildActivityTimelineWidget(
            List<(string Activity, string User, DateTime Date, string Icon)> activities,
            int maxItems = 10)
        {
            var items = activities.Take(maxItems).Select(a => new ListItemViewModel
            {
                Title = a.Activity,
                Subtitle = a.User,
                Icon = a.Icon,
                IconColor = "primary",
                Timestamp = GetTimeAgo(a.Date)
            }).ToList();

            return BuildTimelineListWidget(
                title: "Activity Timeline",
                items: items,
                maxItems: maxItems
            );
        }

        /// <summary>
        /// Helper: Get color for severity
        /// </summary>
        private static string GetColorForSeverity(string severity)
        {
            return severity.ToLower() switch
            {
                "fatal" => "danger",
                "major" => "danger",
                "minor" => "warning",
                "near miss" => "info",
                _ => "secondary"
            };
        }
    }
}
