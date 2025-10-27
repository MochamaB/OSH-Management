# Model Scope Compatibility Audit Report

## Document Overview
**Purpose**: Comprehensive audit of all models for scope compatibility and data integrity
**Date**: 2025-10-23
**Status**: Issues Identified - Action Required
**Scope**: All 41 models in OSHManagement system

---

## Executive Summary

### Audit Results

| Category | Status | Issues Found |
|----------|--------|--------------|
| **Scope Compatibility** | ⚠️ **ISSUES FOUND** | 8 critical issues |
| **Audit Trail Fields** | ⚠️ **INCOMPLETE** | 12 models missing CreatedBy |
| **DbContext Configuration** | ❌ **CRITICAL** | 15+ models not configured |
| **Performance Indexes** | ❌ **MISSING** | 20+ indexes needed |
| **Foreign Key Relationships** | ⚠️ **INCOMPLETE** | 10+ relationships not defined |

### Critical Findings

🔴 **CRITICAL**: 15+ models have NO DbContext configuration (no indexes, no FK constraints)
🔴 **CRITICAL**: Multiple models missing StationId for scope filtering
🟡 **WARNING**: Inconsistent audit trail implementation across models
🟡 **WARNING**: Missing performance indexes will cause slow queries at scale

---

## Section 1: Scope Compatibility Analysis

### Scope Requirements

For proper scope filtering, transactional entities MUST have:
- ✅ `StationId` (int) - for Station-level scope
- ✅ `DepartmentId` (int?) - for Department-level scope (optional)
- ✅ CreatedBy field with Payroll reference
- ✅ Proper navigation properties

---

### 1.1 Models WITH Proper Scope Support ✅

#### Employee
```csharp
✅ StationId: Yes
✅ DepartmentId: Yes (nullable)
✅ CreatedAt: Yes
⚠️  CreatedByPayroll: Missing
✅ DbContext Config: Yes (basic)
📊 Scope Ready: 80%
```

#### Team
```csharp
✅ StationId: Yes
❌ DepartmentId: No (not needed - teams are station-level)
✅ CreatedAt: Yes
⚠️  CreatedByPayroll: Missing
✅ DbContext Config: Yes (complete)
📊 Scope Ready: 90%
```

#### Incident
```csharp
✅ StationId: Yes
❌ DepartmentId: No (uses SectionId instead)
✅ SectionId: Yes (nullable)
✅ ReportedByPayroll: Yes
✅ CreatedAt: Yes
✅ DbContext Config: Partial (no indexes)
📊 Scope Ready: 90%
```

#### Hazard
```csharp
✅ StationId: Yes
✅ SectionId: Yes (nullable)
✅ TeamId: Yes
✅ CreatedAt: Yes
⚠️  CreatedByPayroll: Missing (should track who identified)
❌ DbContext Config: Missing
📊 Scope Ready: 70%
```

#### OshPolicy
```csharp
✅ StationId: Yes
❌ DepartmentId: No (policy is station-level)
✅ SignedByPayroll: Yes
✅ CreatedAt: Yes
⚠️  CreatedByPayroll: Missing
❌ DbContext Config: Missing
📊 Scope Ready: 70%
```

---

### 1.2 Models WITH Scope Issues ⚠️

#### 🔴 RiskMitigationPlan
```csharp
❌ StationId: MISSING (inherits from Hazard)
❌ DepartmentId: MISSING
✅ ResponsiblePersonPayroll: Yes
✅ CreatedAt: Yes
⚠️  CreatedByPayroll: Missing (who created the plan?)
❌ DbContext Config: Missing

**ISSUE**: Cannot scope filter directly without joining to Hazard
**RISK**: Queries will be slow, cannot apply efficient station filtering
**FIX REQUIRED**: Add StationId for denormalization or create view
```

**Scope Filtering Impact**:
```csharp
// CURRENT: Requires expensive join
var plans = await _context.RiskMitigationPlans
    .Include(m => m.Hazard)
    .Where(m => m.Hazard.StationId == userScope.StationId)
    .ToListAsync();

// BETTER: Direct filter (if StationId added)
var plans = await _context.RiskMitigationPlans
    .Where(m => m.StationId == userScope.StationId)
    .ToListAsync();
```

---

#### 🔴 IncidentCause
```csharp
❌ StationId: MISSING (inherits from Incident)
❌ CreatedByPayroll: MISSING
✅ CreatedAt: Yes
❌ DbContext Config: Missing

**ISSUE**: Cannot scope filter without joining to Incident
**RISK**: Same as RiskMitigationPlan
**FIX REQUIRED**: Add StationId for performance
```

---

