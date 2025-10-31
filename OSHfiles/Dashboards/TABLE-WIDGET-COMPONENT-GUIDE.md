# Table Widget Component - Complete Guide

**Component:** Table Widget  
**Created:** October 2025  
**Status:** ✅ Complete - Ready for Use  
**Architecture:** Follows STATCARD_ARCHITECTURE.md pattern

---

## 📋 Overview

The Table Widget component displays tabular data with columns and rows. Perfect for showing lists of incidents, actions, equipment, training compliance, and any structured data.

### **Key Features:**

- ✅ **Multiple column types** (text, number, badge, date, percentage, currency, icon, link)
- ✅ **Pre-built common tables** (incidents, actions, equipment, training)
- ✅ **Auto-formatting** (dates, percentages, currency)
- ✅ **Status badges** with auto-coloring
- ✅ **Clickable rows** with link support
- ✅ **Striped, bordered, hoverable** variants
- ✅ **Responsive design** with horizontal scrolling
- ✅ **Empty state handling**

---

## 🏗️ Architecture (4-Layer Pattern)

```
┌─────────────────────────────────────────────┐
│ Layer 1: Controller                          │
│ - Calls extension methods only               │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│ Layer 2: View                                │
│ - Partial rendering                          │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│ Layer 3: Extension Methods                   │
│ - ALL LOGIC HERE                             │
│ - Build columns and rows                     │
│ - Format data                                │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│ Layer 4: Partial Component                   │
│ - RENDERING ONLY                             │
└──────────────────────────────────────────────┘
```

---

## 📦 Files Created

### **ViewModels:**
- `Models/ViewModels/Dashboard/TableWidgetViewModel.cs`
- `Models/ViewModels/Dashboard/TableColumnViewModel.cs`
- `Models/ViewModels/Dashboard/TableRowViewModel.cs`
- `Models/ViewModels/Dashboard/TableBadgeData.cs`
- `Models/ViewModels/Dashboard/TableIconData.cs`

### **Extension Methods:**
- `Extensions/Dashboard/TableWidgetExtensions.cs`

### **Components:**
- `Views/Shared/Components/DashboardWidgets/_TableWidget.cshtml`
- `Views/Shared/Components/DashboardWidgets/_SkeletonTableWidget.cshtml`

### **Test Page:**
- `Views/Dashboard/TestTableWidgets.cshtml`

---

## 🚀 Quick Start

### **1. Build a Simple Table**

```csharp
using OSHManagement.Extensions.Dashboard;

var columns = new List<TableColumnViewModel>
{
    TableWidgetExtensions.BuildColumn("Name", "Name", ColumnType.Text),
    TableWidgetExtensions.BuildColumn("Status", "Status", ColumnType.Badge),
    TableWidgetExtensions.BuildColumn("Date", "Date", ColumnType.Date)
};

var rows = new List<TableRowViewModel>
{
    TableWidgetExtensions.BuildRow(
        data: new Dictionary<string, object>
        {
            { "Name", "Item 1" },
            { "Status", new TableBadgeData { Text = "Active", ColorClass = "success" } },
            { "Date", DateTime.Now }
        },
        linkUrl: "/Items/Details/1"
    )
};

var table = TableWidgetExtensions.BuildTableWidget(
    title: "My Table",
    columns: columns,
    rows: rows,
    viewAllUrl: "/Items/Index"
);
```

### **2. Render in View**

```cshtml
<div class="row">
    <partial name="~/Views/Shared/Components/DashboardWidgets/_TableWidget.cshtml" model="table" />
</div>
```

---

## 📊 Complete Examples

### **Example 1: Recent Incidents Table (Pre-built)**

```csharp
@using OSHManagement.Extensions.Dashboard

@{
    var incidents = new List<(int Id, string Title, string Location, string Severity, DateTime Date, string ReportedBy)>
    {
        (1, "Worker slipped on wet floor", "Main Workshop", "Minor", DateTime.Now.AddHours(-2), "John Doe"),
        (2, "Chemical spill", "Storage Room B", "Major", DateTime.Now.AddHours(-5), "Jane Smith"),
        (3, "Near miss with forklift", "Warehouse Zone 3", "Near Miss", DateTime.Now.AddDays(-1), "Mike Johnson")
    };

    var table = TableWidgetExtensions.BuildRecentIncidentsTable(incidents, maxRows: 10);
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_TableWidget.cshtml" model="table" />
```

**Features:**
- ✅ Auto-colored severity badges (Red for Major, Orange for Minor)
- ✅ Date formatting ("Today", "Yesterday", "MMM dd, yyyy")
- ✅ Clickable rows linking to incident details

