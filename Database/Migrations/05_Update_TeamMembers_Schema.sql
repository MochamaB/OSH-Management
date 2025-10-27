-- =============================================================================
-- MIGRATION: Update TeamMembers Table Schema
-- =============================================================================
-- Purpose: Replace old MemberRole column with TeamRoleDefinitionId FK
-- Date: 2025-01-XX
-- =============================================================================

USE OSHManagement;
GO

PRINT '========================================';
PRINT 'UPDATING TeamMembers TABLE SCHEMA';
PRINT '========================================';
PRINT '';

-- =============================================================================
-- STEP 1: Check current schema
-- =============================================================================
PRINT '1. Checking current schema...';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamMembers') AND name = 'MemberRole')
    PRINT '   ⚠ MemberRole column exists (OLD - will be removed)';
ELSE
    PRINT '   ✓ MemberRole column does not exist';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamMembers') AND name = 'TeamRoleDefinitionId')
    PRINT '   ✓ TeamRoleDefinitionId column already exists';
ELSE
    PRINT '   ⚠ TeamRoleDefinitionId column missing (will be added)';

PRINT '';

-- =============================================================================
-- STEP 2: Add TeamRoleDefinitionId column if it doesn't exist
-- =============================================================================
PRINT '2. Adding TeamRoleDefinitionId column...';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamMembers') AND name = 'TeamRoleDefinitionId')
BEGIN
    ALTER TABLE TeamMembers
    ADD TeamRoleDefinitionId INT NULL;

    PRINT '   ✓ TeamRoleDefinitionId column added';
END
ELSE
BEGIN
    PRINT '   ℹ TeamRoleDefinitionId column already exists, skipping...';
END

PRINT '';

-- =============================================================================
-- STEP 3: Migrate data from MemberRole to TeamRoleDefinitionId
-- =============================================================================
PRINT '3. Migrating data from MemberRole to TeamRoleDefinitionId...';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamMembers') AND name = 'MemberRole')
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamMembers') AND name = 'TeamRoleDefinitionId')
BEGIN
    -- Try to match MemberRole text to RoleName in TeamRoleDefinitions
    UPDATE tm
    SET tm.TeamRoleDefinitionId = trd.TeamRoleDefinitionId
    FROM TeamMembers tm
    INNER JOIN TeamRoleDefinitions trd ON tm.MemberRole = trd.RoleName
    WHERE tm.TeamRoleDefinitionId IS NULL
      AND tm.MemberRole IS NOT NULL;

    DECLARE @MigratedCount INT = @@ROWCOUNT;
    PRINT '   ✓ Migrated ' + CAST(@MigratedCount AS VARCHAR(10)) + ' records';

    -- Check for records that couldn't be migrated
    DECLARE @UnmigratedCount INT = (
        SELECT COUNT(*)
        FROM TeamMembers
        WHERE TeamRoleDefinitionId IS NULL
          AND MemberRole IS NOT NULL
    );

    IF @UnmigratedCount > 0
    BEGIN
        PRINT '   ⚠ WARNING: ' + CAST(@UnmigratedCount AS VARCHAR(10)) + ' records could not be auto-migrated';
        PRINT '   These records have MemberRole values that do not match any TeamRoleDefinition.RoleName';
        PRINT '   Please review and manually update these records:';
        PRINT '';

        SELECT
            MemberId,
            TeamId,
            EmployeePayroll,
            MemberRole,
            'No matching role found' AS Issue
        FROM TeamMembers
        WHERE TeamRoleDefinitionId IS NULL
          AND MemberRole IS NOT NULL;
    END
END
ELSE
BEGIN
    PRINT '   ℹ No migration needed or columns do not exist';
END

PRINT '';

-- =============================================================================
-- STEP 4: Remove old MemberRole column
-- =============================================================================
PRINT '4. Removing old MemberRole column...';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamMembers') AND name = 'MemberRole')
BEGIN
    -- Drop any constraints on MemberRole first
    DECLARE @ConstraintName NVARCHAR(200);
    DECLARE @SQL NVARCHAR(MAX);

    SELECT @ConstraintName = dc.name
    FROM sys.default_constraints dc
    WHERE dc.parent_object_id = OBJECT_ID('TeamMembers')
      AND COL_NAME(dc.parent_object_id, dc.parent_column_id) = 'MemberRole';

    IF @ConstraintName IS NOT NULL
    BEGIN
        SET @SQL = 'ALTER TABLE TeamMembers DROP CONSTRAINT ' + QUOTENAME(@ConstraintName);
        EXEC sp_executesql @SQL;
        PRINT '   ✓ Dropped constraint on MemberRole';
    END

    -- Drop the column
    ALTER TABLE TeamMembers DROP COLUMN MemberRole;
    PRINT '   ✓ MemberRole column removed';
END
ELSE
BEGIN
    PRINT '   ℹ MemberRole column does not exist, skipping...';
END

PRINT '';

-- =============================================================================
-- STEP 5: Add Foreign Key constraint
-- =============================================================================
PRINT '5. Adding Foreign Key constraint...';

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TeamMembers_TeamRoleDefinitions')
BEGIN
    ALTER TABLE TeamMembers
    ADD CONSTRAINT FK_TeamMembers_TeamRoleDefinitions
    FOREIGN KEY (TeamRoleDefinitionId)
    REFERENCES TeamRoleDefinitions(TeamRoleDefinitionId);

    PRINT '   ✓ Foreign Key constraint added';
END
ELSE
BEGIN
    PRINT '   ℹ Foreign Key constraint already exists';
END

PRINT '';

-- =============================================================================
-- STEP 6: Verification
-- =============================================================================
PRINT '========================================';
PRINT 'VERIFICATION';
PRINT '========================================';
PRINT '';

DECLARE @TotalMembers INT = (SELECT COUNT(*) FROM TeamMembers);
DECLARE @MembersWithRole INT = (SELECT COUNT(*) FROM TeamMembers WHERE TeamRoleDefinitionId IS NOT NULL);
DECLARE @MembersWithoutRole INT = (SELECT COUNT(*) FROM TeamMembers WHERE TeamRoleDefinitionId IS NULL);

PRINT 'TeamMembers Table Status:';
PRINT '  Total Members: ' + CAST(@TotalMembers AS VARCHAR(10));
PRINT '  Members with Role: ' + CAST(@MembersWithRole AS VARCHAR(10));
PRINT '  Members without Role: ' + CAST(@MembersWithoutRole AS VARCHAR(10));

IF @MembersWithoutRole > 0
    PRINT '  ⚠ Some members do not have roles assigned';
ELSE
    PRINT '  ✓ All members have roles assigned';

PRINT '';
PRINT '========================================';
PRINT 'MIGRATION COMPLETE';
PRINT '========================================';

GO