#### 🔴 IncidentInvestigation
```csharp
❌ StationId: MISSING (inherits from Incident)
❌ DepartmentId: MISSING
✅ InvestigationLeadPayroll: Yes
✅ CreatedAt: Yes
⚠️  CreatedByPayroll: Missing (who initiated investigation?)
❌ DbContext Config: Missing

**ISSUE**: Cannot scope filter efficiently
**RECOMMENDATION**: Add StationId (denormalization justified for performance)
```

---

#### 🟡 ControlAction
```csharp
❌ StationId: MISSING (inherits from Incident)
✅ AssignedDepartmentId: Yes (good!)
✅ AssignedToPayroll: Yes
✅ CreatedAt: Yes
⚠️  CreatedByPayroll: Missing (who created the action?)
❌ DbContext Config: Missing

**ISSUE**: Has DepartmentId but no StationId
**RISK**: Department-level filtering works, but station-level requires join
**RECOMMENDATION**: Add StationId for consistency
```

---

#### 🟡 LessonLearned
```csharp
❌ StationId: MISSING (inherits from Incident)
❌ DepartmentId: MISSING
✅ ApplicableToStations: Yes (JSON - good for cross-station sharing)
✅ SharedByPayroll: Yes
✅ CreatedAt: Yes
❌ DbContext Config: Missing

**ISSUE**: No direct station filtering
**NOTE**: Lessons are meant to be cross-station, but still need origin station
**RECOMMENDATION**: Add OriginStationId to track where lesson originated
```

---

#### 🔴 CommitteeIssue
```csharp
❌ StationId: MISSING (inherits from Team)
❌ DepartmentId: MISSING
✅ TeamId: Yes
✅ RaisedByMemberId: Yes (FK to TeamMember)
✅ CreatedAt: Yes
⚠️  CreatedByPayroll: Missing
❌ DbContext Config: Missing

**ISSUE**: Must join through Team to get StationId
**RISK**: Complex queries, slow performance
**FIX REQUIRED**: Add StationId for denormalization
```

---

#### 🔴 CommitteeRecommendation
```csharp
❌ StationId: MISSING (inherits from Team/Issue)
❌ DepartmentId: MISSING
✅ TeamId: Yes
✅ RecommendedByMemberId: Yes
✅ CreatedAt: Yes
⚠️  CreatedByPayroll: Missing
❌ DbContext Config: Missing

**ISSUE**: Two-level join required (Recommendation → Issue → Team → Station)
**RISK**: Very slow queries
**FIX REQUIRED**: Definitely needs StationId
```

---

#### 🔴 CommitteeAction
```csharp
❌ StationId: MISSING (inherits from Team/Recommendation)
❌ DepartmentId: MISSING
✅ TeamId: Yes
✅ AssignedToPayroll: Yes
✅ CreatedAt: Yes
⚠️  CreatedByPayroll: Missing
❌ DbContext Config: Missing

**ISSUE**: Three-level join required!
**RISK**: Catastrophic performance at scale
**FIX REQUIRED**: MUST add StationId immediately
```

---

### 1.3 Configuration Models (No Scope Needed) ✅

These models don't need StationId because they're station-specific by design:

#### OshCommitteeConfig ✅
```csharp
✅ TeamId: Yes (team is already scoped to station)
⚠️  CreatedByPayroll: Missing
❌ DbContext Config: Missing
📊 Status: OK (inherits scope from Team)
```

#### RiskAssessmentConfig ✅
```csharp
✅ TeamId: Yes
⚠️  CreatedByPayroll: Missing
❌ DbContext Config: Missing
📊 Status: OK (inherits scope from Team)
```

#### IncidentInvestigationConfig ✅
```csharp
✅ TeamId: Yes
⚠️  CreatedByPayroll: Missing
❌ DbContext Config: Missing
📊 Status: OK (inherits scope from Team)
```

---

### 1.4 Media Models (Already Analyzed) ⚠️

See `MediaManagementImplementationPlan.md` for detailed analysis.

**Summary**:
- MediaCollection: ⚠️ Missing CreatedByPayroll
- MediaFile: ✅ Has UploadedByPayroll, needs scope filter
- MediaAssociation: 🔴 Wrong data type (int → string), missing CreatedByPayroll
- MediaAccessLog: ✅ OK
- MediaConversionJob: ⚠️ Not analyzed yet

---

### 1.5 Reference/Lookup Models (No Scope Needed) ✅

These models are global/reference data:

- Permission ✅
- Role ✅
- TeamTypeDefinition ✅
- TeamRoleDefinition ✅
- OrgCategory ✅
- Station ✅ (IS the scope boundary)
- Department ✅ (scoped to Station via FK)
- Section ✅ (scoped to Station via FK)
- NotificationTemplate ✅
- NotificationChannelConfig ✅

---

## Section 2: Audit Trail Compliance

### Requirements

