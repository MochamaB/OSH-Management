using Microsoft.AspNetCore.Mvc.Rendering;

namespace OSHManagement.Models.ViewModels
{
    public class DataTableViewModel
    {
        public string Title { get; set; } = "Data Table";
        public string TableId { get; set; } = "dataTable";

        // Search Component
        public SearchBoxViewModel? SearchBox { get; set; }

        // Filter Components (can have multiple)
        public List<FilterDropdownViewModel> FilterDropdowns { get; set; } = new List<FilterDropdownViewModel>();
        public List<FilterSelectViewModel> FilterSelects { get; set; } = new List<FilterSelectViewModel>();

        // Action Buttons
        public string CreateButtonText { get; set; } = "";
        public string CreateButtonUrl { get; set; } = "";
        public List<HeaderAction> HeaderActions { get; set; } = new List<HeaderAction>();

        // Table Structure
        public List<string> Columns { get; set; } = new List<string>();
        public Func<object, Microsoft.AspNetCore.Mvc.Razor.HelperResult> TableContent { get; set; }

        // Pagination
        public bool ShowPagination { get; set; } = false;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalRecords { get; set; } = 0;
        public int PageSize { get; set; } = 10;
    }

    public class HeaderAction
    {
        public string Text { get; set; } = "";
        public string Url { get; set; } = "";
        public string IconClass { get; set; } = "";
        public string ColorClass { get; set; } = "primary";
    }
}
