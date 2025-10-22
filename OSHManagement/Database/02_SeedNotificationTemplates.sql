/*
==============================================================================
OSH Management System - Seed Notification Templates
Phase 1: Initial Templates for Employee and Team Events
==============================================================================
Description: Seeds initial notification templates for key events
Author: OSH Management Development Team
Date: 2025-10-21
==============================================================================
*/

USE OSHManagement;
GO

PRINT 'Seeding Notification Templates...';
PRINT '';

-- =============================================================================
-- EMPLOYEE MANAGEMENT TEMPLATES
-- =============================================================================

-- Employee Created - InApp
IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'EmployeeCreated' AND Channel = 'InApp')
BEGIN
    INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, Description, IsActive, CreatedAt)
    VALUES (
        'EmployeeCreated',
        'Employee',
        'InApp',
        'New Employee Added',
        '{EmployeeName} (Payroll: {PayrollNo}) has been added to {StationName}. Created on {CreatedDate}.',
        'Notification when a new employee is added to the system',
        1,
        GETUTCDATE()
    );
    PRINT '✓ EmployeeCreated (InApp) template created';
END

-- Employee Created - Email
IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'EmployeeCreated' AND Channel = 'Email')
BEGIN
    INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, Description, IsActive, CreatedAt)
    VALUES (
        'EmployeeCreated',
        'Employee',
        'Email',
        'New Employee Added - {EmployeeName}',
        '<p>Hello,</p>
<p>A new employee has been added to the OSH Management System:</p>
<ul>
<li><strong>Name:</strong> {EmployeeName}</li>
<li><strong>Payroll Number:</strong> {PayrollNo}</li>
<li><strong>Station:</strong> {StationName}</li>
<li><strong>Date Added:</strong> {CreatedDate}</li>
</ul>
<p>You can view the full employee profile <a href="{ActionUrl}">here</a>.</p>
<p>Best regards,<br/>OSH Management System</p>',
        'Email notification when a new employee is added',
        1,
        GETUTCDATE()
    );
    PRINT '✓ EmployeeCreated (Email) template created';
END

-- Employee Updated - InApp
IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'EmployeeUpdated' AND Channel = 'InApp')
BEGIN
    INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, Description, IsActive, CreatedAt)
    VALUES (
        'EmployeeUpdated',
        'Employee',
        'InApp',
        'Employee Details Updated',
        'Employee {EmployeeName} (Payroll: {PayrollNo}) has been updated. Changes: {ChangesSummary}',
        'Notification when employee details are modified',
        1,
        GETUTCDATE()
    );
    PRINT '✓ EmployeeUpdated (InApp) template created';
END

-- =============================================================================
-- TEAM MANAGEMENT TEMPLATES
-- =============================================================================

-- Team Created - InApp
IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'TeamCreated' AND Channel = 'InApp')
BEGIN
    INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, Description, IsActive, CreatedAt)
    VALUES (
        'TeamCreated',
        'Team',
        'InApp',
        'New Team Formed',
        'Team "{TeamName}" has been created at {StationName}. Type: {TeamType}. Formation Date: {FormationDate}',
        'Notification when a new team is formed',
        1,
        GETUTCDATE()
    );
    PRINT '✓ TeamCreated (InApp) template created';
END

-- Team Created - Email
IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'TeamCreated' AND Channel = 'Email')
BEGIN
    INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, Description, IsActive, CreatedAt)
    VALUES (
        'TeamCreated',
        'Team',
        'Email',
        'New Team Formed - {TeamName}',
        '<p>Hello,</p>
<p>A new team has been formed:</p>
<ul>
<li><strong>Team Name:</strong> {TeamName}</li>
<li><strong>Team Type:</strong> {TeamType}</li>
<li><strong>Station:</strong> {StationName}</li>
<li><strong>Formation Date:</strong> {FormationDate}</li>
</ul>
<p>View team details <a href="{ActionUrl}">here</a>.</p>
<p>Best regards,<br/>OSH Management System</p>',
        'Email notification when a new team is formed',
        1,
        GETUTCDATE()
    );
    PRINT '✓ TeamCreated (Email) template created';
END

-- Team Member Added - InApp
IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'TeamMemberAdded' AND Channel = 'InApp')
BEGIN
    INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, Description, IsActive, CreatedAt)
    VALUES (
        'TeamMemberAdded',
        'Team',
        'InApp',
        'New Team Member',
        '{EmployeeName} has been added to team "{TeamName}" as {MemberRole}. Appointment Date: {AppointmentDate}',
        'Notification when a member is added to a team',
        1,
        GETUTCDATE()
    );
    PRINT '✓ TeamMemberAdded (InApp) template created';
END

