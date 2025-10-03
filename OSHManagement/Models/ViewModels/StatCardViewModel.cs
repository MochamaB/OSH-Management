namespace OSHManagement.Models.ViewModels
{
    /// <summary>
    /// Simple configuration - NO LOGIC, just data (like TableConfig)
    /// Define what stats to show, extension method handles the rest
    /// </summary>
    public class StatsRowConfig
    {
        public List<string> Titles { get; set; } = new List<string>();
        public List<string> Values { get; set; } = new List<string>();
        public List<string> Icons { get; set; } = new List<string>();
        public List<string>? ColorThemes { get; set; }
        public List<string>? BadgeValues { get; set; }
        public List<string>? TrendPercentages { get; set; }
        public List<TrendDirection>? TrendDirections { get; set; }

        public CardType CardType { get; set; } = CardType.LeftBorderCard;
        public string ColumnClass { get; set; } = "col-xxl-3 col-xl-6";
    }

    /// <summary>
    /// Internal ViewModel for rendering (created by extension method)
    /// </summary>
    public class StatCardViewModel
    {
        public string Title { get; set; } = "";
        public string Value { get; set; } = "";
        public string? BadgeValue { get; set; }
        public string? BadgeColor { get; set; }
        public string? TrendPercentage { get; set; }
        public TrendDirection? TrendDirection { get; set; }
        public string? TrendPeriod { get; set; } = "this month";
        public string ColorTheme { get; set; } = "primary";
        public string Icon { get; set; } = "ri-file-list-line";
        public string? CustomSvg { get; set; }
        public string? LinkUrl { get; set; }
        public string ColumnClass { get; set; } = "col-xxl-3 col-xl-6";
        public CardType CardType { get; set; } = CardType.LeftBorderCard;
    }

    public enum CardType
    {
        LeftBorderCard,      // Card with left border accent
        TopBorderCard,       // Card with top border accent
        NoBorderCard,        // Card without border
        BackgroundFillCard   // Card with filled background
    }

    public enum TrendDirection
    {
        Up,
        Down,
        Neutral
    }
}
