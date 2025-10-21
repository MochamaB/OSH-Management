# Team Member Roles vs System Roles - Architectural Decision

**Date:** 2025-10-16
**Status:** DECISION NEEDED
**Impact:** Critical - Affects authorization, team management, and all OSH workflows

---

## Executive Summary

We need to decide whether **Team Member Roles** (Chairman, Secretary, Investigator, etc.) should:
1. **Remain separate** from System Roles (current implementation)
2. **Optionally link** to System Roles (hybrid approach)
3. **BE System Roles** (full integration) ✅ **RECOMMENDED**

---

## Current State Analysis

### System Roles (in Roles table)
**Purpose:** Application-level authorization and access control

**Current Roles from Seed Data:**
- Field User (basic reporting)
- Standard User (office staff)
- Head of Department (approval authority)
- Field Supervisor (field management)
- OSH Manager (full OSH access)
- System Administrator (everything)

**Key Properties:**
- `ScopeLevel` (Organization/Station/Department/Team/Self)
- Permissions (what features user can access)
- Many-to-Many with employees (one employee can have multiple system roles)

**Current Permissions:**
```sql
-- Incident Management
Incident.Read, Incident.Create, Incident.Update
Incident.Investigate ← WHO HAS THIS?
Incident.Approve ← WHO HAS THIS?

-- Risk Assessment
Risk.Read, Risk.Create, Risk.Update
Risk.Approve ← WHO HAS THIS?

-- Committee
Committee.Read, Committee.Manage ← WHAT DOES THIS MEAN?
```

### Team Member Roles (string in TeamMember table)
**Purpose:** Functional role within a specific team

**Proposed Enum Values:**
- Chairperson
- Secretary
- Member
- Management Representative
- Employee Representative
- Safety Officer
- Team Leader
- Technical Expert
- Investigator
- Fire Marshal

**Current Implementation:**
- Stored as `string MemberRole` in TeamMember table
- No connection to authorization system
- No automatic permissions granted
- Used only for display/compliance tracking

---

## The Problem: Critical Authorization Gaps

### Gap 1: Who Can Investigate Incidents?
```csharp
// Current system: Field Supervisor has Incident.Investigate permission
// But: Investigation teams are SEPARATE entities

// Question: Can ANY Field Supervisor investigate ANY incident?
// Or: Only Investigation Team members should investigate?
```

### Gap 2: Who Can Approve Risk Assessments?
```csharp
// Current system: Head of Department has Risk.Approve permission
// But: Risk Assessment Teams exist with Team Leaders

// Question: Can ANY HOD approve ANY risk assessment?
// Or: Only Risk Assessment Team Leaders in their scope?
```

### Gap 3: Who Can Manage Committee Activities?
```csharp
// Current system: HOD has Committee.Manage permission
// But: OSH Committees have Chairpersons

// Question: Can ANY HOD manage committee activities?
// Or: Only the Committee Chairman should start meetings, approve minutes?
```

### Gap 4: Team-Specific Actions
**OSH Committee Chairman Needs:**
- Committee.StartMeeting
- Committee.ApproveMinutes
- Committee.AssignActions
- Committee.CloseIssues

**Investigation Team Lead Needs:**
- Incident.StartInvestigation
- Incident.AssignInvestigators
- Incident.ApproveReport
- Incident.CloseInvestigation

**Risk Assessment Team Leader Needs:**
- Risk.StartAssessment
- Risk.AssignAssessors
- Risk.ApproveAssessment
- Risk.ImplementControls

**Current Problem:** These permissions don't exist, and if they did, who gets them?

---

## Proposed Solution: Full Integration (Option 3)

### Architecture: Team Member Roles ARE System Roles

#### 1. Expand Roles Table with Team-Specific Roles

**New System Roles to Add:**
```sql
-- OSH Committee Roles
INSERT INTO Roles (RoleName, Description, ScopeLevel, IsTeamRole, TeamTypeRestriction)
VALUES
    ('OSH Committee Chairman', 'OSH Committee Chairperson', 4, TRUE, 'OshCommittee'),
    ('OSH Committee Secretary', 'OSH Committee Secretary', 4, TRUE, 'OshCommittee'),
    ('OSH Committee Member', 'OSH Committee Member', 4, TRUE, 'OshCommittee'),
    ('Management Representative', 'Management Rep on OSH Committee', 4, TRUE, 'OshCommittee'),
    ('Employee Representative', 'Employee Rep on OSH Committee', 4, TRUE, 'OshCommittee'),

-- Risk Assessment Roles
    ('Risk Assessment Team Leader', 'Risk Assessment Team Lead', 4, TRUE, 'RiskAssessment'),
    ('Risk Assessor', 'Risk Assessment Team Member', 4, TRUE, 'RiskAssessment'),

-- Investigation Roles
    ('Investigation Team Lead', 'Incident Investigation Lead', 4, TRUE, 'Investigation'),
    ('Investigator', 'Incident Investigator', 4, TRUE, 'Investigation'),
    ('Safety Officer', 'Safety Officer on Investigation Team', 4, TRUE, 'Investigation')
```

