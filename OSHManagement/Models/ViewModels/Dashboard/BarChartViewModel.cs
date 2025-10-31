using OSHManagement.Models.ViewModels;

namespace OSHManagement.Models.ViewModels.Dashboard
{
    /// <summary>
    /// ViewModel for Bar Chart Widget using ApexCharts
    /// Shows data in vertical or horizontal bar format
    /// </summary>
    public class BarChartViewModel
    {
        // Basic Properties
        public string Title { get; set; } = string.Empty;
        public List<BarChartSeriesViewModel> Series { get; set; } = new List<BarChartSeriesViewModel>();
        public List<string> Categories { get; set; } = new List<string>();
        public List<string> Colors { get; set; } = new List<string>();

        // Optional Properties
        public string? Subtitle { get; set; }
        public string? Icon { get; set; }
        public string? ViewAllUrl { get; set; }
        public string? ViewAllText { get; set; } = "View Details";

        // Chart Options
        public int Height { get; set; } = 350;
        public bool Horizontal { get; set; } = false;
        public bool Stacked { get; set; } = false;
        public bool ShowLegend { get; set; } = true;
        public bool ShowDataLabels { get; set; } = false;
        public bool ShowGrid { get; set; } = true;
        public string YAxisLabel { get; set; } = string.Empty;
        public string XAxisLabel { get; set; } = string.Empty;

        // Layout Properties
        public string ColumnClass { get; set; } = "col-xl-6 col-md-12";
        public string ChartId { get; set; } = Guid.NewGuid().ToString("N");

        // Helper Properties
        public bool HasData => Series != null && Series.Any() && Series.Any(s => s.Data.Any());

        /// <summary>
        /// Get colors in CSS format for ApexCharts
        /// </summary>
        public string GetColorsJson()
        {
            if (Colors == null || !Colors.Any())
            {
                return "[\"#6366f1\", \"#22c55e\", \"#f59e0b\", \"#ef4444\", \"#8b5cf6\", \"#06b6d4\", \"#f97316\", \"#ec4899\"]";
            }

            var formattedColors = Colors.Select(c => ConvertColorClassToHex(c));
            return System.Text.Json.JsonSerializer.Serialize(formattedColors);
        }

        /// <summary>
        /// Get series data as JSON with camelCase property names for ApexCharts
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
        /// Get categories as JSON
        /// </summary>
        public string GetCategoriesJson()
        {
            return System.Text.Json.JsonSerializer.Serialize(Categories);
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
    /// Series data for bar chart
    /// </summary>
    public class BarChartSeriesViewModel
    {
        public string Name { get; set; } = string.Empty;
        public List<decimal> Data { get; set; } = new List<decimal>();
    }
}
