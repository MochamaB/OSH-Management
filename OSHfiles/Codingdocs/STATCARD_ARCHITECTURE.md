# Statistic Cards Architecture - Clean & Generic Implementation

## 🎯 Design Philosophy

**Same Pattern as DataTable:**
- Views only provide data (titles, values, icons)
- Controller calculates statistics
- Extension methods handle all logic
- Reusable across all modules

---

## 📁 File Structure

```
OSHManagement/
├── Models/ViewModels/
│   └── StatCardViewModel.cs              # StatsRowConfig & CardType enum
├── Extensions/
│   └── StatCardExtensions.cs             # BuildStatsRow() method
├── Views/Shared/Components/StatisticCards/
│   ├── _LeftBorderCard.cshtml            # ✅ Implemented
│   ├── _TopBorderCard.cshtml             # ⏳ TODO
│   ├── _NoBorderCard.cshtml              # ⏳ TODO
│   └── _BackgroundFillCard.cshtml        # ⏳ TODO
└── Controllers/
    └── OrganizationController.cs         # Calculates stats in ViewBag
```

---

## 🏗️ Architecture Overview

### **Layer 1: Controller - CALCULATIONS**
```csharp
public async Task<IActionResult> Categories(string? search, string? status)
{
    var categories = await query.ToListAsync();

    // Calculate statistics
    ViewBag.TotalCategories = categories.Count;
    ViewBag.ActiveCategories = categories.Count(c => c.IsActive);
    ViewBag.InactiveCategories = categories.Count(c => !c.IsActive);
    ViewBag.TotalStations = categories.Sum(c => c.StationCount);

    return View(categories);
}
```

### **Layer 2: View - CONFIGURATION ONLY**
```cshtml
@{
    var statsConfig = new StatsRowConfig
    {
        Titles = new List<string> { "Total Categories", "Active", "Inactive", "Stations" },
        Values = new List<string>
        {
            ViewBag.TotalCategories.ToString(),
            ViewBag.ActiveCategories.ToString(),
            ViewBag.InactiveCategories.ToString(),
            ViewBag.TotalStations.ToString()
        },
        Icons = new List<string> { "ri-folder-line", "ri-checkbox-circle-line", "ri-close-circle-line", "ri-building-line" },
        ColorThemes = new List<string> { "primary", "success", "danger", "secondary" },
        CardType = CardType.LeftBorderCard
    };

    var statCards = statsConfig.BuildStatsRow();
}

<div class="row">
    @foreach (var card in statCards)
    {
        <partial name="~/Views/Shared/Components/StatisticCards/_LeftBorderCard.cshtml" model="card" />
    }
</div>
```

### **Layer 3: Extension Method - ALL LOGIC**
```csharp
public static List<StatCardViewModel> BuildStatsRow(this StatsRowConfig config)
{
    var cards = new List<StatCardViewModel>();
    var defaultColorThemes = new[] { "primary", "secondary", "success", "warning" };

    for (int i = 0; i < config.Titles.Count; i++)
    {
        var card = new StatCardViewModel
        {
            Title = config.Titles[i],
            Value = config.Values[i],
            Icon = config.Icons[i],
            ColorTheme = config.ColorThemes?[i] ?? defaultColorThemes[i % 4],
            CardType = config.CardType
        };
        cards.Add(card);
    }
    return cards;
}
```

### **Layer 4: Partial Component - RENDERING**
```cshtml
<!-- _LeftBorderCard.cshtml -->
<div class="@Model.ColumnClass">
    <div class="card custom-card dashboard-main-card @Model.ColorTheme">
        <div class="card-body p-4">
            <div class="d-flex align-items-start gap-3">
                <div class="flex-fill">
                    <h6 class="mb-2 fs-12">@Model.Title</h6>
                    <h4 class="fw-medium mb-0">@Model.Value</h4>
                </div>
                <div class="avatar avatar-lg bg-@Model.ColorTheme-transparent">
                    <i class="@Model.Icon fs-24"></i>
                </div>
            </div>
        </div>
    </div>
</div>
```

---