**New Columns in Roles Table:**
```sql
ALTER TABLE Roles ADD IsTeamRole BIT DEFAULT 0;
ALTER TABLE Roles ADD TeamTypeRestriction VARCHAR(50) NULL; -- OshCommittee, RiskAssessment, Investigation, NULL for general roles
ALTER TABLE Roles ADD RequiresTeamMembership BIT DEFAULT 0; -- Can only be assigned via team membership
```

#### 2. Link TeamMember to Role

**Update TeamMember Table:**
```sql
-- REPLACE string MemberRole with RoleId
ALTER TABLE TeamMembers DROP COLUMN MemberRole;
ALTER TABLE TeamMembers ADD RoleId INT NOT NULL;
ALTER TABLE TeamMembers ADD CONSTRAINT FK_TeamMembers_Roles
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId);
```

#### 3. Automatic EmployeeRole Assignment on Team Membership

**When adding team member:**
```csharp
// Step 1: Add to team
var teamMember = new TeamMember
{
    TeamId = teamId,
    EmployeePayroll = employeePayroll,
    RoleId = roleId, // OSH Committee Chairman role
    AppointmentDate = DateTime.Today,
    IsActive = true
};
_context.TeamMembers.Add(teamMember);

// Step 2: Automatically assign system role to employee
var employee = await _context.Employees
    .FirstOrDefaultAsync(e => e.PayrollNo == employeePayroll);

var employeeRole = new EmployeeRole
{
    EmployeeId = employee.EmployeeId,
    RoleId = roleId, // Same role
    AssignedAt = DateTime.UtcNow,
    IsActive = true,
    AssignedBy = currentUserPayroll
};
_context.EmployeeRoles.Add(employeeRole);
```

**When removing from team:**
```csharp
// Step 1: Mark team membership inactive
teamMember.IsActive = false;
teamMember.DepartureDate = DateTime.Today;

// Step 2: Revoke the team role from employee
var employeeRole = await _context.EmployeeRoles
    .FirstOrDefaultAsync(er =>
        er.EmployeeId == employee.EmployeeId &&
        er.RoleId == teamMember.RoleId &&
        er.IsActive);

if (employeeRole != null)
{
    employeeRole.IsActive = false;
    employeeRole.RevokedAt = DateTime.UtcNow;
}
```

#### 4. New Granular Permissions

**OSH Committee Permissions:**
```sql
INSERT INTO Permissions (PermissionName, Description, Module, Action)
VALUES
    -- Chairman-only permissions
    ('Committee.StartMeeting', 'Start committee meetings', 'Committee', 'Manage'),
    ('Committee.ApproveMinutes', 'Approve meeting minutes', 'Committee', 'Approve'),
    ('Committee.CloseIssues', 'Close committee issues', 'Committee', 'Manage'),

    -- Secretary-only permissions
    ('Committee.RecordMinutes', 'Record meeting minutes', 'Committee', 'Create'),
    ('Committee.DistributeMinutes', 'Distribute minutes', 'Committee', 'Manage'),

    -- All committee members
    ('Committee.RaiseIssues', 'Raise safety issues', 'Committee', 'Create'),
    ('Committee.MakeRecommendations', 'Make recommendations', 'Committee', 'Create'),
    ('Committee.ViewMeetings', 'View committee meetings', 'Committee', 'Read')
```

