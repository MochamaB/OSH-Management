namespace OSHManagement.Models.ViewModels
{
    /// <summary>
    /// Simple, generic table configuration - no business logic, just data
    /// </summary>
    public class TableConfig
    {
        public string TableId { get; set; } = "dataTable";
        public string ActionUrl { get; set; } = "";
        public List<string> Columns { get; set; } = new List<string>();

        // Search
        public string? SearchPlaceholder { get; set; }
        public string? SearchValue { get; set; }

        // Filters - completely generic
        public List<FilterConfig> Filters { get; set; } = new List<FilterConfig>();

        // Actions
        public string? CreateButtonText { get; set; }
        public string? CreateButtonUrl { get; set; }
        public List<HeaderActionConfig> HeaderActions { get; set; } = new List<HeaderActionConfig>();
    }

    /// <summary>
    /// Generic filter configuration - works for any type of filter
    /// </summary>
    public class FilterConfig
    {
        public string Label { get; set; } = "";
        public string ParameterName { get; set; } = "";
        public string? CurrentValue { get; set; }
        public List<FilterOptionConfig> Options { get; set; } = new List<FilterOptionConfig>();
        public FilterType Type { get; set; } = FilterType.Dropdown;
    }

    public class FilterOptionConfig
    {
        public string Text { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public enum FilterType
    {
        Dropdown,   // Simple click to navigate
        Select      // Form select dropdown (for entities like Department, Station)
    }

    public class HeaderActionConfig
    {
        public string Text { get; set; } = "";
        public string Url { get; set; } = "";
        public string IconClass { get; set; } = "";
        public string ColorClass { get; set; } = "secondary";
    }

    /// <summary>
    /// Generic row action configuration
    /// </summary>
    public class RowActionConfig
    {
        public bool ShowView { get; set; } = true;
        public string? ViewUrl { get; set; }

        public bool ShowEdit { get; set; } = true;
        public string? EditUrl { get; set; }

        public bool ShowDelete { get; set; } = true;
        public string? DeleteUrl { get; set; }
        public string? DeleteItemName { get; set; }

        public List<CustomRowAction> CustomActions { get; set; } = new List<CustomRowAction>();
    }

    public class CustomRowAction
    {
        public string Text { get; set; } = "";
        public string Url { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Color { get; set; } = "secondary";
    }
}
