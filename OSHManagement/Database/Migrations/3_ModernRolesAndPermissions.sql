-- =============================================
-- Script: 3_ModernRolesAndPermissions.sql
-- Description: Seed modern roles and permissions
--              No legacy ties - clean, purpose-built system
-- Author: OSH Management System
-- Date: 2025-01-09
-- Run AFTER: 2_AddScopeLevelToRoles.sql
-- =============================================

BEGIN TRANSACTION;

PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT 'Seeding modern roles and permissions...';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';

-- ==========================================
-- PART 1: PERMISSIONS
-- ==========================================

PRINT '';
PRINT 'Creating permissions...';

-- Employee Management
INSERT INTO Permissions (PermissionName, Description, Module, Action, IsActive, CreatedAt)
VALUES
    ('Employee.Read', 'View employee information', 'Employee', 'Read', 1, GETUTCDATE()),
    ('Employee.Create', 'Create new employees', 'Employee', 'Create', 1, GETUTCDATE()),
    ('Employee.Update', 'Update employee information', 'Employee', 'Update', 1, GETUTCDATE()),
    ('Employee.Delete', 'Delete employees', 'Employee', 'Delete', 1, GETUTCDATE()),
    ('Employee.ManageRoles', 'Assign roles to employees', 'Employee', 'ManageRoles', 1, GETUTCDATE());

-- Incident Management
INSERT INTO Permissions (PermissionName, Description, Module, Action, IsActive, CreatedAt)
VALUES
    ('Incident.Read', 'View incidents', 'Incident', 'Read', 1, GETUTCDATE()),
    ('Incident.Create', 'Report new incidents', 'Incident', 'Create', 1, GETUTCDATE()),
    ('Incident.Update', 'Update incident information', 'Incident', 'Update', 1, GETUTCDATE()),
    ('Incident.Delete', 'Delete incidents', 'Incident', 'Delete', 1, GETUTCDATE()),
    ('Incident.Investigate', 'Conduct incident investigations', 'Incident', 'Investigate', 1, GETUTCDATE()),
    ('Incident.Approve', 'Approve incident reports', 'Incident', 'Approve', 1, GETUTCDATE()),
    ('Incident.Close', 'Close incident cases', 'Incident', 'Close', 1, GETUTCDATE());

-- Hazard/Risk Management
INSERT INTO Permissions (PermissionName, Description, Module, Action, IsActive, CreatedAt)
VALUES
    ('Hazard.Read', 'View hazards and risks', 'Hazard', 'Read', 1, GETUTCDATE()),
    ('Hazard.Create', 'Report new hazards', 'Hazard', 'Create', 1, GETUTCDATE()),
    ('Hazard.Update', 'Update hazard information', 'Hazard', 'Update', 1, GETUTCDATE()),
    ('Hazard.Delete', 'Delete hazards', 'Hazard', 'Delete', 1, GETUTCDATE()),
    ('Hazard.Assess', 'Conduct risk assessments', 'Hazard', 'Assess', 1, GETUTCDATE()),
    ('Hazard.Approve', 'Approve risk assessments', 'Hazard', 'Approve', 1, GETUTCDATE());

-- Organization Management
INSERT INTO Permissions (PermissionName, Description, Module, Action, IsActive, CreatedAt)
VALUES
    ('Organization.Read', 'View organization structure', 'Organization', 'Read', 1, GETUTCDATE()),
    ('Organization.Manage', 'Manage stations, departments, sections', 'Organization', 'Manage', 1, GETUTCDATE());

-- Administration
INSERT INTO Permissions (PermissionName, Description, Module, Action, IsActive, CreatedAt)
VALUES
    ('Admin.Roles', 'Manage roles and permissions', 'Administration', 'Roles', 1, GETUTCDATE()),
    ('Admin.System', 'System configuration', 'Administration', 'System', 1, GETUTCDATE()),
    ('Admin.Reports', 'Generate and view reports', 'Administration', 'Reports', 1, GETUTCDATE()),
    ('Admin.Audit', 'View audit logs', 'Administration', 'Audit', 1, GETUTCDATE());

