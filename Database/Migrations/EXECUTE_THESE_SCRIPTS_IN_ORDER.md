# TeamTypeDefinition Migration - Scripts to Execute

## Current Status

✅ **Database Structure**: All required columns already exist in your database
✅ **C# Models**: Now updated to match database structure
✅ **DbContext**: Now configured with proper relationships

## What You Need to Do

Since your database **already has all the required columns** (TeamTypeDefinitionId, term management columns, classification columns), you only need to:

1. Run Scripts 3-6 from the implementation plan to **seed data and add constraints**
2. No need to run Scripts 1-2 (they were already executed before)

---

## Scripts to Execute in SSMS (In Order)

### Step 1: Verify Database Structure (OPTIONAL)
**File:** `Database/Migrations/Update_Models_Only.sql`
```sql
-- This script verifies your database has all required columns
-- Run this first to confirm everything is ready
```
**Expected Output:** All green checkmarks ✓

---

### Step 2: Seed Team Type Definitions
**File:** Copy from implementation plan - Script 3
**Purpose:** Create the 4 standard team types (OSH Committee, Risk Assessment, Investigation, Custom)

**Location:** `OSHfiles\Codingdocs\TeamTypeDefinition_Implementation_Plan.md` (Lines 351-555)

**Key Points:**
- Creates 4 team type records
- OSH Committee has statutory compliance rules (2/3 employee reps, gender balance, quorum, etc.)
- Risk Assessment and Investigation have simpler rules
- Custom type is for user-defined teams

---

### Step 3: Seed Role Definitions
**File:** Use the FIXED version we created earlier: `Database/04_Seed_TeamRoleDefinitions_Fixed.sql`

**Purpose:** Create role definitions for each team type

**Important Notes:**
- Uses both `TeamTypeDefinitionId` (new FK) AND `TeamType` (old string) for backward compatibility
- Seeds 16 roles total:
  - OSH Committee: 6 roles (Chairperson, Secretary, Employee Rep, Employer Rep, etc.)
  - Risk Assessment: 4 roles (Team Leader, Risk Assessor, Technical Expert, Worker Rep)
  - Investigation: 4 roles (Lead Investigator, Investigator, Technical Specialist, Safety Rep)
  - Custom: 2 roles (Team Captain, Member)

---

### Step 4: Migrate Existing Data (SKIP IF NO DATA)
**File:** Copy from implementation plan - Script 5
**Purpose:** Link existing teams/roles to TeamTypeDefinitions

**Location:** `OSHfiles\Codingdocs\TeamTypeDefinition_Implementation_Plan.md` (Lines 930-1077)

**You can SKIP this** since you deleted all teams and team members.

**What it does (for reference):**
```sql
-- Updates Teams.TeamTypeDefinitionId based on Teams.TeamType string
UPDATE t
SET t.TeamTypeDefinitionId = ttd.TeamTypeDefinitionId
FROM Teams t
INNER JOIN TeamTypeDefinitions ttd ON t.TeamType = ttd.TypeCode;

-- Same for TeamRoleDefinitions
-- Classifies existing roles as Employee/Employer representatives
```

---

### Step 5: Add Foreign Key Constraints
**File:** Copy from implementation plan - Script 6
**Purpose:** Enforce referential integrity

**Location:** `OSHfiles\Codingdocs\TeamTypeDefinition_Implementation_Plan.md` (Lines 1090-1219)

**What it does:**
```sql
-- Add FK: Teams -> TeamTypeDefinitions
ALTER TABLE Teams
ADD CONSTRAINT FK_Teams_TeamTypeDefinitions
FOREIGN KEY (TeamTypeDefinitionId)
REFERENCES TeamTypeDefinitions(TeamTypeDefinitionId);

-- Add FK: TeamRoleDefinitions -> TeamTypeDefinitions
ALTER TABLE TeamRoleDefinitions
ADD CONSTRAINT FK_TeamRoleDefinitions_TeamTypeDefinitions
FOREIGN KEY (TeamTypeDefinitionId)
REFERENCES TeamTypeDefinitions(TeamTypeDefinitionId);

-- Add indexes for performance
CREATE NONCLUSTERED INDEX IX_Teams_TeamTypeDefinitionId ON Teams(TeamTypeDefinitionId);
CREATE NONCLUSTERED INDEX IX_TeamRoleDefinitions_TeamTypeDefinitionId ON TeamRoleDefinitions(TeamTypeDefinitionId);
```