### **Example 2: Actions Table (Pre-built)**

```csharp
@{
    var actions = new List<(int Id, string Title, string AssignedTo, string Status, DateTime DueDate, string Priority)>
    {
        (1, "Complete safety training", "John Doe", "In Progress", DateTime.Now.AddDays(3), "High"),
        (2, "Inspect fire extinguishers", "Jane Smith", "Pending", DateTime.Now.AddDays(1), "Medium"),
        (3, "Review incident report", "Mike Johnson", "Overdue", DateTime.Now.AddDays(-2), "High")
    };

    var table = TableWidgetExtensions.BuildActionsTable(actions, maxRows: 10);
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_TableWidget.cshtml" model="table" />
```

**Features:**
- ✅ Status badges (Overdue = Red, In Progress = Orange, Completed = Green)
- ✅ Priority badges (High = Red, Medium = Orange, Low = Blue)
- ✅ Clickable rows

### **Example 3: Equipment Table (Pre-built)**

```csharp
@{
    var equipment = new List<(int Id, string Name, string Type, string Status, DateTime LastInspection, string Condition)>
    {
        (1, "Fire Extinguisher FE-001", "Safety Equipment", "Active", DateTime.Now.AddDays(-30), "Good"),
        (2, "First Aid Kit FK-023", "Medical", "Active", DateTime.Now.AddDays(-15), "Fair"),
        (3, "Safety Helmet SH-145", "PPE", "Active", DateTime.Now.AddDays(-5), "Excellent")
    };

    var table = TableWidgetExtensions.BuildEquipmentTable(equipment, maxRows: 10);
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_TableWidget.cshtml" model="table" />
```

### **Example 4: Training Compliance Table (Pre-built)**

```csharp
@{
    var training = new List<(int Id, string EmployeeName, string Department, int CompletedCourses, int TotalCourses, decimal ComplianceRate)>
    {
        (1, "John Doe", "Production", 8, 10, 80.0m),
        (2, "Jane Smith", "Maintenance", 10, 10, 100.0m),
        (3, "Mike Johnson", "Warehouse", 5, 10, 50.0m)
    };

    var table = TableWidgetExtensions.BuildTrainingComplianceTable(training, maxRows: 10);
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_TableWidget.cshtml" model="table" />
```

**Features:**
- ✅ Percentage formatting (80.0%)
- ✅ Compliance status badges (Compliant = Green, Partial = Orange, Non-Compliant = Red)
- ✅ Auto-calculated based on compliance rate

---

## 🔧 Column Types

### **Available Column Types:**

| Type | Description | Example |
|------|-------------|---------|
| `Text` | Plain text | "John Doe" |
| `Number` | Numeric value (right-aligned) | 42 |
| `Badge` | Status badge with color | <span style="background: #e7f3ff; padding: 2px 8px; border-radius: 4px; color: #0066cc;">Active</span> |
| `Icon` | Icon with color | 🔴 ⚠️ ✅ |
| `Date` | Formatted date | "Today", "Yesterday", "Jan 15, 2025" |
| `DateTime` | Full date and time | "Jan 15, 2025 2:30 PM" |
| `Percentage` | Formatted percentage | "75.5%" |
| `Currency` | Formatted currency | "$1,234.56" |
| `Link` | Clickable hyperlink | <a href="#">Click here</a> |
| `Action` | Action buttons | [View] [Edit] [Delete] |

---

## 🎨 Badge Colors (Auto-Applied)

### **By Status:**
- **Open/Pending/In Progress:** `warning` (Orange)
- **Closed/Completed/Resolved:** `success` (Green)
- **Overdue/Critical/Urgent:** `danger` (Red)
- **Cancelled/Rejected:** `secondary` (Gray)
- **Approved/Active:** `success` (Green)
- **Draft:** `info` (Blue)

### **By Severity:**
- **Fatal/Major:** `danger` (Red)
- **Minor:** `warning` (Orange)
- **Near Miss:** `info` (Blue)

### **By Priority:**
- **High/Critical/Urgent:** `danger` (Red)
- **Medium/Normal:** `warning` (Orange)
- **Low:** `info` (Blue)

---

## 📝 Extension Methods

### **1. BuildTableWidget** - Standard table
```csharp
var table = TableWidgetExtensions.BuildTableWidget(
    title: "My Table",
    columns: columns,
    rows: rows,
    viewAllUrl: "/Items/Index",
    viewAllText: "See All",
    striped: true,
    hoverable: true,
    maxRows: 10
);
```

