# Data Table Architecture - Clean & Generic Implementation

## 🎯 Design Philosophy

**The Problem We Solved:**
- Views had too much business logic (URL building, filter logic, state management)
- Code was not reusable across different entities
- Hard to maintain and replicate

**The Solution:**
- **Data-Driven Configuration**: Views only provide data, not logic
- **Generic Extension Methods**: All logic lives in one place
- **Composable Components**: Mix and match search, filters, actions

---

## 📁 File Structure

```
OSHManagement/
├── Models/ViewModels/
│   ├── TableConfigViewModel.cs          # Simple config (data only)
│   ├── FilterComponentViewModels.cs     # Search, Filter components
│   ├── DataTableViewModel.cs            # Complex internal model (don't use directly!)
│   └── ActionButtonsViewModel.cs        # Action buttons config
├── Extensions/
│   └── DataTableExtensions.cs           # ALL business logic here
├── Views/Shared/Components/
│   ├── _DataTable.cshtml                # Main table shell
│   ├── _SearchBox.cshtml                # Search component
│   ├── _FilterDropdown.cshtml           # Simple filter (Active/Inactive)
│   ├── _FilterSelect.cshtml             # Complex filter (Department, Station)
│   └── _ActionButtons.cshtml            # Row actions (View/Edit/Delete)
└── Views/Organization/
    └── Categories.cshtml                # CLEAN VIEW (just data!)
```

---

## 🏗️ Architecture Overview

### **Layer 1: View (Categories.cshtml)** - ONLY DATA
```cshtml
// Step 1: Get filter values from controller
var currentSearch = ViewBag.CurrentSearch as string;
var currentStatus = ViewBag.CurrentStatus as string;

// Step 2: Create simple config (NO LOGIC!)
var tableConfig = new TableConfig
{
    TableId = "categoriesTable",
    ActionUrl = "/Organization/Categories",
    SearchPlaceholder = "Search categories...",
    SearchValue = currentSearch,
    Columns = new List<string> { "Category", "Description", ... },
    CreateButtonText = "Add Category",
    CreateButtonUrl = "/Organization/CreateCategory",
    Filters = new List<FilterConfig>
    {
        new FilterConfig
        {
            Label = "Status",
            ParameterName = "status",
            CurrentValue = currentStatus,
            Type = FilterType.Dropdown,
            Options = new List<FilterOptionConfig>
            {
                new FilterOptionConfig { Text = "All", Value = "" },
                new FilterOptionConfig { Text = "Active", Value = "active" }
            }
        }
    }
};

// Step 3: Build table (logic in extension method)
var table = tableConfig.BuildTable(
    tableContent: @<text>...</text>,
    currentUrl: Context.Request.Path + Context.Request.QueryString
);

// Step 4: Render
<partial name="_DataTable.cshtml" model="table" />
```

### **Layer 2: Extension Methods (DataTableExtensions.cs)** - ALL LOGIC
```csharp
public static DataTableViewModel BuildTable(
    this TableConfig config,
    Func<object, HelperResult> tableContent,
    string currentUrl)
{
    // Builds URLs with query parameters
    // Preserves filter state
    // Creates component ViewModels
    // Handles all business logic
}
```

### **Layer 3: Components (_DataTable.cshtml, etc.)** - RENDERING
```cshtml
<!-- Renders search, filters, table, pagination -->
<div class="card">
    <div class="card-header">
        <partial name="_SearchBox.cshtml" />
        <partial name="_FilterDropdown.cshtml" />
        <button data-bs-toggle="collapse">Advanced Filters</button>
    </div>
    <div class="collapse">
        <partial name="_FilterSelect.cshtml" />
    </div>
    <table>...</table>
</div>
```

---

