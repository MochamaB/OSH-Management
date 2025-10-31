# Bar & Line Chart Components - Quick Guide

**Components:** Bar Chart & Line Chart (ApexCharts)  
**Created:** October 2025  
**Status:** ✅ Complete - Ready for Use  
**Architecture:** Follows STATCARD_ARCHITECTURE.md pattern

---

## 📋 Overview

Bar and Line Chart components display data using ApexCharts. Perfect for showing trends, comparisons, distributions, and time-series data.

### **Key Features:**

- ✅ **ApexCharts integration** - Professional, interactive charts
- ✅ **Pre-built common charts** for OSH use cases
- ✅ **Multiple variants** - Vertical, horizontal, stacked bars; smooth, area lines
- ✅ **Auto-coloring** by category/metric
- ✅ **Responsive design** - Works on all screen sizes
- ✅ **Interactive** - Zoom, pan, download
- ✅ **Customizable** - Colors, labels, height, legends

---

## 📊 Bar Chart

### **Files Created:**
- `Models/ViewModels/Dashboard/BarChartViewModel.cs`
- `Extensions/Dashboard/BarChartExtensions.cs`
- `Views/Shared/Components/DashboardWidgets/_BarChart.cshtml`
- `Views/Shared/Components/DashboardWidgets/_SkeletonBarChart.cshtml`

### **Pre-built Bar Charts:**

#### **1. Incidents by Month**
```csharp
var data = new Dictionary<string, int>
{
    { "Jan", 12 }, { "Feb", 8 }, { "Mar", 15 }, ...
};
var chart = BarChartExtensions.BuildIncidentsByMonthChart(data, 2025);
```

#### **2. Incidents by Department (Horizontal)**
```csharp
var data = new Dictionary<string, int>
{
    { "Production", 45 }, { "Maintenance", 32 }, ...
};
var chart = BarChartExtensions.BuildIncidentsByDepartmentChart(data);
```

#### **3. Training Completion (Stacked)**
```csharp
var data = new Dictionary<string, (int Completed, int Total)>
{
    { "Production", (45, 60) }, { "Maintenance", (30, 35) }, ...
};
var chart = BarChartExtensions.BuildTrainingCompletionChart(data);
```

#### **4. Actions by Status**
```csharp
var chart = BarChartExtensions.BuildActionsByStatusChart(
    completed: 45,
    inProgress: 12,
    pending: 8,
    overdue: 5
);
```

#### **5. Equipment by Condition**
```csharp
var chart = BarChartExtensions.BuildEquipmentByConditionChart(
    excellent: 45,
    good: 78,
    fair: 23,
    poor: 8
);
```

### **Custom Bar Chart:**
```csharp
var series = new List<BarChartSeriesViewModel>
{
    new() { Name = "2024", Data = new List<decimal> { 10, 20, 15, 25 } },
    new() { Name = "2025", Data = new List<decimal> { 12, 18, 20, 22 } }
};

var chart = BarChartExtensions.BuildBarChart(
    title: "Year Comparison",
    series: series,
    categories: new List<string> { "Q1", "Q2", "Q3", "Q4" },
    colors: new List<string> { "primary", "success" },
    stacked: false
);
```

---

## 📈 Line Chart

### **Files Created:**
- `Models/ViewModels/Dashboard/LineChartViewModel.cs`
- `Extensions/Dashboard/LineChartExtensions.cs`
- `Views/Shared/Components/DashboardWidgets/_LineChart.cshtml`
- `Views/Shared/Components/DashboardWidgets/_SkeletonLineChart.cshtml`

### **Pre-built Line Charts:**

#### **1. Incident Trend (with area fill)**
```csharp
var data = new Dictionary<string, int>
{
    { "Jan", 12 }, { "Feb", 8 }, { "Mar", 15 }, ...
};
var chart = LineChartExtensions.BuildIncidentTrendChart(data, 2025);
```

#### **2. Training Completion Trend**
```csharp
var data = new Dictionary<string, int>
{
    { "Jan", 45 }, { "Feb", 52 }, { "Mar", 48 }, ...
};
var chart = LineChartExtensions.BuildTrainingCompletionTrendChart(data, 2025);
```

