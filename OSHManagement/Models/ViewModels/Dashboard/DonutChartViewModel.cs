using OSHManagement.Models.ViewModels;

namespace OSHManagement.Models.ViewModels.Dashboard
{
    /// <summary>
    /// ViewModel for Donut Chart Widget using ApexCharts
    /// Shows data in donut/pie chart format with legend
    /// </summary>
    public class DonutChartViewModel
    {
        // Basic Properties
        public string Title { get; set; } = string.Empty;
        public List<decimal> Series { get; set; } = new List<decimal>();
        public List<string> Labels { get; set; } = new List<string>();
        public List<string> Colors { get; set; } = new List<string>();

        // Optional Properties
        public string? Subtitle { get; set; }
        public string? Icon { get; set; }
        public string? ViewAllUrl { get; set; }
        public string? ViewAllText { get; set; } = "View Details";

        // Chart Options
        public int Height { get; set; } = 320;
        public bool ShowLegend { get; set; } = true;
        public string LegendPosition { get; set; } = "bottom"; // bottom, top, left, right
        public bool ShowDataLabels { get; set; } = true;
        public bool ShowValuesInLegend { get; set; } = false; // Show values next to legend labels
        public bool ShowTotal { get; set; } = false;
        public string? TotalLabel { get; set; } = "Total";

        // Layout Properties
        public string ColumnClass { get; set; } = "col-xl-6 col-md-12";
        public string ChartId { get; set; } = Guid.NewGuid().ToString("N");

        // Helper Properties
        public bool HasData => Series != null && Series.Any() && Series.Sum() > 0;
        public decimal Total => Series?.Sum() ?? 0;
        
        /// <summary>
        /// Get colors in CSS format for ApexCharts
        /// </summary>
        public string GetColorsJson()
        {
            if (Colors == null || !Colors.Any())
            {
                // Default color palette
                return "[\"#6366f1\", \"#22c55e\", \"#f59e0b\", \"#ef4444\", \"#8b5cf6\", \"#06b6d4\", \"#f97316\", \"#ec4899\"]";
            }
            
            var formattedColors = Colors.Select(c => 
            {
                // Convert Bootstrap color classes to hex
                return ConvertColorClassToHex(c);
            });
            
            return System.Text.Json.JsonSerializer.Serialize(formattedColors);
        }

        /// <summary>
        /// Get series data as JSON
        /// </summary>
        public string GetSeriesJson()
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };
            return System.Text.Json.JsonSerializer.Serialize(Series, options);
        }

        /// <summary>
        /// Get labels as JSON
        /// </summary>
        public string GetLabelsJson()
        {
            return System.Text.Json.JsonSerializer.Serialize(Labels);
        }

        /// <summary>
        /// Convert Bootstrap color class to hex
        /// </summary>
        private string ConvertColorClassToHex(string colorClass)
        {
            return colorClass.ToLower() switch
            {
                "primary" => "#6366f1",
                "secondary" => "#6c757d",
                "success" => "#22c55e",
                "danger" => "#ef4444",
                "warning" => "#f59e0b",
                "info" => "#06b6d4",
                "light" => "#f8f9fa",
                "dark" => "#1f2937",
                "purple" => "#8b5cf6",
                "pink" => "#ec4899",
                "orange" => "#f97316",
                "teal" => "#14b8a6",
                _ => colorClass.StartsWith("#") ? colorClass : "#6366f1"
            };
        }
    }

    /// <summary>
    /// Chart data item for building donut charts
    /// </summary>
    public class ChartDataItem
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string? Color { get; set; }
    }
}
