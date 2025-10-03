using OSHManagement.Models.ViewModels;

namespace OSHManagement.Extensions
{
    public static class StatCardExtensions
    {
        /// <summary>
        /// Builds a list of StatCardViewModels from simple StatsRowConfig
        /// This keeps ALL logic out of the view
        /// </summary>
        public static List<StatCardViewModel> BuildStatsRow(this StatsRowConfig config)
        {
            var cards = new List<StatCardViewModel>();

            // Default color themes if not provided
            var defaultColorThemes = new[] { "primary", "secondary", "success", "warning" };

            for (int i = 0; i < config.Titles.Count; i++)
            {
                var card = new StatCardViewModel
                {
                    Title = config.Titles[i],
                    Value = config.Values[i],
                    Icon = config.Icons[i],
                    ColorTheme = config.ColorThemes != null && i < config.ColorThemes.Count
                        ? config.ColorThemes[i]
                        : defaultColorThemes[i % defaultColorThemes.Length],
                    ColumnClass = config.ColumnClass,
                    CardType = config.CardType
                };

                // Add badge if provided
                if (config.BadgeValues != null && i < config.BadgeValues.Count)
                {
                    card.BadgeValue = config.BadgeValues[i];
                    card.BadgeColor = card.ColorTheme; // Badge matches card theme
                }

                // Add trend if provided
                if (config.TrendPercentages != null && i < config.TrendPercentages.Count)
                {
                    card.TrendPercentage = config.TrendPercentages[i];

                    if (config.TrendDirections != null && i < config.TrendDirections.Count)
                    {
                        card.TrendDirection = config.TrendDirections[i];
                    }
                }

                cards.Add(card);
            }

            return cards;
        }
    }
}
