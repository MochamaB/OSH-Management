using OSHManagement.Models.ViewModels;
using OSHManagement.Models.ViewModels.Dashboard;

namespace OSHManagement.Extensions.Dashboard
{
    /// <summary>
    /// Extension methods for building Bar Chart components
    /// ALL LOGIC - NO RENDERING
    /// </summary>
    public static class BarChartExtensions
    {
        /// <summary>
        /// Build a bar chart widget
        /// </summary>
        public static BarChartViewModel BuildBarChart(
            string title,
            List<BarChartSeriesViewModel> series,
            List<string> categories,
            List<string>? colors = null,
            string? subtitle = null,
            bool horizontal = false,
            bool stacked = false,
            int height = 350)
        {
            return new BarChartViewModel
            {
                Title = title,
                Series = series,
                Categories = categories,
                Colors = colors ?? GetDefaultColors(),
                Subtitle = subtitle,
                Horizontal = horizontal,
                Stacked = stacked,
                Height = height
            };
        }

        /// <summary>
        /// Build single series bar chart
        /// </summary>
        public static BarChartViewModel BuildSingleSeriesBarChart(
            string title,
            string seriesName,
            Dictionary<string, decimal> data,
            string? color = null,
            string? subtitle = null,
            bool horizontal = false,
            int height = 350)
        {
            var series = new List<BarChartSeriesViewModel>
            {
                new BarChartSeriesViewModel
                {
                    Name = seriesName,
                    Data = data.Values.ToList()
                }
            };

            return new BarChartViewModel
            {
                Title = title,
                Series = series,
                Categories = data.Keys.ToList(),
                Colors = color != null ? new List<string> { color } : GetDefaultColors(),
                Subtitle = subtitle,
                Horizontal = horizontal,
                Height = height
            };
        }

        /// <summary>
        /// Build incidents by month bar chart (common use case)
        /// </summary>
        public static BarChartViewModel BuildIncidentsByMonthChart(
            Dictionary<string, int> monthlyIncidents,
            int currentYear)
        {
            var data = monthlyIncidents.ToDictionary(
                kvp => kvp.Key,
                kvp => (decimal)kvp.Value
            );

            return BuildSingleSeriesBarChart(
                title: $"Incidents by Month ({currentYear})",
                seriesName: "Incidents",
                data: data,
                color: "danger",
                subtitle: $"Total: {monthlyIncidents.Values.Sum()} incidents"
            );
        }

        /// <summary>
        /// Build incidents by department bar chart (common use case)
        /// </summary>
        public static BarChartViewModel BuildIncidentsByDepartmentChart(
            Dictionary<string, int> departmentIncidents)
        {
            var data = departmentIncidents
                .OrderByDescending(kvp => kvp.Value)
                .Take(10)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => (decimal)kvp.Value
                );

            return BuildSingleSeriesBarChart(
                title: "Incidents by Department",
                seriesName: "Incidents",
                data: data,
                color: "warning",
                subtitle: $"Total: {departmentIncidents.Values.Sum()} incidents",
                horizontal: true
            );
        }

        /// <summary>
        /// Build training completion by department (common use case)
        /// </summary>
        public static BarChartViewModel BuildTrainingCompletionChart(
            Dictionary<string, (int Completed, int Total)> departmentTraining)
        {
            var categories = departmentTraining.Keys.ToList();
            
            var completedSeries = new BarChartSeriesViewModel
            {
                Name = "Completed",
                Data = departmentTraining.Values.Select(v => (decimal)v.Completed).ToList()
            };

            var pendingSeries = new BarChartSeriesViewModel
            {
                Name = "Pending",
                Data = departmentTraining.Values.Select(v => (decimal)(v.Total - v.Completed)).ToList()
            };

            return BuildBarChart(
                title: "Training Completion by Department",
                series: new List<BarChartSeriesViewModel> { completedSeries, pendingSeries },
                categories: categories,
                colors: new List<string> { "success", "warning" },
                stacked: true,
                subtitle: "Stacked view of completed vs pending training"
            );
        }

        /// <summary>
        /// Build actions by status bar chart (common use case)
        /// </summary>
        public static BarChartViewModel BuildActionsByStatusChart(
            int completed,
            int inProgress,
            int pending,
            int overdue)
        {
            var data = new Dictionary<string, decimal>
            {
                { "Completed", completed },
                { "In Progress", inProgress },
                { "Pending", pending },
                { "Overdue", overdue }
            };

            var series = new List<BarChartSeriesViewModel>
            {
                new BarChartSeriesViewModel
                {
                    Name = "Actions",
                    Data = data.Values.ToList()
                }
            };

            var colors = new List<string> { "success", "info", "warning", "danger" };

            return new BarChartViewModel
            {
                Title = "Actions by Status",
                Series = series,
                Categories = data.Keys.ToList(),
                Colors = colors,
                Subtitle = $"Total: {completed + inProgress + pending + overdue} actions",
                Height = 300
            };
        }

        /// <summary>
        /// Build equipment by condition bar chart (common use case)
        /// </summary>
        public static BarChartViewModel BuildEquipmentByConditionChart(
            int excellent,
            int good,
            int fair,
            int poor)
        {
            var data = new Dictionary<string, decimal>
            {
                { "Excellent", excellent },
                { "Good", good },
                { "Fair", fair },
                { "Poor", poor }
            };

            return BuildSingleSeriesBarChart(
                title: "Equipment by Condition",
                seriesName: "Equipment",
                data: data,
                color: "info",
                subtitle: $"Total: {excellent + good + fair + poor} items"
            );
        }

        /// <summary>
        /// Build multi-series comparison chart
        /// </summary>
        public static BarChartViewModel BuildComparisonChart(
            string title,
            List<string> categories,
            Dictionary<string, List<decimal>> seriesData,
            List<string>? colors = null,
            string? subtitle = null)
        {
            var series = seriesData.Select(kvp => new BarChartSeriesViewModel
            {
                Name = kvp.Key,
                Data = kvp.Value
            }).ToList();

            return BuildBarChart(
                title: title,
                series: series,
                categories: categories,
                colors: colors,
                subtitle: subtitle
            );
        }

        /// <summary>
        /// Get default color palette
        /// </summary>
        private static List<string> GetDefaultColors()
        {
            return new List<string>
            {
                "primary",
                "success",
                "warning",
                "danger",
                "info",
                "purple",
                "orange",
                "teal"
            };
        }
    }
}
