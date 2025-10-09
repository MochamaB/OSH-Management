-- =============================================
-- Script: 2_AddScopeLevelToRoles.sql
-- Description: Add ScopeLevel and enhanced columns to Roles
--              Removes LegacyRoleMapping column
-- Author: OSH Management System
-- Date: 2025-01-09
-- =============================================

BEGIN TRANSACTION;

PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT 'Adding ScopeLevel to Roles table...';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

-- 1. Add new columns
ALTER TABLE Roles
ADD
    ScopeLevel INT NULL,
    IsSystemRole BIT NOT NULL DEFAULT 0,
    AllowCrossDepartmentAccess BIT NOT NULL DEFAULT 0,
    AllowCrossStationAccess BIT NOT NULL DEFAULT 0;

PRINT '✅ Added new columns to Roles table';

-- 2. Drop legacy column (if exists)
IF COL_LENGTH('Roles', 'LegacyRoleMapping') IS NOT NULL
BEGIN
    ALTER TABLE Roles DROP COLUMN LegacyRoleMapping;
    PRINT '✅ Dropped LegacyRoleMapping column';
END
ELSE
BEGIN
    PRINT '⚠️  LegacyRoleMapping column does not exist (skipped)';
END

-- 3. Note: ScopeLevel will be made NOT NULL after ModernRolesAndPermissions.sql runs

COMMIT TRANSACTION;

PRINT '';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '✅ Migration completed successfully';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '';
PRINT 'Column changes:';
PRINT '  + ScopeLevel (INT NULL)';
PRINT '  + IsSystemRole (BIT NOT NULL DEFAULT 0)';
PRINT '  + AllowCrossDepartmentAccess (BIT NOT NULL DEFAULT 0)';
PRINT '  + AllowCrossStationAccess (BIT NOT NULL DEFAULT 0)';
PRINT '  - LegacyRoleMapping (REMOVED)';
PRINT '';
PRINT 'Next step: Run 3_ModernRolesAndPermissions.sql to seed new data';
