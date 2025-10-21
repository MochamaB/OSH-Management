using OSHManagement.Models.ViewModels;

namespace OSHManagement.Extensions
{
    /// <summary>
    /// Extension methods for building DetailCard ViewModels
    /// Follows same pattern as DataTableExtensions, FormWizardExtensions, TabsExtensions
    /// </summary>
    public static class DetailCardExtensions
    {
        /// <summary>
        /// Builds a DetailCardViewModel from DetailCardConfig
        /// This keeps ALL logic out of the view
        /// </summary>
        public static DetailCardViewModel BuildDetailCard(this DetailCardConfig config)
        {
            return new DetailCardViewModel
            {
                CardId = config.CardId,
                Icon = config.Icon,
                IconColor = config.IconColor,
                Title = config.Title,
                Subtitle = config.Subtitle,
                Category = config.Category,
                Fields = config.Fields,
                Actions = config.Actions,
                StatusBadge = config.StatusBadge,
                ShowCard = config.ShowCard
            };
        }

        /// <summary>
        /// Helper method to add a field
        /// </summary>
        public static DetailCardConfig AddField(this DetailCardConfig config, string label, string value, string? icon = null)
        {
            config.Fields.Add(new DetailField
            {
                Label = label,
                Value = value,
                Icon = icon
            });
            return config;
        }

        /// <summary>
        /// Helper method to add a field with badge
        /// </summary>
        public static DetailCardConfig AddFieldWithBadge(this DetailCardConfig config, string label, string badgeText, string badgeColor = "primary")
        {
            config.Fields.Add(new DetailField
            {
                Label = label,
                Value = "", // Not used when badge is present
                Badge = new DetailBadge
                {
                    Text = badgeText,
                    Color = badgeColor
                }
            });
            return config;
        }

        /// <summary>
        /// Helper method to add an action button
        /// </summary>
        public static DetailCardConfig AddAction(this DetailCardConfig config, string text, string? url = null, string icon = "", string buttonStyle = "primary")
        {
            config.Actions.Add(new DetailCardAction
            {
                Text = text,
                Icon = icon,
                Url = url,
                ButtonStyle = buttonStyle
            });
            return config;
        }

        /// <summary>
        /// Helper method to add a dropdown action
        /// </summary>
        public static DetailCardConfig AddDropdownAction(this DetailCardConfig config, string text, List<DropdownItem> items, string icon = "ri-more-2-fill")
        {
            config.Actions.Add(new DetailCardAction
            {
                Text = text,
                Icon = icon,
                IsDropdown = true,
                DropdownItems = items
            });
            return config;
        }

        /// <summary>
        /// Helper method to set status badge
        /// </summary>
        public static DetailCardConfig SetStatusBadge(this DetailCardConfig config, string text, string color = "primary", string? icon = null)
        {
            config.StatusBadge = new DetailBadge
            {
                Text = text,
                Color = color,
                Icon = icon
            };
            return config;
        }
    }
}
