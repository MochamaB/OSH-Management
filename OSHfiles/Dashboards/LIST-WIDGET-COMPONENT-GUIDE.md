# List Widget Component - Complete Guide

**Component:** List Widget  
**Created:** October 2025  
**Status:** ✅ Complete - Ready for Use  
**Architecture:** Follows STATCARD_ARCHITECTURE.md pattern

---

## 📋 Overview

The List Widget component displays lists of items with icons, badges, timestamps, and subtitles. Perfect for showing recent incidents, actions, notifications, and activity timelines.

### **Key Features:**

- ✅ **Multiple display variants** (standard, compact, timeline, notification)
- ✅ **Pre-built common widgets** (incidents, actions, notifications, timeline)
- ✅ **Flexible item configuration** (icons, badges, timestamps, avatars)
- ✅ **Empty state handling**
- ✅ **Read/unread state** support for notifications
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
│ - Partial rendering                          │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│ Layer 3: Extension Methods                   │
│ - ALL LOGIC HERE                             │
│ - Build list items                           │
│ - Format timestamps                          │
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
- `Models/ViewModels/Dashboard/ListWidgetViewModel.cs`
- `Models/ViewModels/Dashboard/ListItemViewModel.cs`

### **Extension Methods:**
- `Extensions/Dashboard/ListWidgetExtensions.cs`

### **Components:**
- `Views/Shared/Components/DashboardWidgets/_ListWidget.cshtml` - Standard list widget
- `Views/Shared/Components/DashboardWidgets/_ActivityListWidget.cshtml` - Timeline-style widget with vertical line
- `Views/Shared/Components/DashboardWidgets/_SkeletonListWidget.cshtml` - Loading skeleton

### **Test Page:**
- `Views/Dashboard/TestListWidgets.cshtml`

---

## 🚀 Quick Start

### **1. Build a Simple List Widget**

```csharp
using OSHManagement.Extensions.Dashboard;

var items = ListWidgetExtensions.BuildListItems(
    titles: new List<string> { "Item 1", "Item 2", "Item 3" },
    subtitles: new List<string> { "Subtitle 1", "Subtitle 2", "Subtitle 3" },
    icons: new List<string> { "ri-alert-line", "ri-todo-line", "ri-book-line" }
);

var widget = ListWidgetExtensions.BuildListWidget(
    title: "My List",
    items: items,
    viewAllUrl: "/Items/Index"
);
```

### **2. Render in View**

```cshtml
<div class="row">
    <partial name="~/Views/Shared/Components/DashboardWidgets/_ListWidget.cshtml" model="widget" />
</div>
```

---

## 📊 Complete Examples

### **Example 1: Recent Incidents Widget (Pre-built)**

```csharp
@using OSHManagement.Extensions.Dashboard

@{
    var incidents = new List<(string Title, string Location, string Severity, DateTime Date, int Id)>
    {
        ("Worker slipped on wet floor", "Main Workshop", "Minor", DateTime.Now.AddHours(-2), 1),
        ("Chemical spill", "Storage Room B", "Major", DateTime.Now.AddHours(-5), 2),
        ("Near miss with forklift", "Warehouse Zone 3", "Near Miss", DateTime.Now.AddDays(-1), 3)
    };

    var widget = ListWidgetExtensions.BuildRecentIncidentsWidget(incidents, maxItems: 5);
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_ListWidget.cshtml" model="widget" />
```

**Output:**
- ✅ Automatically colored by severity (red for Major, orange for Minor, etc.)
- ✅ Icons and badges configured
- ✅ Timestamps formatted ("2h ago", "1d ago")
- ✅ Links to incident details

### **Example 2: Recent Actions Widget (Pre-built)**

```csharp
@{
    var actions = new List<(string Title, string AssignedTo, string Status, DateTime DueDate, int Id)>
    {
        ("Complete safety training", "John Doe", "In Progress", DateTime.Now.AddDays(3), 1),
        ("Inspect fire extinguishers", "Jane Smith", "Pending", DateTime.Now.AddDays(1), 2),
        ("Review incident report", "Mike Johnson", "Overdue", DateTime.Now.AddDays(-2), 3)
    };

    var widget = ListWidgetExtensions.BuildRecentActionsWidget(actions, maxItems: 5);
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_ListWidget.cshtml" model="widget" />
```

### **Example 3: Notifications Widget (Pre-built)**

```csharp
@{
    var notifications = new List<(string Title, string Message, DateTime Date, bool IsRead, int Id)>
    {
        ("New incident reported", "Worker slipped in workshop", DateTime.Now.AddMinutes(-30), false, 1),
        ("Action assigned", "Complete training by Friday", DateTime.Now.AddHours(-2), false, 2),
        ("Training reminder", "Fire safety training tomorrow", DateTime.Now.AddHours(-5), true, 3)
    };

    var widget = ListWidgetExtensions.BuildNotificationsWidget(notifications, maxItems: 10);
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_ListWidget.cshtml" model="widget" />
```

**Features:**
- ✅ Shows "New" badge for unread items
- ✅ Highlights unread notifications with light background
- ✅ Timestamps auto-formatted

