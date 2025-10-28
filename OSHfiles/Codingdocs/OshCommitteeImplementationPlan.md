# OSH Committee Module - Implementation Plan

## Executive Summary
OSH Committee module builds on Team Management to provide specialized committee operations: activation, issue tracking, recommendations, actions, and document management.

---

## Architecture Overview

### Two-Tier System
```
Team Module (Generic) → Creates team structure, manages members
         ↓
OSH Committee Module (Specific) → Activates committee, manages operations
```

### Database (Already Built ✅)
- `OshCommitteeConfig` - Committee configuration
- `CommitteeIssue` - Issues raised
- `CommitteeRecommendation` - Recommendations from issues  
- `CommitteeAction` - Action items from recommendations
- `Team` + `TeamMember` - Base team structure
- `MediaAssociation` - Document uploads

---

## Complete User Flows

### Flow 1: Committee Setup (Initial Activation)

**Step 1: Create Team (Team Module)**
```
Factory Manager → Team Management → Create New Team
  ├─ Select Station: CHEBUT
  ├─ Team Type: "OSH Committee"
  ├─ Team Name: "CHEBUT OSH Committee"
  ├─ Formation Date: Oct 1, 2025
  └─ [Save] → Team Status: "Forming"
```

**Step 2: Add Members (Team Module)**
```
Team Members → Add Members
  ├─ Search Employee: "John Mwangi"
  ├─ Assign Role: "Chairman"
  ├─ Section: "Production"
  ├─ Type: Management Representative
  └─ Repeat for 5-10 members
System validates: ✓ Min 5 members ✓ Section coverage
```

**Step 3: Activate Committee (OSH Committee Module)**
```
OSH Committee → Activate Committee
  ├─ Configuration Form:
  │   ├─ [✓] Committee Trained → Date: Oct 15, 2025
  │   ├─ [✓] Meeting Schedule Exists
  │   ├─ Inspection Frequency: [Quarterly]
  │   └─ Last Inspection: Oct 1, 2025
  ├─ Upload Documents:
  │   ├─ Training Certificate (PDF)
  │   └─ Meeting Schedule (PDF)
  └─ [Activate Committee]
System creates OshCommitteeConfig
Team Status: "Forming" → "Active"
```

---

### Flow 2: Issue → Recommendation → Action

**SCENARIO: Safety Issue During Meeting**

**1. RAISE ISSUE**
```
Committee Member: David Chelal
Issues Tab → [+ New Issue]
  ├─ Title: "Inadequate Fire Extinguisher Coverage"
  ├─ Category: [Risk Assessment]
  ├─ Description: "Production floor lacks fire extinguishers in Section B..."
  ├─ Raised By: David Chelal (auto-filled)
  ├─ Date: Oct 28, 2025
  ├─ Previous Outcome: "Discussed in Q2, no action taken"
  └─ Status: [Open]
Result: Issue #12 Created
```

**2. CREATE RECOMMENDATION**
```
Issue #12 Details → [+ Add Recommendation]
  ├─ Issue: #12 (auto-linked)
  ├─ Recommendation: "Install 5 additional fire extinguishers in Section B"
  ├─ Recommended By: John Mwangi (Chairman)
  ├─ Date: Oct 28, 2025
  ├─ Target Completion: Nov 30, 2025
  └─ Status: [Pending]
Result: Recommendation #8 Created
```

**3. CREATE ACTIONS**
```
Recommendation #8 → [+ Add Actions]

Action 1:
  ├─ Description: "Purchase 5 CO2 fire extinguishers"
  ├─ Assign To: Peter Kamau (Procurement)
  ├─ Due Date: Nov 10, 2025
  └─ Status: Pending (auto)

Action 2:
  ├─ Description: "Install extinguishers in designated locations"
  ├─ Assign To: James Ochieng (Maintenance)
  ├─ Due Date: Nov 25, 2025
  └─ Status: Pending (auto)

Result: 2 Actions Created, Assignees Notified
```

**4. TRACK & COMPLETE**
```
Peter Kamau Dashboard → "1 Action Assigned"
  ├─ Oct 30: Status → "In Progress"
  ├─ Nov 8: Extinguishers purchased
  └─ [Mark Complete]
      ├─ Completion Date: Nov 8, 2025
      ├─ Notes: "Procured from SafetySupplies Ltd"
      └─ Upload Receipt (PDF)
Action #45 → "Completed"

When all actions complete:
  └─ Recommendation #8 → "Implemented"
  
Committee reviews:
  ├─ Issue #12 Status → "Resolved"
  ├─ Resolution Date: Nov 26, 2025
  └─ Notes: "All extinguishers installed and tested"
```

