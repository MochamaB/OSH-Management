using OSHManagement.Models.ViewModels;

namespace OSHManagement.Models.ViewModels.Dashboard
{
    /// <summary>
    /// ViewModel for List Widget - displays lists of items (recent incidents, actions, etc.)
    /// Shows items with icons, labels, timestamps, and badges
    /// </summary>
    public class ListWidgetViewModel
    {
        // Basic Properties
        public string Title { get; set; } = string.Empty;
        public List<ListItemViewModel> Items { get; set; } = new List<ListItemViewModel>();

        // Optional Properties
        public string? ViewAllUrl { get; set; }
        public string? ViewAllText { get; set; } = "View All";
        public string? EmptyMessage { get; set; } = "No items to display";
        public string? Icon { get; set; } // Widget header icon

        // Display Options
        public bool ShowIcons { get; set; } = true;
        public bool ShowTimestamps { get; set; } = true;
        public bool ShowBadges { get; set; } = true;
        public bool ShowSubtitles { get; set; } = true;
        public int MaxItems { get; set; } = 10;

        // Layout Properties
        public string ColumnClass { get; set; } = "col-xl-6 col-md-12";
        public ListWidgetType WidgetType { get; set; } = ListWidgetType.Standard;

        // Helper Property
        public bool HasItems => Items != null && Items.Any();
    }

    /// <summary>
    /// Individual list item
    /// </summary>
    public class ListItemViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? Icon { get; set; }
        public string? IconColor { get; set; } = "primary";
        public string? Badge { get; set; }
        public string? BadgeColor { get; set; } = "primary";
        public string? Timestamp { get; set; }
        public string? LinkUrl { get; set; }
        public string? AvatarUrl { get; set; } // For user avatars
        public string? AvatarInitials { get; set; } // E.g., "JD" for John Doe
        public bool IsRead { get; set; } = true; // For notification-style lists
        public string? SecondaryText { get; set; } // Additional info on the right

        // Helper Properties
        public string IconColorClass => IconColor ?? "primary";
        public string BadgeColorClass => !string.IsNullOrEmpty(BadgeColor) 
            ? BadgeColor 
            : "primary";
    }

    /// <summary>
    /// List Widget display variants
    /// </summary>
    public enum ListWidgetType
    {
        Standard,           // Simple list with icons and text
        WithTimeline,       // Timeline-style list (left border, dots)
        Compact,            // Minimal spacing, smaller text
        Notification        // Notification-style with read/unread states
    }
}