All transactional entities SHOULD have:
- ✅ `CreatedAt` (DateTime)
- ✅ `UpdatedAt` (DateTime?)
- ✅ `CreatedByPayroll` (string) - WHO created the record

---

### 2.1 Models WITH Complete Audit Trail ✅

| Model | CreatedAt | UpdatedAt | CreatedBy | Status |
|-------|-----------|-----------|-----------|--------|
| Employee | ✅ | ✅ | ❌ | Incomplete |
| Team | ✅ | ✅ | ❌ | Incomplete |
| TeamMember | ✅ | ✅ | ❌ | Incomplete |
| Incident | ✅ | ✅ | ✅ (ReportedBy) | Complete |
| MediaFile | ✅ | ✅ | ✅ (UploadedBy) | Complete |
| MediaAccessLog | ✅ | ❌ | ✅ (UserPayroll) | Complete |
| Notification | ✅ | ❌ | ✅ (Sender) | Complete |

---

### 2.2 Models MISSING CreatedBy Field ⚠️

| Model | CreatedAt | UpdatedAt | CreatedBy | Impact |
|-------|-----------|-----------|-----------|--------|
| **Hazard** | ✅ | ✅ | ❌ | Cannot track who identified hazard |
| **RiskMitigationPlan** | ✅ | ✅ | ❌ | Cannot track who created plan |
| **OshPolicy** | ✅ | ✅ | ❌ | Cannot track who drafted policy |
| **IncidentCause** | ✅ | ✅ | ❌ | Cannot track who analyzed cause |
| **IncidentInvestigation** | ✅ | ✅ | ❌ | Cannot track who initiated investigation |
| **ControlAction** | ✅ | ✅ | ❌ | Cannot track who created action |
| **LessonLearned** | ✅ | ✅ | ❌ | Has SharedBy, needs CreatedBy |
| **CommitteeIssue** | ✅ | ✅ | ❌ | Has RaisedBy (member), needs CreatedBy |
| **CommitteeRecommendation** | ✅ | ✅ | ❌ | Has RecommendedBy (member), needs CreatedBy |
| **CommitteeAction** | ✅ | ✅ | ❌ | Cannot track who created action |
| **MediaCollection** | ✅ | ✅ | ❌ | Cannot track who created collection |
| **MediaAssociation** | ✅ | ✅ | ❌ | Cannot track who associated file |

**Total**: 12 models missing CreatedBy field

---

## Section 3: DbContext Configuration Audit

### 3.1 Models WITH Configuration ✅

| Model | Has Config | Indexes | FK Constraints | Default Values |
|-------|------------|---------|----------------|----------------|
| Employee | ✅ | ✅ PayrollNo | ❌ | ✅ CreatedAt |
| Role | ✅ | ❌ | ✅ | ❌ |
| Permission | ✅ | ❌ | ❌ | ❌ |
| EmployeeRole | ✅ | ❌ | ✅ | ❌ |
| RolePermission | ✅ | ❌ | ✅ | ❌ |
| TeamTypeDefinition | ✅ | ✅ TypeCode | ❌ | ✅ CreatedAt |
| Team | ✅ | ❌ | ✅ | ✅ CreatedAt |
| TeamRoleDefinition | ✅ | ❌ | ✅ | ✅ CreatedAt |
| TeamMember | ✅ | ❌ | ✅ | ❌ |
| Notification | ✅ | ✅ Multiple | ❌ | ✅ CreatedAt |
| NotificationTemplate | ✅ | ✅ Multiple | ❌ | ✅ CreatedAt |
| NotificationDelivery | ✅ | ✅ Multiple | ✅ | ✅ CreatedAt |
| NotificationPreference | ✅ | ✅ Multiple | ✅ | ✅ CreatedAt |
| NotificationChannelConfig | ✅ | ✅ Multiple | ❌ | ✅ CreatedAt |

**Total Configured**: 14 models

---

### 3.2 Models WITHOUT Configuration ❌

The following models have **NO DbContext configuration** at all:

1. **Station** ❌
2. **Department** ❌
3. **Section** ❌
4. **OrgCategory** ❌
5. **OrgMetadata** ❌
6. **OshPolicy** ❌
7. **Hazard** ❌
8. **RiskMitigationPlan** ❌
9. **Incident** ❌
10. **IncidentCause** ❌
11. **IncidentInvestigation** ❌
12. **ControlAction** ❌
13. **LessonLearned** ❌
14. **CommitteeIssue** ❌
15. **CommitteeRecommendation** ❌
16. **CommitteeAction** ❌
17. **OshCommitteeConfig** ❌
18. **RiskAssessmentConfig** ❌
19. **IncidentInvestigationConfig** ❌
20. **MediaCollection** ❌
21. **MediaFile** ❌
22. **MediaAssociation** ❌
23. **MediaAccessLog** ❌
24. **MediaConversionJob** ❌

