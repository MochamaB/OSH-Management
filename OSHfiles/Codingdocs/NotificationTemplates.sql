-- =====================================================
-- Notification Templates for Testing
-- OSH Management System
-- =====================================================

-- Check if NotificationTemplates table exists
IF OBJECT_ID('NotificationTemplates', 'U') IS NOT NULL
BEGIN
    PRINT 'NotificationTemplates table found. Inserting templates...'
    
    -- =====================================================
    -- EMPLOYEE TEMPLATES
    -- =====================================================
    
    -- Employee Created
    IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'EmployeeCreated' AND Channel = 'InApp')
    BEGIN
        INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, IsActive, Description, CreatedAt)
        VALUES (
            'EmployeeCreated',
            'Employee',
            'InApp',
            'New Employee Added',
            '{EmployeeName} (Payroll: {PayrollNo}) has been added to {StationName} as {Designation}.',
            1,
            'Notification sent when a new employee is created in the system',
            GETUTCDATE()
        );
        PRINT '✅ Template created: EmployeeCreated (InApp)'
    END
    ELSE
        PRINT '⚠️ Template already exists: EmployeeCreated (InApp)'
    
    -- Employee Updated
    IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'EmployeeUpdated' AND Channel = 'InApp')
    BEGIN
        INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, IsActive, Description, CreatedAt)
        VALUES (
            'EmployeeUpdated',
            'Employee',
            'InApp',
            'Employee Profile Updated',
            'Your employee profile has been updated. Fields changed: {ChangedFields}. Updated by {UpdatedBy} on {UpdatedDate}.',
            1,
            'Notification sent when employee details are modified',
            GETUTCDATE()
        );
        PRINT '✅ Template created: EmployeeUpdated (InApp)'
    END
    ELSE
        PRINT '⚠️ Template already exists: EmployeeUpdated (InApp)'
    
    -- Employee Deactivated
    IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'EmployeeDeactivated' AND Channel = 'InApp')
    BEGIN
        INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, IsActive, Description, CreatedAt)
        VALUES (
            'EmployeeDeactivated',
            'Employee',
            'InApp',
            'Employee Deactivated',
            '{EmployeeName} (Payroll: {PayrollNo}) from {StationName} has been deactivated. Reason: {Reason}',
            1,
            'Notification sent when an employee is deactivated',
            GETUTCDATE()
        );
        PRINT '✅ Template created: EmployeeDeactivated (InApp)'
    END
    ELSE
        PRINT '⚠️ Template already exists: EmployeeDeactivated (InApp)'
    
    -- Employee Transferred
    IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'EmployeeTransferred' AND Channel = 'InApp')
    BEGIN
        INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, IsActive, Description, CreatedAt)
        VALUES (
            'EmployeeTransferred',
            'Employee',
            'InApp',
            'Employee Transferred',
            '{EmployeeName} (Payroll: {PayrollNo}) has been transferred from {OldStation} to {NewStation} effective {TransferDate}.',
            1,
            'Notification sent when an employee is transferred to a new station',
            GETUTCDATE()
        );
        PRINT '✅ Template created: EmployeeTransferred (InApp)'
    END
    ELSE
        PRINT '⚠️ Template already exists: EmployeeTransferred (InApp)'
    
    -- Role Assigned
    IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'RoleAssigned' AND Channel = 'InApp')
    BEGIN
        INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, IsActive, Description, CreatedAt)
        VALUES (
            'RoleAssigned',
            'Employee',
            'InApp',
            'New Role Assigned',
            'You have been assigned the role of {RoleName}. Assigned by {AssignedBy} on {AssignedDate}.',
            1,
            'Notification sent when a new role is assigned to an employee',
            GETUTCDATE()
        );
        PRINT '✅ Template created: RoleAssigned (InApp)'
    END
    ELSE
        PRINT '⚠️ Template already exists: RoleAssigned (InApp)'
    
    -- Employee Promoted
    IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'EmployeePromoted' AND Channel = 'InApp')
    BEGIN
        INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, IsActive, Description, CreatedAt)
        VALUES (
            'EmployeePromoted',
            'Employee',
            'InApp',
            'Congratulations on Your Promotion!',
            'Congratulations {EmployeeName}! You have been promoted from {OldDesignation} to {NewDesignation} effective {PromotionDate}.',
            1,
            'Notification sent when an employee is promoted',
            GETUTCDATE()
        );
        PRINT '✅ Template created: EmployeePromoted (InApp)'
    END
    ELSE
        PRINT '⚠️ Template already exists: EmployeePromoted (InApp)'
    
    -- =====================================================
    -- TEAM TEMPLATES
    -- =====================================================
    
    -- Team Created
    IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'TeamCreated' AND Channel = 'InApp')
    BEGIN
        INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, IsActive, Description, CreatedAt)
        VALUES (
            'TeamCreated',
            'Team',
            'InApp',
            'New Team Created',
            'A new {TeamType} team "{TeamName}" has been created at {StationName} on {CreatedDate}.',
            1,
            'Notification sent when a new team is created',
            GETUTCDATE()
        );
        PRINT '✅ Template created: TeamCreated (InApp)'
    END
    ELSE
        PRINT '⚠️ Template already exists: TeamCreated (InApp)'
    
    -- Team Member Added
    IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'TeamMemberAdded' AND Channel = 'InApp')
    BEGIN
        INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, IsActive, Description, CreatedAt)
        VALUES (
            'TeamMemberAdded',
            'Team',
            'InApp',
            'New Team Member Added',
            '{EmployeeName} (Payroll: {PayrollNo}) has been added to {TeamName} as {MemberRole} on {AppointmentDate}.',
            1,
            'Notification sent when a new member is added to a team',
            GETUTCDATE()
        );
        PRINT '✅ Template created: TeamMemberAdded (InApp)'
    END
    ELSE
        PRINT '⚠️ Template already exists: TeamMemberAdded (InApp)'
    
    -- Team Member Removed
    IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'TeamMemberRemoved' AND Channel = 'InApp')
    BEGIN
        INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, IsActive, Description, CreatedAt)
        VALUES (
            'TeamMemberRemoved',
            'Team',
            'InApp',
            'Team Member Removed',
            '{EmployeeName} (Payroll: {PayrollNo}) has been removed from {TeamName}. Reason: {Reason}',
            1,
            'Notification sent when a member is removed from a team',
            GETUTCDATE()
        );
        PRINT '✅ Template created: TeamMemberRemoved (InApp)'
    END
    ELSE
        PRINT '⚠️ Template already exists: TeamMemberRemoved (InApp)'
    
    -- Team Role Changed
    IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'TeamRoleChanged' AND Channel = 'InApp')
    BEGIN
        INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, IsActive, Description, CreatedAt)
        VALUES (
            'TeamRoleChanged',
            'Team',
            'InApp',
            'Team Role Changed',
            '{EmployeeName} role in {TeamName} has been changed from {OldRole} to {NewRole} on {ChangeDate}.',
            1,
            'Notification sent when a team member''s role changes',
            GETUTCDATE()
        );
        PRINT '✅ Template created: TeamRoleChanged (InApp)'
    END
    ELSE
        PRINT '⚠️ Template already exists: TeamRoleChanged (InApp)'
    
    -- Team Activated
    IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'TeamActivated' AND Channel = 'InApp')
    BEGIN
        INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, IsActive, Description, CreatedAt)
        VALUES (
            'TeamActivated',
            'Team',
            'InApp',
            'Team Activated',
            'The {TeamType} team "{TeamName}" at {StationName} has been activated on {ActivatedDate}.',
            1,
            'Notification sent when a team is activated',
            GETUTCDATE()
        );
        PRINT '✅ Template created: TeamActivated (InApp)'
    END
    ELSE
        PRINT '⚠️ Template already exists: TeamActivated (InApp)'
    
    -- Team Deactivated
    IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'TeamDeactivated' AND Channel = 'InApp')
    BEGIN
        INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, IsActive, Description, CreatedAt)
        VALUES (
            'TeamDeactivated',
            'Team',
            'InApp',
            'Team Deactivated',
            'The {TeamType} team "{TeamName}" at {StationName} has been deactivated. Reason: {Reason}',
            1,
            'Notification sent when a team is deactivated',
            GETUTCDATE()
        );
        PRINT '✅ Template created: TeamDeactivated (InApp)'
    END
    ELSE
        PRINT '⚠️ Template already exists: TeamDeactivated (InApp)'
    
    -- Team Updated
    IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'TeamUpdated' AND Channel = 'InApp')
    BEGIN
        INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, IsActive, Description, CreatedAt)
        VALUES (
            'TeamUpdated',
            'Team',
            'InApp',
            'Team Details Updated',
            'Team "{TeamName}" has been updated. Fields changed: {ChangedFields}. Updated on {UpdatedDate}.',
            1,
            'Notification sent when team details are updated',
            GETUTCDATE()
        );
        PRINT '✅ Template created: TeamUpdated (InApp)'
    END
    ELSE
        PRINT '⚠️ Template already exists: TeamUpdated (InApp)'
    
    PRINT ''
    PRINT '================================================'
    PRINT 'Template setup complete!'
    PRINT '================================================'
    
    -- Show summary
    SELECT 
        Category,
        COUNT(*) AS TemplateCount
    FROM NotificationTemplates
    WHERE IsActive = 1
    GROUP BY Category
    ORDER BY Category;
    
END
ELSE
BEGIN
    PRINT '❌ ERROR: NotificationTemplates table does not exist!'
    PRINT 'Please run the notification system migration first.'
END
GO
