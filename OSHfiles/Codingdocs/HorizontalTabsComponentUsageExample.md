# Horizontal Tabs Component (Tab Style-8) - Usage Guide

## Overview
The Horizontal Tabs Component uses **Vyzor Tab Style-8** with an elegant bottom border animation. It supports optional icons and follows the same pattern as the Vertical Tabs component.

---

## Key Features
- ✅ **Tab Style-8 Design**: Bottom border with smooth scaleX animation
- ✅ **Optional Icons**: Support for Remix Icon or any icon library
- ✅ **Responsive**: Auto-scrolls horizontally on mobile devices
- ✅ **Accessible**: Full ARIA support and keyboard navigation
- ✅ **Configurable**: Card wrapper, titles, and layout options
- ✅ **Follows Pattern**: Same structure as VerticalTabs component

---

## Basic Usage

### Step 1: Configure Tabs in Controller

```csharp
using OSHManagement.Models.ViewModels;
using OSHManagement.Extensions;

public IActionResult Index()
{
    var tabsConfig = new TabsConfig
    {
        TabsId = "teamTabs",
        Type = TabsType.HorizontalStyle8,
        WrapInCard = true,
        CardTitle = "Team Management",
        CardSubtitle = "Manage team details and members",
        Tabs = new List<TabConfig>
        {
            new TabConfig
            {
                TabId = "overview",
                Title = "Overview",
                Icon = "ri-home-line", // Optional icon
                IsActive = true,
                Content = null // Will be provided in the view
            },
            new TabConfig
            {
                TabId = "members",
                Title = "Team Members",
                Icon = "ri-team-line",
                Content = null
            },
            new TabConfig
            {
                TabId = "activities",
                Title = "Activities",
                Icon = "ri-calendar-line",
                Content = null
            },
            new TabConfig
            {
                TabId = "reports",
                Title = "Reports",
                Icon = "ri-file-chart-line",
                IsDisabled = true // Disabled tab
            }
        }
    };

    // Build the ViewModel
    var tabs = tabsConfig.BuildTabs();

    ViewBag.TabsViewModel = tabs;
    return View();
}
```

---

### Step 2: Render Tabs in View

```cshtml
@using OSHManagement.Extensions
@{
    var tabsConfig = new OSHManagement.Models.ViewModels.TabsConfig
    {
        TabsId = "teamTabs",
        Type = OSHManagement.Models.ViewModels.TabsType.HorizontalStyle8,
        WrapInCard = true,
        CardTitle = "Team Management",
        Tabs = new List<OSHManagement.Models.ViewModels.TabConfig>
        {
            new OSHManagement.Models.ViewModels.TabConfig
            {
                TabId = "overview",
                Title = "Overview",
                Icon = "ri-home-line",
                IsActive = true,
                Content = @<text>
                    <h5>Team Overview</h5>
                    <p>View team statistics and summary information.</p>
                    <div class="row">
                        <div class="col-md-4">
                            <div class="card">
                                <div class="card-body">
                                    <h6>Total Members</h6>
                                    <h3 class="text-primary">24</h3>
                                </div>
                            </div>
                        </div>
                        <!-- More cards... -->
                    </div>
                </text>
            },
            new OSHManagement.Models.ViewModels.TabConfig
            {
                TabId = "members",
                Title = "Team Members",
                Icon = "ri-team-line",
                Content = @<text>
                    <h5>Team Members</h5>
                    <p>Manage team members, roles, and assignments.</p>
                    <!-- DataTable or member list here -->
                </text>
            },
            new OSHManagement.Models.ViewModels.TabConfig
            {
                TabId = "activities",
                Title = "Activities",
                Icon = "ri-calendar-line",
                Content = @<text>
                    <h5>Team Activities</h5>
                    <p>View and manage team activities and tasks.</p>
                    <!-- Activities list here -->
                </text>
            }
        }
    };

    var tabs = tabsConfig.BuildTabs();
}

<!-- Render Horizontal Tabs -->
<partial name="~/Views/Shared/Components/Tabs/HorizontalTabs.cshtml" model="tabs" />
```