### **Example 4: Activity Timeline Widget (Pre-built)**

```csharp
@{
    var activities = new List<(string Activity, string User, DateTime Date, string Icon)>
    {
        ("Incident reported", "John Doe", DateTime.Now.AddMinutes(-15), "ri-alert-line"),
        ("Action completed", "Jane Smith", DateTime.Now.AddHours(-1), "ri-checkbox-circle-line"),
        ("Document uploaded", "Mike Johnson", DateTime.Now.AddHours(-3), "ri-file-upload-line")
    };

    var widget = ListWidgetExtensions.BuildActivityTimelineWidget(activities, maxItems: 10);
}

@* Option 1: Standard List Widget *@
<partial name="~/Views/Shared/Components/DashboardWidgets/_ListWidget.cshtml" model="widget" />

@* Option 2: Activity Timeline Widget (with vertical line and colored dots) *@
<partial name="~/Views/Shared/Components/DashboardWidgets/_ActivityListWidget.cshtml" model="widget" />
```

**Features:**
- ✅ Same ViewModels and Extensions for both components
- ✅ `_ListWidget.cshtml` - Standard list display
- ✅ `_ActivityListWidget.cshtml` - Timeline display with vertical line and colored icon dots
- ✅ Timeline auto-connects items with vertical line
- ✅ Last item has no line continuation

### **Example 5: Custom List Items**

```csharp
@{
    var items = new List<ListItemViewModel>
    {
        new() {
            Title = "High Priority Hazard",
            Subtitle = "Exposed electrical wiring",
            Icon = "ri-error-warning-line",
            IconColor = "danger",
            Badge = "Critical",
            BadgeColor = "danger",
            Timestamp = "1h ago",
            LinkUrl = "/Hazard/Details/1"
        },
        new() {
            Title = "Scheduled Maintenance",
            Subtitle = "Fire suppression system",
            Icon = "ri-tools-line",
            IconColor = "info",
            Badge = "Scheduled",
            BadgeColor = "info",
            Timestamp = "Tomorrow"
        }
    };

    var widget = ListWidgetExtensions.BuildListWidget(
        title: "Important Updates",
        items: items,
        viewAllUrl: "/Dashboard/Updates"
    );
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_ListWidget.cshtml" model="widget" />
```

### **Example 6: Compact List (Minimal Spacing)**

```csharp
@{
    var items = ListWidgetExtensions.BuildListItems(
        titles: new List<string> { 
            "Safety Manual v2.3", 
            "Incident Report Form", 
            "PPE Guidelines"
        }
    );

    var widget = ListWidgetExtensions.BuildCompactListWidget(
        title: "Recent Documents",
        items: items,
        viewAllUrl: "/Document/Index",
        maxItems: 15
    );
}

<partial name="~/Views/Shared/Components/DashboardWidgets/_ListWidget.cshtml" model="widget" />
```

---

## 🔧 Helper Extension Methods

### **1. BuildListWidget** - Standard widget
```csharp
var widget = ListWidgetExtensions.BuildListWidget(
    title: "My List",
    items: items,
    viewAllUrl: "/Items/Index",
    viewAllText: "See All",
    showIcons: true,
    showTimestamps: true,
    maxItems: 10
);
```

### **2. BuildRecentIncidentsWidget** - Pre-built
```csharp
var incidents = new List<(string Title, string Location, string Severity, DateTime Date, int Id)> { /* ... */ };
var widget = ListWidgetExtensions.BuildRecentIncidentsWidget(incidents, maxItems: 5);
```

### **3. BuildRecentActionsWidget** - Pre-built
```csharp
var actions = new List<(string Title, string AssignedTo, string Status, DateTime DueDate, int Id)> { /* ... */ };
var widget = ListWidgetExtensions.BuildRecentActionsWidget(actions, maxItems: 5);
```

### **4. BuildNotificationsWidget** - Pre-built
```csharp
var notifications = new List<(string Title, string Message, DateTime Date, bool IsRead, int Id)> { /* ... */ };
var widget = ListWidgetExtensions.BuildNotificationsWidget(notifications, maxItems: 10);
```

### **5. BuildActivityTimelineWidget** - Pre-built
```csharp
var activities = new List<(string Activity, string User, DateTime Date, string Icon)> { /* ... */ };
var widget = ListWidgetExtensions.BuildActivityTimelineWidget(activities, maxItems: 10);
```

### **6. BuildListItems** - Quick list from data
```csharp
var items = ListWidgetExtensions.BuildListItems(
    titles: new List<string> { "Item 1", "Item 2" },
    subtitles: new List<string> { "Subtitle 1", "Subtitle 2" },
    icons: new List<string> { "ri-alert-line", "ri-todo-line" },
    badges: new List<string> { "New", "Pending" },
    timestamps: new List<string> { "2h ago", "1d ago" }
);
```

### **7. BuildListItem** - Single item
```csharp
var item = ListWidgetExtensions.BuildListItem(
    title: "High Priority Task",
    subtitle: "Complete by end of day",
    icon: "ri-alert-line",
    iconColor: "danger",
    badge: "Urgent",
    badgeColor: "danger",
    timestamp: "Due today",
    linkUrl: "/Task/Details/1"
);
```

