using OSHManagement.Models.ViewModels;

namespace OSHManagement.Extensions
{
    /// <summary>
    /// Extension methods for building Form Wizard ViewModels
    /// Follows same pattern as DataTableExtensions and FormExtensions
    /// </summary>
    public static class FormWizardExtensions
    {
        /// <summary>
        /// Builds a FormWizardViewModel from FormWizardConfig
        /// This keeps ALL logic out of the view
        /// </summary>
        public static FormWizardViewModel BuildWizard(this FormWizardConfig config)
        {
            var wizard = new FormWizardViewModel
            {
                WizardId = config.WizardId,
                ActionUrl = config.ActionUrl,
                Method = config.Method,
                Type = config.Type,
                PreviousButtonText = config.PreviousButtonText,
                NextButtonText = config.NextButtonText,
                SubmitButtonText = config.SubmitButtonText,
                CancelButtonText = config.CancelButtonText,
                CancelUrl = config.CancelUrl,
                ValidateOnStepChange = config.ValidateOnStepChange,
                ShowStepNumbers = config.ShowStepNumbers,
                ShowProgressBar = config.ShowProgressBar,
                WrapInCard = config.WrapInCard,
                CardTitle = config.CardTitle,
                CardSubtitle = config.CardSubtitle,
                Steps = new List<WizardStepViewModel>()
            };

            // Build steps with field ViewModels
            for (int i = 0; i < config.Steps.Count; i++)
            {
                var stepConfig = config.Steps[i];
                var step = new WizardStepViewModel
                {
                    StepId = stepConfig.StepId,
                    Title = stepConfig.Title,
                    Icon = stepConfig.Icon,
                    Description = stepConfig.Description,
                    StepNumber = i + 1,
                    IsActive = i == 0, // First step is active by default
                    IsLastStep = i == config.Steps.Count - 1,
                    FieldsPerRow = stepConfig.FieldsPerRow,
                    FieldColumnClass = stepConfig.FieldColumnClass,
                    Fields = new List<FormFieldViewModel>()
                };

                // Build fields for this step (reuse FormFieldConfig → FormFieldViewModel mapping)
                foreach (var fieldConfig in stepConfig.Fields)
                {
                    var field = MapFieldConfigToViewModel(fieldConfig, config.WizardId, stepConfig.StepId);
                    step.Fields.Add(field);
                }

                wizard.Steps.Add(step);
            }

            return wizard;
        }

        /// <summary>
        /// Maps FormFieldConfig to FormFieldViewModel
        /// Reuses logic similar to FormExtensions
        /// </summary>
        private static FormFieldViewModel MapFieldConfigToViewModel(
            FormFieldConfig fieldConfig,
            string wizardId,
            string stepId)
        {
            var field = new FormFieldViewModel
            {
                Name = fieldConfig.Name,
                PropertyName = !string.IsNullOrEmpty(fieldConfig.PropertyName)
                    ? fieldConfig.PropertyName
                    : fieldConfig.Name,
                Label = fieldConfig.Label,
                Type = fieldConfig.Type,
                Placeholder = fieldConfig.Placeholder,
                Value = fieldConfig.Value,
                DefaultValue = fieldConfig.DefaultValue,
                Required = fieldConfig.Required,
                Disabled = fieldConfig.Disabled,
                ReadOnly = fieldConfig.ReadOnly,
                Rows = fieldConfig.Rows,
                Options = fieldConfig.Options,
                ValidationMessage = fieldConfig.ValidationMessage,
                MaxLength = fieldConfig.MaxLength,
                MinLength = fieldConfig.MinLength,
                CustomAttributes = fieldConfig.CustomAttributes,
                CustomCssClass = fieldConfig.CustomCssClass,
                HelpText = fieldConfig.HelpText,
                // Generate unique ID: wizardId_stepId_fieldName
                UniqueId = GenerateUniqueFieldId(wizardId, stepId, fieldConfig.Name)
            };

            return field;
        }

        /// <summary>
        /// Generates unique field ID for wizard steps
        /// Format: {wizardId}_{stepId}_{fieldName}
        /// Example: createEmployeeWizard_personalInfo_FirstName
        /// </summary>
        private static string GenerateUniqueFieldId(string wizardId, string stepId, string fieldName)
        {
            return $"{wizardId}_{stepId}_{fieldName}";
        }

        /// <summary>
        /// Helper method to get step by step number
        /// </summary>
        public static WizardStepViewModel? GetStepByNumber(this FormWizardViewModel wizard, int stepNumber)
        {
            return wizard.Steps.FirstOrDefault(s => s.StepNumber == stepNumber);
        }

        /// <summary>
        /// Helper method to get current active step
        /// </summary>
        public static WizardStepViewModel? GetActiveStep(this FormWizardViewModel wizard)
        {
            return wizard.Steps.FirstOrDefault(s => s.IsActive);
        }

        /// <summary>
        /// Calculate progress percentage
        /// </summary>
        public static int GetProgressPercentage(this FormWizardViewModel wizard)
        {
            if (wizard.TotalSteps == 0) return 0;
            return (int)((wizard.CurrentStepNumber / (double)wizard.TotalSteps) * 100);
        }
    }
}