PRINT '✅ Created ' + CAST(@@ROWCOUNT AS VARCHAR) + ' permissions';

-- ==========================================
-- PART 2: ROLES WITH SCOPES
-- ==========================================

PRINT '';
PRINT 'Creating roles with scope levels...';

-- Organization-Level (ScopeLevel = 1)
INSERT INTO Roles (RoleName, Description, ScopeLevel, IsSystemRole, IsActive, CreatedAt)
VALUES
    ('Admin', 'System Administrator - Full system access', 1, 1, 1, GETUTCDATE()),
    ('OSH Manager', 'Manages OSH system across entire organization', 1, 1, 1, GETUTCDATE()),
    ('HR Manager', 'Manages employees across entire organization', 1, 0, 1, GETUTCDATE());

-- Station-Level (ScopeLevel = 2)
INSERT INTO Roles (RoleName, Description, ScopeLevel, IsSystemRole, IsActive, CreatedAt)
VALUES
    ('Station Manager', 'Manages all OSH activities within a station', 2, 0, 1, GETUTCDATE()),
    ('Safety Officer', 'Station-level safety monitoring and compliance', 2, 0, 1, GETUTCDATE());

-- Department-Level (ScopeLevel = 3)
INSERT INTO Roles (RoleName, Description, ScopeLevel, IsSystemRole, IsActive, CreatedAt)
VALUES
    ('Department Head', 'Departmental oversight and approvals', 3, 0, 1, GETUTCDATE()),
    ('Department Safety Rep', 'Department safety representative', 3, 0, 1, GETUTCDATE());

-- Team-Level (ScopeLevel = 4)
INSERT INTO Roles (RoleName, Description, ScopeLevel, IsSystemRole, IsActive, CreatedAt)
VALUES
    ('Supervisor', 'Team supervision and incident management', 4, 0, 1, GETUTCDATE()),
    ('Safety Coordinator', 'Team-level safety coordination', 4, 0, 1, GETUTCDATE());

-- Self-Level (ScopeLevel = 5)
INSERT INTO Roles (RoleName, Description, ScopeLevel, IsSystemRole, IsActive, CreatedAt)
VALUES
    ('Employee', 'Standard employee - own data access only', 5, 1, 1, GETUTCDATE()),
    ('Contractor', 'Contractor - limited own data access', 5, 0, 1, GETUTCDATE());

PRINT '✅ Created ' + CAST(@@ROWCOUNT AS VARCHAR) + ' roles';

-- ==========================================
-- PART 3: ROLE-PERMISSION ASSIGNMENTS
-- ==========================================

PRINT '';
PRINT 'Assigning permissions to roles...';

DECLARE @AssignmentCount INT = 0;

-- Admin (Everything)
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT r.RoleId, p.PermissionId, GETUTCDATE()
FROM Roles r CROSS JOIN Permissions p
WHERE r.RoleName = 'Admin';
SET @AssignmentCount = @AssignmentCount + @@ROWCOUNT;

-- OSH Manager (Full OSH, no system config)
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT r.RoleId, p.PermissionId, GETUTCDATE()
FROM Roles r CROSS JOIN Permissions p
WHERE r.RoleName = 'OSH Manager'
  AND (p.Module IN ('Incident', 'Hazard', 'Organization', 'Employee')
       OR p.PermissionName = 'Admin.Reports');
SET @AssignmentCount = @AssignmentCount + @@ROWCOUNT;

-- HR Manager (Employee management + Reports)
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT r.RoleId, p.PermissionId, GETUTCDATE()
FROM Roles r CROSS JOIN Permissions p
WHERE r.RoleName = 'HR Manager'
  AND (p.Module = 'Employee' OR p.PermissionName IN ('Organization.Read', 'Admin.Reports'));
SET @AssignmentCount = @AssignmentCount + @@ROWCOUNT;

