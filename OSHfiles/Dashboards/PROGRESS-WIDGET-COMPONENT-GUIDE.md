# Progress Widget Component - Complete Guide

**Component:** Progress Widget  
**Created:** October 2025  
**Status:** ✅ Complete - Ready for Use  
**Architecture:** Follows STATCARD_ARCHITECTURE.md pattern

---

## 📋 Overview

The Progress Widget component displays completion and progress metrics with visual progress bars. It supports threshold-based coloring, icons, and various display styles.

### **Key Features:**

- ✅ **Auto-calculated percentages** from current/total values
- ✅ **Threshold-based coloring** (changes color based on percentage)
- ✅ **Multiple display variants** (standard, with icon, detailed)
- ✅ **Pre-built common widgets** (training, compliance, action closure)
- ✅ **Striped and animated** progress bars
- ✅ **Responsive design**

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
│ - Foreach loop with partials                 │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│ Layer 3: Extension Methods                   │
│ - ALL LOGIC HERE                             │
│ - Calculate percentages                      │
│ - Apply thresholds                           │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│ Layer 4: Partial Component                   │
│ - RENDERING ONLY                             │
└──────────────────────────────────────────────┘
```

---

## 📦 Files Created

### **ViewModel:**
- `Models/ViewModels/Dashboard/ProgressWidgetViewModel.cs`

### **Extension Methods:**
- `Extensions/Dashboard/ProgressWidgetExtensions.cs`

### **Components:**
- `Views/Shared/Components/DashboardWidgets/_ProgressWidget.cshtml`
- `Views/Shared/Components/DashboardWidgets/_SkeletonProgressWidget.cshtml`

### **Test Page:**
- `Views/Dashboard/TestProgressWidgets.cshtml`

---

## 🚀 Quick Start

### **1. Build a Simple Progress Widget**

```csharp
using OSHManagement.Extensions.Dashboard;

var widget = ProgressWidgetExtensions.BuildProgressWidget(
    title: "Training Completion",
    currentValue: 150,
    totalValue: 200,
    description: "150 of 200 employees completed",
    colorClass: "primary",
    striped: true
);
```

### **2. Render in View**

```cshtml
<div class="row">
    <partial name="~/Views/Shared/Components/DashboardWidgets/_ProgressWidget.cshtml" model="widget" />
</div>
```

---

## 📊 Complete Examples

### **Example 1: Multiple Progress Widgets**

```csharp
@using OSHManagement.Extensions.Dashboard
@using OSHManagement.Models.ViewModels.Dashboard

@{
    var widgets = ProgressWidgetExtensions.BuildProgressWidgets(
        titles: new List<string> { 
            "Training Completion", 
            "Action Closure", 
            "PPE Compliance" 
        },
        currentValues: new List<int> { 150, 85, 234 },
        totalValues: new List<int> { 200, 100, 250 },
        colorClasses: new List<string> { "primary", "success", "info" },
        striped: true
    );
}

<div class="row">
    @foreach (var widget in widgets)
    {
        <partial name="~/Views/Shared/Components/DashboardWidgets/_ProgressWidget.cshtml" model="widget" />
    }