### **8. Utility Methods**
```csharp
// Format timestamp
string timeAgo = ListWidgetExtensions.GetTimeAgo(DateTime.Now.AddHours(-2)); // "2h ago"

// Get icon for item type
string icon = ListWidgetExtensions.GetIconForItemType("incident"); // "ri-alert-line"

// Get color for status
string color = ListWidgetExtensions.GetColorForStatus("overdue"); // "danger"
```

---

## 🎨 Icon Options

| Item Type | Icon | Example Use |
|-----------|------|-------------|
| Incident | `ri-alert-line` | Incident lists |
| Action | `ri-todo-line` | Action items |
| Training | `ri-book-open-line` | Training sessions |
| Inspection | `ri-search-eye-line` | Inspections |
| Audit | `ri-file-list-3-line` | Audit reports |
| Hazard | `ri-error-warning-line` | Hazard reports |
| Meeting | `ri-team-line` | Meeting schedules |
| Document | `ri-file-text-line` | Documents |
| Notification | `ri-notification-3-line` | Notifications |
| Task | `ri-task-line` | General tasks |

**Helper Method:**
```csharp
string icon = ListWidgetExtensions.GetIconForItemType("incident");
```

---

## 🎯 Color Themes

### **By Status:**
- **Open/Pending/In Progress:** `warning` (Orange)
- **Closed/Completed/Resolved:** `success` (Green)
- **Overdue/Critical/Urgent:** `danger` (Red)
- **Cancelled/Rejected:** `secondary` (Gray)
- **Approved:** `success` (Green)
- **Draft:** `info` (Blue)

**Helper Method:**
```csharp
string color = ListWidgetExtensions.GetColorForStatus("overdue"); // "danger"
```

---

## 📝 ViewModel Properties

### **ListWidgetViewModel:**
```csharp
public string Title { get; set; }
public List<ListItemViewModel> Items { get; set; }
public string? ViewAllUrl { get; set; }
public string? ViewAllText { get; set; } = "View All";
public string? EmptyMessage { get; set; }
public bool ShowIcons { get; set; } = true;
public bool ShowTimestamps { get; set; } = true;
public bool ShowBadges { get; set; } = true;
public int MaxItems { get; set; } = 10;
public string ColumnClass { get; set; } = "col-xl-6";
public ListWidgetType WidgetType { get; set; }
```

### **ListItemViewModel:**
```csharp
public string Title { get; set; }
public string? Subtitle { get; set; }
public string? Icon { get; set; }
public string? IconColor { get; set; }
public string? Badge { get; set; }
public string? BadgeColor { get; set; }
public string? Timestamp { get; set; }
public string? LinkUrl { get; set; }
public bool IsRead { get; set; } = true;
```

---

## 🧪 Testing

### **View Test Page:**

Navigate to: `https://localhost:xxxx/Dashboard/TestListWidgets`

This page shows:
- Recent incidents widget
- Recent actions widget
- Notifications widget
- Activity timeline widget
- Simple and compact lists
- Custom list items
- Skeleton loading states
- Code examples

---

## ✅ Best Practices

### **DO:**
✅ Use extension methods for ALL logic  
✅ Use pre-built widgets for common scenarios  
✅ Format timestamps with `GetTimeAgo()`  
✅ Provide meaningful empty states  
✅ Include link URLs for navigation  
✅ Use appropriate icons and colors  

### **DON'T:**
❌ Put logic in views or components  
❌ Hardcode colors or icons in views  
❌ Skip empty state handling  
❌ Forget accessibility attributes  
❌ Mix different widget types inconsistently  

---

## 🚀 Common Use Cases in OSH

### **1. Recent Incidents**
```csharp
var widget = ListWidgetExtensions.BuildRecentIncidentsWidget(incidents, 5);
```

### **2. Overdue Actions**
```csharp
var widget = ListWidgetExtensions.BuildRecentActionsWidget(overdueActions, 5);
```

### **3. Unread Notifications**
```csharp
var widget = ListWidgetExtensions.BuildNotificationsWidget(unreadNotifications, 10);
```

### **4. Recent Activity**
```csharp
var widget = ListWidgetExtensions.BuildActivityTimelineWidget(activities, 10);
```

### **5. Equipment Status**
```csharp
var items = ListWidgetExtensions.BuildListItems(
    titles: equipmentNames,
    subtitles: equipmentStatuses,
    icons: equipmentIcons,
    badges: statusBadges
);
var widget = ListWidgetExtensions.BuildListWidget("Equipment Status", items);
```

---

## 📚 References

- **Implementation Plan:** `OSHfiles/Dashboards/implementation-plan.md`
- **Architecture Pattern:** `OSHfiles/Codingdocs/STATCARD_ARCHITECTURE.md`
- **Test Page:** `/Dashboard/TestListWidgets`

---

**Component Status:** ✅ Complete and Ready for Production Use  
**Created By:** OSH Development Team  
**Last Updated:** October 2025
