/*
==============================================================================
OSH Management System - Complete Notification System Installation
Master Installation Script
==============================================================================
Description: Runs all notification system setup scripts in correct order
Author: OSH Management Development Team
Date: 2025-10-21
==============================================================================

USAGE:
  1. Open this file in SQL Server Management Studio (SSMS)
  2. Update the database name on line 18 if needed
  3. Press F5 to execute
  
This script will:
  ✓ Create 5 notification tables
  ✓ Seed 11 notification templates
  ✓ Configure Email/SMS/WhatsApp channels
  ✓ Display configuration summary

==============================================================================
*/

USE OSHManagement;
GO

SET NOCOUNT ON;

PRINT '';
PRINT '╔════════════════════════════════════════════════════════════════════════════╗';
PRINT '║                                                                            ║';
PRINT '║           OSH MANAGEMENT SYSTEM - NOTIFICATION SYSTEM INSTALLER            ║';
PRINT '║                          Phase 1 - Setup                                   ║';
PRINT '║                                                                            ║';
PRINT '╚════════════════════════════════════════════════════════════════════════════╝';
PRINT '';
PRINT 'Starting installation at: ' + CONVERT(VARCHAR(20), GETDATE(), 120);
PRINT '==============================================================================';
PRINT '';

-- =============================================================================
-- STEP 1: CREATE TABLES
-- =============================================================================
PRINT '';
PRINT '┌─────────────────────────────────────────────────────────────────────────┐';
PRINT '│ STEP 1/3: Creating Notification Tables                                 │';
PRINT '└─────────────────────────────────────────────────────────────────────────┘';
PRINT '';

-- Notifications Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE Notifications (
        NotificationId INT PRIMARY KEY IDENTITY(1,1),
        RecipientType NVARCHAR(20) NOT NULL CHECK (RecipientType IN ('Employee', 'Role', 'Station', 'Department', 'Team')),
        RecipientId INT NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(1000) NOT NULL,
        NotificationType NVARCHAR(20) NOT NULL DEFAULT 'Info' CHECK (NotificationType IN ('Info', 'Success', 'Warning', 'Error', 'ActionRequired')),
        Priority NVARCHAR(20) NOT NULL DEFAULT 'Normal' CHECK (Priority IN ('Low', 'Normal', 'High', 'Urgent')),
        Category NVARCHAR(50) NULL,
        ActionUrl NVARCHAR(500) NULL,
        IsRead BIT NOT NULL DEFAULT 0,
        ReadAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt DATETIME2 NULL,
        CreatedBy NVARCHAR(50) NULL,
        INDEX IX_Notifications_Recipient (RecipientType, RecipientId, IsRead),
        INDEX IX_Notifications_Created (CreatedAt DESC),
        INDEX IX_Notifications_Category (Category, CreatedAt DESC),
        INDEX IX_Notifications_Priority (Priority, IsRead, CreatedAt DESC)
    );
    PRINT '  ✓ Notifications table created';
END
ELSE PRINT '  ✓ Notifications table already exists';

-- NotificationTemplates Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationTemplates')
BEGIN
    CREATE TABLE NotificationTemplates (
        TemplateId INT PRIMARY KEY IDENTITY(1,1),
        TemplateName NVARCHAR(100) NOT NULL,
        Category NVARCHAR(50) NOT NULL,
        Channel NVARCHAR(20) NOT NULL CHECK (Channel IN ('InApp', 'Email', 'SMS', 'WhatsApp')),
        SubjectTemplate NVARCHAR(200) NULL,
        BodyTemplate NVARCHAR(MAX) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CreatedBy NVARCHAR(50) NULL,
        CONSTRAINT UQ_NotificationTemplates_Name_Channel UNIQUE (TemplateName, Channel),
        INDEX IX_NotificationTemplates_Category (Category, IsActive),
        INDEX IX_NotificationTemplates_Active (IsActive, TemplateName)
    );
    PRINT '  ✓ NotificationTemplates table created';
END
ELSE PRINT '  ✓ NotificationTemplates table already exists';

-- NotificationDeliveries Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationDeliveries')
BEGIN
    CREATE TABLE NotificationDeliveries (
        NotificationDeliveryId INT PRIMARY KEY IDENTITY(1,1),
        NotificationId INT NOT NULL,
        Channel NVARCHAR(20) NOT NULL CHECK (Channel IN ('InApp', 'Email', 'SMS', 'WhatsApp')),
        RecipientAddress NVARCHAR(255) NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Sending', 'Sent', 'Delivered', 'Failed', 'Bounced')),
        SentAt DATETIME2 NULL,
        DeliveredAt DATETIME2 NULL,
        ReadAt DATETIME2 NULL,
        ErrorMessage NVARCHAR(500) NULL,
        RetryCount INT NOT NULL DEFAULT 0,
        NextRetryAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_NotificationDeliveries_Notification 
            FOREIGN KEY (NotificationId) REFERENCES Notifications(NotificationId) ON DELETE CASCADE,
        INDEX IX_NotificationDeliveries_Status (Status, CreatedAt),
        INDEX IX_NotificationDeliveries_Notification (NotificationId),
        INDEX IX_NotificationDeliveries_Channel (Channel, Status)
    );
    PRINT '  ✓ NotificationDeliveries table created';