**Total Unconfigured**: 24 models (59% of all models!)

---

### 3.3 Impact of Missing Configuration

#### No Foreign Key Constraints
```csharp
// CURRENT: No FK enforcement
var hazard = new Hazard { TeamId = 99999, StationId = 99999 };
_context.Hazards.Add(hazard);
await _context.SaveChangesAsync(); // ✅ SUCCEEDS even with invalid IDs!

// WITH FK CONSTRAINTS:
await _context.SaveChangesAsync(); // ❌ FAILS with FK violation
```

#### No Indexes = Slow Queries
```sql
-- Query without index on StationId
SELECT * FROM Incidents WHERE StationId = 5;
-- Result: TABLE SCAN (slow with >10,000 records)

-- Query with index on StationId
-- Result: INDEX SEEK (fast even with millions of records)
```

#### No Default Values
```csharp
// Manual CreatedAt management (error-prone)
var policy = new OshPolicy { CreatedAt = DateTime.UtcNow };

// With default value in DbContext:
var policy = new OshPolicy(); // CreatedAt set automatically
```

---

## Section 4: Required Indexes Analysis

### 4.1 Critical Indexes Needed (Performance)

#### Organizational Hierarchy Models

**Station**
```sql
CREATE INDEX IX_Stations_OrgCategoryId ON Stations(OrgCategoryId);
CREATE INDEX IX_Stations_Status ON Stations(StationStatus) WHERE StationStatus = 'Active';
```

**Department**
```sql
CREATE INDEX IX_Departments_StationId ON Departments(StationId);
CREATE INDEX IX_Departments_HodPayroll ON Departments(HodPayroll);
```

**Section**
```sql
CREATE INDEX IX_Sections_StationId ON Sections(StationId);
CREATE INDEX IX_Sections_DepartmentId ON Sections(DepartmentId) WHERE DepartmentId IS NOT NULL;
```

**Employee**
```sql
-- Already has: IX_Employees_PayrollNo (unique)
CREATE INDEX IX_Employees_Station ON Employees(StationId, EmploymentStatus);
CREATE INDEX IX_Employees_Department ON Employees(DepartmentId) WHERE DepartmentId IS NOT NULL;
CREATE INDEX IX_Employees_Supervisor ON Employees(SupervisorPayroll) WHERE SupervisorPayroll IS NOT NULL;
CREATE INDEX IX_Employees_HOD ON Employees(HodPayroll) WHERE HodPayroll IS NOT NULL;
```

---

#### Team & Committee Models

**Team**
```sql
CREATE INDEX IX_Teams_Station ON Teams(StationId, TeamStatus);
CREATE INDEX IX_Teams_TypeDefinition ON Teams(TeamTypeDefinitionId);
CREATE INDEX IX_Teams_Status ON Teams(TeamStatus, FormationDate DESC);
```

**TeamMember**
```sql
CREATE INDEX IX_TeamMembers_Team ON TeamMembers(TeamId, IsActive);
CREATE INDEX IX_TeamMembers_Employee ON TeamMembers(EmployeePayroll, IsActive);
CREATE INDEX IX_TeamMembers_Active ON TeamMembers(IsActive, AppointmentDate DESC) WHERE IsActive = 1;
```

**CommitteeIssue** (⚠️ Add StationId first!)
```sql
CREATE INDEX IX_CommitteeIssues_Team ON CommitteeIssues(TeamId, IssueStatus);
-- AFTER adding StationId:
CREATE INDEX IX_CommitteeIssues_Station ON CommitteeIssues(StationId, RaisedDate DESC);
```

---

#### Incident Models

**Incident**
```sql
CREATE INDEX IX_Incidents_Station ON Incidents(StationId, IncidentDate DESC);
CREATE INDEX IX_Incidents_Status ON Incidents(IncidentStatus, CreatedAt DESC);
CREATE INDEX IX_Incidents_Severity ON Incidents(IncidentSeverity, IncidentDate DESC);
CREATE INDEX IX_Incidents_ReportedBy ON Incidents(ReportedByPayroll, IncidentDate DESC);
CREATE INDEX IX_Incidents_PersonAffected ON Incidents(PersonAffectedPayroll) WHERE PersonAffectedPayroll IS NOT NULL;
```

**IncidentCause** (⚠️ Add StationId first!)
```sql
CREATE INDEX IX_IncidentCauses_Incident ON IncidentCauses(IncidentId);
```