## 🔄 Data Flow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. USER REQUESTS: /Organization/Categories                  │
└────────────────────────┬────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. CONTROLLER: Fetches data & calculates statistics         │
│    ViewBag.TotalCategories = categories.Count;              │
│    ViewBag.ActiveCategories = categories.Count(c => c.IsActive); │
└────────────────────────┬────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. VIEW: Creates StatsRowConfig (simple data object)        │
│    Titles, Values, Icons, ColorThemes                       │
│    Calls: statsConfig.BuildStatsRow()                       │
└────────────────────────┬────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. EXTENSION: StatCardExtensions.BuildStatsRow()            │
│    - Loops through config arrays                            │
│    - Creates StatCardViewModel for each card                │
│    - Assigns default colors if not provided                 │
│    - Returns List<StatCardViewModel>                        │
└────────────────────────┬────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. PARTIAL: _LeftBorderCard.cshtml                          │
│    - Renders card with title, value, icon                   │
│    - Applies color theme classes                            │
└─────────────────────────────────────────────────────────────┘
```

---

## 📝 Key Concepts

### **1. StatsRowConfig - Pure Data**
```csharp
var statsConfig = new StatsRowConfig
{
    Titles = new List<string> { ... },        // Card titles
    Values = new List<string> { ... },        // Values from ViewBag
    Icons = new List<string> { ... },         // Remix icon classes
    ColorThemes = new List<string> { ... },   // Optional: primary, success, etc.
    CardType = CardType.LeftBorderCard,       // Card style type
    ColumnClass = "col-xxl-3 col-xl-6"        // Bootstrap grid
};
```

**NO LOGIC HERE!** Just arrays of data.

### **2. Card Types**

| CardType | Status | Description |
|----------|--------|-------------|
| `LeftBorderCard` | ✅ Done | Card with left border accent (invoice-list.html style) |
| `TopBorderCard` | ⏳ TODO | Card with top border accent |
| `NoBorderCard` | ⏳ TODO | Minimal card without border |
| `BackgroundFillCard` | ⏳ TODO | Card with filled background color |

### **3. Color Themes**
- `primary` (blue)
- `secondary` (purple)
- `success` (green)
- `warning` (orange)
- `danger` (red)
- `info` (cyan)

### **4. Optional Features (for LeftBorderCard)**

**Badge Value:**
```csharp
BadgeValues = new List<string> { "12,345", "4,176", ... }
```

**Trend Indicators:**
```csharp
TrendPercentages = new List<string> { "3.25", "1.16", ... },
TrendDirections = new List<TrendDirection> { TrendDirection.Up, TrendDirection.Down, ... }
```

---

## 🚀 How to Use (Step-by-Step)

### **Step 1: Controller - Calculate Stats**

```csharp
public async Task<IActionResult> Index()
{
    var data = await _context.YourEntities.ToListAsync();

    // Calculate statistics
    ViewBag.TotalRecords = data.Count;
    ViewBag.ActiveRecords = data.Count(x => x.IsActive);
    ViewBag.PendingRecords = data.Count(x => x.Status == "Pending");

    return View(data);
}
```

### **Step 2: View - Configure Cards**

```cshtml
@using OSHManagement.Extensions

@{
    var statsConfig = new StatsRowConfig
    {
        Titles = new List<string> { "Total", "Active", "Pending" },
        Values = new List<string>
        {
            ViewBag.TotalRecords.ToString(),
            ViewBag.ActiveRecords.ToString(),
            ViewBag.PendingRecords.ToString()
        },
        Icons = new List<string> { "ri-file-list-line", "ri-check-line", "ri-time-line" },
        ColorThemes = new List<string> { "primary", "success", "warning" },
        CardType = CardType.LeftBorderCard
    };

    var statCards = statsConfig.BuildStatsRow();
}

<!-- Render stat cards -->
<div class="row">
    @foreach (var card in statCards)
    {
        <partial name="~/Views/Shared/Components/StatisticCards/_LeftBorderCard.cshtml" model="card" />
    }
