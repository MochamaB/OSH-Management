using OSHManagement.Models.ViewModels;

namespace OSHManagement.Models.ViewModels.Dashboard
{
    /// <summary>
    /// ViewModel for Progress Widget - displays completion/progress metrics
    /// Shows progress bars with percentage, labels, and optional thresholds
    /// </summary>
    public class ProgressWidgetViewModel
    {
        // Basic Properties
        public string Title { get; set; } = string.Empty;
        public int CurrentValue { get; set; }
        public int TotalValue { get; set; }
        
        // Calculated Properties
        public decimal Percentage => TotalValue > 0 
            ? Math.Round((decimal)CurrentValue / TotalValue * 100, 1) 
            : 0;

        // Optional Properties
        public string? Description { get; set; } // e.g., "150 of 200 completed"
        public string? Subtitle { get; set; }
        public string? Icon { get; set; }
        public string? LinkUrl { get; set; }

        // Progress Bar Styling
        public string ColorClass { get; set; } = "primary"; // primary, success, warning, danger, info
        public bool Striped { get; set; } = false;
        public bool Animated { get; set; } = false;
        public int Height { get; set; } = 20; // Progress bar height in pixels

        // Threshold-based Coloring
        public List<ProgressThreshold>? Thresholds { get; set; }

        // Layout Properties
        public string ColumnClass { get; set; } = "col-xl-6 col-md-12";
        public ProgressWidgetType WidgetType { get; set; } = ProgressWidgetType.Standard;

        // Helper Properties
        public string ProgressBarColorClass
        {
            get
            {
                // If thresholds defined, use threshold-based color
                if (Thresholds != null && Thresholds.Any())
                {
                    var threshold = Thresholds
                        .FirstOrDefault(t => Percentage >= t.MinValue && Percentage <= t.MaxValue);
                    
                    if (threshold != null)
                        return threshold.ColorClass;
                }

                // Otherwise use specified color
                return ColorClass;
            }
        }

        public string ProgressBarClasses
        {
            get
            {
                var classes = new List<string> { "progress-bar", $"bg-{ProgressBarColorClass}" };
                
                if (Striped)
                    classes.Add("progress-bar-striped");
                
                if (Animated)
                    classes.Add("progress-bar-animated");

                return string.Join(" ", classes);
            }
        }

        public string FormattedDescription => !string.IsNullOrEmpty(Description)
            ? Description
            : $"{CurrentValue} of {TotalValue}";
    }

    /// <summary>
    /// Threshold configuration for progress bar color changes
    /// </summary>
    public class ProgressThreshold
    {
        public decimal MinValue { get; set; } // Minimum percentage (inclusive)
        public decimal MaxValue { get; set; } // Maximum percentage (inclusive)
        public string ColorClass { get; set; } = "primary"; // Color when in this range
        public string? Label { get; set; } // Optional label (e.g., "Poor", "Good", "Excellent")
    }

    /// <summary>
    /// Progress Widget display variants
    /// </summary>
    public enum ProgressWidgetType
    {
        Standard,       // Simple progress bar with label
        WithIcon,       // Progress bar with icon
        Detailed,       // Progress bar with additional stats/info
        Multiple        // Multiple progress bars in one widget
    }
}