**IncidentInvestigation** (⚠️ Add StationId first!)
```sql
CREATE INDEX IX_IncidentInvestigations_Incident ON IncidentInvestigations(IncidentId);
CREATE INDEX IX_IncidentInvestigations_Team ON IncidentInvestigations(InvestigationTeamId);
CREATE INDEX IX_IncidentInvestigations_Status ON IncidentInvestigations(InvestigationStatus, InvestigationStartDate DESC);
```

**ControlAction**
```sql
CREATE INDEX IX_ControlActions_Incident ON ControlActions(IncidentId, ActionStatus);
CREATE INDEX IX_ControlActions_Assigned ON ControlActions(AssignedToPayroll, ActionStatus);
CREATE INDEX IX_ControlActions_Department ON ControlActions(AssignedDepartmentId) WHERE AssignedDepartmentId IS NOT NULL;
CREATE INDEX IX_ControlActions_TargetDate ON ControlActions(TargetCompletionDate) WHERE TargetCompletionDate IS NOT NULL AND ActionStatus != 'Completed';
```

---

#### Hazard & Risk Models

**Hazard**
```sql
CREATE INDEX IX_Hazards_Station ON Hazards(StationId, IdentifiedDate DESC);
CREATE INDEX IX_Hazards_Team ON Hazards(TeamId, PriorityLevel);
CREATE INDEX IX_Hazards_Priority ON Hazards(PriorityLevel, RiskRating DESC);
CREATE INDEX IX_Hazards_Section ON Hazards(SectionId) WHERE SectionId IS NOT NULL;
```

**RiskMitigationPlan** (⚠️ Add StationId first!)
```sql
CREATE INDEX IX_RiskMitigationPlans_Hazard ON RiskMitigationPlans(HazardId);
CREATE INDEX IX_RiskMitigationPlans_Responsible ON RiskMitigationPlans(ResponsiblePersonPayroll, ImplementationStatus);
CREATE INDEX IX_RiskMitigationPlans_Status ON RiskMitigationPlans(ImplementationStatus, TargetCompletionDate);
```

---

#### Policy Models

**OshPolicy**
```sql
CREATE INDEX IX_OshPolicies_Station ON OshPolicies(StationId, PolicyStatus);
CREATE INDEX IX_OshPolicies_Status ON OshPolicies(PolicyStatus, LastReviewedDate DESC);
CREATE INDEX IX_OshPolicies_SignedBy ON OshPolicies(SignedByPayroll) WHERE SignedByPayroll IS NOT NULL;
```

---

### 4.2 Index Count Summary

| Category | Models | Indexes Needed | Priority |
|----------|--------|----------------|----------|
| Organizational | 4 | 8 | 🔴 HIGH |
| Team & Committee | 6 | 12 | 🔴 HIGH |
| Incident | 4 | 15 | 🔴 CRITICAL |
| Hazard & Risk | 2 | 8 | 🔴 HIGH |
| Policy | 1 | 3 | 🟡 MEDIUM |
| Media | 5 | 15 | 🔴 HIGH |
| **TOTAL** | **22** | **61** | - |

**Already Created**: 14 indexes (Notification system only)
**Still Needed**: 47 indexes

---

## Section 5: Recommendations & Action Plan

### 5.1 CRITICAL Issues (Fix Immediately)

#### Issue 1: Add StationId to Child Entities

**Models Requiring StationId**:
1. RiskMitigationPlan
2. IncidentCause
3. IncidentInvestigation
4. ControlAction (has DepartmentId, needs StationId too)
5. CommitteeIssue
6. CommitteeRecommendation
7. CommitteeAction

