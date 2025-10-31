using OSHManagement.Models.ViewModels;
using OSHManagement.Models.ViewModels.Dashboard;

namespace OSHManagement.Extensions.Dashboard
{
    /// <summary>
    /// Extension methods for building Progress Widget components
    /// ALL LOGIC - NO RENDERING
    /// </summary>
    public static class ProgressWidgetExtensions
    {
        /// <summary>
        /// Build a standard progress widget
        /// </summary>
        public static ProgressWidgetViewModel BuildProgressWidget(
            string title,
            int currentValue,
            int totalValue,
            string? description = null,
            string colorClass = "primary",
            bool striped = false,
            bool animated = false,
            string? linkUrl = null)
        {
            return new ProgressWidgetViewModel
            {
                Title = title,
                CurrentValue = currentValue,
                TotalValue = totalValue,
                Description = description,
                ColorClass = colorClass,
                Striped = striped,
                Animated = animated,
                LinkUrl = linkUrl,
                WidgetType = ProgressWidgetType.Standard
            };
        }

        /// <summary>
        /// Build a progress widget with icon
        /// </summary>
        public static ProgressWidgetViewModel BuildProgressWidgetWithIcon(
            string title,
            int currentValue,
            int totalValue,
            string icon,
            string? description = null,
            string colorClass = "primary",
            string? linkUrl = null)
        {
            return new ProgressWidgetViewModel
            {
                Title = title,
                CurrentValue = currentValue,
                TotalValue = totalValue,
                Icon = icon,
                Description = description,
                ColorClass = colorClass,
                LinkUrl = linkUrl,
                WidgetType = ProgressWidgetType.WithIcon
            };
        }

        /// <summary>
        /// Build a progress widget with threshold-based coloring
        /// Automatically changes color based on completion percentage
        /// </summary>
        public static ProgressWidgetViewModel BuildProgressWidgetWithThresholds(
            string title,
            int currentValue,
            int totalValue,
            List<ProgressThreshold> thresholds,
            string? description = null,
            bool striped = false,
            string? linkUrl = null)
        {
            return new ProgressWidgetViewModel
            {
                Title = title,
                CurrentValue = currentValue,
                TotalValue = totalValue,
                Description = description,
                Thresholds = thresholds,
                Striped = striped,
                LinkUrl = linkUrl,
                WidgetType = ProgressWidgetType.Standard
            };
        }

        /// <summary>
        /// Build multiple progress widgets at once
        /// </summary>
        public static List<ProgressWidgetViewModel> BuildProgressWidgets(
            List<string> titles,
            List<int> currentValues,
            List<int> totalValues,
            List<string>? colorClasses = null,
            List<string>? descriptions = null,
            bool striped = false,
            bool animated = false)
        {
            var widgets = new List<ProgressWidgetViewModel>();
            var defaultColors = new[] { "primary", "success", "info", "warning", "danger" };

            for (int i = 0; i < titles.Count; i++)
            {
                var widget = new ProgressWidgetViewModel
                {
                    Title = titles[i],
                    CurrentValue = currentValues[i],
                    TotalValue = totalValues[i],
                    Description = descriptions != null && i < descriptions.Count ? descriptions[i] : null,
                    ColorClass = colorClasses != null && i < colorClasses.Count 
                        ? colorClasses[i] 
                        : defaultColors[i % defaultColors.Length],
                    Striped = striped,
                    Animated = animated,
                    WidgetType = ProgressWidgetType.Standard
                };
                widgets.Add(widget);
            }

            return widgets;
        }

        /// <summary>
        /// Build progress widget from percentage (when you only have percentage, not actual values)
        /// </summary>
        public static ProgressWidgetViewModel BuildProgressWidgetFromPercentage(
            string title,
            decimal percentage,
            string? description = null,
            string colorClass = "primary",
            string? linkUrl = null)
        {
            // Convert percentage to whole numbers (e.g., 75.5% becomes 755/1000)
            var currentValue = (int)(percentage * 10);
            var totalValue = 1000;

            return new ProgressWidgetViewModel
            {
                Title = title,
                CurrentValue = currentValue,
                TotalValue = totalValue,
                Description = description,
                ColorClass = colorClass,
                LinkUrl = linkUrl,
                WidgetType = ProgressWidgetType.Standard
            };
        }

