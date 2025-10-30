# OSH Management System - Dashboard Component Implementation Plan

**Version:** 1.0  
**Created:** October 2025  
**Status:** Implementation Ready

---

## 📋 Executive Summary

### Objective
Build a reusable dashboard component library enabling rapid development of 12+ dashboards with consistency.

### Approach
Component-based architecture using partial views, strongly-typed ViewModels, and extension methods.

### Timeline
- **Phase 1 (Core Components):** 2 weeks
- **Phase 2 (Chart Integration):** 1 week  
- **Phase 3 (First Dashboard):** 1 week
- **Phase 4 (Remaining Dashboards):** 4 weeks
- **Total:** ~8 weeks

---

## 🔍 Vyzor Template Analysis

### Widget Types Identified from `widgets.html`:

#### **1. KPI Cards (3 Patterns)**
- **Pattern A:** Icon + Label + Value + Trend + Badge
- **Pattern B:** Icon + Label + Value + Sparkline
- **Pattern C:** Nested Avatar + Label + Value + Trend  

**Use Cases:** Total incidents, compliance rate, training completion

#### **2. Chart Widgets**
- Line, Bar, Pie, Donut, Radial, Gauge, Heat Map, Sparkline
- Standard structure: Header + Body (chart) + Optional Footer

#### **3. List Widgets**
- Recent items with icon, title, subtitle, timestamp, badge

#### **4. Table Widgets**
- Mini tables with max 5-10 rows, "View All" link

#### **5. Progress Widgets**
- Label + Progress bar + Stats + Threshold colors

#### **6. Combined Widgets**
- Chart + Legend + Footer stats

---

## 🏗️ Component Architecture

### File Structure

```
Views/Shared/Components/
├── DashboardWidgets/
│   ├── _KPICard.cshtml
│   ├── _KPICardWithTrend.cshtml
│   ├── _KPICardWithSparkline.cshtml
│   ├── _MultiStatCard.cshtml
│   ├── _ProgressWidget.cshtml
│   ├── _ListWidget.cshtml
│   ├── _TableWidget.cshtml
│   ├── _AlertWidget.cshtml
│   ├── _ChartWidget.cshtml
│   └── _EmptyState.cshtml
│
├── Charts/
│   ├── _LineChart.cshtml
│   ├── _BarChart.cshtml
│   ├── _PieChart.cshtml
│   ├── _DonutChart.cshtml
│   ├── _GaugeChart.cshtml
│   └── _HeatMap.cshtml
│
└── Layouts/
    ├── _DashboardGrid.cshtml
    ├── _WidgetCard.cshtml
    └── _DashboardFilters.cshtml

Models/ViewModels/Dashboard/
├── KPICardConfig.cs
├── ChartConfig.cs
├── ListWidgetConfig.cs
├── TableWidgetConfig.cs
└── ProgressWidgetConfig.cs

Extensions/
├── DashboardExtensions.cs
└── ChartExtensions.cs
```

---

## 📅 Implementation Phases

### **Phase 1: Core Components (Week 1-2)**

**Week 1:**
1. Create folder structure
2. Build 4 core widgets: KPI Card, MultiStat, List, Table
3. Create ViewModels
4. Build DashboardExtensions

**Week 2:**
1. Build remaining widgets: Progress, Alert, EmptyState
2. Create layout components
3. Add CSS styling
4. Responsive design

### **Phase 2: Chart Integration (Week 3)**
1. Integrate ApexCharts
2. Create 8 chart components
3. Build ChartExtensions
4. Implement AJAX loading
5. Add export functionality

### **Phase 3: First Dashboard (Week 4)**
1. Build "My Dashboard" as proof of concept
2. Create DashboardController
3. Implement data service
4. Add scope-based filtering
5. User testing

### **Phase 4: Remaining Dashboards (Week 5-8)**
- Week 5: OSH Overview, Incident Management
- Week 6: Compliance, Risk Assessment
- Week 7: Training, Team & Committee
- Week 8: Safety Analytics, Station

---

## 🧩 Component Specifications

### **1. KPI Card**

**ViewModel:**
```csharp
public class KPICardConfig
{
    public string Title { get; set; }
    public string Value { get; set; }
    public string Icon { get; set; }
    public string ColorTheme { get; set; }
    public string? TrendValue { get; set; }
    public TrendDirection? TrendDirection { get; set; }
    public string? Badge { get; set; }
    public string? LinkUrl { get; set; }
}
```

**Usage:**
```csharp
@{
    var kpi = new KPICardConfig
    {
        Title = "Total Incidents",
        Value = "247",
        Icon = "ri-alert-line",
        ColorTheme = "danger",
        TrendValue = "+12.5%",
        TrendDirection = TrendDirection.Up
    };
}
<partial name="~/Views/Shared/Components/DashboardWidgets/_KPICard.cshtml" model="kpi" />
```

### **2. List Widget**

**ViewModel:**
```csharp
public class ListWidgetConfig
{
    public string Title { get; set; }
    public List<ListItemConfig> Items { get; set; }
    public int MaxItems { get; set; } = 5;
    public string? ViewAllUrl { get; set; }
}

public class ListItemConfig
{
    public string Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Icon { get; set; }
    public string? Badge { get; set; }
    public string? Timestamp { get; set; }
    public string? LinkUrl { get; set; }
}
```