</div>
```

### **Step 3: That's It!**

You now have beautiful stat cards displaying your data!

---

## 🔧 Advanced Examples

### **Example 1: With Badges and Trends**

```csharp
var statsConfig = new StatsRowConfig
{
    Titles = new List<string> { "Total Revenue", "New Customers" },
    Values = new List<string> { "$471k", "245" },
    Icons = new List<string> { "ri-money-dollar-circle-line", "ri-user-add-line" },
    ColorThemes = new List<string> { "primary", "success" },

    // Optional: Add badges
    BadgeValues = new List<string> { "12,345", "56" },

    // Optional: Add trend indicators
    TrendPercentages = new List<string> { "3.25", "2.5" },
    TrendDirections = new List<TrendDirection> { TrendDirection.Up, TrendDirection.Up },

    CardType = CardType.LeftBorderCard
};
```

### **Example 2: Different Column Sizes**

```csharp
var statsConfig = new StatsRowConfig
{
    Titles = new List<string> { "Total", "Active" },
    Values = new List<string> { "100", "75" },
    Icons = new List<string> { "ri-dashboard-line", "ri-check-line" },
    ColumnClass = "col-md-6"  // 2 cards per row
};
```

---

## 📊 Benefits

| Feature | Old Way | New Way |
|---------|---------|---------|
| **View Length** | Manual card HTML | ~15 lines config |
| **Logic Location** | Scattered in view | Extension method |
| **Reusability** | Copy-paste HTML | Config only |
| **Maintainability** | Change every view | Change 1 partial |
| **Consistency** | Manual styling | Automatic |

---

## 🎓 Summary

**What You Write (View):**
```cshtml
var statsConfig = new StatsRowConfig { ... data ... };
var statCards = statsConfig.BuildStatsRow();

@foreach (var card in statCards)
{
    <partial name="~/Views/Shared/Components/StatisticCards/_LeftBorderCard.cshtml" model="card" />
}
```

**What You Get:**
- Professional dashboard cards
- Consistent styling
- Automatic color themes
- Icon integration
- Optional badges and trends
- Zero business logic in view

**The Golden Rule:**
> Views describe WHAT to show, Extensions determine HOW to show it.

---

## ⏳ TODO - Pending Card Types

1. **TopBorderCard** - Analyze theme and implement
2. **NoBorderCard** - Analyze theme and implement
3. **BackgroundFillCard** - Analyze theme and implement

Each card type will follow the same pattern but with different CSS classes and structure.

---

## 🔍 Troubleshooting

**Q: Cards not showing?**
A: Check that ViewBag values are set in controller

**Q: Wrong colors?**
A: ColorThemes list must match number of cards, or leave null for defaults

**Q: Icons not visible?**
A: Use Remix icon classes (ri-*), check icon exists in theme

**Q: Want to add custom SVG?**
A: Use CustomSvg property in StatCardViewModel (advanced)

---

## 📝 Real Example - Categories Module

**Controller (OrganizationController.cs:65-69):**
```csharp
ViewBag.TotalCategories = categories.Count;
ViewBag.ActiveCategories = categories.Count(c => c.IsActive);
ViewBag.InactiveCategories = categories.Count(c => !c.IsActive);
ViewBag.TotalStations = categories.Sum(c => c.StationCount);
```

**View (Categories.cshtml:11-28):**
```cshtml
var statsConfig = new StatsRowConfig
{
    Titles = new List<string> { "Total Categories", "Active Categories", "Inactive Categories", "Total Stations" },
    Values = new List<string>
    {
        ViewBag.TotalCategories.ToString(),
        ViewBag.ActiveCategories.ToString(),
        ViewBag.InactiveCategories.ToString(),
        ViewBag.TotalStations.ToString()
    },
    Icons = new List<string> { "ri-folder-line", "ri-checkbox-circle-line", "ri-close-circle-line", "ri-building-line" },
    ColorThemes = new List<string> { "primary", "success", "danger", "secondary" },
    CardType = CardType.LeftBorderCard
};

var statCards = statsConfig.BuildStatsRow();
```

**Result:** 4 beautiful stat cards above the categories table!
