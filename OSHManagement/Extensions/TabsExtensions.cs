using OSHManagement.Models.ViewModels;

namespace OSHManagement.Extensions
{
    /// <summary>
    /// Extension methods for building Tabs ViewModels
    /// Follows same pattern as DataTableExtensions and FormWizardExtensions
    /// </summary>
    public static class TabsExtensions
    {
        /// <summary>
        /// Builds a TabsViewModel from TabsConfig
        /// This keeps ALL logic out of the view
        /// </summary>
        public static TabsViewModel BuildTabs(this TabsConfig config)
        {
            var tabs = new TabsViewModel
            {
                TabsId = config.TabsId,
                Type = config.Type,
                NavColumnClass = config.NavColumnClass,
                ContentColumnClass = config.ContentColumnClass,
                WrapInCard = config.WrapInCard,
                CardTitle = config.CardTitle,
                CardSubtitle = config.CardSubtitle,
                Tabs = new List<TabViewModel>()
            };

            // Ensure at least one tab is active
            if (config.Tabs.Any() && !config.Tabs.Any(t => t.IsActive))
            {
                config.Tabs[0].IsActive = true;
            }

            // Build tab ViewModels
            for (int i = 0; i < config.Tabs.Count; i++)
            {
                var tabConfig = config.Tabs[i];
                var tab = new TabViewModel
                {
                    TabId = !string.IsNullOrEmpty(tabConfig.TabId)
                        ? tabConfig.TabId
                        : GenerateTabId(config.TabsId, i),
                    Title = tabConfig.Title,
                    Icon = tabConfig.Icon,
                    IsActive = tabConfig.IsActive,
                    IsDisabled = tabConfig.IsDisabled,
                    TabIndex = i,
                    Content = tabConfig.Content
                };

                tabs.Tabs.Add(tab);
            }

            return tabs;
        }

        /// <summary>
        /// Generates unique tab ID
        /// Format: {tabsId}_tab{index}
        /// Example: employeeTabs_tab0, employeeTabs_tab1
        /// </summary>
        private static string GenerateTabId(string tabsId, int index)
        {
            return $"{tabsId}_tab{index}";
        }

        /// <summary>
        /// Helper method to get tab by ID
        /// </summary>
        public static TabViewModel? GetTabById(this TabsViewModel tabs, string tabId)
        {
            return tabs.Tabs.FirstOrDefault(t => t.TabId == tabId);
        }

        /// <summary>
        /// Helper method to get tab by index
        /// </summary>
        public static TabViewModel? GetTabByIndex(this TabsViewModel tabs, int index)
        {
            if (index < 0 || index >= tabs.Tabs.Count) return null;
            return tabs.Tabs[index];
        }

        /// <summary>
        /// Helper method to get active tab
        /// </summary>
        public static TabViewModel? GetActiveTab(this TabsViewModel tabs)
        {
            return tabs.Tabs.FirstOrDefault(t => t.IsActive);
        }

        /// <summary>
        /// Helper method to get enabled tabs only
        /// </summary>
        public static List<TabViewModel> GetEnabledTabs(this TabsViewModel tabs)
        {
            return tabs.Tabs.Where(t => !t.IsDisabled).ToList();
        }
    }
}