---

## Proposed Views

### 1. Committee List View (Main Index)

**Route:** `/OshCommittee/Index/`

**Purpose:** Shows all OSH Committee teams within user's scope

**Components:**
- **Page Header**
  - Title: "OSH Committees"
  - Breadcrumb: Home → OSH Management → Committees
  - Actions: [View All Stations] [Export Report]

- **Summary Cards** (4x _LeftBackgroundFillCard)
  - Total Committees (count by scope)
  - Active Committees (with config)
  - Forming Committees (no config yet)
  - Compliance Rate (% active)

- **DataTable: Committee List**
  - Columns:
    - Committee Name
    - Station (with factory/section badge)
    - Formation Date
    - Members Count (e.g., "7/10")
    - Status (Active/Forming badge)
    - Activation Status (✅ Activated / ⚠️ Pending)
    - Last Activity (date)
    - Actions (View Details, Configure)
  - Filters: Station, Status, Activation Status
  - Search: Committee name, station
  - Click row → Navigate to `/OshCommittee/Details/{teamId}`

**Scope Logic:**
```csharp
// User sees committees based on their scope
var committees = await _context.Teams
    .Include(t => t.Station)
    .Include(t => t.TeamTypeDefinition)
    .Include(t => t.OshCommitteeConfig)
    .Where(t => t.TeamTypeDefinition.TypeName == "OSH_Committee")
    .Where(t => _scopeFilterService.CanViewStation(t.StationId))
    .OrderBy(t => t.Station.StationName)
    .ToListAsync();
```

**Activation Status Indicator:**
```csharp
// Committee is "Activated" if:
var isActivated = team.OshCommitteeConfig != null;
var statusBadge = isActivated 
    ? "<span class='badge bg-success'>✅ Activated</span>"
    : "<span class='badge bg-warning'>⚠️ Pending Activation</span>";
```

---

### 2. Committee Details View (Dashboard)

**Route:** `/OshCommittee/Details/{teamId}` (formerly Index/{teamId})

**Purpose:** Detailed dashboard for specific committee

**Components:**
- **Detail Card** (left border + transparent bg)
  - Committee name, station, status
  - Key metrics: Members, Training, Meetings, Next Inspection
  - Actions: Edit Config, Actions dropdown, Back to List

- **Statistic Cards** (4x _LeftBackgroundFillCard)
  - Issues (Open: 5, Total: 18)
  - Recommendations (Pending: 3, Implemented: 12)
  - Actions (Active: 8, Done: 34)
  - Overdue (Actions: 2, Urgent alert)

- **Horizontal Tabs:**
  1. Overview - Dashboard widgets
  2. Issues - Issue list table
  3. Recommendations - Recommendations list
  4. Actions - Actions board (Kanban)
  5. Documents - Document repository
  6. Members - Team members list
  7. Settings - Configuration management
  8. Activity - Change log

---

### 3. Activation View

**Route:** `/OshCommittee/Activate/{teamId}`

**Vertical Tabs:**

**Tab 1: Committee Information** (Read-Only)
- Team name, station, formation date
- Current members count
- Section coverage status
- Validation checklist

**Tab 2: Configuration** ⭐
```
Training Section:
  [✓] Committee has been trained
  Training Date: [Date Picker]

Meeting Section:
  [✓] Meeting schedule exists
  Meeting Frequency: [Monthly/Quarterly]

Inspection Section:
  Inspection Frequency: [Monthly/Quarterly/Biannual]
  Last Inspection Date: [Date Picker]
```

**Tab 3: Document Uploads**
- Training Documents (required)
- Meeting Schedules (required)
- Committee Charter (optional)
- Drag & drop or [Upload] buttons

**Tab 4: Validation Checklist**
```
✅ Minimum 5 members added
✅ All required sections represented
✅ Chairman assigned
✅ Secretary assigned
⚠️ Training certificate uploaded
⚠️ Meeting schedule uploaded
```

**[Activate Committee]** (enabled when all ✅)

---

### 4. Issue Details View

**Route:** `/OshCommittee/Issues/Details/{issueId}`

**Detail Card:**
- Issue title, status badge, category
- Key fields: Raised date, Raised by, Recommendations count, Actions count
- Actions: Edit, Add Recommendation, Close Issue, Back

**Horizontal Tabs:**
1. **Overview** - Description, previous outcome, timeline
2. **Recommendations** - Linked recommendations with status
3. **Actions** - Actions from recommendations
4. **Documents** - Related documents
5. **History** - Change history, comments

---

### 5. Actions Board (Kanban)