-- Station Manager
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT r.RoleId, p.PermissionId, GETUTCDATE()
FROM Roles r CROSS JOIN Permissions p
WHERE r.RoleName = 'Station Manager'
  AND p.PermissionName IN (
      'Employee.Read', 'Employee.Create', 'Employee.Update',
      'Incident.Read', 'Incident.Create', 'Incident.Update', 'Incident.Approve', 'Incident.Close',
      'Hazard.Read', 'Hazard.Create', 'Hazard.Update', 'Hazard.Approve',
      'Organization.Read', 'Admin.Reports'
  );
SET @AssignmentCount = @AssignmentCount + @@ROWCOUNT;

-- Safety Officer
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT r.RoleId, p.PermissionId, GETUTCDATE()
FROM Roles r CROSS JOIN Permissions p
WHERE r.RoleName = 'Safety Officer'
  AND p.PermissionName IN (
      'Employee.Read',
      'Incident.Read', 'Incident.Create', 'Incident.Update', 'Incident.Investigate',
      'Hazard.Read', 'Hazard.Create', 'Hazard.Update', 'Hazard.Assess',
      'Organization.Read'
  );
SET @AssignmentCount = @AssignmentCount + @@ROWCOUNT;

-- Department Head
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT r.RoleId, p.PermissionId, GETUTCDATE()
FROM Roles r CROSS JOIN Permissions p
WHERE r.RoleName = 'Department Head'
  AND p.PermissionName IN (
      'Employee.Read', 'Employee.Update',
      'Incident.Read', 'Incident.Create', 'Incident.Update', 'Incident.Approve',
      'Hazard.Read', 'Hazard.Create', 'Hazard.Update', 'Hazard.Approve',
      'Organization.Read', 'Admin.Reports'
  );
SET @AssignmentCount = @AssignmentCount + @@ROWCOUNT;

-- Department Safety Rep
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT r.RoleId, p.PermissionId, GETUTCDATE()
FROM Roles r CROSS JOIN Permissions p
WHERE r.RoleName = 'Department Safety Rep'
  AND p.PermissionName IN (
      'Employee.Read',
      'Incident.Read', 'Incident.Create', 'Incident.Update',
      'Hazard.Read', 'Hazard.Create', 'Hazard.Update', 'Hazard.Assess'
  );
SET @AssignmentCount = @AssignmentCount + @@ROWCOUNT;

-- Supervisor
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT r.RoleId, p.PermissionId, GETUTCDATE()
FROM Roles r CROSS JOIN Permissions p
WHERE r.RoleName = 'Supervisor'
  AND p.PermissionName IN (
      'Employee.Read',
      'Incident.Read', 'Incident.Create', 'Incident.Update',
      'Hazard.Read', 'Hazard.Create'
  );
SET @AssignmentCount = @AssignmentCount + @@ROWCOUNT;

-- Safety Coordinator
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT r.RoleId, p.PermissionId, GETUTCDATE()
FROM Roles r CROSS JOIN Permissions p
WHERE r.RoleName = 'Safety Coordinator'
  AND p.PermissionName IN (
      'Employee.Read',
      'Incident.Read', 'Incident.Create', 'Incident.Update',
      'Hazard.Read', 'Hazard.Create', 'Hazard.Assess'
  );
SET @AssignmentCount = @AssignmentCount + @@ROWCOUNT;

-- Employee (Basic)
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT r.RoleId, p.PermissionId, GETUTCDATE()
FROM Roles r CROSS JOIN Permissions p
WHERE r.RoleName = 'Employee'
  AND p.PermissionName IN (
      'Employee.Read',
      'Incident.Read', 'Incident.Create',
      'Hazard.Read', 'Hazard.Create'
  );
SET @AssignmentCount = @AssignmentCount + @@ROWCOUNT;

