# Donut Chart Component - Complete Guide

**Component:** Donut Chart (ApexCharts)  
**Created:** October 2025  
**Status:** ✅ Complete - Ready for Use  
**Architecture:** Follows STATCARD_ARCHITECTURE.md pattern

---

## 📋 Overview

The Donut Chart component displays data in a visually appealing donut/pie chart format using ApexCharts. Perfect for showing distributions, breakdowns, and proportions of incidents, actions, training, equipment, and more.

### **Key Features:**

- ✅ **ApexCharts integration** - Professional, interactive charts
- ✅ **Pre-built common charts** (incidents, actions, training, equipment)
- ✅ **Auto-coloring** by category/severity/status
- ✅ **Responsive design** - Works on all screen sizes
- ✅ **Interactive legends** - Click to show/hide segments
- ✅ **Customizable** - Colors, labels, height, legend position
- ✅ **Show total** in center option
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
│ - Build chart data                           │
│ - Configure colors                           │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│ Layer 4: Partial Component + ApexCharts      │
│ - RENDERING ONLY                             │
└──────────────────────────────────────────────┘
```

---

## 📦 Files Created

### **ViewModels:**
- `Models/ViewModels/Dashboard/DonutChartViewModel.cs`
- `Models/ViewModels/Dashboard/ChartDataItem.cs`

### **Extension Methods:**
- `Extensions/Dashboard/DonutChartExtensions.cs`

### **Components:**
- `Views/Shared/Components/DashboardWidgets/_DonutChart.cshtml`
- `Views/Shared/Components/DashboardWidgets/_SkeletonDonutChart.cshtml`

### **Test Page:**
- `Views/Dashboard/TestDonutCharts.cshtml`

### **Dependencies Added:**
- `Views/Shared/styles.cshtml` - Added ApexCharts CSS
- `Views/Shared/_Scripts.cshtml` - Added ApexCharts JS

---

## 🚀 Quick Start

### **1. Build a Simple Chart**

```csharp
using OSHManagement.Extensions.Dashboard;

var chart = DonutChartExtensions.BuildDonutChart(
    title: "Incident Categories",
    series: new List<decimal> { 25, 15, 35, 10 },
    labels: new List<string> { "Fire", "Chemical", "Slip/Fall", "Equipment" },
    colors: new List<string> { "danger", "warning", "info", "success" }
);
```

### **2. Render in View**

```cshtml
<div class="row">
    <partial name="~/Views/Shared/Components/DashboardWidgets/_DonutChart.cshtml" model="chart" />
</div>
```

---

## 📊 Complete Examples

### **Example 1: Incident Severity Chart (Pre-built)**

```csharp
@using OSHManagement.Extensions.Dashboard

