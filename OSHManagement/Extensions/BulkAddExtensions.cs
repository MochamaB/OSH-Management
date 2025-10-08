using OSHManagement.Models.ViewModels;

namespace OSHManagement.Extensions
{
    public static class BulkAddExtensions
    {
        /// <summary>
        /// Builds a BulkAddViewModel from simple BulkAddConfig
        /// This keeps ALL logic out of the view
        /// </summary>
        public static BulkAddViewModel BuildBulkAdd(this BulkAddConfig config)
        {
            var bulkAdd = new BulkAddViewModel
            {
                ComponentId = config.ComponentId,
                Title = config.Title,
                Description = config.Description,
                Icon = config.Icon,
                EntityName = config.EntityName,
                EntityNamePlural = config.EntityNamePlural,
                InitialRows = config.InitialRows,
                MinRows = config.MinRows,
                MaxRows = config.MaxRows,
               
                ParentSelectors = config.ParentSelectors,
                ActionUrl = config.ActionUrl,
                Method = config.Method,
                SubmitButtonText = config.SubmitButtonText,
                CancelUrl = config.CancelUrl,
                WrapInCard = config.WrapInCard
            };
            
            // Build fields with unique ID templates and ordering
            foreach (var fieldConfig in config.Fields.OrderBy(f => f.DisplayOrder))
            {
                var field = new BulkAddFieldViewModel
                {
                    Name = fieldConfig.Name,
                    Label = fieldConfig.Label,
                    Type = fieldConfig.Type,
                    Required = fieldConfig.Required,
                    Placeholder = fieldConfig.Placeholder,
                    MaxLength = fieldConfig.MaxLength,
                    Options = fieldConfig.Options,
                    ValidationMessage = fieldConfig.ValidationMessage,
                    HelpText = fieldConfig.HelpText,
                    ColumnClass = fieldConfig.ColumnClass,
                    FilterByParent = fieldConfig.FilterByParent,
                    FilterPropertyName = fieldConfig.FilterPropertyName,
                    UniqueIdTemplate = $"{config.ComponentId}_{{{{index}}}}_{ fieldConfig.Name}"
                };
                
                bulkAdd.Fields.Add(field);
            }
            
            return bulkAdd;
        }
    }
}