---

## Without Card Wrapper

```csharp
var tabsConfig = new TabsConfig
{
    TabsId = "simpleTabs",
    Type = TabsType.HorizontalStyle8,
    WrapInCard = false, // No card wrapper
    Tabs = new List<TabConfig>
    {
        new TabConfig
        {
            TabId = "tab1",
            Title = "Tab 1",
            IsActive = true,
            Content = null
        },
        new TabConfig
        {
            TabId = "tab2",
            Title = "Tab 2",
            Content = null
        }
    }
};
```

---

## Without Icons

If you don't want icons, simply omit the `Icon` property:

```csharp
new TabConfig
{
    TabId = "overview",
    Title = "Overview", // No icon
    IsActive = true,
    Content = null
}
```

---

## Advanced Example: Dynamic Tab Content

```cshtml
@using OSHManagement.Extensions
@model OSHManagement.Models.ViewModels.TeamViewModel

@{
    var tabsConfig = new OSHManagement.Models.ViewModels.TabsConfig
    {
        TabsId = "teamDetailsTabs",
        Type = OSHManagement.Models.ViewModels.TabsType.HorizontalStyle8,
        WrapInCard = true,
        CardTitle = $"Team: {Model.TeamName}",
        CardSubtitle = $"{Model.TeamType} | {Model.StationName}",
        Tabs = new List<OSHManagement.Models.ViewModels.TabConfig>
        {
            new OSHManagement.Models.ViewModels.TabConfig
            {
                TabId = "details",
                Title = "Details",
                Icon = "ri-information-line",
                IsActive = true,
                Content = @<text>
                    <div class="row">
                        <div class="col-md-6">
                            <label class="text-muted small">Team Type</label>
                            <div class="fw-semibold">@Model.TeamType</div>
                        </div>
                        <div class="col-md-6">
                            <label class="text-muted small">Station</label>
                            <div class="fw-semibold">@Model.StationName</div>
                        </div>
                        <div class="col-md-6 mt-3">
                            <label class="text-muted small">Formation Date</label>
                            <div class="fw-semibold">@Model.FormationDate.ToString("dd MMM yyyy")</div>
                        </div>
                        <div class="col-md-6 mt-3">
                            <label class="text-muted small">Status</label>
                            <span class="badge bg-@(Model.TeamStatus == "Active" ? "success" : "secondary")">
                                @Model.TeamStatus
                            </span>
                        </div>
                    </div>
                </text>
            },
            new OSHManagement.Models.ViewModels.TabConfig
            {
                TabId = "members",
                Title = "Members",
                Icon = "ri-team-line",
                Content = @<text>
                    <div class="d-flex justify-content-between mb-3">
                        <h6>Team Members (@Model.ActiveMemberCount/@Model.MaxMemberCount)</h6>
                        <a href="/Team/AddMember/@Model.TeamId" class="btn btn-sm btn-primary">
                            <i class="ri-add-line"></i> Add Member
                        </a>
                    </div>
                    <!-- Members DataTable here -->
                    <partial name="_TeamMembersTable" model="Model" />
                </text>
            },
            new OSHManagement.Models.ViewModels.TabConfig
            {
                TabId = "history",
                Title = "History",
                Icon = "ri-history-line",
                Content = @<text>
                    <h6>Team History</h6>
                    <p class="text-muted">Audit trail of team changes and activities.</p>
                    <!-- History timeline here -->
                </text>
            }
        }
    };

    var tabs = tabsConfig.BuildTabs();
}

<partial name="~/Views/Shared/Components/Tabs/HorizontalTabs.cshtml" model="tabs" />
```

---

## Responsive Behavior

The horizontal tabs automatically adapt to mobile screens:

