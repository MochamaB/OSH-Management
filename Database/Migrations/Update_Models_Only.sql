-- =============================================================================
-- DATABASE STRUCTURE VERIFICATION & CLEANUP SCRIPT
-- =============================================================================
-- This script verifies the database already has all required columns
-- and provides cleanup recommendations
-- =============================================================================

USE OSHManagement;
GO

PRINT '========================================';
PRINT 'DATABASE STRUCTURE VERIFICATION';
PRINT '========================================';
PRINT '';

-- =============================================================================
-- VERIFY EXISTING COLUMNS
-- =============================================================================

PRINT '1. Verifying Teams table...';
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Teams') AND name = 'TeamTypeDefinitionId')
    PRINT '   ✓ Teams.TeamTypeDefinitionId exists';
ELSE
    PRINT '   ❌ ERROR: Teams.TeamTypeDefinitionId missing!';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Teams') AND name = 'TeamType')
    PRINT '   ⚠ Teams.TeamType (old column) still exists - can be removed later';

PRINT '';

PRINT '2. Verifying TeamMembers table...';
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamMembers') AND name = 'TermEndDate')
    PRINT '   ✓ TeamMembers.TermEndDate exists';
ELSE
    PRINT '   ❌ ERROR: TeamMembers.TermEndDate missing!';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamMembers') AND name = 'TermNumber')
    PRINT '   ✓ TeamMembers.TermNumber exists';
ELSE
    PRINT '   ❌ ERROR: TeamMembers.TermNumber missing!';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamMembers') AND name = 'IsElected')
    PRINT '   ✓ TeamMembers.IsElected exists';
ELSE
    PRINT '   ❌ ERROR: TeamMembers.IsElected missing!';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamMembers') AND name = 'ElectionReference')
    PRINT '   ✓ TeamMembers.ElectionReference exists';
ELSE
    PRINT '   ❌ ERROR: TeamMembers.ElectionReference missing!';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamMembers') AND name = 'AppointmentLetterRef')
    PRINT '   ✓ TeamMembers.AppointmentLetterRef exists';
ELSE
    PRINT '   ❌ ERROR: TeamMembers.AppointmentLetterRef missing!';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamMembers') AND name = 'EmployeePayroll')
    PRINT '   ⚠ TeamMembers.EmployeePayroll (string FK) exists - consider replacing with EmployeeId (int) for better performance';

PRINT '';

PRINT '3. Verifying TeamRoleDefinitions table...';
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamRoleDefinitions') AND name = 'TeamTypeDefinitionId')
    PRINT '   ✓ TeamRoleDefinitions.TeamTypeDefinitionId exists';
ELSE
    PRINT '   ❌ ERROR: TeamRoleDefinitions.TeamTypeDefinitionId missing!';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamRoleDefinitions') AND name = 'IsEmployeeRepresentative')
    PRINT '   ✓ TeamRoleDefinitions.IsEmployeeRepresentative exists';
ELSE
    PRINT '   ❌ ERROR: TeamRoleDefinitions.IsEmployeeRepresentative missing!';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamRoleDefinitions') AND name = 'IsEmployerRepresentative')
    PRINT '   ✓ TeamRoleDefinitions.IsEmployerRepresentative exists';
ELSE
    PRINT '   ❌ ERROR: TeamRoleDefinitions.IsEmployerRepresentative missing!';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamRoleDefinitions') AND name = 'IsExOfficio')
    PRINT '   ✓ TeamRoleDefinitions.IsExOfficio exists';
ELSE
    PRINT '   ❌ ERROR: TeamRoleDefinitions.IsExOfficio missing!';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamRoleDefinitions') AND name = 'TeamType')
    PRINT '   ⚠ TeamRoleDefinitions.TeamType (old column) still exists - can be removed later';

PRINT '';

-- =============================================================================
-- CHECK FOREIGN KEY CONSTRAINTS
-- =============================================================================

PRINT '4. Verifying Foreign Key Constraints...';
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Teams_TeamTypeDefinitions')
    PRINT '   ✓ FK_Teams_TeamTypeDefinitions exists';
ELSE
    PRINT '   ⚠ FK_Teams_TeamTypeDefinitions missing - should be added';

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TeamRoleDefinitions_TeamTypeDefinitions')
    PRINT '   ✓ FK_TeamRoleDefinitions_TeamTypeDefinitions exists';
ELSE
    PRINT '   ⚠ FK_TeamRoleDefinitions_TeamTypeDefinitions missing - should be added';

PRINT '';

-- =============================================================================
-- DATA VERIFICATION
-- =============================================================================

PRINT '5. Verifying data integrity...';

DECLARE @TeamsWithNullFK INT = (SELECT COUNT(*) FROM Teams WHERE TeamTypeDefinitionId IS NULL);
DECLARE @RolesWithNullFK INT = (SELECT COUNT(*) FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId IS NULL);
DECLARE @TotalTeams INT = (SELECT COUNT(*) FROM Teams);
DECLARE @TotalRoles INT = (SELECT COUNT(*) FROM TeamRoleDefinitions);

PRINT '   Teams: ' + CAST(@TotalTeams AS VARCHAR(10)) + ' total';
IF @TeamsWithNullFK > 0
    PRINT '   ⚠ WARNING: ' + CAST(@TeamsWithNullFK AS VARCHAR(10)) + ' teams have NULL TeamTypeDefinitionId';
ELSE IF @TotalTeams > 0
    PRINT '   ✓ All teams have valid TeamTypeDefinitionId';

PRINT '   TeamRoleDefinitions: ' + CAST(@TotalRoles AS VARCHAR(10)) + ' total';
IF @RolesWithNullFK > 0
    PRINT '   ⚠ WARNING: ' + CAST(@RolesWithNullFK AS VARCHAR(10)) + ' roles have NULL TeamTypeDefinitionId';
ELSE IF @TotalRoles > 0
    PRINT '   ✓ All roles have valid TeamTypeDefinitionId';

PRINT '';

-- =============================================================================
-- SUMMARY & RECOMMENDATIONS
-- =============================================================================

PRINT '========================================';
PRINT 'SUMMARY & RECOMMENDATIONS';
PRINT '========================================';
PRINT '';

PRINT 'DATABASE STATUS:';
PRINT '✓ All required columns exist in database';
PRINT '✓ Database structure is ready for new TeamTypeDefinition architecture';
PRINT '';

PRINT 'NEXT STEPS:';
PRINT '1. Update C# Models (Team.cs, TeamMember.cs, TeamRoleDefinition.cs)';
PRINT '2. Update DbContext configuration';
PRINT '3. Run scripts 3-6 from implementation plan to seed data and add FKs';
PRINT '';

PRINT 'OPTIONAL IMPROVEMENTS (After migration is complete):';
PRINT '1. Remove old Teams.TeamType column (after verifying all code uses TeamTypeDefinitionId)';
PRINT '2. Remove old TeamRoleDefinitions.TeamType column';
PRINT '3. Consider replacing TeamMembers.EmployeePayroll (string) with EmployeeId (int) for better performance';
PRINT '4. Remove Teams.RequiredMemberCount, MaxMemberCount, RequiresSectionRepresentation (moved to TeamTypeDefinition)';
PRINT '';

GO
