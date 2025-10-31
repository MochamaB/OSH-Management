using OSHManagement.Models.ViewModels;
using OSHManagement.Models.ViewModels.Dashboard;

namespace OSHManagement.Extensions.Dashboard
{
    /// <summary>
    /// Extension methods for building Line Chart components
    /// ALL LOGIC - NO RENDERING
    /// </summary>
    public static class LineChartExtensions
    {
        /// <summary>
        /// Build a line chart widget
        /// </summary>
        public static LineChartViewModel BuildLineChart(
            string title,
            List<LineChartSeriesViewModel> series,
            List<string> categories,
            List<string>? colors = null,
            string? subtitle = null,
            bool smooth = true,
            bool showArea = false,
            int height = 350)
        {
            return new LineChartViewModel
            {
                Title = title,
                Series = series,
                Categories = categories,
                Colors = colors ?? GetDefaultColors(),
                Subtitle = subtitle,
                Smooth = smooth,
                ShowArea = showArea,
                Height = height
            };
        }

        /// <summary>
        /// Build single series line chart
        /// </summary>
        public static LineChartViewModel BuildSingleSeriesLineChart(
            string title,
            string seriesName,
            Dictionary<string, decimal> data,
            string? color = null,
            string? subtitle = null,
            bool smooth = true,
            bool showArea = false,
            int height = 350)
        {
            var series = new List<LineChartSeriesViewModel>
            {
                new LineChartSeriesViewModel
                {
                    Name = seriesName,
                    Data = data.Values.ToList()
                }
            };

            return new LineChartViewModel
            {
                Title = title,
                Series = series,
                Categories = data.Keys.ToList(),
                Colors = color != null ? new List<string> { color } : GetDefaultColors(),
                Subtitle = subtitle,
                Smooth = smooth,
                ShowArea = showArea,
                Height = height
            };
        }

        /// <summary>
        /// Build incident trend line chart (common use case)
        /// </summary>
        public static LineChartViewModel BuildIncidentTrendChart(
            Dictionary<string, int> monthlyData,
            int year)
        {
            var data = monthlyData.ToDictionary(
                kvp => kvp.Key,
                kvp => (decimal)kvp.Value
            );

            return BuildSingleSeriesLineChart(
                title: $"Incident Trend ({year})",
                seriesName: "Incidents",
                data: data,
                color: "danger",
                subtitle: $"Total: {monthlyData.Values.Sum()} incidents",
                smooth: true,
                showArea: true
            );
        }

        /// <summary>
        /// Build training completion trend (common use case)
        /// </summary>
        public static LineChartViewModel BuildTrainingCompletionTrendChart(
            Dictionary<string, int> monthlyCompletions,
            int year)
        {
            var data = monthlyCompletions.ToDictionary(
                kvp => kvp.Key,
                kvp => (decimal)kvp.Value
            );

            return BuildSingleSeriesLineChart(
                title: $"Training Completion Trend ({year})",
                seriesName: "Trainings Completed",
                data: data,
                color: "success",
                subtitle: $"Total: {monthlyCompletions.Values.Sum()} completed",
                smooth: true,
                showArea: true
            );
        }

        /// <summary>
        /// Build multi-metric trend chart (common use case)
        /// </summary>
        public static LineChartViewModel BuildMultiMetricTrendChart(
            string title,
            List<string> months,
            Dictionary<string, List<int>> metricsData,
            List<string>? colors = null,
            string? subtitle = null)
        {
            var series = metricsData.Select(kvp => new LineChartSeriesViewModel
            {
                Name = kvp.Key,
                Data = kvp.Value.Select(v => (decimal)v).ToList()
            }).ToList();

            return BuildLineChart(
                title: title,
                series: series,
                categories: months,
                colors: colors,
                subtitle: subtitle,
                smooth: true
            );
        }

        /// <summary>
        /// Build incidents vs actions chart (common use case)
        /// </summary>
        public static LineChartViewModel BuildIncidentsVsActionsChart(
            Dictionary<string, (int Incidents, int Actions)> monthlyData,
            int year)
        {
            var categories = monthlyData.Keys.ToList();

            var incidentsSeries = new LineChartSeriesViewModel
            {
                Name = "Incidents",
                Data = monthlyData.Values.Select(v => (decimal)v.Incidents).ToList()
            };

            var actionsSeries = new LineChartSeriesViewModel
            {
                Name = "Actions",
                Data = monthlyData.Values.Select(v => (decimal)v.Actions).ToList()
            };

            return BuildLineChart(
                title: $"Incidents vs Actions ({year})",
                series: new List<LineChartSeriesViewModel> { incidentsSeries, actionsSeries },
                categories: categories,
                colors: new List<string> { "danger", "success" },
                subtitle: "Comparison of incidents reported and actions completed",
                smooth: true
            );
        }

        /// <summary>
        /// Build compliance rate trend (common use case)
        /// </summary>
        public static LineChartViewModel BuildComplianceRateTrendChart(
            Dictionary<string, decimal> monthlyRates,
            int year)
        {
            return BuildSingleSeriesLineChart(
                title: $"Compliance Rate Trend ({year})",
                seriesName: "Compliance %",
                data: monthlyRates,
                color: "info",
                subtitle: $"Average: {monthlyRates.Values.Average():F1}%",
                smooth: true,
                showArea: false
            );
        }

        /// <summary>
        /// Build year-over-year comparison chart
        /// </summary>
        public static LineChartViewModel BuildYearOverYearComparisonChart(
            string title,
            List<string> months,
            Dictionary<string, List<decimal>> yearlyData,
            string? subtitle = null)
        {
            var series = yearlyData.Select(kvp => new LineChartSeriesViewModel
            {
                Name = kvp.Key,
                Data = kvp.Value
            }).ToList();

            return BuildLineChart(
                title: title,
                series: series,
                categories: months,
                subtitle: subtitle,
                smooth: true
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

        /// <summary>
        /// Get last N months names
        /// </summary>
        public static List<string> GetLastMonths(int count)
        {
            var months = new List<string>();
            var currentDate = DateTime.Now;

            for (int i = count - 1; i >= 0; i--)
            {
                months.Add(currentDate.AddMonths(-i).ToString("MMM"));
            }

            return months;
        }

        /// <summary>
        /// Get last 12 months names
        /// </summary>
        public static List<string> GetLast12Months()
        {
            return GetLastMonths(12);
        }
    }
}