**Route:** `/OshCommittee/Actions/Board/{teamId}`

**Layout:**
```
[+ New Action] [Filter] [My Actions Only]

┌───────────┬──────────────┬─────────────┬──────────────┐
│ PENDING   │ IN PROGRESS  │ OVERDUE⚠️   │ COMPLETED    │
│ (8)       │ (5)          │ (2)         │ (34)         │
├───────────┼──────────────┼─────────────┼──────────────┤
│ [Action#45│ [Action#43]  │ [Action#41] │ [Action#40]  │
│  Purchase │  Install...  │  🔴15 days  │  ✅ Verified │
│  extingu. │  Assigned:   │  overdue    │  Oct 15      │
│  Due:Nov10│  P.Kamau     │             │              │
│  [View]   │  [Update]    │  [Escalate] │              │
│           │              │             │              │
└───────────┴──────────────┴─────────────┴──────────────┘
```

Drag & drop between columns to update status.

---

## Step-by-Step Implementation

### PHASE 1: Foundation (Week 1-2)

#### Step 1.1: Create Controller
**File:** `Controllers/OshCommitteeController.cs`

```csharp
public class OshCommitteeController : ScopedController
{
    // Committee List (Main Index)
    Task<IActionResult> Index() // Lists all committees in user's scope
    Task<JsonResult> GetCommittees() // DataTable AJAX endpoint
    
    // Committee Details Dashboard
    Task<IActionResult> Details(int teamId) // Specific committee dashboard
    
    // Activation
    Task<IActionResult> Activate(int teamId)
    [HttpPost] Task<IActionResult> Activate(model)
    
    // Configuration
    Task<IActionResult> Configure(int teamId)
}
```

#### Step 1.2: Create ViewModels
**File:** `Models/ViewModels/OshCommitteeViewModel.cs`

```csharp
public class OshCommitteeViewModel
{
    public int TeamId { get; set; }
    public string TeamName { get; set; }
    public string StationName { get; set; }
    public string TeamStatus { get; set; }
    public OshCommitteeConfigViewModel Config { get; set; }
    public CommitteeMetrics Metrics { get; set; }
}

public class CommitteeMetrics
{
    public int TotalMembers { get; set; }
    public int OpenIssues { get; set; }
    public int PendingRecommendations { get; set; }
    public int ActiveActions { get; set; }
    public int OverdueActions { get; set; }
    public DateTime? NextInspectionDate { get; set; }
}
```

#### Step 1.3: Create Committee List View
**Files:**
- `Views/OshCommittee/Index.cshtml` (Committee list with DataTable)

**DataTable Configuration:**
```csharp
var dataTableConfig = new DataTableConfig
{
    TableId = "committeesTable",
    AjaxUrl = "/OshCommittee/GetCommittees",
    Columns = new List<DataTableColumn>
    {
        new() { Data = "teamName", Title = "Committee Name" },
        new() { Data = "stationName", Title = "Station" },
        new() { Data = "formationDate", Title = "Formation Date", Width = "110px" },
        new() { Data = "memberCount", Title = "Members", Width = "80px" },
        new() { Data = "teamStatus", Title = "Status", Width = "90px", Render = "statusBadge" },
        new() { Data = "isActivated", Title = "Activation", Width = "100px", Render = "activationBadge" },
        new() { Data = "lastActivity", Title = "Last Activity", Width = "110px" },
        new() { Data = "actions", Title = "Actions", Width = "120px", Orderable = false }
    }
};
```

#### Step 1.4: Create Committee Details Dashboard
**Files:**
- `Views/OshCommittee/Details.cshtml` (formerly Index.cshtml)
- `Views/OshCommittee/_DashboardOverview.cshtml`

#### Step 1.5: Create Activation Views
**Files:**
- `Views/OshCommittee/Activate.cshtml`
- `Views/OshCommittee/_ActivateOverview.cshtml`
- `Views/OshCommittee/_ActivateConfiguration.cshtml`
- `Views/OshCommittee/_ActivateDocuments.cshtml`
- `Views/OshCommittee/_ActivateValidation.cshtml`

---

### PHASE 2: Issues Management (Week 3-4)

#### Step 2.1: Issues CRUD
**Controller Actions:**
```csharp
Task<IActionResult> IssueIndex(int teamId)
Task<IActionResult> CreateIssue(int teamId)
[HttpPost] Task<IActionResult> CreateIssue(model)
Task<IActionResult> IssueDetails(int issueId)
Task<IActionResult> EditIssue(int issueId)
[HttpPost] Task<IActionResult> ResolveIssue(int issueId, notes)
```

