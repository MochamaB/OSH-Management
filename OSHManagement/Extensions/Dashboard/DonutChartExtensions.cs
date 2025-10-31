using OSHManagement.Models.ViewModels;
using OSHManagement.Models.ViewModels.Dashboard;

namespace OSHManagement.Extensions.Dashboard
{
    /// <summary>
    /// Extension methods for building Donut Chart components
    /// ALL LOGIC - NO RENDERING
    /// </summary>
    public static class DonutChartExtensions
    {
        /// <summary>
        /// Build a donut chart widget
        /// </summary>
        public static DonutChartViewModel BuildDonutChart(
            string title,
            List<decimal> series,
            List<string> labels,
            List<string>? colors = null,
            string? subtitle = null,
            bool showLegend = true,
            int height = 320)
        {
            return new DonutChartViewModel
            {
                Title = title,
                Series = series,
                Labels = labels,
                Colors = colors ?? GetDefaultColors(),
                Subtitle = subtitle,
                ShowLegend = showLegend,
                Height = height
            };
        }

        /// <summary>
        /// Build donut chart from data items
        /// </summary>
        public static DonutChartViewModel BuildDonutChartFromData(
            string title,
            List<ChartDataItem> data,
            string? subtitle = null,
            bool showLegend = true,
            int height = 320)
        {
            return new DonutChartViewModel
            {
                Title = title,
                Series = data.Select(d => d.Value).ToList(),
                Labels = data.Select(d => d.Label).ToList(),
                Colors = data.Select(d => d.Color ?? "primary").ToList(),
                Subtitle = subtitle,
                ShowLegend = showLegend,
                Height = height
            };
        }

        /// <summary>
        /// Build incident severity donut chart (common use case)
        /// </summary>
        public static DonutChartViewModel BuildIncidentSeverityChart(
            int fatal,
            int major,
            int minor,
            int nearMiss)
        {
            var data = new List<ChartDataItem>
            {
                new() { Label = "Fatal", Value = fatal, Color = "danger" },
                new() { Label = "Major", Value = major, Color = "danger" },
                new() { Label = "Minor", Value = minor, Color = "warning" },
                new() { Label = "Near Miss", Value = nearMiss, Color = "info" }
            };

            // Filter out zero values
            data = data.Where(d => d.Value > 0).ToList();

            return BuildDonutChartFromData(
                title: "Incidents by Severity",
                data: data,
                subtitle: $"Total: {fatal + major + minor + nearMiss} incidents",
                showLegend: true,
                height: 320
            );
        }

        /// <summary>
        /// Build action status donut chart (common use case)
        /// </summary>
        public static DonutChartViewModel BuildActionStatusChart(
            int completed,
            int inProgress,
            int pending,
            int overdue)
        {
            var data = new List<ChartDataItem>
            {
                new() { Label = "Completed", Value = completed, Color = "success" },
                new() { Label = "In Progress", Value = inProgress, Color = "info" },
                new() { Label = "Pending", Value = pending, Color = "warning" },
                new() { Label = "Overdue", Value = overdue, Color = "danger" }
            };

            // Filter out zero values
            data = data.Where(d => d.Value > 0).ToList();

            return BuildDonutChartFromData(
                title: "Actions by Status",
                data: data,
                subtitle: $"Total: {completed + inProgress + pending + overdue} actions",
                showLegend: true,
                height: 320
            );
        }

        /// <summary>
        /// Build training compliance donut chart (common use case)
        /// </summary>
        public static DonutChartViewModel BuildTrainingComplianceChart(
            int compliant,
            int partial,
            int nonCompliant)
        {
            var data = new List<ChartDataItem>
            {
                new() { Label = "Compliant", Value = compliant, Color = "success" },
                new() { Label = "Partial", Value = partial, Color = "warning" },
                new() { Label = "Non-Compliant", Value = nonCompliant, Color = "danger" }
            };

            // Filter out zero values
            data = data.Where(d => d.Value > 0).ToList();

            return BuildDonutChartFromData(
                title: "Training Compliance",
                data: data,
                subtitle: $"Total: {compliant + partial + nonCompliant} employees",
                showLegend: true,
                height: 320
            );
        }

        /// <summary>
        /// Build department distribution donut chart (common use case)
        /// </summary>
        public static DonutChartViewModel BuildDepartmentDistributionChart(
            Dictionary<string, int> departmentCounts,
            string title = "Distribution by Department")
        {
            var data = departmentCounts
                .Where(kvp => kvp.Value > 0)
                .Select((kvp, index) => new ChartDataItem
                {
                    Label = kvp.Key,
                    Value = kvp.Value,
                    Color = GetColorByIndex(index)
                })
                .ToList();

            return BuildDonutChartFromData(
                title: title,
                data: data,
                subtitle: $"Total: {departmentCounts.Values.Sum()} items",
                showLegend: true,
                height: 320
            );
        }

        /// <summary>
        /// Build equipment condition donut chart (common use case)
        /// </summary>
        public static DonutChartViewModel BuildEquipmentConditionChart(
            int excellent,
            int good,
            int fair,
            int poor)
        {
            var data = new List<ChartDataItem>
            {
                new() { Label = "Excellent", Value = excellent, Color = "success" },
                new() { Label = "Good", Value = good, Color = "info" },
                new() { Label = "Fair", Value = fair, Color = "warning" },
                new() { Label = "Poor", Value = poor, Color = "danger" }
            };

            // Filter out zero values
            data = data.Where(d => d.Value > 0).ToList();

            return BuildDonutChartFromData(
                title: "Equipment Condition",
                data: data,
                subtitle: $"Total: {excellent + good + fair + poor} items",
                showLegend: true,
                height: 320
            );
        }

        /// <summary>
        /// Get default color palette
        /// </summary>
        private static List<string> GetDefaultColors()
        {
            return new List<string>
            {
                "primary",   // Blue
                "success",   // Green
                "warning",   // Orange
                "danger",    // Red
                "purple",    // Purple
                "info",      // Cyan
                "orange",    // Orange
                "pink"       // Pink
            };
        }

        /// <summary>
        /// Get color by index (cycles through palette)
        /// </summary>
        private static string GetColorByIndex(int index)
        {
            var colors = GetDefaultColors();
            return colors[index % colors.Count];
        }

        /// <summary>
        /// Calculate percentage for display
        /// </summary>
        public static decimal CalculatePercentage(decimal value, decimal total)
        {
            if (total == 0) return 0;
            return Math.Round((value / total) * 100, 1);
        }
    }
}
