namespace OSHManagement.Models.ViewModels
{
    /// <summary>
    /// Simple form configuration - NO LOGIC, just data (like TableConfig)
    /// Define form fields, extension method handles the rest
    /// </summary>
    public class FormConfig
    {
        public string FormId { get; set; } = "mainForm";
        public string ActionUrl { get; set; } = "";
        public string Method { get; set; } = "POST";
        public FormType FormType { get; set; } = FormType.VerticalForm;

        // Fields configuration
        public List<FormFieldConfig> Fields { get; set; } = new List<FormFieldConfig>();

        // Horizontal form options
        public int LabelColumnWidth { get; set; } = 2;  // col-sm-2 for labels
        public int InputColumnWidth { get; set; } = 10; // col-sm-10 for inputs

        // Layout options for multi-column forms
        public int FieldsPerRow { get; set; } = 1; // 1 = single column, 2 = two columns, etc.
        public string FieldColumnClass { get; set; } = "col-md-12"; // For single column width (e.g., col-md-8 to center)
        public List<string>? FieldColumnClasses { get; set; } // For multi-column: ["col-md-8", "col-md-4"]

        // Buttons
        public string SubmitButtonText { get; set; } = "Submit";
        public string SubmitButtonClass { get; set; } = "btn btn-primary";
        public string? CancelButtonText { get; set; } = "Cancel";
        public string? CancelUrl { get; set; }

        // Card wrapper
        public bool WrapInCard { get; set; } = true;
        public string? CardTitle { get; set; }
    }

    /// <summary>
    /// Individual field configuration
    /// </summary>
    public class FormFieldConfig
    {
        public string Name { get; set; } = "";
        public string PropertyName { get; set; } = ""; // For binding to ViewModel properties
        public string Label { get; set; } = "";
        public FieldType Type { get; set; } = FieldType.Text;
        public string? Placeholder { get; set; }
        public string? Value { get; set; }
        public string? DefaultValue { get; set; }
        public bool Required { get; set; } = false;
        public bool Disabled { get; set; } = false;
        public bool ReadOnly { get; set; } = false;

        // For Textarea
        public int Rows { get; set; } = 3;

        // For Select/Dropdown
        public List<FormSelectOption>? Options { get; set; }

        // Validation
        public string? ValidationMessage { get; set; }
        public int? MaxLength { get; set; }
        public int? MinLength { get; set; }

        // Custom attributes
        public Dictionary<string, string>? CustomAttributes { get; set; }

        // CSS
        public string? CustomCssClass { get; set; }

        // Help text
        public string? HelpText { get; set; }
    }

    /// <summary>
    /// Internal ViewModel for rendering (created by extension method)
    /// </summary>
    public class FormViewModel
    {
        public string FormId { get; set; } = "";
        public string ActionUrl { get; set; } = "";
        public string Method { get; set; } = "POST";
        public FormType FormType { get; set; } = FormType.VerticalForm;
        public List<FormFieldViewModel> Fields { get; set; } = new List<FormFieldViewModel>();
        public int LabelColumnWidth { get; set; } = 2;
        public int InputColumnWidth { get; set; } = 10;
        public int FieldsPerRow { get; set; } = 1;
        public string FieldColumnClass { get; set; } = "col-md-12";
        public List<string>? FieldColumnClasses { get; set; }
        public string SubmitButtonText { get; set; } = "Submit";
        public string SubmitButtonClass { get; set; } = "btn btn-primary";
        public string? CancelButtonText { get; set; }
        public string? CancelUrl { get; set; }
        public bool WrapInCard { get; set; } = true;
        public string? CardTitle { get; set; }
    }

    public class FormFieldViewModel
    {
        public string Name { get; set; } = "";
        public string PropertyName { get; set; } = "";
        public string Label { get; set; } = "";
        public FieldType Type { get; set; } = FieldType.Text;
        public string? Placeholder { get; set; }
        public string? Value { get; set; }
        public string? DefaultValue { get; set; }
        public bool Required { get; set; } = false;
        public bool Disabled { get; set; } = false;
        public bool ReadOnly { get; set; } = false;
        public int Rows { get; set; } = 3;
        public List<FormSelectOption>? Options { get; set; }
        public string? ValidationMessage { get; set; }
        public int? MaxLength { get; set; }
        public int? MinLength { get; set; }
        public Dictionary<string, string>? CustomAttributes { get; set; }
        public string? CustomCssClass { get; set; }
        public string? HelpText { get; set; }
        public string UniqueId { get; set; } = ""; // Generated unique ID for field
    }

    public enum FormType
    {
        VerticalForm,        // Standard vertical layout
        HorizontalForm,      // Labels on left, inputs on right
        FloatingLabelForm,   // Floating labels (mobile-friendly)
        InlineForm          // Compact inline layout
    }

    public enum FieldType
    {
        Text,
        Email,
        Password,
        Number,
        Tel,
        Url,
        Textarea,
        Select,
        Checkbox,
        Radio,
        Date,
        DateTime,
        Time,
        File,
        Hidden,
        Color
    }

    /// <summary>
    /// Select option for form dropdowns (separate from table filter SelectOption)
    /// </summary>
    public class FormSelectOption
    {
        public string Text { get; set; } = "";
        public string Value { get; set; } = "";
        public bool Selected { get; set; } = false;
        public bool Disabled { get; set; } = false;
    }
}