## 🔄 Data Flow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. USER REQUESTS: /Organization/Categories?status=active    │
└────────────────────────┬────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. CONTROLLER: OrganizationController.Categories()          │
│    - Receives: search, status query parameters              │
│    - Filters data in database (server-side)                 │
│    - Returns: IEnumerable<OrgCategoryViewModel>             │
│    - Sets: ViewBag.CurrentSearch, ViewBag.CurrentStatus     │
└────────────────────────┬────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. VIEW: Categories.cshtml                                   │
│    - Creates TableConfig (simple data object)               │
│    - Calls: tableConfig.BuildTable(...)                     │
└────────────────────────┬────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. EXTENSION: DataTableExtensions.BuildTable()              │
│    - Builds URLs: /Categories?status=active&search=tea      │
│    - Creates SearchBoxViewModel                             │
│    - Creates FilterDropdownViewModel                        │
│    - Creates FilterSelectViewModel (if FilterType.Select)   │
│    - Preserves query parameters across filters              │
│    - Returns: DataTableViewModel                            │
└────────────────────────┬────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. PARTIAL: _DataTable.cshtml                               │
│    - Renders search box (left side)                         │
│    - Renders filter dropdowns (left side)                   │
│    - Renders "Advanced Filters" button (collapsible)        │
│    - Renders create button (right side)                     │
│    - Renders table with data                                │
└─────────────────────────────────────────────────────────────┘
```

---

## 📝 Key Concepts

### **1. TableConfig - Pure Data**
```csharp
var tableConfig = new TableConfig
{
    TableId = "myTable",               // HTML ID
    ActionUrl = "/Controller/Action",  // Where forms submit
    SearchPlaceholder = "Search...",   // Search box text
    SearchValue = currentSearch,       // Current search term
    Columns = new List<string> {...},  // Table column headers
    Filters = new List<FilterConfig> {...} // Filters configuration
};
```

**NO LOGIC HERE!** Just data about what the table should look like.

### **2. FilterConfig - Describe, Don't Implement**
```csharp
new FilterConfig
{
    Label = "Status",              // Display label
    ParameterName = "status",      // Query parameter name
    CurrentValue = currentStatus,  // Current selected value
    Type = FilterType.Dropdown,    // Dropdown or Select
    Options = new List<FilterOptionConfig>
    {
        new FilterOptionConfig { Text = "All", Value = "" },
        new FilterOptionConfig { Text = "Active", Value = "active" }
    }
}
```

**This works for ANY filter!** Department, Station, Status, etc.

### **3. Two Filter Types**

**FilterType.Dropdown** (Simple, in header)
- Direct link navigation
- Example: Status (All/Active/Inactive)
- Appears in header next to search

**FilterType.Select** (Complex, collapsible)
- Form submission with dropdown
- Example: Department (50+ options)
- Appears in collapsible "Advanced Filters" area

### **4. URL Building with State Preservation**

Example: User searches "tea" then filters by "active"

```
Initial:          /Categories
After search:     /Categories?search=tea
After filter:     /Categories?search=tea&status=active
Click "All":      /Categories?search=tea
```

**The extension method handles ALL this logic!**

---

## 🚀 How to Use (Step-by-Step)

### **Step 1: Controller (Server-Side Filtering)**

```csharp
public async Task<IActionResult> Categories(string? search, string? status)
{
    // Start with base query
    var query = _context.OrgCategories.AsQueryable();

    // Apply search filter
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(c => c.CategoryName.Contains(search));
    }

    // Apply status filter
    if (status == "active") query = query.Where(c => c.IsActive);
    if (status == "inactive") query = query.Where(c => !c.IsActive);

    // Execute query
    var categories = await query.ToListAsync();

    // Pass filter values to view
    ViewBag.CurrentSearch = search;
    ViewBag.CurrentStatus = status;

    return View(categories);
}
```

### **Step 2: View (Configuration)**

```cshtml
@using OSHManagement.Extensions
@model IEnumerable<OrgCategoryViewModel>
@{
    var currentSearch = ViewBag.CurrentSearch as string;
    var currentStatus = ViewBag.CurrentStatus as string;

    var tableConfig = new TableConfig
    {
        TableId = "categoriesTable",
        ActionUrl = "/Organization/Categories",
        SearchPlaceholder = "Search categories...",
        SearchValue = currentSearch,
        Columns = new List<string> { "Category", "Status", "Actions" },
        CreateButtonText = "Add Category",
        CreateButtonUrl = "/Organization/CreateCategory",
        Filters = new List<FilterConfig>
        {
            new FilterConfig
            {
                Label = "Status",
                ParameterName = "status",
                CurrentValue = currentStatus,
                Type = FilterType.Dropdown,
                Options = new List<FilterOptionConfig>
                {
                    new FilterOptionConfig { Text = "All", Value = "" },
                    new FilterOptionConfig { Text = "Active", Value = "active" },
                    new FilterOptionConfig { Text = "Inactive", Value = "inactive" }
                }
            }
        }
    };

    var table = tableConfig.BuildTable(
        tableContent: @<text>
            @foreach (var item in Model)
            {
                <tr>
                    <td>@item.CategoryName</td>
                    <td><span class="badge">@item.StatusText</span></td>
                    <td>
                        @{
                            var actions = DataTableExtensions.BuildRowActions(
                                item.OrgCategoryId,
                                "/Organization/Category"
                            );
                        }
                        <partial name="_ActionButtons.cshtml" model="actions" />
                    </td>
                </tr>
            }
        </text>,
        currentUrl: Context.Request.Path + Context.Request.QueryString
    );
}

