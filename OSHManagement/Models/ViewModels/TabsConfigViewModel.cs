using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OSHManagement.Models.ViewModels
{
    /// <summary>
    /// Configuration for tabs component - NO LOGIC, just data
    /// Follows same pattern as TableConfig and FormWizardConfig
    /// </summary>
    public class TabsConfig
    {
        public string TabsId { get; set; } = "defaultTabs";
        public TabsType Type { get; set; } = TabsType.VerticalStyle1;
        public List<TabConfig> Tabs { get; set; } = new List<TabConfig>();

        // Layout options
        public string NavColumnClass { get; set; } = "col-md-3"; // Left column for vertical tabs
        public string ContentColumnClass { get; set; } = "col-md-9"; // Right column for content

        // Card wrapper
        public bool WrapInCard { get; set; } = true;
        public string? CardTitle { get; set; }
        public string? CardSubtitle { get; set; }
    }

    /// <summary>
    /// Individual tab configuration
    /// </summary>
    public class TabConfig
    {
        public string TabId { get; set; } = "";
        public string Title { get; set; } = "";
        public string? Icon { get; set; }
        public bool IsActive { get; set; } = false;
        public bool IsDisabled { get; set; } = false;

        // Content can be provided as Razor template
        public Func<object, HelperResult>? Content { get; set; }
    }

    /// <summary>
    /// Presentation ViewModel for rendering tabs (created by extension method)
    /// </summary>
    public class TabsViewModel
    {
        public string TabsId { get; set; } = "";
        public TabsType Type { get; set; } = TabsType.VerticalStyle1;
        public List<TabViewModel> Tabs { get; set; } = new List<TabViewModel>();

        public string NavColumnClass { get; set; } = "col-md-3";
        public string ContentColumnClass { get; set; } = "col-md-9";

        public bool WrapInCard { get; set; } = true;
        public string? CardTitle { get; set; }
        public string? CardSubtitle { get; set; }

        // Calculated properties
        public int TotalTabs => Tabs.Count;
        public TabViewModel? ActiveTab => Tabs.FirstOrDefault(t => t.IsActive);
    }

    /// <summary>
    /// Individual tab ViewModel
    /// </summary>
    public class TabViewModel
    {
        public string TabId { get; set; } = "";
        public string Title { get; set; } = "";
        public string? Icon { get; set; }
        public bool IsActive { get; set; } = false;
        public bool IsDisabled { get; set; } = false;
        public int TabIndex { get; set; } // 0-based index

        // Content template
        public Func<object, HelperResult>? Content { get; set; }

        // Helper properties
        public string NavId => $"{TabId}-tab";
        public string PanelId => $"{TabId}-panel";
        public string AriaSelected => IsActive ? "true" : "false";
        public string TabIndexAttr => IsDisabled ? "-1" : "0";
    }

    /// <summary>
    /// Tabs type enumeration
    /// </summary>
    public enum TabsType
    {
        VerticalStyle1,      // Pills on left, content on right (default - Vyzor tab-style-7)
        VerticalStyle2,      // Alternative vertical style
        HorizontalStyle8,    // Horizontal tabs with bottom border animation (Vyzor tab-style-8)
        Horizontal,          // Traditional horizontal tabs
        Pills,               // Horizontal pills
        Justified            // Full-width tabs
    }
}