#### Step 2.2: Create Issue Views
**Files:**
- `Views/OshCommittee/Issues/Index.cshtml` (DataTable)
- `Views/OshCommittee/Issues/Create.cshtml`
- `Views/OshCommittee/Issues/Details.cshtml` (Horizontal tabs)
- `Views/OshCommittee/Issues/Edit.cshtml`

#### Step 2.3: Issue Categories
```
- Risk Assessment
- Accident Investigation
- OSH Data Review
- Statutory Compliance
```

---

### PHASE 3: Recommendations & Actions (Week 5-6)

#### Step 3.1: Recommendations
**Controller Actions:**
```csharp
Task<IActionResult> CreateRecommendation(int issueId)
[HttpPost] Task<IActionResult> UpdateRecommendationStatus(id, status)
Task<IActionResult> RecommendationDetails(int recommendationId)
```

**Status Flow:** `Pending → In Progress → Implemented / Rejected`

#### Step 3.2: Actions Management
**Controller Actions:**
```csharp
Task<IActionResult> CreateAction(int recommendationId)
Task<IActionResult> ActionsBoard(int teamId) // Kanban
[HttpPost] Task<IActionResult> CompleteAction(id, date, notes)
Task<IActionResult> GetOverdueActions(int teamId)
```

**Auto-Status Logic:**
```csharp
public string CalculateActionStatus(CommitteeAction action)
{
    if (action.CompletionDate.HasValue) return "Completed";
    if (action.DueDate < DateTime.UtcNow) return "Overdue";
    if (action.AssignedToPayroll != null) return "In Progress";
    return "Pending";
}
```

#### Step 3.3: Create Actions Board
**File:** `Views/OshCommittee/Actions/Board.cshtml`
- Kanban columns: Pending, In Progress, Overdue, Completed
- Drag & drop functionality (Sortable.js)
- Action cards with assignee, due date, status

---

### PHASE 4: Advanced Features (Week 7-8)

#### Step 4.1: Analytics Service
**File:** `Services/CommitteeAnalyticsService.cs`

```csharp
public class CommitteeAnalyticsService
{
    Task<CommitteeMetrics> CalculateMetrics(int teamId)
    Task<int> GetAverageResolutionDays(int teamId)
    Task<decimal> CalculateImplementationRate(int teamId)
    Task<decimal> CalculateActionCompletionRate(int teamId)
    Task<DateTime> CalculateNextInspectionDate(config)
}
```

#### Step 4.2: Notifications
**Triggers:**
- Action assigned → Notify assignee
- Action overdue → Escalation notice
- Inspection due (7 days before) → Notify chairman
- Recommendation pending (30 days) → Reminder

#### Step 4.3: Reports
**Types:**
- Committee Effectiveness Report
- Issue Resolution Report (date range)
- Action Completion Report
- Section Representation Report

---

## Document Types

**Media Association Types:**
```
training_certificate  → Training documents
meeting_schedule      → Annual meeting schedules
meeting_minutes       → Meeting minutes PDFs
inspection_report     → Inspection reports
committee_charter     → Committee charter
```

**Upload Integration:**
```csharp
await _mediaService.AssociateMediaAsync(
    mediaId: uploadedFileId,
    associatedTable: "Teams",
    associatedRecordId: teamId.ToString(),
    associationType: "training_certificate"
);
```

---

## Key Business Rules

### Committee Activation Requirements:
- ✅ Minimum 5 members
- ✅ Required sections represented (minimum 3)
- ✅ Chairman assigned
- ✅ Secretary assigned
- ✅ Training certificate uploaded
- ✅ Meeting schedule uploaded

### Issue Status Progression:
```
Open → In Progress → Resolved / Closed
```

### Recommendation Status:
```
Pending → In Progress → Implemented / Rejected
```

### Action Status (Auto-calculated):
```
Pending → In Progress → Overdue (if past due) → Completed
```

---

## Integration Points

### With Team Module:
- Committee uses Team's member management
- Committee config changes Team.TeamStatus to "Active"
- Validation checks (min members, section representation)

### With Media Module:
- All documents via MediaAssociation
- Document types tracked by `associationType`

### With Employee Module:
- Action assignments to employees
- Committee member lookup
- Qualification verification

### With Notifications:
- Action assignments
- Overdue alerts
- Inspection reminders

---

## Success Metrics

**Committee Effectiveness:**
- % of stations with active committees
- Section representation coverage
- Training completion rates
- Meeting schedule compliance

**Issue Resolution:**
- Average time to resolve issues
- Recommendation implementation rates
- Action completion percentages
- Recurring issue identification

