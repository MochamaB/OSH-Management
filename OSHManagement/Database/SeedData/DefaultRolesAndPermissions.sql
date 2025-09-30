-- =============================================
-- Author: OSH Management System
-- Create date: 2024-09-29
-- Description: Seed default roles and permissions for OSH Management System
-- =============================================

-- Insert default OrgCategory (required for stations)
IF NOT EXISTS (SELECT 1 FROM OrgCategories WHERE CategoryName = 'Tea Factory')
BEGIN
    INSERT INTO OrgCategories (CategoryName, Description, IsActive, CreatedAt)
    VALUES ('Tea Factory', 'KTDA Tea Processing Factories', 1, GETUTCDATE())
END

-- Insert default permissions
INSERT INTO Permissions (PermissionName, Description, Module, Action, IsActive, CreatedAt)
VALUES 
    -- OSH Module Permissions
    ('OSH.Policy.Read', 'View OSH policies', 'OSH', 'Read', 1, GETUTCDATE()),
    ('OSH.Policy.Create', 'Create OSH policies', 'OSH', 'Create', 1, GETUTCDATE()),
    ('OSH.Policy.Update', 'Update OSH policies', 'OSH', 'Update', 1, GETUTCDATE()),
    ('OSH.Policy.Delete', 'Delete OSH policies', 'OSH', 'Delete', 1, GETUTCDATE()),
    
    -- Incident Management Permissions
    ('Incident.Read', 'View incidents', 'Incidents', 'Read', 1, GETUTCDATE()),
    ('Incident.Create', 'Report incidents', 'Incidents', 'Create', 1, GETUTCDATE()),
    ('Incident.Update', 'Update incidents', 'Incidents', 'Update', 1, GETUTCDATE()),
    ('Incident.Investigate', 'Investigate incidents', 'Incidents', 'Investigate', 1, GETUTCDATE()),
    ('Incident.Approve', 'Approve incident reports', 'Incidents', 'Approve', 1, GETUTCDATE()),
    
    -- Risk Assessment Permissions
    ('Risk.Read', 'View risk assessments', 'RiskAssessment', 'Read', 1, GETUTCDATE()),
    ('Risk.Create', 'Create risk assessments', 'RiskAssessment', 'Create', 1, GETUTCDATE()),
    ('Risk.Update', 'Update risk assessments', 'RiskAssessment', 'Update', 1, GETUTCDATE()),
    ('Risk.Approve', 'Approve risk assessments', 'RiskAssessment', 'Approve', 1, GETUTCDATE()),
    
    -- Committee Management Permissions
    ('Committee.Read', 'View committee activities', 'Committee', 'Read', 1, GETUTCDATE()),
    ('Committee.Manage', 'Manage committee activities', 'Committee', 'Manage', 1, GETUTCDATE()),
    
    -- Administration Permissions
    ('Admin.Users', 'Manage users and roles', 'Administration', 'Manage', 1, GETUTCDATE()),
    ('Admin.System', 'System administration', 'Administration', 'Manage', 1, GETUTCDATE()),
    ('Admin.Reports', 'Generate system reports', 'Administration', 'Read', 1, GETUTCDATE())

-- Insert default roles (mapping to legacy roles)
INSERT INTO Roles (RoleName, Description, LegacyRoleMapping, IsActive, CreatedAt)
VALUES 
    ('Field User', 'Basic field operations and incident reporting', 'FieldUser', 1, GETUTCDATE()),
    ('Standard User', 'General system access for office staff', 'user', 1, GETUTCDATE()),
    ('Head of Department', 'Departmental oversight and approvals', 'Hod', 1, GETUTCDATE()),
    ('Field Supervisor', 'Field operations management and supervision', 'FieldSupervisor', 1, GETUTCDATE()),
    ('OSH Manager', 'Full OSH system management', NULL, 1, GETUTCDATE()),
    ('System Administrator', 'Complete system administration', NULL, 1, GETUTCDATE())

-- Assign permissions to roles
DECLARE @FieldUserId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Field User')
DECLARE @StandardUserId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Standard User')
DECLARE @HodId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Head of Department')
DECLARE @SupervisorId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Field Supervisor')
DECLARE @OSHManagerId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'OSH Manager')
DECLARE @AdminId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'System Administrator')

-- Field User Permissions (Basic)
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT @FieldUserId, PermissionId, GETUTCDATE()
FROM Permissions 
WHERE PermissionName IN ('OSH.Policy.Read', 'Incident.Read', 'Incident.Create', 'Risk.Read')

-- Standard User Permissions (Office Staff)
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT @StandardUserId, PermissionId, GETUTCDATE()
FROM Permissions 
WHERE PermissionName IN ('OSH.Policy.Read', 'Incident.Read', 'Incident.Create', 'Risk.Read', 'Risk.Create', 'Committee.Read')

-- Head of Department Permissions (Approval Authority)
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT @HodId, PermissionId, GETUTCDATE()
FROM Permissions 
WHERE PermissionName IN ('OSH.Policy.Read', 'OSH.Policy.Update', 'Incident.Read', 'Incident.Create', 'Incident.Update', 
                        'Incident.Approve', 'Risk.Read', 'Risk.Create', 'Risk.Update', 'Risk.Approve', 
                        'Committee.Read', 'Committee.Manage', 'Admin.Reports')

-- Field Supervisor Permissions (Field Management)
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT @SupervisorId, PermissionId, GETUTCDATE()
FROM Permissions 
WHERE PermissionName IN ('OSH.Policy.Read', 'Incident.Read', 'Incident.Create', 'Incident.Update', 'Incident.Investigate',
                        'Risk.Read', 'Risk.Create', 'Risk.Update', 'Committee.Read')

-- OSH Manager Permissions (Full OSH Access)
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT @OSHManagerId, PermissionId, GETUTCDATE()
FROM Permissions 
WHERE Module IN ('OSH', 'Incidents', 'RiskAssessment', 'Committee') OR PermissionName = 'Admin.Reports'

-- System Administrator Permissions (Everything)
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
SELECT @AdminId, PermissionId, GETUTCDATE()
FROM Permissions

PRINT 'Default roles and permissions seeded successfully'
