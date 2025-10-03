namespace OSHManagement.Models.ViewModels
{
    /// <summary>
    /// Simple dropdown filter that navigates to URLs (for simple status filters)
    /// </summary>
    public class FilterDropdownViewModel
    {
        public string Label { get; set; } = "Filter";
        public List<FilterDropdownOption> Options { get; set; } = new List<FilterDropdownOption>();
    }

    public class FilterDropdownOption
    {
        public string Text { get; set; } = "";
        public string Url { get; set; } = "";
        public bool IsActive { get; set; } = false;
    }

    /// <summary>
    /// Select dropdown with form submission (for complex filters like Department, Station)
    /// </summary>
    public class FilterSelectViewModel
    {
        public string FormId { get; set; } = "filterForm";
        public string ActionUrl { get; set; } = "";
        public string ParameterName { get; set; } = "filterId";
        public string PlaceholderText { get; set; } = "All";
        public List<SelectOption> Options { get; set; } = new List<SelectOption>();
        public Dictionary<string, string> PreserveQueryParams { get; set; } = new Dictionary<string, string>();
    }

    public class SelectOption
    {
        public string Text { get; set; } = "";
        public string Value { get; set; } = "";
        public bool IsSelected { get; set; } = false;
    }

    /// <summary>
    /// Search box with optional submit button
    /// </summary>
    public class SearchBoxViewModel
    {
        public string InputId { get; set; } = "searchInput";
        public string ActionUrl { get; set; } = "";
        public string ParameterName { get; set; } = "search";
        public string PlaceholderText { get; set; } = "Search...";
        public string CurrentValue { get; set; } = "";
        public bool ShowButton { get; set; } = true;
        public Dictionary<string, string> PreserveQueryParams { get; set; } = new Dictionary<string, string>();
    }
}