<div class="row">
    <div class="col-xl-12">
        <partial name="~/Views/Shared/Components/_DataTable.cshtml" model="table" />
    </div>
</div>
```

### **Step 3: That's It!**

You now have:
- ✅ Search box (with preserved filters)
- ✅ Status filter dropdown
- ✅ Create button
- ✅ Action buttons (View/Edit/Delete)
- ✅ Responsive design
- ✅ All URLs correctly built

---

## 🔧 Advanced Examples

### **Example 1: Add Department Filter (Complex Select)**

```csharp
// In Controller
ViewBag.Departments = await _context.Departments
    .Select(d => new { d.DepartmentId, d.DepartmentName })
    .ToListAsync();

// In View
Filters = new List<FilterConfig>
{
    // Status filter (Dropdown - simple)
    new FilterConfig { ... },

    // Department filter (Select - complex, collapsible)
    new FilterConfig
    {
        Label = "Department",
        ParameterName = "departmentId",
        CurrentValue = currentDepartmentId,
        Type = FilterType.Select,  // Collapsible!
        Options = ((List<dynamic>)ViewBag.Departments)
            .Select(d => new FilterOptionConfig
            {
                Text = d.DepartmentName,
                Value = d.DepartmentId.ToString()
            }).ToList()
    }
}
```

### **Example 2: Custom Action Buttons**

```csharp
var actions = DataTableExtensions.BuildRowActions(
    item.EmployeeId,
    "/Organization/Employee",
    new RowActionConfig
    {
        ViewUrl = "/Organization/ViewEmployee/{id}",
        EditUrl = "/Organization/EditEmployee/{id}",
        DeleteConfirmMessage = $"Delete {item.FullName}?",
        CustomActions = new List<CustomRowAction>
        {
            new CustomRowAction
            {
                Text = "Reset Password",
                Url = "/Account/ResetPassword/{id}",
                Icon = "ri-lock-password-line",
                Color = "warning"
            }
        }
    }
);
```

---

## 📊 Benefits

| Feature | Old Way | New Way |
|---------|---------|---------|
| **View Length** | 150+ lines | ~70 lines |
| **URL Building** | Manual in view | Automatic |
| **State Preservation** | Manual tracking | Automatic |
| **Reusability** | Copy-paste | Config only |
| **Maintainability** | Change 10 files | Change 1 extension |
| **Testability** | Hard (view logic) | Easy (pure methods) |

---

## 🎓 Summary

**What You Write (View):**
```cshtml
var config = new TableConfig { ... data ... };
var table = config.BuildTable(...);
<partial name="_DataTable.cshtml" model="table" />
```

**What You Get:**
- Search with state preservation
- Multiple filter types (dropdown + select)
- Collapsible advanced filters
- Action buttons
- Responsive layout
- Consistent URLs
- Zero business logic in view

**The Golden Rule:**
> Views describe WHAT to show, Extensions determine HOW to show it.

---

## 🔍 Troubleshooting

**Q: Filter not working?**
A: Check controller has parameter: `public IActionResult Index(string? status)`

**Q: Search not preserving filter?**
A: Extension method automatically preserves all query params!

**Q: Want to add new filter?**
A: Just add to `Filters` list in config. Zero code changes!

**Q: Custom action button?**
A: Use `RowActionConfig.CustomActions` list.

---

## 📝 Next Steps

1. Test Categories page: `/Organization/Categories`
2. Try search: type "factory"
3. Try filter: click "Status" → "Active"
4. Verify URL: should be `/Categories?search=factory&status=active`
5. Copy pattern for Employees, Departments, Stations!
