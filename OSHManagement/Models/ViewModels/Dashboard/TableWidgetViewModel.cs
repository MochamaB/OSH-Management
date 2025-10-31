using OSHManagement.Models.ViewModels;

namespace OSHManagement.Models.ViewModels.Dashboard
{
    /// <summary>
    /// ViewModel for Table Widget - displays tabular data
    /// Shows data in rows and columns with optional sorting, badges, and actions
    /// </summary>
    public class TableWidgetViewModel
    {
        // Basic Properties
        public string Title { get; set; } = string.Empty;
        public List<TableColumnViewModel> Columns { get; set; } = new List<TableColumnViewModel>();
        public List<TableRowViewModel> Rows { get; set; } = new List<TableRowViewModel>();

        // Optional Properties
        public string? ViewAllUrl { get; set; }
        public string? ViewAllText { get; set; } = "View All";
        public string? EmptyMessage { get; set; } = "No data to display";
        public string? Icon { get; set; } // Widget header icon

        // Display Options
        public bool ShowHeader { get; set; } = true;
        public bool Striped { get; set; } = true;
        public bool Bordered { get; set; } = false;
        public bool Hoverable { get; set; } = true;
        public bool Compact { get; set; } = false;
        public int MaxRows { get; set; } = 10;

        // Layout Properties
        public string ColumnClass { get; set; } = "col-12";
        public TableWidgetType WidgetType { get; set; } = TableWidgetType.Standard;

        // Helper Property
        public bool HasData => Rows != null && Rows.Any();
        
        public string TableClasses
        {
            get
            {
                var classes = new List<string> { "table", "text-nowrap" };
                
                if (Striped)
                    classes.Add("table-striped");
                
                if (Bordered)
                    classes.Add("table-bordered");
                
                if (Hoverable)
                    classes.Add("table-hover");
                
                if (Compact)
                    classes.Add("table-sm");

                return string.Join(" ", classes);
            }
        }
    }

    /// <summary>
    /// Table column definition
    /// </summary>
    public class TableColumnViewModel
    {
        public string Header { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string? CssClass { get; set; }
        public bool Sortable { get; set; } = false;
        public ColumnType Type { get; set; } = ColumnType.Text;
        public int? Width { get; set; } // Width percentage or pixels
    }

    /// <summary>
    /// Table row data
    /// </summary>
    public class TableRowViewModel
    {
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public string? LinkUrl { get; set; }
        public string? RowCssClass { get; set; }
        public string? Id { get; set; } // Row identifier
    }

    /// <summary>
    /// Column data types
    /// </summary>
    public enum ColumnType
    {
        Text,           // Plain text
        Number,         // Numeric value (right-aligned)
        Badge,          // Badge/status
        Icon,           // Icon display
        Date,           // Date format
        DateTime,       // DateTime format
        Currency,       // Currency format
        Percentage,     // Percentage format
        Link,           // Hyperlink
        Action          // Action buttons
    }

    /// <summary>
    /// Table Widget display variants
    /// </summary>
    public enum TableWidgetType
    {
        Standard,       // Standard table
        Compact,        // Minimal spacing
        Detailed        // With more information per row
    }

    /// <summary>
    /// Badge/Status data for table cells
    /// </summary>
    public class TableBadgeData
    {
        public string Text { get; set; } = string.Empty;
        public string ColorClass { get; set; } = "primary";
    }

    /// <summary>
    /// Icon data for table cells
    /// </summary>
    public class TableIconData
    {
        public string Icon { get; set; } = string.Empty;
        public string ColorClass { get; set; } = "primary";
        public string? Tooltip { get; set; }
    }
}