-- Team Member Added - Email
IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'TeamMemberAdded' AND Channel = 'Email')
BEGIN
    INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, Description, IsActive, CreatedAt)
    VALUES (
        'TeamMemberAdded',
        'Team',
        'Email',
        'Team Member Added - {TeamName}',
        '<p>Hello,</p>
<p>A new member has been added to your team:</p>
<ul>
<li><strong>Team:</strong> {TeamName}</li>
<li><strong>New Member:</strong> {EmployeeName}</li>
<li><strong>Role:</strong> {MemberRole}</li>
<li><strong>Appointment Date:</strong> {AppointmentDate}</li>
</ul>
<p>View team details <a href="{ActionUrl}">here</a>.</p>
<p>Best regards,<br/>OSH Management System</p>',
        'Email notification when a member is added to a team',
        1,
        GETUTCDATE()
    );
    PRINT '✓ TeamMemberAdded (Email) template created';
END

-- Team Member Removed - InApp
IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'TeamMemberRemoved' AND Channel = 'InApp')
BEGIN
    INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, Description, IsActive, CreatedAt)
    VALUES (
        'TeamMemberRemoved',
        'Team',
        'InApp',
        'Team Member Removed',
        '{EmployeeName} has been removed from team "{TeamName}". Departure Date: {DepartureDate}',
        'Notification when a member is removed from a team',
        1,
        GETUTCDATE()
    );
    PRINT '✓ TeamMemberRemoved (InApp) template created';
END

-- =============================================================================
-- INCIDENT MANAGEMENT TEMPLATES (Preview for Phase 2)
-- =============================================================================

-- Incident Reported - InApp
IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'IncidentReported' AND Channel = 'InApp')
BEGIN
    INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, Description, IsActive, CreatedAt)
    VALUES (
        'IncidentReported',
        'Incident',
        'InApp',
        '⚠️ Incident Reported',
        'Incident Type: {IncidentType} at {Location}. Severity: {Severity}. Reported by: {ReportedBy}',
        'Notification when a new incident is reported',
        1,
        GETUTCDATE()
    );
    PRINT '✓ IncidentReported (InApp) template created';
END

-- Incident Reported - Email
IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'IncidentReported' AND Channel = 'Email')
BEGIN
    INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, Description, IsActive, CreatedAt)
    VALUES (
        'IncidentReported',
        'Incident',
        'Email',
        '⚠️ URGENT: Incident Reported - {IncidentType}',
        '<p><strong>⚠️ INCIDENT ALERT</strong></p>
<p>A new incident has been reported and requires immediate attention:</p>
<ul>
<li><strong>Incident Type:</strong> {IncidentType}</li>
<li><strong>Location:</strong> {Location}</li>
<li><strong>Severity:</strong> {Severity}</li>
<li><strong>Reported By:</strong> {ReportedBy}</li>
<li><strong>Date/Time:</strong> {ReportedDate}</li>
</ul>
<p><a href="{ActionUrl}" style="background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">View Incident Details</a></p>
<p>Best regards,<br/>OSH Management System</p>',
        'Email notification for urgent incident reports',
        1,
        GETUTCDATE()
    );
    PRINT '✓ IncidentReported (Email) template created';
END

-- =============================================================================
-- SYSTEM NOTIFICATIONS
-- =============================================================================

-- Welcome Message - InApp
IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'WelcomeMessage' AND Channel = 'InApp')
BEGIN
    INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, Description, IsActive, CreatedAt)
    VALUES (
        'WelcomeMessage',
        'System',
        'InApp',
        'Welcome to OSH Management System',
        'Welcome {EmployeeName}! Your account has been created. You can now access the OSH Management System.',
        'Welcome notification for new users',
        1,
        GETUTCDATE()
    );
    PRINT '✓ WelcomeMessage (InApp) template created';
END

-- =============================================================================
-- Summary
-- =============================================================================
DECLARE @TemplateCount INT;
SELECT @TemplateCount = COUNT(*) FROM NotificationTemplates WHERE IsActive = 1;

PRINT '';
PRINT '==============================================================================';
PRINT 'Notification Templates Seeded Successfully';
PRINT '==============================================================================';
PRINT 'Total Active Templates: ' + CAST(@TemplateCount AS NVARCHAR(10));
PRINT '';
PRINT 'Templates by Category:';
SELECT 
    Category,
    COUNT(*) AS TemplateCount,
    STRING_AGG(Channel, ', ') AS Channels
FROM NotificationTemplates
WHERE IsActive = 1
GROUP BY Category
ORDER BY Category;
PRINT '';
PRINT 'Next Step: Run 03_SeedNotificationChannelConfigs.sql';
PRINT '==============================================================================';
GO