### **2. BuildRecentIncidentsTable** - Pre-built
```csharp
var incidents = new List<(int Id, string Title, string Location, string Severity, DateTime Date, string ReportedBy)> { /* ... */ };
var table = TableWidgetExtensions.BuildRecentIncidentsTable(incidents, maxRows: 10);
```

### **3. BuildActionsTable** - Pre-built
```csharp
var actions = new List<(int Id, string Title, string AssignedTo, string Status, DateTime DueDate, string Priority)> { /* ... */ };
var table = TableWidgetExtensions.BuildActionsTable(actions, maxRows: 10);
```

### **4. BuildEquipmentTable** - Pre-built
```csharp
var equipment = new List<(int Id, string Name, string Type, string Status, DateTime LastInspection, string Condition)> { /* ... */ };
var table = TableWidgetExtensions.BuildEquipmentTable(equipment, maxRows: 10);
```

### **5. BuildTrainingComplianceTable** - Pre-built
```csharp
var training = new List<(int Id, string EmployeeName, string Department, int CompletedCourses, int TotalCourses, decimal ComplianceRate)> { /* ... */ };
var table = TableWidgetExtensions.BuildTrainingComplianceTable(training, maxRows: 10);
```

### **6. BuildColumn** - Single column
```csharp
var column = TableWidgetExtensions.BuildColumn(
    header: "Status",
    propertyName: "Status",
    type: ColumnType.Badge,
    cssClass: "text-center",
    sortable: true,
    width: 15
);
```

### **7. BuildRow** - Single row
```csharp
var row = TableWidgetExtensions.BuildRow(
    data: new Dictionary<string, object>
    {
        { "Name", "Item 1" },
        { "Status", new TableBadgeData { Text = "Active", ColorClass = "success" } }
    },
    linkUrl: "/Items/Details/1",
    id: "1"
);
```

### **8. Formatting Utilities**
```csharp
// Format date
string date = TableWidgetExtensions.FormatDate(DateTime.Now); // "Today"

// Format percentage
string percent = TableWidgetExtensions.FormatPercentage(75.5m); // "75.5%"

// Format currency
string amount = TableWidgetExtensions.FormatCurrency(1234.56m); // "$1,234.56"
```

---

## 🧪 Testing

### **View Test Page:**

Navigate to: `https://localhost:xxxx/Dashboard/TestTableWidgets`

This page shows:
- Recent incidents table
- Actions table
- Equipment table
- Training compliance table
- Custom table with manual columns/rows
- Skeleton loading states
- Code examples

---

## ✅ Best Practices

### **DO:**
✅ Use extension methods for ALL logic  
✅ Use pre-built tables for common scenarios  
✅ Use appropriate column types for data  
✅ Provide meaningful column headers  
✅ Include link URLs for drill-down  
✅ Use auto-formatting for dates, percentages, currency  

### **DON'T:**
❌ Put logic in views or components  
❌ Hardcode colors or formatting in views  
❌ Skip empty state handling  
❌ Forget to set proper column types  
❌ Mix different data types in same column  

---

## 🚀 Common Use Cases in OSH

### **1. Recent Incidents**
```csharp
var table = TableWidgetExtensions.BuildRecentIncidentsTable(incidents, 10);
```

### **2. Action Items**
```csharp
var table = TableWidgetExtensions.BuildActionsTable(actions, 10);
```

### **3. Equipment Status**
```csharp
var table = TableWidgetExtensions.BuildEquipmentTable(equipment, 10);
```

### **4. Training Compliance**
```csharp
var table = TableWidgetExtensions.BuildTrainingComplianceTable(training, 10);
```

### **5. Custom Inventory Table**
```csharp
var columns = new List<TableColumnViewModel>
{
    TableWidgetExtensions.BuildColumn("Item", "Item", ColumnType.Text),
    TableWidgetExtensions.BuildColumn("Quantity", "Quantity", ColumnType.Number),
    TableWidgetExtensions.BuildColumn("Status", "Status", ColumnType.Badge)
};

var rows = /* build from data */;
var table = TableWidgetExtensions.BuildTableWidget("Inventory", columns, rows);
```

---

## 📚 References

- **Implementation Plan:** `OSHfiles/Dashboards/implementation-plan.md`
- **Architecture Pattern:** `OSHfiles/Codingdocs/STATCARD_ARCHITECTURE.md`
- **Test Page:** `/Dashboard/TestTableWidgets`

---

**Component Status:** ✅ Complete and Ready for Production Use  
**Created By:** OSH Development Team  
**Last Updated:** October 2025