**SQL Script**:
```sql
-- Add StationId columns
ALTER TABLE RiskMitigationPlans ADD StationId INT NOT NULL DEFAULT 1;
ALTER TABLE IncidentCauses ADD StationId INT NOT NULL DEFAULT 1;
ALTER TABLE IncidentInvestigations ADD StationId INT NOT NULL DEFAULT 1;
ALTER TABLE ControlActions ADD StationId INT NOT NULL DEFAULT 1;
ALTER TABLE CommitteeIssues ADD StationId INT NOT NULL DEFAULT 1;
ALTER TABLE CommitteeRecommendations ADD StationId INT NOT NULL DEFAULT 1;
ALTER TABLE CommitteeActions ADD StationId INT NOT NULL DEFAULT 1;

-- Add FK constraints
ALTER TABLE RiskMitigationPlans ADD CONSTRAINT FK_RiskMitigationPlans_Stations FOREIGN KEY (StationId) REFERENCES Stations(StationId);
ALTER TABLE IncidentCauses ADD CONSTRAINT FK_IncidentCauses_Stations FOREIGN KEY (StationId) REFERENCES Stations(StationId);
ALTER TABLE IncidentInvestigations ADD CONSTRAINT FK_IncidentInvestigations_Stations FOREIGN KEY (StationId) REFERENCES Stations(StationId);
ALTER TABLE ControlActions ADD CONSTRAINT FK_ControlActions_Stations FOREIGN KEY (StationId) REFERENCES Stations(StationId);
ALTER TABLE CommitteeIssues ADD CONSTRAINT FK_CommitteeIssues_Stations FOREIGN KEY (StationId) REFERENCES Stations(StationId);
ALTER TABLE CommitteeRecommendations ADD CONSTRAINT FK_CommitteeRecommendations_Stations FOREIGN KEY (StationId) REFERENCES Stations(StationId);
ALTER TABLE CommitteeActions ADD CONSTRAINT FK_CommitteeActions_Stations FOREIGN KEY (StationId) REFERENCES Stations(StationId);

-- Update with correct StationId from parent entities
UPDATE RiskMitigationPlans SET StationId = h.StationId
FROM RiskMitigationPlans rmp
INNER JOIN Hazards h ON rmp.HazardId = h.HazardId;

UPDATE IncidentCauses SET StationId = i.StationId
FROM IncidentCauses ic
INNER JOIN Incidents i ON ic.IncidentId = i.IncidentId;

UPDATE IncidentInvestigations SET StationId = i.StationId
FROM IncidentInvestigations ii
INNER JOIN Incidents i ON ii.IncidentId = i.IncidentId;

UPDATE ControlActions SET StationId = i.StationId
FROM ControlActions ca
INNER JOIN Incidents i ON ca.IncidentId = i.IncidentId;

UPDATE CommitteeIssues SET StationId = t.StationId
FROM CommitteeIssues ci
INNER JOIN Teams t ON ci.TeamId = t.StationId;

UPDATE CommitteeRecommendations SET StationId = t.StationId
FROM CommitteeRecommendations cr
INNER JOIN Teams t ON cr.TeamId = t.TeamId;

UPDATE CommitteeActions SET StationId = t.StationId
FROM CommitteeActions ca
INNER JOIN Teams t ON ca.TeamId = t.TeamId;

-- Remove default constraint (was only for initial data)
-- (Run ALTER TABLE ... DROP CONSTRAINT commands for each default)
```

**C# Model Updates**: Add `public int StationId { get; set; }` and navigation property to each model.

---

#### Issue 2: Add CreatedByPayroll Fields

**Models Requiring CreatedByPayroll**:
All 12 models listed in Section 2.2

**SQL Script**:
```sql
ALTER TABLE Hazards ADD CreatedByPayroll VARCHAR(20) NULL;
ALTER TABLE RiskMitigationPlans ADD CreatedByPayroll VARCHAR(20) NULL;
ALTER TABLE OshPolicies ADD CreatedByPayroll VARCHAR(20) NULL;
ALTER TABLE IncidentCauses ADD CreatedByPayroll VARCHAR(20) NULL;
ALTER TABLE IncidentInvestigations ADD CreatedByPayroll VARCHAR(20) NULL;
ALTER TABLE ControlActions ADD CreatedByPayroll VARCHAR(20) NULL;
ALTER TABLE CommitteeIssues ADD CreatedByPayroll VARCHAR(20) NULL;
ALTER TABLE CommitteeRecommendations ADD CreatedByPayroll VARCHAR(20) NULL;
ALTER TABLE CommitteeActions ADD CreatedByPayroll VARCHAR(20) NULL;

-- Add FK constraints
ALTER TABLE Hazards ADD CONSTRAINT FK_Hazards_CreatedBy FOREIGN KEY (CreatedByPayroll) REFERENCES Employees(PayrollNo);
ALTER TABLE RiskMitigationPlans ADD CONSTRAINT FK_RiskMitigationPlans_CreatedBy FOREIGN KEY (CreatedByPayroll) REFERENCES Employees(PayrollNo);
-- ... (repeat for all 12 models)
```

---

#### Issue 3: Add DbContext Configurations

Create new file: `Data/OshDbContextConfiguration.cs`

