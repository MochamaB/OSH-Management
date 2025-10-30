using OSHManagement.Models.ViewModels;
using OSHManagement.Models.ViewModels.Dashboard;

namespace OSHManagement.Extensions.Dashboard
{
    /// <summary>
    /// Extension methods for building KPI Card widgets
    /// ALL LOGIC - NO RENDERING
    /// </summary>
    public static class KPICardExtensions
    {
        /// <summary>
        /// Build a single standard KPI card (Pattern A)
        /// </summary>
        public static KPICardViewModel BuildKPICard(
            string title,
            string value,
            string icon,
            string colorTheme = "primary",
            string? badge = null,
            string? trendValue = null,
            TrendDirection? trendDirection = null,
            string? linkUrl = null)
        {
            return new KPICardViewModel
            {
                Title = title,
                Value = value,
                Icon = icon,
                ColorTheme = colorTheme,
                Badge = badge,
                TrendValue = trendValue,
                TrendDirection = trendDirection,
                LinkUrl = linkUrl,
                CardType = KPICardType.Standard
            };
        }

        /// <summary>
        /// Build a KPI card with emphasized trend (Pattern B)
        /// </summary>
        public static KPICardViewModel BuildKPICardWithTrend(
            string title,
            string value,
            string icon,
            string trendValue,
            TrendDirection trendDirection,
            string colorTheme = "primary",
            string? trendLabel = null,
            string? linkUrl = null)
        {
            return new KPICardViewModel
            {
                Title = title,
                Value = value,
                Icon = icon,
                ColorTheme = colorTheme,
                TrendValue = trendValue,
                TrendDirection = trendDirection,
                TrendLabel = trendLabel ?? "This Month",
                LinkUrl = linkUrl,
                CardType = KPICardType.WithTrend
            };
        }

        /// <summary>
        /// Build a KPI card with sparkline chart (Pattern C)
        /// </summary>
        public static KPICardViewModel BuildKPICardWithSparkline(
            string title,
            string value,
            string icon,
            List<decimal> sparklineData,
            string colorTheme = "primary",
            string? subtitle = null,
            string? linkUrl = null)
        {
            return new KPICardViewModel
            {
                Title = title,
                Value = value,
                Icon = icon,
                ColorTheme = colorTheme,
                Subtitle = subtitle ?? "Increases Today",
                SparklineData = sparklineData,
                SparklineId = $"sparkline-{Guid.NewGuid():N}",
                SparklineColor = colorTheme,
                LinkUrl = linkUrl,
                CardType = KPICardType.WithSparkline
            };
        }

        /// <summary>
        /// Build a row of KPI cards (multiple cards at once)
        /// </summary>
        public static List<KPICardViewModel> BuildKPICardsRow(
            List<string> titles,
            List<string> values,
            List<string> icons,
            List<string>? colorThemes = null,
            List<string>? badges = null,
            List<string>? trendValues = null,
            List<TrendDirection?>? trendDirections = null,
            KPICardType cardType = KPICardType.Standard)
        {
            var cards = new List<KPICardViewModel>();
            var defaultColorThemes = new[] { "primary", "secondary", "success", "warning", "info", "danger" };

            for (int i = 0; i < titles.Count; i++)
            {
                var card = new KPICardViewModel
                {
                    Title = titles[i],
                    Value = values[i],
                    Icon = icons[i],
                    ColorTheme = colorThemes != null && i < colorThemes.Count 
                        ? colorThemes[i] 
                        : defaultColorThemes[i % defaultColorThemes.Length],
                    Badge = badges != null && i < badges.Count ? badges[i] : null,
                    TrendValue = trendValues != null && i < trendValues.Count ? trendValues[i] : null,
                    TrendDirection = trendDirections != null && i < trendDirections.Count ? trendDirections[i] : null,
                    CardType = cardType
                };
                cards.Add(card);
            }

            return cards;
        }

        /// <summary>
        /// Build KPI card from dynamic data (useful for service layer)
        /// </summary>
        public static KPICardViewModel BuildKPICardFromData(
            string title,
            decimal value,
            string icon,
            string colorTheme,
            decimal? comparisonValue = null,
            string? badgeText = null,
            string? valueFormat = "N0",
            string? linkUrl = null)
        {
            var formattedValue = value.ToString(valueFormat);
            string? trendValue = null;
            TrendDirection? trendDirection = null;

            // Calculate trend if comparison value provided
            if (comparisonValue.HasValue && comparisonValue.Value > 0)
            {
                var percentChange = ((value - comparisonValue.Value) / comparisonValue.Value) * 100;
                trendValue = $"{(percentChange >= 0 ? "+" : "")}{percentChange:F1}%";
                
                if (percentChange > 0)
                    trendDirection = TrendDirection.Up;
                else if (percentChange < 0)
                    trendDirection = TrendDirection.Down;
                else
                    trendDirection = TrendDirection.Neutral;
            }

            return new KPICardViewModel
            {
                Title = title,
                Value = formattedValue,
                Icon = icon,
                ColorTheme = colorTheme,
                Badge = badgeText,
                TrendValue = trendValue,
                TrendDirection = trendDirection,
                LinkUrl = linkUrl,
                CardType = KPICardType.Standard
            };
        }

        /// <summary>
        /// Calculate trend direction based on values
        /// Helper method for determining if increase is good or bad
        /// </summary>
        public static TrendDirection CalculateTrend(decimal current, decimal previous, bool higherIsBetter = true)
        {
            if (current > previous)
                return higherIsBetter ? TrendDirection.Up : TrendDirection.Down;
            else if (current < previous)
                return higherIsBetter ? TrendDirection.Down : TrendDirection.Up;
            else
                return TrendDirection.Neutral;
        }

        /// <summary>
        /// Format trend value as percentage
        /// </summary>
        public static string FormatTrendPercentage(decimal current, decimal previous)
        {
            if (previous == 0) return "N/A";
            
            var percentChange = ((current - previous) / previous) * 100;
            return $"{(percentChange >= 0 ? "+" : "")}{percentChange:F1}%";
        }

        /// <summary>
        /// Get appropriate icon based on metric type
        /// </summary>
        public static string GetMetricIcon(string metricType)
        {
            return metricType.ToLower() switch
            {
                "incident" or "incidents" => "ri-alert-line",
                "employee" or "employees" => "ri-group-3-fill",
                "training" => "ri-book-open-line",
                "compliance" => "ri-shield-check-line",
                "hazard" or "hazards" => "ri-error-warning-line",
                "risk" => "ri-alert-fill",
                "action" or "actions" => "ri-todo-line",
                "inspection" or "inspections" => "ri-search-eye-line",
                "audit" or "audits" => "ri-file-list-3-line",
                "equipment" => "ri-tools-line",
                "ppe" => "ri-shield-user-line",
                "emergency" => "ri-alarm-warning-line",
                "meeting" or "meetings" => "ri-team-line",
                "document" or "documents" => "ri-file-text-line",
                _ => "ri-bar-chart-box-line"
            };
        }

        /// <summary>
        /// Get color theme based on metric type
        /// </summary>
        public static string GetMetricColorTheme(string metricType)
        {
            return metricType.ToLower() switch
            {
                "incident" or "incidents" => "danger",
                "employee" or "employees" => "primary",
                "training" => "info",
                "compliance" => "success",
                "hazard" or "hazards" => "warning",
                "risk" => "danger",
                "action" or "actions" => "secondary",
                "inspection" or "inspections" => "info",
                "audit" or "audits" => "primary",
                "equipment" => "secondary",
                "ppe" => "success",
                "emergency" => "danger",
                _ => "primary"
            };
        }
    }
}