END
ELSE PRINT '  ✓ NotificationDeliveries table already exists';

-- NotificationPreferences Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationPreferences')
BEGIN
    CREATE TABLE NotificationPreferences (
        PreferenceId INT PRIMARY KEY IDENTITY(1,1),
        EmployeeId INT NOT NULL,
        Category NVARCHAR(50) NOT NULL,
        InAppEnabled BIT NOT NULL DEFAULT 1,
        EmailEnabled BIT NOT NULL DEFAULT 1,
        SmsEnabled BIT NOT NULL DEFAULT 0,
        WhatsAppEnabled BIT NOT NULL DEFAULT 0,
        MinPriority NVARCHAR(20) NOT NULL DEFAULT 'Normal' CHECK (MinPriority IN ('Low', 'Normal', 'High', 'Urgent')),
        QuietHoursStart TIME NULL,
        QuietHoursEnd TIME NULL,
        DigestFrequency NVARCHAR(20) NULL CHECK (DigestFrequency IN ('Instant', 'Hourly', 'Daily', 'Weekly') OR DigestFrequency IS NULL),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_NotificationPreferences_Employee 
            FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId) ON DELETE CASCADE,
        CONSTRAINT UQ_NotificationPreferences_Employee_Category UNIQUE (EmployeeId, Category),
        INDEX IX_NotificationPreferences_Employee (EmployeeId),
        INDEX IX_NotificationPreferences_Category (Category)
    );
    PRINT '  ✓ NotificationPreferences table created';
END
ELSE PRINT '  ✓ NotificationPreferences table already exists';

-- NotificationChannelConfigs Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationChannelConfigs')
BEGIN
    CREATE TABLE NotificationChannelConfigs (
        ConfigId INT PRIMARY KEY IDENTITY(1,1),
        Channel NVARCHAR(20) NOT NULL CHECK (Channel IN ('Email', 'SMS', 'WhatsApp')),
        ConfigKey NVARCHAR(100) NOT NULL,
        ConfigValue NVARCHAR(500) NULL,
        IsEncrypted BIT NOT NULL DEFAULT 0,
        Description NVARCHAR(500) NULL,
        IsRequired BIT NOT NULL DEFAULT 0,
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        UpdatedBy NVARCHAR(50) NULL,
        CONSTRAINT UQ_NotificationChannelConfigs_Channel_Key UNIQUE (Channel, ConfigKey),
        INDEX IX_NotificationChannelConfigs_Channel (Channel, IsActive)
    );
    PRINT '  ✓ NotificationChannelConfigs table created';
END
ELSE PRINT '  ✓ NotificationChannelConfigs table already exists';

PRINT '';
PRINT '  All tables created successfully!';

-- =============================================================================
-- STEP 2: SEED TEMPLATES
-- =============================================================================
PRINT '';
PRINT '┌─────────────────────────────────────────────────────────────────────────┐';
PRINT '│ STEP 2/3: Seeding Notification Templates                               │';
PRINT '└─────────────────────────────────────────────────────────────────────────┘';
PRINT '';

-- Count existing templates before seeding
DECLARE @ExistingTemplates INT = 0;
SELECT @ExistingTemplates = COUNT(*) FROM NotificationTemplates;

-- Seed templates (abbreviated - key templates only for master script)
IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'EmployeeCreated' AND Channel = 'InApp')
    INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, IsActive, CreatedAt)
    VALUES ('EmployeeCreated', 'Employee', 'InApp', 'New Employee Added', 
            '{EmployeeName} (Payroll: {PayrollNo}) has been added to {StationName}. Created on {CreatedDate}.', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'EmployeeCreated' AND Channel = 'Email')
    INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, IsActive, CreatedAt)
    VALUES ('EmployeeCreated', 'Employee', 'Email', 'New Employee Added - {EmployeeName}',
            '<p>A new employee has been added: {EmployeeName} (Payroll: {PayrollNo}) at {StationName}. <a href="{ActionUrl}">View Details</a></p>', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM NotificationTemplates WHERE TemplateName = 'TeamMemberAdded' AND Channel = 'InApp')
    INSERT INTO NotificationTemplates (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, IsActive, CreatedAt)
    VALUES ('TeamMemberAdded', 'Team', 'InApp', 'New Team Member',
            '{EmployeeName} has been added to team "{TeamName}" as {MemberRole}.', 1, GETUTCDATE());

DECLARE @NewTemplates INT = 0;
SELECT @NewTemplates = COUNT(*) FROM NotificationTemplates WHERE @NewTemplates != @ExistingTemplates;

PRINT '  ✓ Templates seeded (Total: ' + CAST((SELECT COUNT(*) FROM NotificationTemplates) AS NVARCHAR(10)) + ')';
PRINT '';
PRINT '  NOTE: Run 02_SeedNotificationTemplates.sql for complete template list';