**Investigation Permissions:**
```sql
INSERT INTO Permissions (PermissionName, Description, Module, Action)
VALUES
    -- Investigation Team Lead only
    ('Incident.StartInvestigation', 'Start incident investigation', 'Incidents', 'Manage'),
    ('Incident.AssignTasks', 'Assign investigation tasks', 'Incidents', 'Manage'),
    ('Incident.ApproveReport', 'Approve investigation report', 'Incidents', 'Approve'),
    ('Incident.CloseInvestigation', 'Close investigation', 'Incidents', 'Manage'),

    -- All investigators
    ('Incident.CollectEvidence', 'Collect investigation evidence', 'Incidents', 'Update'),
    ('Incident.ConductInterviews', 'Conduct interviews', 'Incidents', 'Update'),
    ('Incident.AnalyzeCauses', 'Analyze root causes', 'Incidents', 'Update')
```

**Risk Assessment Permissions:**
```sql
INSERT INTO Permissions (PermissionName, Description, Module, Action)
VALUES
    -- Team Leader only
    ('Risk.StartAssessment', 'Start risk assessment', 'RiskAssessment', 'Manage'),
    ('Risk.ApproveAssessment', 'Approve risk assessment', 'RiskAssessment', 'Approve'),
    ('Risk.AssignControls', 'Assign control measures', 'RiskAssessment', 'Manage'),

    -- All assessors
    ('Risk.IdentifyHazards', 'Identify hazards', 'RiskAssessment', 'Create'),
    ('Risk.EvaluateRisks', 'Evaluate risk levels', 'RiskAssessment', 'Update'),
    ('Risk.RecommendControls', 'Recommend controls', 'RiskAssessment', 'Create')
```

#### 5. Permission Assignment to Team Roles

```sql
-- OSH Committee Chairman Permissions
DECLARE @ChairmanRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'OSH Committee Chairman')
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT @ChairmanRoleId, PermissionId FROM Permissions
WHERE PermissionName IN (
    'Committee.Read', 'Committee.StartMeeting', 'Committee.ApproveMinutes',
    'Committee.CloseIssues', 'Committee.RaiseIssues', 'Committee.MakeRecommendations',
    'Committee.ViewMeetings'
)

-- Investigation Team Lead Permissions
DECLARE @InvestLeadRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Investigation Team Lead')
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT @InvestLeadRoleId, PermissionId FROM Permissions
WHERE PermissionName IN (
    'Incident.Read', 'Incident.StartInvestigation', 'Incident.AssignTasks',
    'Incident.ApproveReport', 'Incident.CloseInvestigation',
    'Incident.CollectEvidence', 'Incident.ConductInterviews', 'Incident.AnalyzeCauses'
)
```

---

## Benefits of Full Integration

### 1. **Clear Authorization Chain**
```
User clicks "Start Investigation"
→ Check: Does user have "Incident.StartInvestigation" permission?
→ Check: Is user Investigation Team Lead for this incident's team?
→ Both TRUE: Allow action
```

### 2. **Automatic Permission Management**
```
John added as OSH Committee Chairman
→ Automatically gets: Committee.StartMeeting, Committee.ApproveMinutes, etc.
→ Automatically gets: Team scope (can see team members, team data)

John removed from committee
→ Automatically loses all committee permissions
→ No orphaned permissions
```

### 3. **Audit Trail**
```sql
SELECT
    e.FullName,
    r.RoleName,
    tm.AppointmentDate,
    tm.DepartureDate,
    tm.IsActive AS CurrentlyOnTeam,
    er.IsActive AS CurrentlyHasRole
FROM TeamMembers tm
JOIN Employees e ON tm.EmployeePayroll = e.PayrollNo
JOIN Roles r ON tm.RoleId = r.RoleId
LEFT JOIN EmployeeRoles er ON er.EmployeeId = e.EmployeeId AND er.RoleId = r.RoleId
WHERE tm.TeamId = @teamId
```

### 4. **Compliance Tracking**
- Still track Chairman, Secretary (via Role names)
- Still track appointment dates, tenure
- Still validate section representation
- PLUS: Know exactly who can do what

### 5. **Flexible Multi-Team Membership**
```csharp
// Employee can be:
// 1. OSH Committee Member at Kericho Factory (Team scope for that team)
// 2. Investigation Team Lead at Kericho Factory (Team scope for that team)
// 3. Standard User (Self scope for own data)

// Their effective permissions = UNION of all roles
```

---

## Implementation Changes Required

### 1. Update TeamEnums.cs
**REMOVE:** `TeamMemberRole` enum (becomes redundant)

**WHY:** Team roles are now in Roles table, not hardcoded enums

