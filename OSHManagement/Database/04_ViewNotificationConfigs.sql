/*
==============================================================================
OSH Management System - View Notification System Configuration
Helper Scripts
==============================================================================
Description: Helpful queries to view and manage notification system
Author: OSH Management Development Team
Date: 2025-10-21
==============================================================================
*/

USE OSHManagement;
GO

PRINT '==============================================================================';
PRINT 'Notification System Configuration Dashboard';
PRINT '==============================================================================';
PRINT '';

-- =============================================================================
-- 1. VIEW ALL CHANNEL CONFIGURATIONS
-- =============================================================================
PRINT '--- CHANNEL CONFIGURATIONS ---';
SELECT 
    Channel,
    ConfigKey,
    CASE 
        WHEN IsEncrypted = 1 AND ConfigValue <> '' THEN '********' 
        WHEN ConfigValue = '' THEN '(NOT SET)'
        ELSE ConfigValue 
    END AS ConfigValue,
    IsEncrypted,
    IsRequired,
    IsActive,
    Description,
    CONVERT(VARCHAR(20), UpdatedAt, 120) AS LastUpdated
FROM NotificationChannelConfigs
ORDER BY 
    CASE Channel WHEN 'Email' THEN 1 WHEN 'SMS' THEN 2 WHEN 'WhatsApp' THEN 3 END,
    DisplayOrder;
PRINT '';

-- =============================================================================
-- 2. CHECK CHANNEL STATUS
-- =============================================================================
PRINT '--- CHANNEL STATUS ---';
SELECT 
    Channel,
    CASE 
        WHEN MAX(CASE WHEN ConfigKey = 'Enabled' THEN ConfigValue END) = 'true' THEN 'ENABLED ✓'
        ELSE 'DISABLED ✗'
    END AS Status,
    COUNT(*) AS TotalConfigs,
    SUM(CASE WHEN IsRequired = 1 THEN 1 ELSE 0 END) AS RequiredConfigs,
    SUM(CASE WHEN IsRequired = 1 AND (ConfigValue = '' OR ConfigValue IS NULL) THEN 1 ELSE 0 END) AS MissingRequired
FROM NotificationChannelConfigs
GROUP BY Channel;
PRINT '';

-- =============================================================================
-- 3. VIEW NOTIFICATION TEMPLATES
-- =============================================================================
PRINT '--- NOTIFICATION TEMPLATES ---';
SELECT 
    TemplateName,
    Category,
    Channel,
    CASE WHEN IsActive = 1 THEN 'Active ✓' ELSE 'Inactive ✗' END AS Status,
    LEFT(BodyTemplate, 50) + '...' AS BodyPreview,
    CONVERT(VARCHAR(20), CreatedAt, 120) AS Created
FROM NotificationTemplates
ORDER BY Category, TemplateName, Channel;
PRINT '';

-- =============================================================================
-- 4. TEMPLATES BY CATEGORY
-- =============================================================================
PRINT '--- TEMPLATES BY CATEGORY ---';
SELECT 
    Category,
    COUNT(*) AS TotalTemplates,
    SUM(CASE WHEN Channel = 'InApp' THEN 1 ELSE 0 END) AS InAppTemplates,
    SUM(CASE WHEN Channel = 'Email' THEN 1 ELSE 0 END) AS EmailTemplates,
    SUM(CASE WHEN Channel = 'SMS' THEN 1 ELSE 0 END) AS SmsTemplates,
    SUM(CASE WHEN Channel = 'WhatsApp' THEN 1 ELSE 0 END) AS WhatsAppTemplates
FROM NotificationTemplates
WHERE IsActive = 1
GROUP BY Category
ORDER BY Category;
PRINT '';

-- =============================================================================
-- 5. RECENT NOTIFICATIONS (if any exist)
-- =============================================================================
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Notifications' AND EXISTS (SELECT 1 FROM Notifications))
BEGIN
    PRINT '--- RECENT NOTIFICATIONS (Last 10) ---';
    SELECT TOP 10
        NotificationId,
        RecipientType,
        RecipientId,
        Title,
        Category,
        Priority,
        CASE WHEN IsRead = 1 THEN 'Read ✓' ELSE 'Unread' END AS Status,
        CONVERT(VARCHAR(20), CreatedAt, 120) AS Created
    FROM Notifications
    ORDER BY CreatedAt DESC;
    PRINT '';
    
    PRINT '--- NOTIFICATION STATISTICS ---';
    SELECT 
        COUNT(*) AS TotalNotifications,
        SUM(CASE WHEN IsRead = 0 THEN 1 ELSE 0 END) AS UnreadCount,
        SUM(CASE WHEN IsRead = 1 THEN 1 ELSE 0 END) AS ReadCount,
        COUNT(DISTINCT RecipientType + CAST(RecipientId AS NVARCHAR(10))) AS UniqueRecipients
    FROM Notifications;
    PRINT '';
