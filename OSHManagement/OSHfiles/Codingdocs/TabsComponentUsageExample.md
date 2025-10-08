# Vertical Tabs Component - Usage Guide

## 📋 Overview

The Vertical Tabs component displays navigation pills on the **left side** with content on the **right side**. Perfect for settings pages, profile views, and multi-section displays.

---

## 🎨 Visual Layout

```
┌─────────────────────────────────────────────────┐
│  Employee Details                               │
├──────────────┬──────────────────────────────────┤
│              │                                  │
│ 🔵 Profile   │   Profile Content Here          │
│ ⚪ Work Info │   - Employee Name               │
│ ⚪ Documents │   - Payroll Number              │
│ ⚪ History   │   - Contact Info                │
│              │                                  │
└──────────────┴──────────────────────────────────┘
```

---

## 🚀 Basic Usage

### **Example: Employee View with Tabs**

```csharp
@using OSHManagement.Extensions
@model OSHManagement.Models.ViewModels.EmployeeViewModel
@{
    ViewData["Title"] = "Employee Details";

    // Configure Vertical Tabs
    var tabsConfig = new OSHManagement.Models.ViewModels.TabsConfig
    {
        TabsId = "employeeTabs",
        Type = TabsType.VerticalStyle1,
        NavColumnClass = "col-md-3",     // 25% width for nav
        ContentColumnClass = "col-md-9", // 75% width for content
        WrapInCard = true,
        CardTitle = $"Employee: {Model.FullName}",
        CardSubtitle = $"Payroll: {Model.PayrollNo}",

        Tabs = new List<OSHManagement.Models.ViewModels.TabConfig>
        {
            // Tab 1: Profile
            new TabConfig
            {
                TabId = "profile",
                Title = "Profile",
                Icon = "ri-user-line",
                IsActive = true, // First tab active by default
                Content = @<text>
                    <div class="profile-info">
                        <h5>Personal Information</h5>
                        <div class="row">
                            <div class="col-md-6">
                                <p><strong>Name:</strong> @Model.FullName</p>
                                <p><strong>Email:</strong> @Model.EmailAddress</p>
                                <p><strong>Phone:</strong> @Model.PhoneNo</p>
                            </div>
                            <div class="col-md-6">
                                <p><strong>Payroll No:</strong> @Model.PayrollNo</p>
                                <p><strong>Employee Type:</strong> @Model.EmployeeType</p>
                                <p><strong>Status:</strong> @Model.EmploymentStatus</p>
                            </div>
                        </div>
                    </div>
                </text>
            },

            // Tab 2: Work Information
            new TabConfig
            {
                TabId = "workInfo",
                Title = "Work Information",
                Icon = "ri-building-line",
                Content = @<text>
                    <div class="work-info">
                        <h5>Work Details</h5>
                        <p><strong>Station:</strong> @Model.StationName</p>
                        <p><strong>Department:</strong> @Model.DepartmentName</p>
                        <p><strong>Designation:</strong> @Model.Designation</p>
                        <p><strong>Hire Date:</strong> @Model.FormattedHireDate</p>

                        <h5 class="mt-4">Reporting Structure</h5>
                        <p><strong>HOD:</strong> @Model.HodFullName (@Model.HodPayroll)</p>
                        <p><strong>Supervisor:</strong> @Model.SupervisorFullName (@Model.SupervisorPayroll)</p>
                    </div>
                </text>
            },

            // Tab 3: Roles & Permissions
            new TabConfig
            {
                TabId = "roles",
                Title = "Roles",
                Icon = "ri-shield-check-line",
                Content = @<text>
                    <div class="roles-info">
                        <h5>Assigned Roles</h5>
                        @if (Model.RoleNames.Any())
                        {
                            <ul class="list-group">
                                @foreach (var role in Model.RoleNames)
                                {
                                    <li class="list-group-item">
                                        <i class="ri-shield-star-line me-2 text-primary"></i>@role
                                    </li>
                                }
                            </ul>
                        }
                        else
                        {
                            <p class="text-muted">No roles assigned</p>
                        }
                    </div>
                </text>
            },

            // Tab 4: Activity History
            new TabConfig
            {
                TabId = "history",
                Title = "History",
                Icon = "ri-history-line",
                Content = @<text>
                    <div class="history-info">
                        <h5>Activity Timeline</h5>
                        <p><strong>Created:</strong> @Model.CreatedAt.ToString("MMM dd, yyyy")</p>
                        @if (Model.UpdatedAt.HasValue)
                        {
                            <p><strong>Last Updated:</strong> @Model.UpdatedAt.Value.ToString("MMM dd, yyyy")</p>
                        }

                        <div class="alert alert-info mt-3">
                            <i class="ri-information-line me-2"></i>
                            Full activity history coming soon
                        </div>
                    </div>
                </text>
            }
        }
    };

    var tabs = tabsConfig.BuildTabs();
}

<!-- Render Tabs -->
<div class="row">
    <div class="col-xl-12">
        <partial name="~/Views/Shared/Components/Tabs/VerticalTabs.cshtml" model="tabs" />
    </div>
</div>
```

---

## 🎯 Key Features

### **1. Icon Support**
Every tab can have an icon from RemixIcon:
```csharp
Icon = "ri-user-line"           // Profile
Icon = "ri-building-line"       // Work/Station
Icon = "ri-shield-check-line"   // Roles/Security
Icon = "ri-history-line"        // History
Icon = "ri-file-text-line"      // Documents
Icon = "ri-settings-3-line"     // Settings
```