---

### Step 6: Run Validation Queries
**File:** Copy from implementation plan - Script 7
**Purpose:** Verify everything worked correctly

**Location:** `OSHfiles\Codingdocs\TeamTypeDefinition_Implementation_Plan.md` (Lines 1232-1397)

**What it checks:**
- Team type definitions created correctly
- Role definitions per team type
- No orphaned records
- Foreign key constraints in place

---

## Summary - Quick Execution Checklist

```
□ 1. (Optional) Run Update_Models_Only.sql - Verify database structure
□ 2. Run Script 3 - Seed TeamTypeDefinitions (4 team types)
□ 3. Run Script 4 - Seed TeamRoleDefinitions (16 roles) - USE FIXED VERSION
□ 4. SKIP Script 5 - No data to migrate (you deleted everything)
□ 5. Run Script 6 - Add Foreign Key Constraints
□ 6. Run Script 7 - Validation Queries (check results)
□ 7. Build and run application - Test TeamTypeDefinition UI
```

---

## After Migration is Complete

### Phase 1: Test the UI
1. Run the application
2. Navigate to Teams → Team Types
3. Verify you see 4 team types with all their details
4. Click on each to view role definitions

### Phase 2: Future Cleanup (After Thorough Testing)
Once you're confident the new architecture works:

```sql
-- Remove deprecated columns (run these ONLY after confirming everything works)
ALTER TABLE Teams DROP COLUMN TeamType;
ALTER TABLE Teams DROP COLUMN RequiredMemberCount;
ALTER TABLE Teams DROP COLUMN MaxMemberCount;
ALTER TABLE Teams DROP COLUMN RequiresSectionRepresentation;

ALTER TABLE TeamRoleDefinitions DROP COLUMN TeamType;
```

### Phase 3: Performance Improvement (Optional)
Consider replacing `TeamMembers.EmployeePayroll` (string) with `EmployeeId` (int FK):
- Faster joins
- Better indexing
- Reduced storage
- Requires migration script and updating all related code

---

## Troubleshooting

### If Script 3 fails with "TypeCode already exists"
The team types were already seeded. Just continue to next script.

### If Script 4 fails with "TeamType cannot be NULL"
Use the FIXED version we created: `Database/04_Seed_TeamRoleDefinitions_Fixed.sql`

### If Script 6 fails with "FK constraint violation"
Check validation with:
```sql
-- Find teams without TeamTypeDefinitionId
SELECT * FROM Teams WHERE TeamTypeDefinitionId IS NULL;

-- Find roles without TeamTypeDefinitionId
SELECT * FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId IS NULL;
```

---

## Files Updated in C# Code

✅ `Models/Team.cs` - Added TeamTypeDefinitionId FK and navigation property
✅ `Models/TeamMember.cs` - Added term management fields
✅ `Models/TeamRoleDefinition.cs` - Added TeamTypeDefinitionId FK and classification fields
✅ `Models/TeamTypeDefinition.cs` - Already created
✅ `Data/OshDbContext.cs` - Added relationships configuration
✅ `Controllers/TeamTypeDefinitionController.cs` - Already created
✅ `Views/TeamTypeDefinition/Index.cshtml` - Already created
✅ `Views/TeamTypeDefinition/Details.cshtml` - Already created

---

## Expected Results After Migration

**Database:**
- 4 TeamTypeDefinitions
- 16 TeamRoleDefinitions
- Foreign key constraints enforced
- No orphaned records

**Application:**
- Team Types page shows all 4 types
- Details page shows comprehensive info
- Can toggle active/inactive for custom types
- System types protected from deactivation

**Next Steps:**
- Update Team Create/Edit views to use TeamTypeDefinition dropdown
- Update TeamController to use new architecture
- Implement validation service for statutory compliance

---

**Status:** Ready to execute scripts
**Created:** 2025-10-22
**Last Updated:** 2025-10-22