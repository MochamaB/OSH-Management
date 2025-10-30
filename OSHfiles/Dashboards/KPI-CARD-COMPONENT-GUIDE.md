# KPI Card Component - Complete Guide

**Component:** KPI Card Widget  
**Created:** October 2025  
**Status:** ✅ Complete - Ready for Use  
**Architecture:** Follows STATCARD_ARCHITECTURE.md pattern

---

## 📋 Overview

The KPI Card component displays key performance indicators with **three distinct visual patterns**. It follows the 4-layer architecture for separation of concerns.

### **Three Patterns Available:**

1. **Pattern A - Standard KPI Card**
   - Icon + Label + Value + Trend + Badge
   - Best for: Quick metrics overview
   - Component: `_KPICard.cshtml`

2. **Pattern B - KPI Card with Trend Emphasis**
   - Nested Avatar + Prominent Trend Display
   - Best for: Highlighting change/movement
   - Component: `_KPICardWithTrend.cshtml`

3. **Pattern C - KPI Card with Sparkline**
   - Mini Chart + Value Display
   - Best for: Showing trend visually
   - Component: `_KPICardWithSparkline.cshtml`

---

## 🏗️ Architecture (4-Layer Pattern)

```
┌─────────────────────────────────────────────┐
│ Layer 1: Controller                          │
│ - Calls extension methods only               │
│ - No logic, just data passing                │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│ Layer 2: View                                │
│ - Foreach loop with partials                 │
│ - No logic, just rendering calls             │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│ Layer 3: Extension Methods                   │
│ - ALL LOGIC HERE                             │
│ - Build ViewModels                           │
│ - Calculate trends                           │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│ Layer 4: Partial Component                   │
│ - RENDERING ONLY                             │
│ - No logic, just HTML/Razor                  │
└──────────────────────────────────────────────┘
```

---

## 📦 Files Created

### **ViewModel:**
- `Models/ViewModels/Dashboard/KPICardViewModel.cs`

### **Extension Methods:**
- `Extensions/Dashboard/KPICardExtensions.cs`

### **Components:**
- `Views/Shared/Components/DashboardWidgets/_KPICard.cshtml`
- `Views/Shared/Components/DashboardWidgets/_KPICardWithTrend.cshtml`
- `Views/Shared/Components/DashboardWidgets/_KPICardWithSparkline.cshtml`
- `Views/Shared/Components/DashboardWidgets/_SkeletonKPICard.cshtml`

### **Test Page:**
- `Views/Dashboard/TestKPICards.cshtml`

---

## 🚀 Quick Start

### **1. Build a Single KPI Card**

```csharp
using OSHManagement.Extensions.Dashboard;

// In your controller or view
var card = KPICardExtensions.BuildKPICard(
    title: "Total Incidents",
    value: "247",
    icon: "ri-alert-line",
    colorTheme: "danger",
    badge: "All Time",
    trendValue: "+12.5%",
    trendDirection: TrendDirection.Up,
    linkUrl: "/Incident/Index"
);
```

### **2. Render in View**

```cshtml
<div class="row">
    <partial name="~/Views/Shared/Components/DashboardWidgets/_KPICard.cshtml" model="card" />
</div>
```

---

## 📊 Complete Examples

### **Example 1: Row of Standard KPI Cards**

```csharp
@using OSHManagement.Extensions.Dashboard
@using OSHManagement.Models.ViewModels.Dashboard

@{
    // Build row of 4 KPI cards
    var kpiCards = KPICardExtensions.BuildKPICardsRow(
        titles: new List<string> { 
            "Total Incidents", 
            "Open Actions", 
            "Training Completed", 
            "Compliance Rate" 
        },
        values: new List<string> { "247", "38", "150", "94%" },
        icons: new List<string> { 
            "ri-alert-line", 
            "ri-todo-line", 
            "ri-book-open-line", 
            "ri-shield-check-line" 
        },
        colorThemes: new List<string> { "danger", "warning", "info", "success" },
        badges: new List<string> { "All Time", "Overdue", "This Quarter", "Q4 2025" },
        trendValues: new List<string> { "+12.5%", "-8.2%", "+15%", "+3.5%" },
        trendDirections: new List<TrendDirection?> { 
            TrendDirection.Up, 
            TrendDirection.Down, 
            TrendDirection.Up, 
            TrendDirection.Up 
        }
    );
}

<div class="row">
    @foreach (var card in kpiCards)
    {
        <partial name="~/Views/Shared/Components/DashboardWidgets/_KPICard.cshtml" model="card" />
    }
</div>
```

