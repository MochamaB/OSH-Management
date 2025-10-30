using OSHManagement.Models.ViewModels;

namespace OSHManagement.Models.ViewModels.Dashboard
{
    /// <summary>
    /// ViewModel for KPI Card widgets (supports 3 pattern variants)
    /// Pattern A: Standard KPI with icon, value, trend, badge
    /// Pattern B: KPI with emphasized trend
    /// Pattern C: KPI with sparkline chart
    /// </summary>
    public class KPICardViewModel
    {
        // Basic Properties
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ColorTheme { get; set; } = "primary"; // primary, secondary, success, danger, warning, info

        // Optional Properties
        public string? Badge { get; set; }
        public string? BadgeColor { get; set; } // If null, uses ColorTheme + "-transparent"
        public string? Subtitle { get; set; }
        public string? LinkUrl { get; set; }
        public string? Tooltip { get; set; }

        // Trend Properties (Pattern A & B)
        public string? TrendValue { get; set; } // e.g., "+12.5%"
        public TrendDirection? TrendDirection { get; set; }
        public string? TrendLabel { get; set; } // e.g., "vs last month"

        // Sparkline Properties (Pattern C)
        public string? SparklineId { get; set; } // Chart container ID
        public List<decimal>? SparklineData { get; set; } // Simple array of values for mini chart
        public string? SparklineColor { get; set; } // If null, uses ColorTheme

        // Layout Properties
        public string ColumnClass { get; set; } = "col-xl-3 col-md-6"; // Bootstrap column classes
        public KPICardType CardType { get; set; } = KPICardType.Standard;

        // Helper Properties
        public string BadgeColorClass => !string.IsNullOrEmpty(BadgeColor) 
            ? BadgeColor 
            : $"{ColorTheme}-transparent";

        public string TrendColorClass => TrendDirection switch
        {
            ViewModels.TrendDirection.Up => "success",
            ViewModels.TrendDirection.Down => "danger",
            ViewModels.TrendDirection.Neutral => "muted",
            _ => "muted"
        };

        public string TrendIconClass => TrendDirection switch
        {
            ViewModels.TrendDirection.Up => "ri-arrow-up-line",
            ViewModels.TrendDirection.Down => "ri-arrow-down-line",
            ViewModels.TrendDirection.Neutral => "ri-subtract-line",
            _ => ""
        };
    }

    /// <summary>
    /// KPI Card pattern variants
    /// </summary>
    public enum KPICardType
    {
        Standard,           // Pattern A: Icon + Value + Trend + Badge
        WithTrend,          // Pattern B: Emphasized trend display
        WithSparkline       // Pattern C: Includes mini chart
    }
}
