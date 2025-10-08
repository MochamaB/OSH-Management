namespace OSHManagement.Models.ViewModels
{
    /// <summary>
    /// Configuration for multi-step form wizard - NO LOGIC, just data
    /// Follows same pattern as TableConfig and FormConfig
    /// </summary>
    public class FormWizardConfig
    {
        public string WizardId { get; set; } = "formWizard";
        public string ActionUrl { get; set; } = "";
        public string Method { get; set; } = "POST";
        public WizardType Type { get; set; } = WizardType.Horizontal;

        // Steps configuration
        public List<WizardStepConfig> Steps { get; set; } = new List<WizardStepConfig>();

        // Button configuration
        public string PreviousButtonText { get; set; } = "Previous";
        public string NextButtonText { get; set; } = "Next";
        public string SubmitButtonText { get; set; } = "Submit";
        public string CancelButtonText { get; set; } = "Cancel";
        public string? CancelUrl { get; set; }

        // Validation
        public bool ValidateOnStepChange { get; set; } = true;
        public bool ShowStepNumbers { get; set; } = true;

        // Card wrapper
        public bool WrapInCard { get; set; } = true;
        public string? CardTitle { get; set; }
        public string? CardSubtitle { get; set; }

        // Progress indicator
        public bool ShowProgressBar { get; set; } = false;
    }

    /// <summary>
    /// Individual wizard step configuration
    /// </summary>
    public class WizardStepConfig
    {
        public string StepId { get; set; } = "";
        public string Title { get; set; } = "";
        public string? Icon { get; set; }
        public string? Description { get; set; }

        // Reuse existing FormFieldConfig!
        public List<FormFieldConfig> Fields { get; set; } = new List<FormFieldConfig>();

        // Layout options for this step
        public int FieldsPerRow { get; set; } = 2; // Default 2 columns
        public string FieldColumnClass { get; set; } = "col-md-6"; // Bootstrap column class
    }

    /// <summary>
    /// Presentation ViewModel for rendering wizard (created by extension method)
    /// </summary>
    public class FormWizardViewModel
    {
        public string WizardId { get; set; } = "";
        public string ActionUrl { get; set; } = "";
        public string Method { get; set; } = "POST";
        public WizardType Type { get; set; } = WizardType.Horizontal;

        public List<WizardStepViewModel> Steps { get; set; } = new List<WizardStepViewModel>();

        public string PreviousButtonText { get; set; } = "Previous";
        public string NextButtonText { get; set; } = "Next";
        public string SubmitButtonText { get; set; } = "Submit";
        public string CancelButtonText { get; set; } = "Cancel";
        public string? CancelUrl { get; set; }

        public bool ValidateOnStepChange { get; set; } = true;
        public bool ShowStepNumbers { get; set; } = true;
        public bool ShowProgressBar { get; set; } = false;

        public bool WrapInCard { get; set; } = true;
        public string? CardTitle { get; set; }
        public string? CardSubtitle { get; set; }

        // Calculated properties
        public int TotalSteps => Steps.Count;
        public int CurrentStepNumber => Steps.FirstOrDefault(s => s.IsActive)?.StepNumber ?? 1;
    }

    /// <summary>
    /// Individual wizard step ViewModel
    /// </summary>
    public class WizardStepViewModel
    {
        public string StepId { get; set; } = "";
        public string Title { get; set; } = "";
        public string? Icon { get; set; }
        public string? Description { get; set; }
        public int StepNumber { get; set; }
        public bool IsActive { get; set; } = false;

        // Reuse existing FormFieldViewModel!
        public List<FormFieldViewModel> Fields { get; set; } = new List<FormFieldViewModel>();

        // Layout options
        public int FieldsPerRow { get; set; } = 2;
        public string FieldColumnClass { get; set; } = "col-md-6";

        // Helper properties
        public bool IsFirstStep => StepNumber == 1;
        public bool IsLastStep { get; set; } = false;
        public string TabId => $"step-{StepNumber}";
    }

    /// <summary>
    /// Wizard type enumeration
    /// </summary>
    public enum WizardType
    {
        Horizontal,  // Tabs on top (default)
        Vertical     // Steps on left side (future implementation)
    }
}