#### **3. Incidents vs Actions (Multi-series)**
```csharp
var data = new Dictionary<string, (int Incidents, int Actions)>
{
    { "Jan", (12, 15) }, { "Feb", (8, 10) }, ...
};
var chart = LineChartExtensions.BuildIncidentsVsActionsChart(data, 2025);
```

#### **4. Compliance Rate Trend**
```csharp
var rates = new Dictionary<string, decimal>
{
    { "Jan", 85.5m }, { "Feb", 87.2m }, { "Mar", 88.5m }, ...
};
var chart = LineChartExtensions.BuildComplianceRateTrendChart(rates, 2025);
```

### **Custom Line Chart:**
```csharp
var series = new List<LineChartSeriesViewModel>
{
    new() { Name = "Incidents", Data = new List<decimal> { 10, 8, 15, 12 } },
    new() { Name = "Actions", Data = new List<decimal> { 12, 10, 18, 14 } }
};

var chart = LineChartExtensions.BuildLineChart(
    title: "Monthly Comparison",
    series: series,
    categories: new List<string> { "Jan", "Feb", "Mar", "Apr" },
    colors: new List<string> { "danger", "success" },
    smooth: true,
    showArea: false
);
```

---

## 🎨 Configuration Options

### **Bar Chart Options:**
```csharp
chart.Height = 400;
chart.Horizontal = false;      // true for horizontal bars
chart.Stacked = false;         // true for stacked bars
chart.ShowLegend = true;
chart.ShowDataLabels = false;
chart.ShowGrid = true;
chart.YAxisLabel = "Count";
chart.XAxisLabel = "Months";
```

### **Line Chart Options:**
```csharp
chart.Height = 400;
chart.Smooth = true;           // Smooth or straight lines
chart.ShowArea = false;        // Fill area under line
chart.ShowMarkers = true;      // Show data point markers
chart.ShowLegend = true;
chart.ShowDataLabels = false;
chart.ShowGrid = true;
chart.StrokeWidth = 2;         // Line thickness
chart.YAxisLabel = "Count";
chart.XAxisLabel = "Time";
```

---

## 🚀 Usage

### **Render Bar Chart:**
```cshtml
<div class="row">
    <partial name="~/Views/Shared/Components/DashboardWidgets/_BarChart.cshtml" model="barChart" />
</div>
```

### **Render Line Chart:**
```cshtml
<div class="row">
    <partial name="~/Views/Shared/Components/DashboardWidgets/_LineChart.cshtml" model="lineChart" />
</div>
```

---

## 🧪 Testing

Navigate to: `https://localhost:xxxx/Dashboard/TestCharts`

Shows:
- ✅ Incidents by month (bar chart)
- ✅ Incidents by department (horizontal bar)
- ✅ Training completion (stacked bar)
- ✅ Actions by status (bar chart)
- ✅ Incident trend (line with area)
- ✅ Training completion trend (line with area)
- ✅ Incidents vs actions (multi-series line)
- ✅ Compliance rate trend (line chart)
- ✅ Skeleton loading states
- ✅ Code examples

---

## ✅ Best Practices

### **DO:**
✅ Use pre-built charts for common OSH scenarios  
✅ Use horizontal bars for long category names  
✅ Use stacked bars to show part-to-whole relationships  
✅ Use area charts to emphasize magnitude of change  
✅ Use smooth lines for trends, straight lines for discrete data  
✅ Keep categories/time periods to reasonable numbers (6-12 ideal)  

### **DON'T:**
❌ Use too many series in one chart (3-5 max)  
❌ Use bars for time-series data (use lines instead)  
❌ Skip axis labels for clarity  
❌ Use similar colors for different series  

---

## 📚 Common Use Cases

### **Bar Charts Best For:**
- Comparing categories (departments, statuses)
- Showing rankings or top items
- Part-to-whole with stacked bars
- Horizontal for long labels

### **Line Charts Best For:**
- Time-series trends
- Comparing trends over time
- Showing rate of change
- Multi-metric comparisons

---

## 🎯 Helper Methods

### **Get Last 12 Months:**
```csharp
var months = LineChartExtensions.GetLast12Months();
// Returns: ["Dec", "Jan", "Feb", "Mar", ...]
```

### **Get Last N Months:**
```csharp
var months = LineChartExtensions.GetLastMonths(6);
// Returns last 6 month names
```

---

**Components Status:** ✅ Complete and Ready for Production Use  
**Created By:** OSH Development Team  
**Last Updated:** October 2025