- **Desktop**: Tabs displayed in a row with full labels
- **Mobile**: Tabs become horizontally scrollable, maintaining their layout
- **Touch**: Smooth touch-scrolling enabled on mobile devices

---

## Styling Details

### Tab Style-8 Features:
1. **Bottom Border Animation**: Smooth scaleX animation on active tab
2. **Hover Effect**: Color changes to primary color on hover
3. **Active State**: Primary color with animated bottom border
4. **Disabled State**: Reduced opacity, non-interactive

### CSS Classes Used:
- `.tab-style-8` - Main tab style class
- `.nav-tabs` - Bootstrap tabs class
- `.scaleX` - Enables the scale animation
- `.nav-link` - Individual tab button

---

## Accessibility

The component includes:
- ✅ Full ARIA attributes (`role`, `aria-controls`, `aria-selected`)
- ✅ Keyboard navigation support
- ✅ Proper focus states
- ✅ Disabled tab handling
- ✅ Screen reader friendly

---

## Configuration Properties

### TabsConfig Properties:
- `TabsId` - Unique identifier for the tabs (required)
- `Type` - Use `TabsType.HorizontalStyle8` for Tab Style-8
- `Tabs` - List of tab configurations
- `WrapInCard` - Whether to wrap in a card (default: true)
- `CardTitle` - Card header title (optional)
- `CardSubtitle` - Card header subtitle (optional)

### TabConfig Properties:
- `TabId` - Unique ID for the tab (auto-generated if not provided)
- `Title` - Tab label text (required)
- `Icon` - Icon class (optional, e.g., "ri-home-line")
- `IsActive` - Whether this tab is initially active (default: false, first tab auto-activated)
- `IsDisabled` - Whether this tab is disabled (default: false)
- `Content` - Razor template for tab content

---

## Best Practices

1. **Use Icons Consistently**: Either use icons for all tabs or none
2. **Keep Tab Labels Short**: 1-2 words work best
3. **Limit Number of Tabs**: 3-6 tabs is ideal for horizontal layout
4. **Always Set One Active**: First tab is auto-activated if none specified
5. **Use Semantic IDs**: Use descriptive TabIds like "overview", "members", not "tab1", "tab2"

---

## Examples from OSH Management System

### 1. Team Details Page
```csharp
Type = TabsType.HorizontalStyle8
Tabs = ["Overview", "Members", "Activities", "History"]
Icons = Yes
```

### 2. OSH Committee Page
```csharp
Type = TabsType.HorizontalStyle8
Tabs = ["Committee Info", "Meetings", "Issues", "Reports"]
Icons = Yes
```

### 3. Risk Assessment Page
```csharp
Type = TabsType.HorizontalStyle8
Tabs = ["Hazards", "Controls", "Assessments", "Reviews"]
Icons = Optional
```

---

## Comparison: Horizontal vs Vertical

| Feature | Horizontal Style-8 | Vertical Style-1 |
|---------|-------------------|------------------|
| Layout | Top navigation | Left navigation |
| Best For | 3-6 tabs | Many tabs (6+) |
| Mobile | Horizontal scroll | Vertical scroll |
| Animation | Bottom border scale | Background fill |
| Space Usage | Full width | Side-by-side |

---

## Troubleshooting

### Issue: Tabs not showing
**Solution**: Ensure you've built the ViewModel using `.BuildTabs()`

### Issue: No icons appearing
**Solution**: Verify icon classes (e.g., "ri-home-line") and ensure icon CSS is loaded

### Issue: Active tab not working
**Solution**: Make sure at least one tab has `IsActive = true`, or leave all false (first tab auto-activates)

### Issue: Animation not smooth
**Solution**: Check that `.scaleX` class is on the `<ul>` element with `.tab-style-8`

---

## Related Components
- **VerticalTabs**: For sidebar-style navigation
- **FormWizard**: For multi-step forms
- **DataTable**: For tabular data within tabs
- **Cards**: For content within tab panels
