namespace OSHManagement.Models.ViewModels
{
    /// <summary>
    /// Simple bulk add configuration - NO LOGIC, just data
    /// Define what to bulk add, extension method handles the rest
    /// </summary>
    public class BulkAddConfig
    {
        public string ComponentId { get; set; } = "bulkAdd";
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? Icon { get; set; }
        
        // Entity configuration
        public string EntityName { get; set; } = "Item";
        public string EntityNamePlural { get; set; } = "Items";
        public int InitialRows { get; set; } = 3;
        public int MinRows { get; set; } = 1;
        public int MaxRows { get; set; } = 50;
        
        // Parent selector (optional - e.g., Station for Sections)
        [Obsolete("Use ParentSelectors instead for multiple selectors")]
        public ParentSelectorConfig? ParentSelector { get; set; }
        
        // Multiple parent selectors (e.g., Category + Station)
        public List<ParentSelectorConfig>? ParentSelectors { get; set; }
        
        // Fields in each row
        public List<BulkAddFieldConfig> Fields { get; set; } = new List<BulkAddFieldConfig>();
        
        // Form submission
        public string ActionUrl { get; set; } = "";
        public string Method { get; set; } = "POST";
        public string SubmitButtonText { get; set; } = "Save All";
        public string? CancelUrl { get; set; }
        
        // Card wrapper
        public bool WrapInCard { get; set; } = true;
    }
    
    /// <summary>
    /// Parent selector configuration (e.g., Select Station before adding sections)
    /// </summary>
    public class ParentSelectorConfig
    {
        public string Label { get; set; } = "";
        public string ParameterName { get; set; } = "";
        public bool Required { get; set; } = true;
        public string? Placeholder { get; set; }
        public List<FormSelectOption> Options { get; set; } = new List<FormSelectOption>();
        public string? HelpText { get; set; }
        public bool FiltersRowFields { get; set; } = false; // If true, filters dropdowns in rows
        public string ColumnClass { get; set; } = "col-md-6"; // For side-by-side layout
        public bool FiltersOtherSelector { get; set; } = false; // If true, filters another parent selector
        public string? FilterTargetParameter { get; set; } // Parameter name of selector to filter
    }
    
    /// <summary>
    /// Field configuration for each row
    /// </summary>
    public class BulkAddFieldConfig
    {
        public string Name { get; set; } = "";
        public string Label { get; set; } = "";
        public FieldType Type { get; set; } = FieldType.Text;
        public bool Required { get; set; } = false;
        public string? Placeholder { get; set; }
        public int? MaxLength { get; set; }
        public List<FormSelectOption>? Options { get; set; }
        public string? ValidationMessage { get; set; }
        public string? HelpText { get; set; }
        public string ColumnClass { get; set; } = "col-md-6";
        public int DisplayOrder { get; set; } = 0;
        
        // For filtering based on parent
        public bool FilterByParent { get; set; } = false;
        public string? FilterPropertyName { get; set; } // e.g., "stationId"
    }
    
    /// <summary>
    /// Internal ViewModel for rendering (created by extension method)
    /// </summary>
    public class BulkAddViewModel
    {
        public string ComponentId { get; set; } = "";
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string EntityName { get; set; } = "";
        public string EntityNamePlural { get; set; } = "";
        public int InitialRows { get; set; } = 3;
        public int MinRows { get; set; } = 1;
        public int MaxRows { get; set; } = 50;
        public ParentSelectorConfig? ParentSelector { get; set; }
        public List<ParentSelectorConfig>? ParentSelectors { get; set; }
        public List<BulkAddFieldViewModel> Fields { get; set; } = new List<BulkAddFieldViewModel>();
        public string ActionUrl { get; set; } = "";
        public string Method { get; set; } = "POST";
        public string SubmitButtonText { get; set; } = "";
        public string? CancelUrl { get; set; }
        public bool WrapInCard { get; set; } = true;
    }
    
    public class BulkAddFieldViewModel
    {
        public string Name { get; set; } = "";
        public string Label { get; set; } = "";
        public FieldType Type { get; set; }
        public bool Required { get; set; }
        public string? Placeholder { get; set; }
        public int? MaxLength { get; set; }
        public List<FormSelectOption>? Options { get; set; }
        public string? ValidationMessage { get; set; }
        public string? HelpText { get; set; }
        public string ColumnClass { get; set; } = "";
        public bool FilterByParent { get; set; }
        public string? FilterPropertyName { get; set; }
        public string UniqueIdTemplate { get; set; } = ""; // e.g., "bulkAddSections_{index}_sectionName"
    }
}