### **Example 2: KPI Card with Trend Emphasis**

```csharp
@{
    var card = KPICardExtensions.BuildKPICardWithTrend(
        title: "Monthly Revenue",
        value: "$54,320",
        icon: "ri-money-dollar-circle-line",
        trendValue: "+18.5%",
        trendDirection: TrendDirection.Up,
        colorTheme: "success",
        trendLabel: "vs Last Month",
        linkUrl: "/Finance/Revenue"
    );
}

<div class="row">
    <partial name="~/Views/Shared/Components/DashboardWidgets/_KPICardWithTrend.cshtml" model="card" />
</div>
```

### **Example 3: KPI Card with Sparkline Chart**

```csharp
@{
    var card = KPICardExtensions.BuildKPICardWithSparkline(
        title: "Incidents This Year",
        value: "1,432",
        icon: "ri-alert-line",
        sparklineData: new List<decimal> { 120, 135, 125, 140, 138, 145, 142, 150, 148, 155, 160, 158 },
        colorTheme: "danger",
        subtitle: "Monthly Trend",
        linkUrl: "/Incident/Index"
    );
}

<div class="row">
    <partial name="~/Views/Shared/Components/DashboardWidgets/_KPICardWithSparkline.cshtml" model="card" />
</div>
```

### **Example 4: Dynamic Card from Database Data**

```csharp
// In Controller
public async Task<IActionResult> MyDashboard()
{
    var incidentService = new IncidentDashboardService(_context);
    
    // Get current and previous month counts
    var currentMonthIncidents = await incidentService.GetCurrentMonthIncidentCount();
    var previousMonthIncidents = await incidentService.GetPreviousMonthIncidentCount();
    
    // Build card with auto-calculated trend
    var incidentCard = KPICardExtensions.BuildKPICardFromData(
        title: "Total Incidents",
        value: currentMonthIncidents,
        icon: "ri-alert-line",
        colorTheme: "danger",
        comparisonValue: previousMonthIncidents,
        badgeText: "This Month",
        valueFormat: "N0",
        linkUrl: "/Incident/Index"
    );
    
    ViewBag.IncidentCard = incidentCard;
    return View();
}
```

```cshtml
@* In View *@
<div class="row">
    <partial name="~/Views/Shared/Components/DashboardWidgets/_KPICard.cshtml" 
             model="ViewBag.IncidentCard" />
</div>
```

---

## 🎨 Color Themes

Available color themes (matches Vyzor template):
- `primary` - Blue
- `secondary` - Purple
- `success` - Green
- `danger` - Red
- `warning` - Orange
- `info` - Cyan

---

## 🎯 Icon Options

### **Recommended RemixIcon Icons:**

| Metric Type | Icon | Example |
|-------------|------|---------|
| Incidents | `ri-alert-line` | Total incidents |
| Employees | `ri-group-3-fill` | Employee count |
| Training | `ri-book-open-line` | Training metrics |
| Compliance | `ri-shield-check-line` | Compliance rate |
| Hazards | `ri-error-warning-line` | Hazard count |
| Actions | `ri-todo-line` | Action items |
| Inspections | `ri-search-eye-line` | Inspection count |
| Audits | `ri-file-list-3-line` | Audit metrics |
| Equipment | `ri-tools-line` | Equipment status |
| PPE | `ri-shield-user-line` | PPE compliance |
| Emergency | `ri-alarm-warning-line` | Emergency prep |
| Meetings | `ri-team-line` | Meeting count |

**Helper Method Available:**
```csharp
string icon = KPICardExtensions.GetMetricIcon("incident"); // Returns "ri-alert-line"
string color = KPICardExtensions.GetMetricColorTheme("incident"); // Returns "danger"
```

---

## 🧪 Testing

### **View Test Page:**

Navigate to: `https://localhost:xxxx/Dashboard/TestKPICards`

This page shows:
- All three pattern variations
- Multiple examples
- Skeleton loading states
- Code examples

---

## 🔧 Helper Extension Methods

### **1. BuildKPICard** - Single standard card
```csharp
var card = KPICardExtensions.BuildKPICard(
    title: "Total Incidents",
    value: "247",
    icon: "ri-alert-line",
    colorTheme: "danger",
    badge: "All Time",
    trendValue: "+12.5%",
    trendDirection: TrendDirection.Up,
    linkUrl: "/Incident/Index"
);
```

