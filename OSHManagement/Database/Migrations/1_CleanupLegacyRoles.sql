-- =============================================
-- Script: 1_CleanupLegacyRoles.sql
-- Description: Clean up legacy role data
-- WARNING: This deletes all role assignments!
-- Run BEFORE adding ScopeLevel column
-- Author: OSH Management System
-- Date: 2025-01-09
-- =============================================

BEGIN TRANSACTION;

PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT 'Starting cleanup of legacy role data...';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

-- 1. Backup current admin user (if exists)
DECLARE @AdminEmployeeId INT = (
    SELECT TOP 1 e.EmployeeId
    FROM Employees e
    INNER JOIN EmployeeRoles er ON e.EmployeeId = er.EmployeeId
    INNER JOIN Roles r ON er.RoleId = r.RoleId
    WHERE r.RoleName = 'Admin' AND e.PayrollNo = 'ADMIN001'
);

PRINT 'Admin EmployeeId: ' + ISNULL(CAST(@AdminEmployeeId AS VARCHAR), 'Not found');

-- 2. Delete all role-permission assignments
DELETE FROM RolePermissions;
DBCC CHECKIDENT ('RolePermissions', RESEED, 0);
PRINT '✅ Deleted RolePermissions: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- 3. Delete all employee-role assignments
DELETE FROM EmployeeRoles;
DBCC CHECKIDENT ('EmployeeRoles', RESEED, 0);
PRINT '✅ Deleted EmployeeRoles: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- 4. Delete all permissions
DELETE FROM Permissions;
DBCC CHECKIDENT ('Permissions', RESEED, 0);
PRINT '✅ Deleted Permissions: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- 5. Delete all roles
DELETE FROM Roles;
DBCC CHECKIDENT ('Roles', RESEED, 0);
PRINT '✅ Deleted Roles: ' + CAST(@@ROWCOUNT AS VARCHAR);

COMMIT TRANSACTION;

PRINT '';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '✅ Database cleaned successfully';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '';
PRINT 'Next step: Run 2_AddScopeLevelToRoles.sql';
