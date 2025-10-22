/*
==============================================================================
OSH Management System - Notification System Tables
Phase 1: Core Notification Infrastructure
==============================================================================
Description: Creates all tables needed for the notification system
Author: OSH Management Development Team
Date: 2025-10-21
==============================================================================
*/

USE OSHManagement;
GO

-- =============================================================================
-- 1. NOTIFICATIONS TABLE
-- Stores all in-app notifications using type-discriminator pattern
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE Notifications (
        NotificationId INT PRIMARY KEY IDENTITY(1,1),
        
        -- Recipient Information (Type-Discriminator Pattern)
        RecipientType NVARCHAR(20) NOT NULL CHECK (RecipientType IN ('Employee', 'Role', 'Station', 'Department', 'Team')),
        RecipientId INT NOT NULL, -- Single ID column that works for all recipient types
        
        -- Notification Content
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(1000) NOT NULL,
        NotificationType NVARCHAR(20) NOT NULL DEFAULT 'Info' CHECK (NotificationType IN ('Info', 'Success', 'Warning', 'Error', 'ActionRequired')),
        Priority NVARCHAR(20) NOT NULL DEFAULT 'Normal' CHECK (Priority IN ('Low', 'Normal', 'High', 'Urgent')),
        Category NVARCHAR(50) NULL, -- 'Employee', 'Team', 'Incident', 'Training', 'Safety', etc.
        
        -- Action & Navigation
        ActionUrl NVARCHAR(500) NULL, -- Optional link to related entity
        
        -- Read Tracking
        IsRead BIT NOT NULL DEFAULT 0,
        ReadAt DATETIME2 NULL,
        
        -- Timestamps & Expiry
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt DATETIME2 NULL, -- Optional auto-delete date
        CreatedBy NVARCHAR(50) NULL,
        
        -- Indexes for performance
        INDEX IX_Notifications_Recipient (RecipientType, RecipientId, IsRead),
        INDEX IX_Notifications_Created (CreatedAt DESC),
        INDEX IX_Notifications_Category (Category, CreatedAt DESC),
        INDEX IX_Notifications_Priority (Priority, IsRead, CreatedAt DESC)
    );
    
    PRINT '✓ Notifications table created successfully';
END
ELSE
BEGIN
    PRINT '✓ Notifications table already exists';
END
GO

-- =============================================================================
-- 2. NOTIFICATION TEMPLATES TABLE
-- Stores templates for different notification events and channels
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationTemplates')
BEGIN
    CREATE TABLE NotificationTemplates (
        TemplateId INT PRIMARY KEY IDENTITY(1,1),
        
        -- Template Identification
        TemplateName NVARCHAR(100) NOT NULL, -- 'EmployeeCreated', 'TeamMemberAdded', etc.
        Category NVARCHAR(50) NOT NULL, -- 'Employee', 'Team', 'Incident', etc.
        Channel NVARCHAR(20) NOT NULL CHECK (Channel IN ('InApp', 'Email', 'SMS', 'WhatsApp')),
        
        -- Template Content
        SubjectTemplate NVARCHAR(200) NULL, -- For email/WhatsApp (optional)
        BodyTemplate NVARCHAR(MAX) NOT NULL, -- Supports placeholders: {EmployeeName}, {StationName}, etc.
        
        -- Metadata
        Description NVARCHAR(500) NULL, -- Admin notes about template usage
        IsActive BIT NOT NULL DEFAULT 1,
        
        -- Timestamps
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CreatedBy NVARCHAR(50) NULL,
        
        -- Unique constraint: One template per event+channel combination
        CONSTRAINT UQ_NotificationTemplates_Name_Channel UNIQUE (TemplateName, Channel),
        
        -- Indexes
        INDEX IX_NotificationTemplates_Category (Category, IsActive),
        INDEX IX_NotificationTemplates_Active (IsActive, TemplateName)
    );
    
    PRINT '✓ NotificationTemplates table created successfully';
END
ELSE
BEGIN
    PRINT '✓ NotificationTemplates table already exists';
END
GO

-- =============================================================================
-- 3. NOTIFICATION DELIVERIES TABLE
-- Tracks multi-channel delivery status (Email, SMS, WhatsApp)
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationDeliveries')
BEGIN
    CREATE TABLE NotificationDeliveries (
        NotificationDeliveryId INT PRIMARY KEY IDENTITY(1,1),
        
        -- Foreign Key to Notification
        NotificationId INT NOT NULL,
        
        -- Channel Information
        Channel NVARCHAR(20) NOT NULL CHECK (Channel IN ('InApp', 'Email', 'SMS', 'WhatsApp')),
        RecipientAddress NVARCHAR(255) NULL, -- Email address, phone number, WhatsApp number
        
        -- Delivery Status
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Sending', 'Sent', 'Delivered', 'Failed', 'Bounced')),
        
        -- Timestamps
        SentAt DATETIME2 NULL,
        DeliveredAt DATETIME2 NULL,
        ReadAt DATETIME2 NULL,
        
        -- Error Handling
        ErrorMessage NVARCHAR(500) NULL,
        RetryCount INT NOT NULL DEFAULT 0,
        NextRetryAt DATETIME2 NULL,
        
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        -- Foreign Key Constraint
        CONSTRAINT FK_NotificationDeliveries_Notification 
            FOREIGN KEY (NotificationId) 
            REFERENCES Notifications(NotificationId) 
            ON DELETE CASCADE,
        
        -- Indexes for performance
        INDEX IX_NotificationDeliveries_Status (Status, CreatedAt),
        INDEX IX_NotificationDeliveries_Notification (NotificationId),
        INDEX IX_NotificationDeliveries_Channel (Channel, Status),
        INDEX IX_NotificationDeliveries_Retry (Status, NextRetryAt) 
            WHERE Status = 'Failed' AND NextRetryAt IS NOT NULL
    );
    
    PRINT '✓ NotificationDeliveries table created successfully';
