using OSHManagement.Models.ViewModels;
using Microsoft.AspNetCore.WebUtilities;

namespace OSHManagement.Extensions
{
    public static class DataTableExtensions
    {
        /// <summary>
        /// Builds a DataTableViewModel from simple TableConfig
        /// This keeps ALL logic out of the view
        /// </summary>
        public static DataTableViewModel BuildTable(
            this TableConfig config,
            Func<object, Microsoft.AspNetCore.Mvc.Razor.HelperResult> tableContent,
            string currentUrl)
        {
            var table = new DataTableViewModel
            {
                TableId = config.TableId,
                Columns = config.Columns,
                TableContent = tableContent,
                CreateButtonText = config.CreateButtonText ?? "",
                CreateButtonUrl = config.CreateButtonUrl ?? ""
            };

            // Build Search Box
            if (!string.IsNullOrEmpty(config.SearchPlaceholder))
            {
                table.SearchBox = new SearchBoxViewModel
                {
                    ActionUrl = config.ActionUrl,
                    ParameterName = "search",
                    PlaceholderText = config.SearchPlaceholder,
                    CurrentValue = config.SearchValue ?? "",
                    ShowButton = true,
                    PreserveQueryParams = GetPreserveParams(config, "search")
                };
            }

            // Build Filters
            foreach (var filter in config.Filters)
            {
                if (filter.Type == FilterType.Dropdown)
                {
                    table.FilterDropdowns.Add(BuildDropdownFilter(filter, config, currentUrl));
                }
                else if (filter.Type == FilterType.Select)
                {
                    table.FilterSelects.Add(BuildSelectFilter(filter, config));
                }
            }

            // Build Header Actions
            foreach (var action in config.HeaderActions)
            {
                table.HeaderActions.Add(new HeaderAction
                {
                    Text = action.Text,
                    Url = action.Url,
                    IconClass = action.Icon,
                    ColorClass = action.Color
                });
            }

            return table;
        }

        private static FilterDropdownViewModel BuildDropdownFilter(
            FilterConfig filter,
            TableConfig config,
            string currentUrl)
        {
            var dropdown = new FilterDropdownViewModel
            {
                Label = filter.Label,
                Options = new List<FilterDropdownOption>()
            };

            foreach (var option in filter.Options)
            {
                var url = BuildFilterUrl(config.ActionUrl, filter.ParameterName, option.Value, config, filter.ParameterName);
                var isActive = filter.CurrentValue == option.Value ||
                               (string.IsNullOrEmpty(filter.CurrentValue) && string.IsNullOrEmpty(option.Value));

                dropdown.Options.Add(new FilterDropdownOption
                {
                    Text = option.Text,
                    Url = url,
                    IsActive = isActive
                });
            }

            return dropdown;
        }

        private static FilterSelectViewModel BuildSelectFilter(
            FilterConfig filter,
            TableConfig config)
        {
            var select = new FilterSelectViewModel
            {
                FormId = $"{config.TableId}_{filter.ParameterName}Filter",
                ActionUrl = config.ActionUrl,
                ParameterName = filter.ParameterName,
                PlaceholderText = $"All {filter.Label}",
                Options = new List<SelectOption>(),
                PreserveQueryParams = GetPreserveParams(config, filter.ParameterName)
            };

            foreach (var option in filter.Options)
            {
                select.Options.Add(new SelectOption
                {
                    Text = option.Text,
                    Value = option.Value,
                    IsSelected = filter.CurrentValue == option.Value
                });
            }

            return select;
        }

        private static string BuildFilterUrl(
            string baseUrl,
            string paramName,
            string paramValue,
            TableConfig config,
            string excludeParam)
        {
            var queryParams = new Dictionary<string, string?>();

            // Add the new filter parameter
            if (!string.IsNullOrEmpty(paramValue))
            {
                queryParams[paramName] = paramValue;
            }

            // Preserve other filters
            if (!string.IsNullOrEmpty(config.SearchValue))
            {
                queryParams["search"] = config.SearchValue;
            }

            foreach (var filter in config.Filters)
            {
                if (filter.ParameterName != excludeParam && !string.IsNullOrEmpty(filter.CurrentValue))
                {
                    queryParams[filter.ParameterName] = filter.CurrentValue;
                }
            }

            return QueryHelpers.AddQueryString(baseUrl, queryParams);
        }

        private static Dictionary<string, string> GetPreserveParams(TableConfig config, string excludeParam)
        {
            var preserveParams = new Dictionary<string, string>();

            // Preserve search
            if (excludeParam != "search" && !string.IsNullOrEmpty(config.SearchValue))
            {
                preserveParams["search"] = config.SearchValue;
            }

            // Preserve all filters except the one being changed
            foreach (var filter in config.Filters)
            {
                if (filter.ParameterName != excludeParam && !string.IsNullOrEmpty(filter.CurrentValue))
                {
                    preserveParams[filter.ParameterName] = filter.CurrentValue;
                }
            }

            return preserveParams;
        }

        /// <summary>
        /// Creates standard row actions (View/Edit/Delete)
        /// </summary>
        public static ActionButtonsViewModel BuildRowActions(int id, string baseUrl, RowActionConfig? config = null)
        {
            config ??= new RowActionConfig();

            var viewUrl = config.ViewUrl?.Replace("{id}", id.ToString()) ?? $"{baseUrl}/View/{id}";
            var editUrl = config.EditUrl?.Replace("{id}", id.ToString()) ?? $"{baseUrl}/Edit/{id}";
            var deleteMessage = config.DeleteConfirmMessage ?? "Are you sure you want to delete this item?";

            return new ActionButtonsViewModel
            {
                ShowView = config.ShowView,
                ViewUrl = viewUrl,
                ShowEdit = config.ShowEdit,
                EditUrl = editUrl,
                ShowDelete = config.ShowDelete,
                DeleteJsFunction = $"confirmDelete({id}, '{deleteMessage}')",
                CustomActions = config.CustomActions.Select(a => new CustomAction
                {
                    Title = a.Text,
                    Url = a.Url.Replace("{id}", id.ToString()),
                    IconClass = a.Icon,
                    ColorClass = a.Color
                }).ToList()
            };
        }
    }
}