### **3. Chart Widget**

**ViewModel:**
```csharp
public class ChartConfig
{
    public string ChartId { get; set; }
    public string Title { get; set; }
    public ChartType Type { get; set; }
    public string? DataSourceUrl { get; set; }
    public int Height { get; set; } = 350;
    public bool ShowLegend { get; set; } = true;
    public int? RefreshInterval { get; set; }
}
```

### **4. Progress Widget**

**ViewModel:**
```csharp
public class ProgressWidgetConfig
{
    public string Title { get; set; }
    public int CurrentValue { get; set; }
    public int TotalValue { get; set; }
    public decimal Percentage => (decimal)CurrentValue / TotalValue * 100;
    public string? Description { get; set; }
    public List<ThresholdConfig>? Thresholds { get; set; }
}
```

### **5. Table Widget**

**ViewModel:**
```csharp
public class TableWidgetConfig
{
    public string Title { get; set; }
    public List<string> ColumnHeaders { get; set; }
    public List<Dictionary<string, object>> Rows { get; set; }
    public string? ViewAllUrl { get; set; }
}
```

---

## 🔌 Integration Strategy

### **Dashboard Controller Pattern**

```csharp
public class DashboardController : Controller
{
    public async Task<IActionResult> MyDashboard()
    {
        var viewModel = new DashboardViewModel
        {
            KPICards = new List<KPICardConfig>
            {
                new() { Title = "My Incidents", Value = "3", Icon = "ri-alert-line" }
            },
            Charts = new List<ChartConfig>
            {
                new() { ChartId = "my-training", Title = "Training Progress", Type = ChartType.Radial }
            }
        };
        
        return View(viewModel);
    }
}
```

### **View Pattern**

```cshtml
@model DashboardViewModel

<partial name="~/Views/Shared/Components/Layouts/_DashboardHeader.cshtml" 
         model='new { Title = "My Dashboard" }' />

<div class="row">
    @foreach (var kpi in Model.KPICards)
    {
        <div class="col-xl-3 col-md-6">
            <partial name="~/Views/Shared/Components/DashboardWidgets/_KPICard.cshtml" model="kpi" />
        </div>
    }
</div>

<div class="row">
    @foreach (var chart in Model.Charts)
    {
        <div class="col-xl-6">
            <partial name="~/Views/Shared/Components/DashboardWidgets/_ChartWidget.cshtml" model="chart" />
        </div>
    }
</div>
```

---

## 🧪 Testing Plan

### **Component Testing**
- Visual testing page with all widget variations
- Responsive testing (mobile, tablet, desktop)
- Browser compatibility (Chrome, Firefox, Edge, Safari)

### **Functional Testing**
- Data loading and display
- AJAX refresh
- Chart interactions
- Link navigation
- Permission-based visibility

### **Performance Testing**
- Page load time < 2 seconds
- Chart rendering < 500ms
- Dashboard with 20+ widgets loads smoothly
- Memory usage profiling

---

## ⚡ Performance Optimization

### **Strategies**
1. **Lazy Loading:** Load charts only when visible
2. **Caching:** Cache dashboard data for 5 minutes
3. **Pagination:** Limit list/table widget items
4. **Async Loading:** Use AJAX for heavy data
5. **Minification:** Bundle and minify JS/CSS
6. **CDN:** Use CDN for ApexCharts library

---

## 📝 Naming Conventions

### **Files**
- Components: `_ComponentName.cshtml` (e.g., `_KPICard.cshtml`)
- ViewModels: `ComponentNameConfig.cs` (e.g., `KPICardConfig.cs`)
- Extensions: `FeatureExtensions.cs` (e.g., `DashboardExtensions.cs`)

### **CSS Classes**
- Widget container: `.widget-{type}` (e.g., `.widget-kpi-card`)
- Theme colors: `.bg-{color}-transparent`, `.text-{color}`
- Sizing: `.avatar-{size}`, `.fs-{size}`

### **IDs**
- Charts: `{dashboard}-{chart-name}` (e.g., `incident-trend-chart`)
- Widgets: `{widget-type}-{index}` (e.g., `kpi-card-1`)

---

## 📚 Documentation Deliverables

1. **Component Library Documentation** - Usage examples for each component
2. **Dashboard Development Guide** - Step-by-step guide to create new dashboards
3. **API Documentation** - Data endpoints and response formats
4. **Style Guide** - Visual design standards
5. **Testing Guide** - How to test dashboard components

---

## ✅ Success Criteria

- ✅ All 12 planned dashboards implemented
- ✅ 100% component reusability
- ✅ Consistent UI/UX across dashboards
- ✅ Page load time < 2 seconds
- ✅ Mobile-responsive on all devices
- ✅ Permission-based access working
- ✅ Scope-based data filtering working
- ✅ No code duplication
- ✅ Comprehensive documentation
- ✅ All tests passing

---

**Next Steps:**
1. Review and approve this plan
2. Set up development environment
3. Begin Phase 1 implementation
4. Schedule weekly progress reviews

---

**Document Maintained By:** OSH Development Team  
**Review Date:** Weekly during implementation  
**Contact:** development@oshmanagement.com