### **2. Flexible Layout**
Adjust column widths:
```csharp
// Narrow navigation (20/80)
NavColumnClass = "col-md-2",
ContentColumnClass = "col-md-10",

// Default (25/75)
NavColumnClass = "col-md-3",
ContentColumnClass = "col-md-9",

// Wide navigation (33/67)
NavColumnClass = "col-md-4",
ContentColumnClass = "col-md-8",
```

### **3. Disabled Tabs**
```csharp
new TabConfig
{
    TabId = "premium",
    Title = "Premium Features",
    Icon = "ri-vip-crown-line",
    IsDisabled = true, // ← Grayed out, not clickable
    Content = @<text>Premium content</text>
}
```

### **4. Active Tab Control**
```csharp
// Set any tab as initially active
Tabs = new List<TabConfig>
{
    new TabConfig { Title = "Tab 1", IsActive = false },
    new TabConfig { Title = "Tab 2", IsActive = true }, // ← Opens first
    new TabConfig { Title = "Tab 3", IsActive = false }
}
```

---

## 📐 Architecture Pattern

**Same as all other components!**

```
TabsConfig (Data)
    ↓ [TabsExtensions.BuildTabs()]
TabsViewModel (Presentation)
    ↓
VerticalTabs.cshtml (Rendering)
```

**Benefits:**
- ✅ Zero business logic in views
- ✅ Type-safe configuration
- ✅ Compile-time checking
- ✅ Easy to test

---

## 🎨 Tab Content Options

### **Option 1: Inline Razor Template** (Recommended)
```csharp
Content = @<text>
    <div class="custom-content">
        <h5>@Model.PropertyName</h5>
        <p>Rich HTML content here</p>
    </div>
</text>
```

### **Option 2: Partial View** (For complex content)
```csharp
Content = @<text>
    <partial name="~/Views/Employee/_ProfileTab.cshtml" model="Model" />
</text>
```

### **Option 3: Simple HTML**
```csharp
Content = @<text>
    <p>Simple content without variables</p>
</text>
```

---

## 🔧 Advanced Usage

### **Dynamic Tabs Based on Permissions**
```csharp
var tabs = new List<TabConfig>
{
    new TabConfig { TabId = "profile", Title = "Profile", Icon = "ri-user-line", IsActive = true, Content = profileContent }
};

// Only show roles tab if user has permission
if (User.IsInRole("Admin"))
{
    tabs.Add(new TabConfig
    {
        TabId = "roles",
        Title = "Roles",
        Icon = "ri-shield-check-line",
        Content = rolesContent
    });
}

// Only show settings if user owns this employee
if (Model.EmployeeId == currentUserId)
{
    tabs.Add(new TabConfig
    {
        TabId = "settings",
        Title = "Settings",
        Icon = "ri-settings-3-line",
        Content = settingsContent
    });
}

var tabsConfig = new TabsConfig
{
    TabsId = "employeeTabs",
    Tabs = tabs
};
```

### **Multiple Tabs Components on Same Page**
```csharp
// First tabs group
var personalTabs = new TabsConfig
{
    TabsId = "personalTabs", // ← Unique ID
    CardTitle = "Personal Information",
    Tabs = /* ... */
}.BuildTabs();

// Second tabs group
var workTabs = new TabsConfig
{
    TabsId = "workTabs", // ← Different unique ID
    CardTitle = "Work Information",
    Tabs = /* ... */
}.BuildTabs();

// Render both
<partial name="~/Views/Shared/Components/Tabs/VerticalTabs.cshtml" model="personalTabs" />
<partial name="~/Views/Shared/Components/Tabs/VerticalTabs.cshtml" model="workTabs" />
```

---

## 📱 Responsive Behavior

### **Desktop (> 768px)**
```
┌──────┬────────────┐
│ Nav  │  Content   │
│ (25%)│   (75%)    │
└──────┴────────────┘
```

### **Mobile (< 768px)**
```
┌──────────────────┐
│ [Nav] [Nav] [Nav]│ ← Horizontal scroll
├──────────────────┤
│    Content       │
│    Below         │
└──────────────────┘
```

---

## 🎨 Styling Customization

The component uses `.tab-style-7` class for Vyzor compatibility. You can customize in the component CSS:

```css
/* Change active tab color */
.tab-style-7 .nav-link.active {
    background-color: var(--bs-success); /* Change from primary */
}

/* Larger icons */
.tab-style-7 .nav-link i {
    font-size: 1.3rem; /* Default: 1.1rem */
}

/* More padding */
.tab-style-7 .nav-link {
    padding: 1rem 1.5rem; /* Default: 0.75rem 1rem */
}
```

---

## 🆚 When to Use Tabs vs Wizard vs Accordion

| Component | Use Case | Example |
|-----------|----------|---------|
| **Tabs** | View/display data in categories | Employee profile, Dashboard sections |
| **Wizard** | Create/edit multi-step process | New employee, Risk assessment |
| **Accordion** | Collapsible sections | FAQ, Long forms (edit) |

---

## ✅ Complete Example Files

**Reference Files:**
1. `Models/ViewModels/TabsConfigViewModel.cs` - ViewModels
2. `Extensions/TabsExtensions.cs` - BuildTabs() logic
3. `Views/Shared/Components/Tabs/VerticalTabs.cshtml` - Component
4. `Views/Shared/Components/Tabs/_VerticalTabsContent.cshtml` - Content template

---

## 🎉 Summary

**Vertical Tabs Component = Perfect for View/Display Pages!**

**Features:**
- ✅ 10 minutes to implement
- ✅ Zero JavaScript needed (Bootstrap handles it)
- ✅ Fully responsive
- ✅ Icon support
- ✅ Disabled state support
- ✅ Dynamic content via Razor templates
- ✅ Multiple tabs per page
- ✅ Beautiful Vyzor styling

**Just configure TabsConfig and render the partial!** 🚀