END
ELSE
BEGIN
    PRINT '--- NO NOTIFICATIONS YET ---';
    PRINT 'Notifications will appear here once the system starts sending them.';
    PRINT '';
END

-- =============================================================================
-- 6. USER PREFERENCES (if any exist)
-- =============================================================================
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NotificationPreferences' AND EXISTS (SELECT 1 FROM NotificationPreferences))
BEGIN
    PRINT '--- USER NOTIFICATION PREFERENCES ---';
    SELECT 
        EmployeeId,
        Category,
        CASE WHEN InAppEnabled = 1 THEN '✓' ELSE '✗' END AS InApp,
        CASE WHEN EmailEnabled = 1 THEN '✓' ELSE '✗' END AS Email,
        CASE WHEN SmsEnabled = 1 THEN '✓' ELSE '✗' END AS SMS,
        CASE WHEN WhatsAppEnabled = 1 THEN '✓' ELSE '✗' END AS WhatsApp,
        MinPriority,
        DigestFrequency
    FROM NotificationPreferences
    ORDER BY EmployeeId, Category;
    PRINT '';
END
ELSE
BEGIN
    PRINT '--- NO USER PREFERENCES SET YET ---';
    PRINT 'Preferences will be created when users configure their notification settings.';
    PRINT '';
END

-- =============================================================================
-- 7. VALIDATION CHECKS
-- =============================================================================
PRINT '--- VALIDATION CHECKS ---';
PRINT '';

-- Check Email Configuration
DECLARE @EmailEnabled BIT = 0;
DECLARE @SmtpHost NVARCHAR(500);
DECLARE @SmtpPassword NVARCHAR(500);

SELECT @EmailEnabled = CASE WHEN ConfigValue = 'true' THEN 1 ELSE 0 END
FROM NotificationChannelConfigs 
WHERE Channel = 'Email' AND ConfigKey = 'Enabled';

SELECT @SmtpHost = ConfigValue
FROM NotificationChannelConfigs 
WHERE Channel = 'Email' AND ConfigKey = 'SmtpHost';

SELECT @SmtpPassword = ConfigValue
FROM NotificationChannelConfigs 
WHERE Channel = 'Email' AND ConfigKey = 'SmtpPassword';

IF @EmailEnabled = 1
BEGIN
    PRINT '✓ Email Channel: ENABLED';
    
    IF @SmtpHost IS NOT NULL AND @SmtpHost <> ''
        PRINT '  ✓ SMTP Host configured: ' + @SmtpHost;
    ELSE
        PRINT '  ✗ SMTP Host NOT configured';
    
    IF @SmtpPassword IS NOT NULL AND @SmtpPassword <> ''
        PRINT '  ✓ SMTP Password SET';
    ELSE
        PRINT '  ⚠️  SMTP Password NOT SET - Email sending will fail!';
END
ELSE
BEGIN
    PRINT '✗ Email Channel: DISABLED';
END

PRINT '';
PRINT '==============================================================================';
PRINT 'Configuration Review Complete';
PRINT '==============================================================================';
GO

-- =============================================================================
-- HELPFUL UPDATE QUERIES (Uncomment to use)
-- =============================================================================

-- Enable/Disable Email Channel
-- UPDATE NotificationChannelConfigs SET ConfigValue = 'true' WHERE Channel = 'Email' AND ConfigKey = 'Enabled';
-- UPDATE NotificationChannelConfigs SET ConfigValue = 'false' WHERE Channel = 'Email' AND ConfigKey = 'Enabled';

-- Update SMTP Host
-- UPDATE NotificationChannelConfigs SET ConfigValue = 'smtp.gmail.com', UpdatedAt = GETUTCDATE() WHERE Channel = 'Email' AND ConfigKey = 'SmtpHost';

-- Update SMTP Password (Remember to encrypt in real implementation!)
-- UPDATE NotificationChannelConfigs SET ConfigValue = 'your_password_here', UpdatedAt = GETUTCDATE() WHERE Channel = 'Email' AND ConfigKey = 'SmtpPassword';

-- Update From Email
-- UPDATE NotificationChannelConfigs SET ConfigValue = 'noreply@yourcompany.com', UpdatedAt = GETUTCDATE() WHERE Channel = 'Email' AND ConfigKey = 'FromEmail';

-- Deactivate a template
-- UPDATE NotificationTemplates SET IsActive = 0, UpdatedAt = GETUTCDATE() WHERE TemplateName = 'EmployeeCreated' AND Channel = 'Email';

-- Activate a template
-- UPDATE NotificationTemplates SET IsActive = 1, UpdatedAt = GETUTCDATE() WHERE TemplateName = 'EmployeeCreated' AND Channel = 'Email';