        /// <summary>
        /// Get default thresholds for common metrics (e.g., compliance, training completion)
        /// </summary>
        public static List<ProgressThreshold> GetDefaultThresholds()
        {
            return new List<ProgressThreshold>
            {
                new ProgressThreshold { MinValue = 0, MaxValue = 50, ColorClass = "danger", Label = "Poor" },
                new ProgressThreshold { MinValue = 50, MaxValue = 80, ColorClass = "warning", Label = "Fair" },
                new ProgressThreshold { MinValue = 80, MaxValue = 100, ColorClass = "success", Label = "Good" }
            };
        }

        /// <summary>
        /// Get strict compliance thresholds (higher standards)
        /// </summary>
        public static List<ProgressThreshold> GetStrictThresholds()
        {
            return new List<ProgressThreshold>
            {
                new ProgressThreshold { MinValue = 0, MaxValue = 70, ColorClass = "danger", Label = "Non-Compliant" },
                new ProgressThreshold { MinValue = 70, MaxValue = 90, ColorClass = "warning", Label = "Partial" },
                new ProgressThreshold { MinValue = 90, MaxValue = 100, ColorClass = "success", Label = "Compliant" }
            };
        }

        /// <summary>
        /// Get lenient thresholds (lower standards)
        /// </summary>
        public static List<ProgressThreshold> GetLenientThresholds()
        {
            return new List<ProgressThreshold>
            {
                new ProgressThreshold { MinValue = 0, MaxValue = 30, ColorClass = "danger", Label = "Low" },
                new ProgressThreshold { MinValue = 30, MaxValue = 60, ColorClass = "warning", Label = "Medium" },
                new ProgressThreshold { MinValue = 60, MaxValue = 100, ColorClass = "success", Label = "High" }
            };
        }

        /// <summary>
        /// Calculate color class based on percentage and metric type
        /// </summary>
        public static string GetColorForPercentage(decimal percentage, string metricType = "default")
        {
            return metricType.ToLower() switch
            {
                "compliance" or "safety" => percentage switch
                {
                    >= 90 => "success",
                    >= 70 => "warning",
                    _ => "danger"
                },
                "incident" or "hazard" => percentage switch // Lower is better
                {
                    <= 20 => "success",
                    <= 50 => "warning",
                    _ => "danger"
                },
                _ => percentage switch // Default thresholds
                {
                    >= 80 => "success",
                    >= 50 => "warning",
                    _ => "danger"
                }
            };
        }

        /// <summary>
        /// Get appropriate icon based on progress type
        /// </summary>
        public static string GetProgressIcon(string progressType)
        {
            return progressType.ToLower() switch
            {
                "training" => "ri-book-open-line",
                "compliance" => "ri-shield-check-line",
                "completion" => "ri-checkbox-circle-line",
                "action" or "actions" => "ri-todo-line",
                "inspection" => "ri-search-eye-line",
                "audit" => "ri-file-list-3-line",
                "certification" => "ri-award-line",
                "ppe" => "ri-shield-user-line",
                "equipment" => "ri-tools-line",
                "incident" => "ri-alert-line",
                _ => "ri-percent-line"
            };
        }

        /// <summary>
        /// Build training completion widget (common use case)
        /// </summary>
        public static ProgressWidgetViewModel BuildTrainingProgressWidget(
            int completed,
            int total,
            string? courseName = null)
        {
            var title = string.IsNullOrEmpty(courseName) 
                ? "Training Completion" 
                : $"{courseName} Progress";

            return BuildProgressWidgetWithThresholds(
                title: title,
                currentValue: completed,
                totalValue: total,
                thresholds: GetStrictThresholds(),
                description: $"{completed} of {total} employees completed",
                striped: true
            );
        }

        /// <summary>
        /// Build compliance widget (common use case)
        /// </summary>
        public static ProgressWidgetViewModel BuildComplianceProgressWidget(
            string complianceName,
            int compliantCount,
            int totalCount)
        {
            return BuildProgressWidgetWithIcon(
                title: $"{complianceName} Compliance",
                currentValue: compliantCount,
                totalValue: totalCount,
                icon: "ri-shield-check-line",
                description: $"{compliantCount} of {totalCount} compliant",
                colorClass: GetColorForPercentage(
                    (decimal)compliantCount / totalCount * 100, 
                    "compliance"
                )
            );
        }

        /// <summary>
        /// Build action closure rate widget (common use case)
        /// </summary>
        public static ProgressWidgetViewModel BuildActionClosureWidget(
            int closedActions,
            int totalActions)
        {
            return BuildProgressWidgetWithThresholds(
                title: "Action Closure Rate",
                currentValue: closedActions,
                totalValue: totalActions,
                thresholds: GetDefaultThresholds(),
                description: $"{closedActions} of {totalActions} actions closed",
                striped: true
            );
        }
    }
}