```csharp
public static class OshDbContextConfiguration
{
    public static void ConfigureIncidentModule(this ModelBuilder modelBuilder)
    {
        // Incident
        modelBuilder.Entity<Incident>(entity =>
        {
            entity.HasKey(e => e.IncidentId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => new { e.StationId, e.IncidentDate })
                .HasDatabaseName("IX_Incidents_Station")
                .IsDescending(false, true);
            entity.HasIndex(e => new { e.IncidentStatus, e.CreatedAt })
                .HasDatabaseName("IX_Incidents_Status");
            entity.HasIndex(e => e.ReportedByPayroll)
                .HasDatabaseName("IX_Incidents_ReportedBy");

            // FK constraints
            entity.HasOne(i => i.Station)
                .WithMany()
                .HasForeignKey(i => i.StationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // IncidentCause
        modelBuilder.Entity<IncidentCause>(entity =>
        {
            entity.HasKey(e => e.CauseId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(ic => ic.Incident)
                .WithOne(i => i.IncidentCause)
                .HasForeignKey<IncidentCause>(ic => ic.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.IncidentId).IsUnique();
        });

        // ... (continue for all incident models)
    }

    public static void ConfigureHazardModule(this ModelBuilder modelBuilder)
    {
        // Similar pattern for Hazard, RiskMitigationPlan
    }

    public static void ConfigureCommitteeModule(this ModelBuilder modelBuilder)
    {
        // Similar pattern for CommitteeIssue, CommitteeRecommendation, CommitteeAction
    }
}

// In OshDbContext.cs OnModelCreating:
modelBuilder.ConfigureIncidentModule();
modelBuilder.ConfigureHazardModule();
modelBuilder.ConfigureCommitteeModule();
```

---

### 5.2 HIGH Priority (Fix Within Sprint)

1. **Add ScopeFilterService support** for all new entities
2. **Create performance indexes** (Script 3 in Media plan can be template)
3. **Add FK constraints** to prevent orphaned records
4. **Implement soft delete** for critical transactional entities

---

### 5.3 MEDIUM Priority (Fix Next Sprint)

1. Add data validation attributes
2. Add computed columns where beneficial
3. Implement audit trigger for UpdatedAt
4. Create database views for common complex queries

---

## Section 6: Implementation Scripts

### Script 1: Add StationId to Child Entities

See Section 5.1, Issue 1 above.