**Action Tracking:**
- On-time completion rate
- Average days to complete
- Overdue action count
- Escalation frequency

---

## File Structure

```
Controllers/
├── OshCommitteeController.cs (NEW)

Models/ViewModels/
├── OshCommitteeViewModel.cs (NEW)
├── CommitteeMetrics.cs (NEW)
├── CreateIssueViewModel.cs (NEW)
├── CreateRecommendationViewModel.cs (NEW)
└── CreateActionViewModel.cs (NEW)

Services/
└── CommitteeAnalyticsService.cs (NEW)

Views/OshCommittee/
├── Index.cshtml (NEW - Committee list with DataTable)
├── Details.cshtml (NEW - Committee dashboard with tabs)
├── Activate.cshtml (NEW)
├── Configure.cshtml (NEW)
├── _DashboardOverview.cshtml (NEW)
├── _ActivateOverview.cshtml (NEW)
├── _ActivateConfiguration.cshtml (NEW)
├── _ActivateDocuments.cshtml (NEW)
├── _ActivateValidation.cshtml (NEW)
│
├── Issues/
│   ├── Index.cshtml (NEW)
│   ├── Create.cshtml (NEW)
│   ├── Details.cshtml (NEW)
│   └── Edit.cshtml (NEW)
│
├── Recommendations/
│   ├── Create.cshtml (NEW)
│   └── Details.cshtml (NEW)
│
└── Actions/
    ├── Board.cshtml (NEW - Kanban)
    ├── Create.cshtml (NEW)
    └── _ActionCard.cshtml (NEW - partial)
```

---

## Sidebar Navigation Integration

**Location:** OSH Management Section (same area as OSH Policy)

**Menu Structure:**
```
OSH Management
├── 📋 OSH Policy (existing)
│   ├── All Policies
│   ├── Create New
│   └── Archived
├── 👥 OSH Committees (NEW)
│   ├── All Committees
│   ├── Pending Activation
│   └── Reports
├── Teams (existing - Team Management)
└── ... other OSH modules
```

**Implementation in Sidebar:**
```html
<!-- _Layout.cshtml or Sidebar component -->
<li class="slide has-sub">
    <a href="javascript:void(0);" class="side-menu__item">
        <i class="ri-shield-check-line side-menu__icon"></i>
        <span class="side-menu__label">OSH Management</span>
        <i class="ri-arrow-right-s-line side-menu__angle"></i>
    </a>
    <ul class="slide-menu child1">
        <!-- OSH Policy -->
        <li class="slide">
            <a href="@Url.Action("Index", "OshPolicy")" class="side-menu__item">
                📋 OSH Policy
            </a>
        </li>
        
        <!-- OSH Committees (NEW) -->
        <li class="slide has-sub">
            <a href="javascript:void(0);" class="side-menu__item">
                👥 OSH Committees
                <i class="ri-arrow-right-s-line side-menu__angle"></i>
            </a>
            <ul class="slide-menu child2">
                <li class="slide">
                    <a href="@Url.Action("Index", "OshCommittee")" class="side-menu__item">
                        All Committees
                    </a>
                </li>
                <li class="slide">
                    <a href="@Url.Action("Index", "OshCommittee", new { filter = "pending" })" 
                       class="side-menu__item">
                        Pending Activation
                    </a>
                </li>
            </ul>
        </li>
        
        <!-- Teams -->
        <li class="slide">
            <a href="@Url.Action("Index", "Team")" class="side-menu__item">
                Teams
            </a>
        </li>
    </ul>
</li>
```

**Route Configuration:**
```csharp
// Startup.cs or Program.cs
app.MapControllerRoute(
    name: "oshcommittee_default",
    pattern: "OshCommittee/{action=Index}/{id?}",
    defaults: new { controller = "OshCommittee" });
```

**Navigation Flow:**
```
Sidebar → OSH Committees → All Committees
  └─> /OshCommittee/Index (list view)
       └─> Click Committee Row
            └─> /OshCommittee/Details/{teamId} (dashboard)
                 ├─> /OshCommittee/Activate/{teamId}
                 ├─> /OshCommittee/Issues/Index/{teamId}
                 └─> /OshCommittee/Actions/Board/{teamId}
```

---

## Next Steps

1. **Review & Approve** this implementation plan
2. **Add Sidebar Menu Item** for OSH Committees
3. **Start with Phase 1** (Foundation & Activation)
4. **Create OshCommitteeController** with Index() and Details()
5. **Build Committee List View** (Index.cshtml with DataTable)
6. **Build Activation workflow** (most critical path)
7. **Test with sample data** before proceeding to Phase 2

---

**Ready to begin implementation!** 🚀