### **2. BuildKPICardsRow** - Multiple cards at once
```csharp
var cards = KPICardExtensions.BuildKPICardsRow(
    titles: new List<string> { "Incidents", "Actions", "Training" },
    values: new List<string> { "247", "38", "150" },
    icons: new List<string> { "ri-alert-line", "ri-todo-line", "ri-book-line" },
    colorThemes: new List<string> { "danger", "warning", "info" }
);
```

### **3. BuildKPICardWithTrend** - Emphasized trend
```csharp
var card = KPICardExtensions.BuildKPICardWithTrend(
    title: "Monthly Revenue",
    value: "$54,320",
    icon: "ri-money-dollar-circle-line",
    trendValue: "+18.5%",
    trendDirection: TrendDirection.Up,
    colorTheme: "success",
    trendLabel: "vs Last Month"
);
```

### **4. BuildKPICardWithSparkline** - With mini chart
```csharp
var card = KPICardExtensions.BuildKPICardWithSparkline(
    title: "Incidents",
    value: "1,432",
    icon: "ri-alert-line",
    sparklineData: new List<decimal> { 120, 135, 140, 145, 150, 155 },
    colorTheme: "danger"
);
```

### **5. BuildKPICardFromData** - From database values
```csharp
var card = KPICardExtensions.BuildKPICardFromData(
    title: "Training Completion",
    value: 150,
    icon: "ri-book-open-line",
    colorTheme: "info",
    comparisonValue: 120, // Previous period
    badgeText: "This Quarter",
    valueFormat: "N0"
);
```

### **6. Utility Methods**
```csharp
// Calculate trend
var trend = KPICardExtensions.CalculateTrend(150, 120, higherIsBetter: true);

// Format trend percentage
var trendText = KPICardExtensions.FormatTrendPercentage(150, 120); // "+25.0%"

// Get icon for metric type
var icon = KPICardExtensions.GetMetricIcon("incident"); // "ri-alert-line"

// Get color for metric type
var color = KPICardExtensions.GetMetricColorTheme("incident"); // "danger"
```

---

## 📝 ViewModel Properties

```csharp
public class KPICardViewModel
{
    // Required
    public string Title { get; set; }
    public string Value { get; set; }
    public string Icon { get; set; }
    public string ColorTheme { get; set; }
    
    // Optional
    public string? Badge { get; set; }
    public string? BadgeColor { get; set; }
    public string? Subtitle { get; set; }
    public string? LinkUrl { get; set; }
    public string? Tooltip { get; set; }
    
    // Trend (Pattern A & B)
    public string? TrendValue { get; set; }
    public TrendDirection? TrendDirection { get; set; }
    public string? TrendLabel { get; set; }
    
    // Sparkline (Pattern C)
    public string? SparklineId { get; set; }
    public List<decimal>? SparklineData { get; set; }
    public string? SparklineColor { get; set; }
    
    // Layout
    public string ColumnClass { get; set; } // Default: "col-xl-3 col-md-6"
    public KPICardType CardType { get; set; }
}
```

---

## ✅ Best Practices

### **DO:**
✅ Use extension methods for ALL logic  
✅ Keep components as rendering only  
✅ Use appropriate color themes for metric types  
✅ Provide meaningful trend labels  
✅ Include link URLs for drill-down  
✅ Use skeleton states while loading  

### **DON'T:**
❌ Put logic in views or components  
❌ Mix different card patterns in same row  
❌ Hardcode values in views  
❌ Skip accessibility attributes  
❌ Forget to handle null/empty states  

---

## 🚀 Next Steps

After KPI Cards, implement:
1. **List Widget** - Recent items display
2. **Table Widget** - Tabular data
3. **Multi-Stat Card** - Multiple metrics in one card
4. **Progress Widget** - Completion indicators

---

## 📚 References

- **Implementation Plan:** `OSHfiles/Dashboards/implementation-plan.md`
- **Architecture Pattern:** `OSHfiles/Codingdocs/STATCARD_ARCHITECTURE.md`
- **Vyzor Analysis:** `OSHfiles/vyzor/dist/html/widgets.html`
- **Test Page:** `/Dashboard/TestKPICards`

---

**Component Status:** ✅ Complete and Ready for Production Use  
**Created By:** OSH Development Team  
**Last Updated:** October 2025