**Estimated Time**: 30 minutes
**Risk**: LOW (adds column, doesn't remove anything)
**Rollback**: Simple - DROP COLUMN

---

### Script 2: Add CreatedByPayroll Fields

See Section 5.1, Issue 2 above.

**Estimated Time**: 15 minutes
**Risk**: LOW
**Rollback**: Simple - DROP COLUMN

---

### Script 3: Add Critical Indexes

```sql
-- Run this after Script 1 and 2 are completed
-- Priority 1: Station-scoped queries (most common)

CREATE INDEX IX_Incidents_Station ON Incidents(StationId, IncidentDate DESC);
CREATE INDEX IX_Hazards_Station ON Hazards(StationId, IdentifiedDate DESC);
CREATE INDEX IX_Teams_Station ON Teams(StationId, TeamStatus);
CREATE INDEX IX_Employees_Station ON Employees(StationId, EmploymentStatus);
CREATE INDEX IX_OshPolicies_Station ON OshPolicies(StationId, PolicyStatus);

-- Priority 2: Status queries (dashboards)
CREATE INDEX IX_Incidents_Status ON Incidents(IncidentStatus, CreatedAt DESC);
CREATE INDEX IX_ControlActions_Status ON ControlActions(ActionStatus, TargetCompletionDate);
CREATE INDEX IX_RiskMitigationPlans_Status ON RiskMitigationPlans(ImplementationStatus, TargetCompletionDate);
CREATE INDEX IX_Teams_Status ON Teams(TeamStatus, FormationDate DESC);

-- Priority 3: Assignment queries (user dashboards)
CREATE INDEX IX_ControlActions_Assigned ON ControlActions(AssignedToPayroll, ActionStatus);
CREATE INDEX IX_RiskMitigationPlans_Responsible ON RiskMitigationPlans(ResponsiblePersonPayroll, ImplementationStatus);
CREATE INDEX IX_TeamMembers_Employee ON TeamMembers(EmployeePayroll, IsActive);

-- Priority 4: Date-based queries (reports)
CREATE INDEX IX_Incidents_Date ON Incidents(IncidentDate DESC);
CREATE INDEX IX_Hazards_Priority ON Hazards(PriorityLevel, RiskRating DESC);

-- Verify all indexes created
SELECT
    OBJECT_NAME(object_id) AS TableName,
    name AS IndexName,
    type_desc
FROM sys.indexes
WHERE OBJECT_NAME(object_id) IN (
    'Incidents', 'Hazards', 'Teams', 'Employees', 'OshPolicies',
    'ControlActions', 'RiskMitigationPlans', 'TeamMembers'
)
ORDER BY TableName, name;
```

**Estimated Time**: 10 minutes
**Risk**: VERY LOW (indexes don't affect data)
**Rollback**: DROP INDEX

---

## Section 7: Testing Checklist

After implementing fixes, verify:

### Scope Filtering Tests
- [ ] Station users can only see their station's data
- [ ] Department users can only see their department's data
- [ ] Organization users can see all data
- [ ] Joining child entities doesn't break scope filtering

### Performance Tests
- [ ] List incidents for station (< 100ms with 10k records)
- [ ] List hazards for station (< 100ms with 10k records)
- [ ] List team members for team (< 50ms)
- [ ] Dashboard queries (< 200ms)

### Data Integrity Tests
- [ ] Cannot create Hazard with invalid StationId
- [ ] Cannot create Incident with invalid TeamId
- [ ] Cascade deletes work correctly
- [ ] CreatedBy fields populated correctly

---

## Appendix A: Complete Model Inventory

| # | Model | Scope Compatible | Audit Complete | DbContext | Priority |
|---|-------|------------------|----------------|-----------|----------|
| 1 | Employee | ⚠️ 80% | ⚠️ No CreatedBy | ✅ Partial | HIGH |
| 2 | Role | ✅ N/A | ✅ | ✅ Basic | LOW |
| 3 | Permission | ✅ N/A | ✅ | ✅ Basic | LOW |
| 4 | EmployeeRole | ✅ N/A | ✅ | ✅ Complete | LOW |
| 5 | RolePermission | ✅ N/A | ✅ | ✅ Complete | LOW |
| 6 | Station | ✅ N/A | ⚠️ | ❌ | HIGH |
| 7 | Department | ✅ | ⚠️ | ❌ | HIGH |
| 8 | Section | ✅ | ⚠️ | ❌ | MEDIUM |
| 9 | OrgCategory | ✅ N/A | ✅ | ❌ | LOW |
| 10 | Team | ✅ 90% | ⚠️ | ✅ Complete | MEDIUM |
| 11 | TeamMember | ✅ | ⚠️ | ✅ Complete | MEDIUM |
| 12 | TeamTypeDefinition | ✅ N/A | ✅ | ✅ Complete | LOW |
| 13 | TeamRoleDefinition | ✅ N/A | ✅ | ✅ Complete | LOW |
| 14 | OshPolicy | ✅ 70% | ⚠️ | ❌ | HIGH |
| 15 | Hazard | ✅ 70% | ⚠️ | ❌ | CRITICAL |
| 16 | RiskMitigationPlan | ❌ | ⚠️ | ❌ | CRITICAL |
| 17 | Incident | ✅ 90% | ✅ | ❌ | CRITICAL |
| 18 | IncidentCause | ❌ | ⚠️ | ❌ | CRITICAL |
| 19 | IncidentInvestigation | ❌ | ⚠️ | ❌ | CRITICAL |
| 20 | ControlAction | ⚠️ | ⚠️ | ❌ | CRITICAL |
| 21 | LessonLearned | ⚠️ | ⚠️ | ❌ | HIGH |
| 22 | CommitteeIssue | ❌ | ⚠️ | ❌ | CRITICAL |
| 23 | CommitteeRecommendation | ❌ | ⚠️ | ❌ | CRITICAL |
| 24 | CommitteeAction | ❌ | ⚠️ | ❌ | CRITICAL |
| 25 | OshCommitteeConfig | ✅ | ⚠️ | ❌ | MEDIUM |
| 26 | RiskAssessmentConfig | ✅ | ⚠️ | ❌ | MEDIUM |
| 27 | IncidentInvestigationConfig | ✅ | ⚠️ | ❌ | MEDIUM |
| 28 | MediaCollection | ⚠️ | ⚠️ | ❌ | HIGH |
| 29 | MediaFile | ✅ | ✅ | ❌ | HIGH |
| 30 | MediaAssociation | 🔴 | ⚠️ | ❌ | CRITICAL |
| 31 | MediaAccessLog | ✅ | ✅ | ❌ | MEDIUM |
| 32 | MediaConversionJob | ⚠️ | ⚠️ | ❌ | LOW |
| 33 | Notification | ✅ | ✅ | ✅ Complete | LOW |
| 34 | NotificationTemplate | ✅ N/A | ✅ | ✅ Complete | LOW |
| 35 | NotificationDelivery | ✅ | ✅ | ✅ Complete | LOW |
| 36 | NotificationPreference | ✅ | ✅ | ✅ Complete | LOW |
| 37 | NotificationChannelConfig | ✅ N/A | ✅ | ✅ Complete | LOW |

**Legend**:
- ✅ = Complete/Correct
- ⚠️ = Incomplete/Needs Attention
- ❌ = Missing/Critical Issue
- 🔴 = Broken/Requires Immediate Fix
- N/A = Not Applicable

---

## Summary

**Total Models Audited**: 37
**Critical Issues**: 8 models missing StationId
**High Priority**: 12 models missing CreatedBy
**DbContext Issues**: 24 models unconfigured
**Indexes Needed**: 47+ performance indexes

**Estimated Fix Time**:
- Critical Issues: 2-3 hours
- High Priority: 1-2 hours
- DbContext Config: 4-6 hours
- Indexes: 1-2 hours
**Total**: 8-13 hours of work

**Document End**
