# Team Role Definition Migration Guide

## Overview

This migration transforms the team member role system from a simple string-based approach to a structured, database-driven system with predefined role definitions per team type.

### What Changes?

**Before:**
- `TeamMember.MemberRole` was a free-text string field
- No validation of role names
- No enforcement of team composition rules

**After:**
- `TeamMember.TeamRoleDefinitionId` foreign key to `TeamRoleDefinition` table
- Predefined valid roles for each team type (OSH Committee, Risk Assessment, Investigation)
- Enforced business rules (min/max occurrences, voting rights, qualifications)

---

## Files Created

### 1. Models
- `OSHManagement\Models\TeamRoleDefinition.cs` - New entity model

### 2. Migrations
- `Migrations\20251021100000_AddTeamRoleDefinition.cs` - Creates TeamRoleDefinitions table
- `Migrations\20251021100100_ModifyTeamMemberAddRoleDefinitionFK.cs` - Adds FK to TeamMembers, renames old column
- `Migrations\20251021100200_FinalizeTeamMemberRoleMigration.cs` - Makes FK required, drops old column

### 3. Seeders
- `Data\Seeds\TeamRoleDefinitionSeeder.cs` - Seeds 19 predefined roles
- `Data\Seeds\DatabaseSeeder.cs` - Master seeder coordinator
- `Data\Seeds\MigrateTeamMemberRoles.sql` - SQL script to migrate existing data

### 4. Updated Models
- `Models\TeamMember.cs` - Updated with TeamRoleDefinitionId FK and navigation property
- `Data\OshDbContext.cs` - Added TeamRoleDefinitions DbSet

---

## Migration Steps

### IMPORTANT: Follow these steps IN ORDER!

### Step 1: Apply First Two Migrations

Open **Package Manager Console** in Visual Studio (Tools > NuGet Package Manager > Package Manager Console)

```powershell
# Add the migrations (if not already added by the files)
Add-Migration AddTeamRoleDefinition
Add-Migration ModifyTeamMemberAddRoleDefinitionFK

# Apply migrations to database
Update-Database
```

**What this does:**
- Creates `TeamRoleDefinitions` table
- Adds `TeamRoleDefinitionId` column to `TeamMembers` (nullable)
- Renames `MemberRole` to `MemberRole_Old` to preserve data
- Creates foreign key relationship

---

### Step 2: Run the Seeder

You have two options:

#### Option A: Add to Program.cs (Recommended for Development)

Add this code to `Program.cs` after the database context is configured:

```csharp
// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<OshDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // Run seeder
        OSHManagement.Data.Seeds.DatabaseSeeder.SeedAll(context, logger);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
```

Then run the application. The seeder will execute on startup.

#### Option B: Run Manually via Package Manager Console

```powershell
# In Package Manager Console
$context = Get-DbContext
OSHManagement.Data.Seeds.TeamRoleDefinitionSeeder::SeedTeamRoleDefinitions($context)
```

**Verify:** Check that `TeamRoleDefinitions` table now has 19 rows.

---

### Step 3: Migrate Existing TeamMember Data

Run the SQL script to map existing member roles to the new structure:

1. Open SQL Server Management Studio (SSMS) or Azure Data Studio
2. Connect to your database
3. Open file: `OSHManagement\Data\Seeds\MigrateTeamMemberRoles.sql`
4. **IMPORTANT:** Change line 10 to your actual database name:
   ```sql
   USE OSH_Management; -- Change to your database name
   ```
5. Execute the script
6. Review the output to ensure all records migrated successfully

**What this does:**
- Maps old `MemberRole_Old` values to new `TeamRoleDefinitionId`
- Uses fuzzy matching for common role name variations
- Assigns generic roles to any unmapped members
- Provides verification report

**Verify:** Check that all records in `TeamMembers` now have a non-NULL `TeamRoleDefinitionId`.

---

### Step 4: Finalize Migration

After confirming all data is migrated, apply the final migration:

```powershell
# In Package Manager Console
Add-Migration FinalizeTeamMemberRoleMigration
Update-Database
```

**What this does:**
- Makes `TeamRoleDefinitionId` NOT NULL (required)
- Drops the old `MemberRole_Old` column

---

## Verification Checklist

After completing all steps, verify:

- [ ] `TeamRoleDefinitions` table exists with 19 rows
- [ ] `TeamMembers.TeamRoleDefinitionId` column exists and is NOT NULL
- [ ] Old `TeamMembers.MemberRole` column is dropped
- [ ] Foreign key constraint exists: `FK_TeamMembers_TeamRoleDefinitions_TeamRoleDefinitionId`
- [ ] No NULL values in `TeamMembers.TeamRoleDefinitionId`
- [ ] Run this query to verify data integrity:

```sql
-- Verify all team members have valid roles
SELECT
    tm.MemberId,
    e.FirstName + ' ' + e.LastName AS EmployeeName,
    t.TeamName,
    t.TeamType,
    trd.RoleName,
    trd.RequiresVotingRights,
    tm.IsVotingMember
FROM TeamMembers tm
INNER JOIN Teams t ON tm.TeamId = t.TeamId
INNER JOIN Employees e ON tm.EmployeePayroll = e.PayrollNo
INNER JOIN TeamRoleDefinitions trd ON tm.TeamRoleDefinitionId = trd.TeamRoleDefinitionId
WHERE tm.IsActive = 1
ORDER BY t.TeamName, trd.DisplayOrder;
```

---

## Rollback Instructions

If you need to rollback:

```powershell
# Rollback to before migrations
Update-Database -Migration <NameOfMigrationBeforeTeamRoles>

# Or remove migrations entirely
Remove-Migration
Remove-Migration
Remove-Migration
```

**Note:** Rollback will restore `MemberRole` string column but data migration from Step 3 cannot be auto-reversed.

---

## Team Role Definitions Reference

### OSH Committee Roles
1. **Chairperson** (1 required) - Leads meetings, voting rights
2. **Secretary** (1 required) - Documentation, voting rights
3. **Safety Representative** (2+ required) - Worker safety rep, voting rights
4. **Management Representative** (2+ required) - Management rep, voting rights
5. **Worker Representative** - General worker rep, voting rights
6. **Observer** - No voting rights

### Risk Assessment Team Roles
1. **Team Leader** (1 required) - Leads assessments, voting rights
2. **Risk Assessor** (2+ required) - Conducts assessments, voting rights
3. **Subject Matter Expert** - Technical expertise, voting rights
4. **Documentation Officer** (max 1) - Records findings, voting rights
5. **Reviewer** - Validates findings, no voting rights

### Investigation Team Roles
1. **Lead Investigator** (1 required) - Leads investigation, voting rights
2. **Investigator** (2+ required) - Participates in investigation, voting rights
3. **Technical Specialist** - Technical analysis, voting rights
4. **Witness Liaison** - Conducts interviews, voting rights
5. **Documentation Officer** (1 required) - Prepares reports, voting rights
6. **Observer** - Oversight, no voting rights

---

## Next Steps After Migration

1. **Update TeamController** to use TeamRoleDefinitionId when creating/editing members
2. **Update Views** to show role dropdowns filtered by team type
3. **Add Validation** to enforce min/max role occurrences
4. **Update Reports** to display role names from TeamRoleDefinition
5. **Consider Auto-setting** `IsVotingMember` based on `TeamRoleDefinition.RequiresVotingRights`

---

## Support

If you encounter issues:
1. Check migration output for errors
2. Verify database connection string
3. Check SQL script output for unmapped records
4. Review logs for seeder errors
5. Ensure all prerequisites are met (EF Core tools installed)

---

## Package Manager Console Commands Summary

```powershell
# Step 1: Apply initial migrations
Update-Database

# Step 2: Verify TeamRoleDefinitions were seeded
# (Should happen automatically if Program.cs updated, or run seeder manually)

# Step 3: Run SQL script in SSMS
# (MigrateTeamMemberRoles.sql)

# Step 4: Apply final migration
Update-Database

# Verification query
Invoke-Sqlcmd -Query "SELECT COUNT(*) FROM TeamRoleDefinitions"
Invoke-Sqlcmd -Query "SELECT COUNT(*) FROM TeamMembers WHERE TeamRoleDefinitionId IS NULL"
```

---

**Migration Created:** 2025-10-21
**Database Impact:** TeamMembers, TeamRoleDefinitions tables
**Data Migration Required:** Yes (existing TeamMember records)