@{
    var chart = DonutChartExtensions.BuildIncidentSeverityChart(
        fatal: 2,
        major: 8,
        minor: 25,
        nearMiss: 15
    );
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_DonutChart.cshtml" model="chart" />
```

**Output:**
- ✅ Fatal: Red segment
- ✅ Major: Red segment
- ✅ Minor: Orange segment
- ✅ Near Miss: Blue segment
- ✅ Subtitle: "Total: 50 incidents"

### **Example 2: Action Status Chart (Pre-built)**

```csharp
@{
    var chart = DonutChartExtensions.BuildActionStatusChart(
        completed: 45,
        inProgress: 12,
        pending: 8,
        overdue: 5
    );
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_DonutChart.cshtml" model="chart" />
```

**Output:**
- ✅ Completed: Green
- ✅ In Progress: Blue
- ✅ Pending: Orange
- ✅ Overdue: Red

### **Example 3: Training Compliance Chart (Pre-built)**

```csharp
@{
    var chart = DonutChartExtensions.BuildTrainingComplianceChart(
        compliant: 120,
        partial: 35,
        nonCompliant: 15
    );
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_DonutChart.cshtml" model="chart" />
```

### **Example 4: Equipment Condition Chart (Pre-built)**

```csharp
@{
    var chart = DonutChartExtensions.BuildEquipmentConditionChart(
        excellent: 45,
        good: 78,
        fair: 23,
        poor: 8
    );
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_DonutChart.cshtml" model="chart" />
```

### **Example 5: Department Distribution (Pre-built)**

```csharp
@{
    var departmentCounts = new Dictionary<string, int>
    {
        { "Production", 45 },
        { "Maintenance", 23 },
        { "Warehouse", 18 },
        { "Office", 12 },
        { "Security", 8 }
    };

    var chart = DonutChartExtensions.BuildDepartmentDistributionChart(
        departmentCounts,
        title: "Employees by Department"
    );
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_DonutChart.cshtml" model="chart" />
```

**Features:**
- ✅ Auto-assigns colors from palette
- ✅ Cycles through colors if more than 8 categories

### **Example 6: Custom Chart from Data Items**

```csharp
@{
    var data = new List<ChartDataItem>
    {
        new() { Label = "Fire Extinguishers", Value = 45, Color = "danger" },
        new() { Label = "First Aid Kits", Value = 32, Color = "success" },
        new() { Label = "Safety Helmets", Value = 78, Color = "warning" },
        new() { Label = "Eye Protection", Value = 56, Color = "info" }
    };

    var chart = DonutChartExtensions.BuildDonutChartFromData(
        title: "PPE Inventory Distribution",
        data: data,
        subtitle: "Total: 211 items",
        showLegend: true,
        height: 320
    );
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_DonutChart.cshtml" model="chart" />
```

### **Example 7: Chart with Total in Center**

```csharp
@{
    var chart = DonutChartExtensions.BuildIncidentSeverityChart(30, 45, 78, 22);
    chart.ShowTotal = true;
    chart.TotalLabel = "Total Incidents";
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_DonutChart.cshtml" model="chart" />
```

**Output:**
- ✅ Shows "175" in the center
- ✅ Label: "Total Incidents"

### **Example 8: Chart with Values in Legend (Clean Donut)**

```csharp
@{
    var chart = DonutChartExtensions.BuildActionStatusChart(45, 12, 8, 5);
    chart.ShowValuesInLegend = true; // Values appear in legend
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_DonutChart.cshtml" model="chart" />
```

**Output:**
- ✅ Clean donut with no labels on segments
- ✅ Legend shows: "Completed: 45", "In Progress: 12", "Pending: 8", "Overdue: 5"
- ✅ `ShowDataLabels` automatically disabled when `ShowValuesInLegend = true`

**Features:**
- ✅ Cleaner, more professional appearance
- ✅ Better for charts with many segments
- ✅ Values prominently displayed in legend
- ✅ No overlapping labels on donut

---

## 🔧 Extension Methods

### **1. BuildDonutChart** - Basic chart builder
```csharp
var chart = DonutChartExtensions.BuildDonutChart(
    title: "My Chart",
    series: new List<decimal> { 25, 35, 20, 15 },
    labels: new List<string> { "A", "B", "C", "D" },
    colors: new List<string> { "primary", "success", "warning", "danger" },
    subtitle: "Total: 95 items",
    showLegend: true,
    height: 320
);
```

### **2. BuildDonutChartFromData** - From data items
```csharp
var data = new List<ChartDataItem>
{
    new() { Label = "Category A", Value = 25, Color = "primary" },
    new() { Label = "Category B", Value = 35, Color = "success" }
};

var chart = DonutChartExtensions.BuildDonutChartFromData(
    title: "My Chart",
    data: data,
    subtitle: "Total: 60 items"
);
```

### **3. BuildIncidentSeverityChart** - Pre-built
```csharp
var chart = DonutChartExtensions.BuildIncidentSeverityChart(
    fatal: 2,
    major: 8,
    minor: 25,
    nearMiss: 15
);
```

### **4. BuildActionStatusChart** - Pre-built
```csharp
var chart = DonutChartExtensions.BuildActionStatusChart(
    completed: 45,
    inProgress: 12,
    pending: 8,
    overdue: 5
);
```

### **5. BuildTrainingComplianceChart** - Pre-built
```csharp
var chart = DonutChartExtensions.BuildTrainingComplianceChart(
    compliant: 120,
    partial: 35,
    nonCompliant: 15
);
```

### **6. BuildEquipmentConditionChart** - Pre-built
```csharp
var chart = DonutChartExtensions.BuildEquipmentConditionChart(
    excellent: 45,
    good: 78,
    fair: 23,
    poor: 8
);
```

### **7. BuildDepartmentDistributionChart** - Pre-built
```csharp
var departmentCounts = new Dictionary<string, int>
{
    { "Production", 45 },
    { "Maintenance", 23 }
};

var chart = DonutChartExtensions.BuildDepartmentDistributionChart(
    departmentCounts,
    title: "Distribution by Department"
);
```

---

## 🎨 Color Options

### **Available Color Classes:**
- `primary` - Blue (#6366f1)
- `secondary` - Gray (#6c757d)
- `success` - Green (#22c55e)
- `danger` - Red (#ef4444)
- `warning` - Orange (#f59e0b)
- `info` - Cyan (#06b6d4)
- `purple` - Purple (#8b5cf6)
- `pink` - Pink (#ec4899)
- `orange` - Orange (#f97316)
- `teal` - Teal (#14b8a6)

### **Or use hex colors directly:**
```csharp
colors: new List<string> { "#6366f1", "#22c55e", "#f59e0b" }
```

---

## 📝 Chart Customization

### **Configure Chart Properties:**

```csharp
var chart = DonutChartExtensions.BuildDonutChart(/* ... */);

// Customize appearance
chart.Height = 400;
chart.ShowLegend = true;
chart.LegendPosition = "bottom"; // bottom, top, left, right
chart.ShowDataLabels = true;
chart.ShowValuesInLegend = false; // Show values in legend instead of on donut
chart.ShowTotal = true;
chart.TotalLabel = "Total Items";
chart.Icon = "ri-pie-chart-line";
chart.ViewAllUrl = "/Reports/Details";
```

### **Property Details:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Height` | int | 320 | Chart height in pixels |
| `ShowLegend` | bool | true | Show/hide legend |
| `LegendPosition` | string | "bottom" | Legend position: "bottom", "top", "left", "right" |
| `ShowDataLabels` | bool | true | Show values on donut segments |
| `ShowValuesInLegend` | bool | false | Show values in legend (auto-disables ShowDataLabels) |
| `ShowTotal` | bool | false | Show total value in center |
| `TotalLabel` | string | "Total" | Label for center total |
| `Icon` | string | null | Header icon class |
| `ViewAllUrl` | string | null | "View All" link URL |

---

## 🧪 Testing

### **View Test Page:**

Navigate to: `https://localhost:xxxx/Dashboard/TestDonutCharts`

This page shows:
- Incident severity chart
- Action status chart
- Training compliance chart
- Equipment condition chart
- Department distribution chart
- Custom PPE inventory chart
- Risk assessment chart
- Chart with total in center
- Chart with values in legend (clean donut, no labels)
- Skeleton loading state
- Code examples

---

## ✅ Best Practices

### **DO:**
✅ Use extension methods for ALL logic  
✅ Use pre-built charts for common scenarios  
✅ Filter out zero values (auto-done in pre-built methods)  
✅ Provide meaningful labels and subtitles  
✅ Use appropriate colors for categories  
✅ Keep segments to reasonable numbers (4-8 ideal)  
✅ Use `ShowValuesInLegend = true` for cleaner appearance and better readability  
✅ Use `ShowValuesInLegend = true` when chart has many segments (6+)  

### **DON'T:**
❌ Put logic in views or components  
❌ Hardcode colors or data in views  
❌ Skip empty state handling  
❌ Use too many segments (makes chart unreadable)  
❌ Forget to include ApexCharts CSS/JS  

---

## 🚀 Common Use Cases in OSH

### **1. Incident Severity Distribution**
```csharp
var chart = DonutChartExtensions.BuildIncidentSeverityChart(fatal, major, minor, nearMiss);
```

### **2. Action Item Status**
```csharp
var chart = DonutChartExtensions.BuildActionStatusChart(completed, inProgress, pending, overdue);
```

### **3. Training Compliance**
```csharp
var chart = DonutChartExtensions.BuildTrainingComplianceChart(compliant, partial, nonCompliant);
```

### **4. Equipment Health**
```csharp
var chart = DonutChartExtensions.BuildEquipmentConditionChart(excellent, good, fair, poor);
```

### **5. Department Breakdown**
```csharp
var chart = DonutChartExtensions.BuildDepartmentDistributionChart(departmentCounts);
```

---

## 📚 ApexCharts Integration

### **Files Added:**
- **CSS:** `~/lib/apexcharts/dist/apexcharts.css` (added to `styles.cshtml`)
- **JS:** `~/lib/apexcharts/dist/apexcharts.min.js` (added to `_Scripts.cshtml`)

### **Chart Options Configured:**
- ✅ Donut type with 70% inner radius
- ✅ Responsive breakpoints
- ✅ Custom font family (Inter)
- ✅ Interactive legends
- ✅ Data labels with formatting
- ✅ Tooltips
- ✅ Total in center (optional)

---

## 🔍 Troubleshooting

### **Chart not rendering?**
1. Check browser console for errors
2. Verify ApexCharts JS is loaded: `typeof ApexCharts !== 'undefined'`
3. Ensure chart ID is unique
4. Check that series has data

### **Colors not showing correctly?**
- Verify color class names match supported colors
- Use hex codes if custom colors needed

### **Legend overlapping on mobile?**
- Legend auto-moves to bottom on small screens (< 480px)
- Adjust height if needed

---

## 📚 References

- **ApexCharts Docs:** https://apexcharts.com/docs/chart-types/pie-donut/
- **Implementation Plan:** `OSHfiles/Dashboards/implementation-plan.md`
- **Architecture Pattern:** `OSHfiles/Codingdocs/STATCARD_ARCHITECTURE.md`
- **Test Page:** `/Dashboard/TestDonutCharts`

---

**Component Status:** ✅ Complete and Ready for Production Use  
**Created By:** OSH Development Team  
**Last Updated:** October 2025