END
ELSE
BEGIN
    PRINT '✓ NotificationDeliveries table already exists';
END
GO

-- =============================================================================
-- 4. NOTIFICATION PREFERENCES TABLE
-- User-specific notification preferences per category
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationPreferences')
BEGIN
    CREATE TABLE NotificationPreferences (
        PreferenceId INT PRIMARY KEY IDENTITY(1,1),
        
        -- User Reference
        EmployeeId INT NOT NULL,
        
        -- Category-specific preferences
        Category NVARCHAR(50) NOT NULL, -- 'Employee', 'Team', 'Incident', 'All', etc.
        
        -- Channel Toggles
        InAppEnabled BIT NOT NULL DEFAULT 1,
        EmailEnabled BIT NOT NULL DEFAULT 1,
        SmsEnabled BIT NOT NULL DEFAULT 0,
        WhatsAppEnabled BIT NOT NULL DEFAULT 0,
        
        -- Priority Filter
        MinPriority NVARCHAR(20) NOT NULL DEFAULT 'Normal' CHECK (MinPriority IN ('Low', 'Normal', 'High', 'Urgent')),
        
        -- Quiet Hours (for Email/SMS/WhatsApp only, InApp always works)
        QuietHoursStart TIME NULL, -- e.g., 22:00
        QuietHoursEnd TIME NULL, -- e.g., 07:00
        
        -- Digest Settings
        DigestFrequency NVARCHAR(20) NULL CHECK (DigestFrequency IN ('Instant', 'Hourly', 'Daily', 'Weekly') OR DigestFrequency IS NULL),
        
        -- Timestamps
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        
        -- Foreign Key Constraint
        CONSTRAINT FK_NotificationPreferences_Employee 
            FOREIGN KEY (EmployeeId) 
            REFERENCES Employees(EmployeeId) 
            ON DELETE CASCADE,
        
        -- Unique constraint: One preference per employee+category
        CONSTRAINT UQ_NotificationPreferences_Employee_Category UNIQUE (EmployeeId, Category),
        
        -- Indexes
        INDEX IX_NotificationPreferences_Employee (EmployeeId),
        INDEX IX_NotificationPreferences_Category (Category)
    );
    
    PRINT '✓ NotificationPreferences table created successfully';
END
ELSE
BEGIN
    PRINT '✓ NotificationPreferences table already exists';
END
GO

-- =============================================================================
-- 5. NOTIFICATION CHANNEL CONFIGS TABLE (NEW!)
-- Dynamic configuration for Email, SMS, WhatsApp - Editable via Admin UI
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationChannelConfigs')
BEGIN
    CREATE TABLE NotificationChannelConfigs (
        ConfigId INT PRIMARY KEY IDENTITY(1,1),
        
        -- Channel Identification
        Channel NVARCHAR(20) NOT NULL CHECK (Channel IN ('Email', 'SMS', 'WhatsApp')),
        ConfigKey NVARCHAR(100) NOT NULL, -- 'SmtpHost', 'SmtpPort', 'ApiKey', 'FromEmail', etc.
        
        -- Configuration Value
        ConfigValue NVARCHAR(500) NULL, -- The actual value (e.g., 'smtp.gmail.com', '587', etc.)
        IsEncrypted BIT NOT NULL DEFAULT 0, -- For passwords, API keys (should be encrypted)
        
        -- Metadata
        Description NVARCHAR(500) NULL, -- Help text for admins
        IsRequired BIT NOT NULL DEFAULT 0, -- Is this config required for channel to work?
        DisplayOrder INT NOT NULL DEFAULT 0, -- Order in admin UI
        
        -- Status
        IsActive BIT NOT NULL DEFAULT 1,
        
        -- Timestamps
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        UpdatedBy NVARCHAR(50) NULL,
        
        -- Unique constraint: One value per channel+key
        CONSTRAINT UQ_NotificationChannelConfigs_Channel_Key UNIQUE (Channel, ConfigKey),
        
        -- Indexes
        INDEX IX_NotificationChannelConfigs_Channel (Channel, IsActive),
        INDEX IX_NotificationChannelConfigs_Required (IsRequired, IsActive)
    );
    
    PRINT '✓ NotificationChannelConfigs table created successfully';
END
ELSE
BEGIN
    PRINT '✓ NotificationChannelConfigs table already exists';
END
GO

-- =============================================================================
-- Summary
-- =============================================================================
PRINT '';
PRINT '==============================================================================';
PRINT 'Notification System Tables Created Successfully';
PRINT '==============================================================================';
PRINT 'Tables:';
PRINT '  1. Notifications - Stores all in-app notifications';
PRINT '  2. NotificationTemplates - Event templates for each channel';
PRINT '  3. NotificationDeliveries - Multi-channel delivery tracking';
PRINT '  4. NotificationPreferences - User notification preferences';
PRINT '  5. NotificationChannelConfigs - Dynamic channel configuration (NEW!)';
PRINT '';
PRINT 'Next Steps:';
PRINT '  1. Run: 02_SeedNotificationTemplates.sql (Initial templates)';
PRINT '  2. Run: 03_SeedNotificationChannelConfigs.sql (Email/SMS config)';
PRINT '==============================================================================';
GO