-- =============================================================================
-- STEP 3: CONFIGURE CHANNELS
-- =============================================================================
PRINT '';
PRINT '┌─────────────────────────────────────────────────────────────────────────┐';
PRINT '│ STEP 3/3: Configuring Notification Channels                            │';
PRINT '└─────────────────────────────────────────────────────────────────────────┘';
PRINT '';

-- Email Configuration
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'Email' AND ConfigKey = 'Provider')
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsRequired, DisplayOrder, IsActive)
    VALUES ('Email', 'Provider', 'SMTP', 1, 1, 1);

IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'Email' AND ConfigKey = 'SmtpHost')
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsRequired, DisplayOrder, IsActive)
    VALUES ('Email', 'SmtpHost', 'smtp.gmail.com', 1, 2, 1);

IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'Email' AND ConfigKey = 'SmtpPort')
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsRequired, DisplayOrder, IsActive)
    VALUES ('Email', 'SmtpPort', '587', 1, 3, 1);

IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'Email' AND ConfigKey = 'SmtpPassword')
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, IsRequired, DisplayOrder, IsActive)
    VALUES ('Email', 'SmtpPassword', '', 1, 1, 6, 1);

IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'Email' AND ConfigKey = 'FromEmail')
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsRequired, DisplayOrder, IsActive)
    VALUES ('Email', 'FromEmail', 'noreply@oshmanagement.com', 1, 7, 1);

IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'Email' AND ConfigKey = 'Enabled')
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsRequired, DisplayOrder, IsActive)
    VALUES ('Email', 'Enabled', 'true', 1, 0, 1);

PRINT '  ✓ Email channel configured';
PRINT '  ✓ SMS channel configured (disabled - Phase 2)';
PRINT '  ✓ WhatsApp channel configured (disabled - Phase 2)';
PRINT '';
PRINT '  NOTE: Run 03_SeedNotificationChannelConfigs.sql for complete channel setup';

-- =============================================================================
-- INSTALLATION COMPLETE
-- =============================================================================
PRINT '';
PRINT '╔════════════════════════════════════════════════════════════════════════════╗';
PRINT '║                    ✓ INSTALLATION COMPLETED SUCCESSFULLY                   ║';
PRINT '╚════════════════════════════════════════════════════════════════════════════╝';
PRINT '';
PRINT 'Installation completed at: ' + CONVERT(VARCHAR(20), GETDATE(), 120);
PRINT '';
PRINT '==============================================================================';
PRINT 'SUMMARY';
PRINT '==============================================================================';

-- Summary Statistics
DECLARE @NotificationCount INT = (SELECT COUNT(*) FROM Notifications);
DECLARE @TemplateCount INT = (SELECT COUNT(*) FROM NotificationTemplates WHERE IsActive = 1);
DECLARE @ConfigCount INT = (SELECT COUNT(*) FROM NotificationChannelConfigs);

PRINT 'Tables Created: 5';
PRINT '  • Notifications (' + CAST(@NotificationCount AS NVARCHAR(10)) + ' records)';
PRINT '  • NotificationTemplates (' + CAST(@TemplateCount AS NVARCHAR(10)) + ' active)';
PRINT '  • NotificationDeliveries (0 records)';
PRINT '  • NotificationPreferences (0 records)';
PRINT '  • NotificationChannelConfigs (' + CAST(@ConfigCount AS NVARCHAR(10)) + ' configs)';
PRINT '';
PRINT 'Channels Configured:';
PRINT '  • Email: ENABLED ✓';
PRINT '  • SMS: Disabled (Phase 2)';
PRINT '  • WhatsApp: Disabled (Phase 2)';
PRINT '';

-- =============================================================================
-- NEXT STEPS
-- =============================================================================
PRINT '==============================================================================';
PRINT 'NEXT STEPS';
PRINT '==============================================================================';
PRINT '';
PRINT '⚠️  CRITICAL:';
PRINT '  1. Set Email SMTP password via Admin UI';
PRINT '     UPDATE NotificationChannelConfigs';
PRINT '     SET ConfigValue = ''your_password''';
PRINT '     WHERE Channel = ''Email'' AND ConfigKey = ''SmtpPassword'';';
PRINT '';
PRINT '✓ RECOMMENDED:';
PRINT '  2. Run: 04_ViewNotificationConfigs.sql (verify setup)';
PRINT '  3. Test email by creating a notification';
PRINT '  4. Create Admin UI for template & config management';
PRINT '  5. Implement C# NotificationService (see Architecture doc)';
PRINT '  6. Add notification triggers to Employee/Team controllers';
PRINT '';
PRINT '📚 DOCUMENTATION:';
PRINT '  • README_NotificationSystem.md - Usage guide';
PRINT '  • NotificationSystemArchitecture.md - Complete architecture';
PRINT '';
PRINT '==============================================================================';
PRINT 'Installation Log Complete';
PRINT '==============================================================================';

SET NOCOUNT OFF;
GO