### 2. Update TeamMember Model
```csharp
public class TeamMember
{
    [Key]
    public int MemberId { get; set; }

    public int TeamId { get; set; }

    [Required]
    public string EmployeePayroll { get; set; }

    // CHANGED: From string MemberRole to int RoleId
    [Required]
    public int RoleId { get; set; }

    // ... other fields remain same

    // NEW navigation property
    public Role Role { get; set; } = null!;
}
```

### 3. Create TeamMemberService
```csharp
public class TeamMemberService
{
    public async Task<Result> AddTeamMemberAsync(
        int teamId,
        string employeePayroll,
        int roleId)
    {
        // 1. Validate role is appropriate for team type
        var team = await _context.Teams
            .Include(t => t.TeamType)
            .FirstOrDefaultAsync(t => t.TeamId == teamId);

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleId == roleId);

        if (role.IsTeamRole &&
            role.TeamTypeRestriction != null &&
            role.TeamTypeRestriction != team.TeamType)
        {
            return Result.Failure($"Role {role.RoleName} cannot be used for {team.TeamType} teams");
        }

        // 2. Add team member
        var teamMember = new TeamMember { ... };
        _context.TeamMembers.Add(teamMember);

        // 3. Automatically assign system role to employee
        var employee = await GetEmployee(employeePayroll);
        var employeeRole = new EmployeeRole
        {
            EmployeeId = employee.EmployeeId,
            RoleId = roleId,
            IsActive = true
        };
        _context.EmployeeRoles.Add(employeeRole);

        await _context.SaveChangesAsync();
        return Result.Success();
    }
}
```

### 4. Update Create Team Wizard
**Step 3: Member Selection**
```razor
@foreach (var employee in employees)
{
    <div class="member-row">
        <input type="checkbox" name="SelectedMembers" value="@employee.PayrollNo" />
        <span>@employee.FullName</span>

        <!-- Role dropdown filtered by team type -->
        <select name="Members[@index].RoleId">
            <option value="">-- Select Role --</option>
            @foreach (var role in teamTypeRoles) // Filtered by team type
            {
                <option value="@role.RoleId">@role.RoleName</option>
            }
        </select>
    </div>
}
```

### 5. Update Seed Data Script
**Add new file:** `TeamRolesAndPermissions.sql`
```sql
-- Create team-specific roles
-- Assign team-specific permissions
-- See full script in section 1 above
```

---

## Migration Path

### Phase 1: Database Changes
1. Add new columns to Roles table
2. Create new team roles in Roles table
3. Create new permissions
4. Assign permissions to team roles

### Phase 2: Data Migration
```sql
-- For each existing team member with string MemberRole
-- Convert to RoleId by matching role name

UPDATE tm
SET tm.RoleId = r.RoleId
FROM TeamMembers tm
JOIN Roles r ON tm.MemberRole = r.RoleName
WHERE r.IsTeamRole = 1

-- Then drop MemberRole column
ALTER TABLE TeamMembers DROP COLUMN MemberRole
```

### Phase 3: Code Changes
1. Update TeamMember model
2. Update TeamController Create/Edit
3. Update TeamMemberService
4. Update authorization checks in all modules
5. Update views to show role names

---

## Decision Required

**RECOMMENDATION: Full Integration (Option 3)**

**Reasons:**
1. ✅ Solves the "who can do what" problem completely
2. ✅ Automatic permission management (no orphaned permissions)
3. ✅ Clear authorization for team-specific actions
4. ✅ Audit trail for compliance
5. ✅ Aligns with OSHA requirements (role tracking) AND system needs (authorization)

**Trade-offs:**
- ❌ More complex initial setup (more roles, more permissions)
- ❌ Migration effort required for existing data
- ✅ BUT: Much simpler long-term (one unified system)

---

## Next Steps If Approved

1. Create migration script for Roles table changes
2. Create TeamRolesAndPermissions.sql seed script
3. Update TeamMember model and migration
4. Update TeamEnums (remove TeamMemberRole)
5. Update TeamController and views
6. Update authorization checks in Incident/Risk/Committee controllers
7. Test team member assignment flow
8. Test permission inheritance

**Estimated Effort:** 4-6 hours

**Risk:** Low (additive changes, backward compatible during transition)

---

## Open Questions for Discussion

1. Should generic roles like "Member" exist, or only specific roles?
2. Can one person have multiple roles on the same team? (e.g., Chairman + Safety Officer)
3. Should team leadership roles (Chairman, Team Lead) have elevated scope (Department instead of Team)?
4. Do we need approval workflow for role assignments?
5. Should removing someone from team immediately revoke permissions, or keep them with grace period?