-- Contractor
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT r.RoleId, p.PermissionId, GETUTCDATE()
FROM Roles r CROSS JOIN Permissions p
WHERE r.RoleName = 'Contractor'
  AND p.PermissionName IN (
      'Employee.Read',
      'Incident.Read', 'Incident.Create',
      'Hazard.Read', 'Hazard.Create'
  );
SET @AssignmentCount = @AssignmentCount + @@ROWCOUNT;

PRINT '✅ Created ' + CAST(@AssignmentCount AS VARCHAR) + ' role-permission assignments';

-- ==========================================
-- PART 4: ASSIGN ADMIN ROLE TO ADMIN USER
-- ==========================================

PRINT '';
PRINT 'Assigning Admin role to ADMIN001 user...';

DECLARE @AdminEmployeeId INT = (
    SELECT TOP 1 EmployeeId
    FROM Employees
    WHERE PayrollNo = 'ADMIN001'
);

IF @AdminEmployeeId IS NOT NULL
BEGIN
    DECLARE @AdminRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Admin');

    INSERT INTO EmployeeRoles (EmployeeId, RoleId, AssignedAt, IsActive, AssignedBy)
    VALUES (@AdminEmployeeId, @AdminRoleId, GETUTCDATE(), 1, 'SYSTEM');

    PRINT '✅ Assigned Admin role to ADMIN001 (EmployeeId: ' + CAST(@AdminEmployeeId AS VARCHAR) + ')';
END
ELSE
BEGIN
    PRINT '⚠️  WARNING: Admin user (ADMIN001) not found - skipping role assignment';
    PRINT '   You will need to manually assign Admin role to an employee';
END

-- ==========================================
-- PART 5: MAKE SCOPELEVEL REQUIRED
-- ==========================================

PRINT '';
PRINT 'Making ScopeLevel column required...';

ALTER TABLE Roles
ALTER COLUMN ScopeLevel INT NOT NULL;

PRINT '✅ ScopeLevel is now required (NOT NULL)';

COMMIT TRANSACTION;

-- ==========================================
-- SUMMARY
-- ==========================================

PRINT '';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '✅ Modern roles and permissions seeded successfully';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '';
PRINT 'Summary:';

-- Use variables for counts (SQL Server doesn't allow subqueries in PRINT)
DECLARE @RoleCount INT = (SELECT COUNT(*) FROM Roles);
DECLARE @PermissionCount INT = (SELECT COUNT(*) FROM Permissions);
DECLARE @AssignCount INT = (SELECT COUNT(*) FROM RolePermissions);
DECLARE @OrgCount INT = (SELECT COUNT(*) FROM Roles WHERE ScopeLevel = 1);
DECLARE @StationCount INT = (SELECT COUNT(*) FROM Roles WHERE ScopeLevel = 2);
DECLARE @DeptCount INT = (SELECT COUNT(*) FROM Roles WHERE ScopeLevel = 3);
DECLARE @TeamCount INT = (SELECT COUNT(*) FROM Roles WHERE ScopeLevel = 4);
DECLARE @SelfCount INT = (SELECT COUNT(*) FROM Roles WHERE ScopeLevel = 5);

PRINT '  Roles created:               ' + CAST(@RoleCount AS VARCHAR);
PRINT '  Permissions created:         ' + CAST(@PermissionCount AS VARCHAR);
PRINT '  Role-Permission assignments: ' + CAST(@AssignCount AS VARCHAR);
PRINT '';
PRINT 'Roles by Scope Level:';
PRINT '  Organization (1): ' + CAST(@OrgCount AS VARCHAR) + ' roles';
PRINT '  Station (2):      ' + CAST(@StationCount AS VARCHAR) + ' roles';
PRINT '  Department (3):   ' + CAST(@DeptCount AS VARCHAR) + ' roles';
PRINT '  Team (4):         ' + CAST(@TeamCount AS VARCHAR) + ' roles';
PRINT '  Self (5):         ' + CAST(@SelfCount AS VARCHAR) + ' roles';
PRINT '';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '';
PRINT 'Next: Update C# models and services (see instructions)';