</div>
```

### **Example 2: Progress with Icon**

```csharp
@{
    var widget = ProgressWidgetExtensions.BuildProgressWidgetWithIcon(
        title: "Safety Training 2025",
        currentValue: 175,
        totalValue: 200,
        icon: "ri-book-open-line",
        description: "175 employees completed mandatory training",
        colorClass: "success"
    );
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_ProgressWidget.cshtml" model="widget" />
```

### **Example 3: Progress with Auto-Color Thresholds**

```csharp
@{
    var widget = ProgressWidgetExtensions.BuildProgressWidgetWithThresholds(
        title: "Overall Compliance Score",
        currentValue: 95,
        totalValue: 100,
        thresholds: ProgressWidgetExtensions.GetStrictThresholds(),
        description: "Excellent compliance across all areas",
        striped: true
    );
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_ProgressWidget.cshtml" model="widget" />
```

**GetStrictThresholds()** automatically colors:
- 0-70%: 🔴 Red (Danger)
- 70-90%: 🟠 Orange (Warning)
- 90-100%: 🟢 Green (Success)

### **Example 4: Pre-built Training Widget**

```csharp
@{
    var widget = ProgressWidgetExtensions.BuildTrainingProgressWidget(
        completed: 180,
        total: 200,
        courseName: "Fire Safety Essentials"
    );
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_ProgressWidget.cshtml" model="widget" />
```

### **Example 5: Pre-built Compliance Widget**

```csharp
@{
    var widget = ProgressWidgetExtensions.BuildComplianceProgressWidget(
        complianceName: "PPE",
        compliantCount: 234,
        totalCount: 250
    );
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_ProgressWidget.cshtml" model="widget" />
```

### **Example 6: From Percentage Only**

```csharp
@{
    // When you only have percentage, not absolute values
    var widget = ProgressWidgetExtensions.BuildProgressWidgetFromPercentage(
        title: "Annual Safety Goal",
        percentage: 87.5m,
        description: "87.5% of goals achieved",
        colorClass: "success"
    );
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_ProgressWidget.cshtml" model="widget" />
```

---

## 🎨 Threshold Presets

### **1. GetDefaultThresholds()**
General purpose thresholds:
- 0-50%: 🔴 **Danger** - "Poor"
- 50-80%: 🟠 **Warning** - "Fair"
- 80-100%: 🟢 **Success** - "Good"

**Use for:** General metrics, training completion

### **2. GetStrictThresholds()**
High standards (compliance-focused):
- 0-70%: 🔴 **Danger** - "Non-Compliant"
- 70-90%: 🟠 **Warning** - "Partial"
- 90-100%: 🟢 **Success** - "Compliant"

**Use for:** Compliance, safety certifications, audits

### **3. GetLenientThresholds()**
Lower standards (progress-focused):
- 0-30%: 🔴 **Danger** - "Low"
- 30-60%: 🟠 **Warning** - "Medium"
- 60-100%: 🟢 **Success** - "High"

**Use for:** New initiatives, exploratory projects

### **4. Custom Thresholds**

```csharp
var customThresholds = new List<ProgressThreshold>
{
    new() { MinValue = 0, MaxValue = 40, ColorClass = "danger", Label = "Critical" },
    new() { MinValue = 40, MaxValue = 75, ColorClass = "warning", Label = "Needs Attention" },
    new() { MinValue = 75, MaxValue = 100, ColorClass = "success", Label = "Excellent" }
};

var widget = ProgressWidgetExtensions.BuildProgressWidgetWithThresholds(
    title: "Custom Progress",
    currentValue: 60,
    totalValue: 100,
    thresholds: customThresholds
);
```

---

## 🔧 Helper Extension Methods

### **1. BuildProgressWidget** - Standard widget
```csharp
var widget = ProgressWidgetExtensions.BuildProgressWidget(
    title: "Training Completion",
    currentValue: 150,
    totalValue: 200,
    description: "150 of 200 employees",
    colorClass: "primary",
    striped: true,
    animated: false,
    linkUrl: "/Training/Index"
);
```

### **2. BuildProgressWidgetWithIcon** - With icon
```csharp
var widget = ProgressWidgetExtensions.BuildProgressWidgetWithIcon(
    title: "Safety Training",
    currentValue: 175,
    totalValue: 200,
    icon: "ri-book-open-line",
    description: "175 completed",
    colorClass: "success",
    linkUrl: "/Training/Details/1"
);
```

### **3. BuildProgressWidgetWithThresholds** - Auto color
```csharp
var widget = ProgressWidgetExtensions.BuildProgressWidgetWithThresholds(
    title: "Compliance",
    currentValue: 95,
    totalValue: 100,
    thresholds: ProgressWidgetExtensions.GetStrictThresholds(),
    description: "Excellent",
    striped: true
);
```

### **4. BuildProgressWidgets** - Multiple at once
```csharp
var widgets = ProgressWidgetExtensions.BuildProgressWidgets(
    titles: new List<string> { "Training", "Actions", "PPE" },
    currentValues: new List<int> { 150, 85, 234 },
    totalValues: new List<int> { 200, 100, 250 },
    colorClasses: new List<string> { "primary", "success", "info" }
);
```

### **5. BuildTrainingProgressWidget** - Pre-built
```csharp
var widget = ProgressWidgetExtensions.BuildTrainingProgressWidget(
    completed: 180,
    total: 200,
    courseName: "Fire Safety"
);
```

### **6. BuildComplianceProgressWidget** - Pre-built
```csharp
var widget = ProgressWidgetExtensions.BuildComplianceProgressWidget(
    complianceName: "PPE",
    compliantCount: 234,
    totalCount: 250
);
```

### **7. BuildActionClosureWidget** - Pre-built
```csharp
var widget = ProgressWidgetExtensions.BuildActionClosureWidget(
    closedActions: 85,
    totalActions: 100
);
```

### **8. Utility Methods**
```csharp
// Get color based on percentage
string color = ProgressWidgetExtensions.GetColorForPercentage(85, "compliance");

// Get icon for progress type
string icon = ProgressWidgetExtensions.GetProgressIcon("training"); // "ri-book-open-line"
```

---

## 🎯 Icon Options

| Progress Type | Icon | Example Use |
|--------------|------|-------------|
| Training | `ri-book-open-line` | Training completion |
| Compliance | `ri-shield-check-line` | Compliance rates |
| Completion | `ri-checkbox-circle-line` | Task completion |
| Actions | `ri-todo-line` | Action items |
| Inspection | `ri-search-eye-line` | Inspection progress |
| Audit | `ri-file-list-3-line` | Audit completion |
| Certification | `ri-award-line` | Certification progress |
| PPE | `ri-shield-user-line` | PPE compliance |
| Equipment | `ri-tools-line` | Equipment maintenance |

**Helper Method:**
```csharp
string icon = ProgressWidgetExtensions.GetProgressIcon("training");
```

---

## 📝 ViewModel Properties

```csharp
public class ProgressWidgetViewModel
{
    // Required
    public string Title { get; set; }
    public int CurrentValue { get; set; }
    public int TotalValue { get; set; }
    
    // Auto-calculated
    public decimal Percentage { get; } // Auto-calculated from Current/Total
    
    // Optional
    public string? Description { get; set; }
    public string? Subtitle { get; set; }
    public string? Icon { get; set; }
    public string? LinkUrl { get; set; }
    
    // Styling
    public string ColorClass { get; set; } = "primary";
    public bool Striped { get; set; } = false;
    public bool Animated { get; set; } = false;
    public int Height { get; set; } = 20; // pixels
    
    // Thresholds
    public List<ProgressThreshold>? Thresholds { get; set; }
    
    // Layout
    public string ColumnClass { get; set; } = "col-xl-6";
    public ProgressWidgetType WidgetType { get; set; }
}
```

---

## 🧪 Testing

### **View Test Page:**

Navigate to: `https://localhost:xxxx/Dashboard/TestProgressWidgets`

This page shows:
- Standard progress widgets
- Progress with icons
- Threshold-based auto-coloring
- Pre-built common widgets
- Skeleton loading states
- Code examples

---

## ✅ Best Practices

### **DO:**
✅ Use extension methods for ALL logic  
✅ Use threshold presets for common scenarios  
✅ Provide meaningful descriptions  
✅ Use pre-built widgets for common use cases  
✅ Include link URLs for drill-down  
✅ Use striped bars for ongoing processes  

### **DON'T:**
❌ Put logic in views or components  
❌ Hardcode percentage calculations in views  
❌ Mix different widget types inconsistently  
❌ Forget to handle zero totals (division by zero)  
❌ Skip accessibility attributes  

---

## 🚀 Common Use Cases in OSH

### **1. Training Completion**
```csharp
var widget = ProgressWidgetExtensions.BuildTrainingProgressWidget(180, 200, "Fire Safety");
```

### **2. Compliance Tracking**
```csharp
var widget = ProgressWidgetExtensions.BuildComplianceProgressWidget("PPE", 234, 250);
```

### **3. Action Closure Rate**
```csharp
var widget = ProgressWidgetExtensions.BuildActionClosureWidget(85, 100);
```

### **4. Inspection Progress**
```csharp
var widget = ProgressWidgetExtensions.BuildProgressWidgetWithIcon(
    "Monthly Inspections",
    45,
    50,
    "ri-search-eye-line",
    colorClass: "info"
);
```

### **5. Certification Status**
```csharp
var widget = ProgressWidgetExtensions.BuildProgressWidgetWithThresholds(
    "Safety Certifications",
    155,
    200,
    ProgressWidgetExtensions.GetStrictThresholds()
);
```

---

## 📚 References

- **Implementation Plan:** `OSHfiles/Dashboards/implementation-plan.md`
- **Architecture Pattern:** `OSHfiles/Codingdocs/STATCARD_ARCHITECTURE.md`
- **Test Page:** `/Dashboard/TestProgressWidgets`

---

**Component Status:** ✅ Complete and Ready for Production Use  
**Created By:** OSH Development Team  
**Last Updated:** October 2025
