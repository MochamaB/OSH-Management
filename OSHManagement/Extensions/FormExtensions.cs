using OSHManagement.Models.ViewModels;

namespace OSHManagement.Extensions
{
    public static class FormExtensions
    {
        /// <summary>
        /// Builds a FormViewModel from simple FormConfig
        /// This keeps ALL logic out of the view
        /// </summary>
        public static FormViewModel BuildForm(this FormConfig config)
        {
            var form = new FormViewModel
            {
                FormId = config.FormId,
                ActionUrl = config.ActionUrl,
                Method = config.Method,
                FormType = config.FormType,
                LabelColumnWidth = config.LabelColumnWidth,
                InputColumnWidth = config.InputColumnWidth,
                FieldsPerRow = config.FieldsPerRow,
                FieldColumnClass = config.FieldColumnClass,
                FieldColumnClasses = config.FieldColumnClasses,
                SubmitButtonText = config.SubmitButtonText,
                SubmitButtonClass = config.SubmitButtonClass,
                CancelButtonText = config.CancelButtonText,
                CancelUrl = config.CancelUrl,
                WrapInCard = config.WrapInCard,
                CardTitle = config.CardTitle
            };

            // Build fields with unique IDs
            foreach (var fieldConfig in config.Fields)
            {
                var field = new FormFieldViewModel
                {
                    Name = fieldConfig.Name,
                    PropertyName = !string.IsNullOrEmpty(fieldConfig.PropertyName) ? fieldConfig.PropertyName : fieldConfig.Name,
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
                    UniqueId = GenerateUniqueId(config.FormId, fieldConfig.Name)
                };

                form.Fields.Add(field);
            }

            return form;
        }

        private static string GenerateUniqueId(string formId, string fieldName)
        {
            // Generate unique ID like: createCategoryForm_categoryName
            return $"{formId}_{fieldName}";
        }
    }
}
