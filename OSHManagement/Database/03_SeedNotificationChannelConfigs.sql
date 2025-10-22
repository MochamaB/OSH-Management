/*
==============================================================================
OSH Management System - Seed Notification Channel Configurations
Phase 1: Email Configuration (SMS & WhatsApp for Phase 2)
==============================================================================
Description: Seeds initial channel configurations for Email, SMS, WhatsApp
             These can be edited via Admin UI instead of appsettings.json
Author: OSH Management Development Team
Date: 2025-10-21
==============================================================================
*/

USE OSHManagement;
GO

PRINT 'Seeding Notification Channel Configurations...';
PRINT '';

-- =============================================================================
-- EMAIL CHANNEL CONFIGURATION
-- =============================================================================
PRINT 'Configuring Email Channel...';

-- Email Provider
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'Email' AND ConfigKey = 'Provider')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('Email', 'Provider', 'SMTP', 0, 'Email provider type (SMTP, SendGrid, Mailgun)', 1, 1, 1);
    PRINT '  ✓ Email Provider configured';
END

-- SMTP Host
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'Email' AND ConfigKey = 'SmtpHost')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('Email', 'SmtpHost', 'smtp.gmail.com', 0, 'SMTP server hostname', 1, 2, 1);
    PRINT '  ✓ SMTP Host configured';
END

-- SMTP Port
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'Email' AND ConfigKey = 'SmtpPort')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('Email', 'SmtpPort', '587', 0, 'SMTP server port (587 for TLS, 465 for SSL)', 1, 3, 1);
    PRINT '  ✓ SMTP Port configured';
END

-- Enable SSL
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'Email' AND ConfigKey = 'EnableSsl')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('Email', 'EnableSsl', 'true', 0, 'Enable SSL/TLS encryption (true/false)', 1, 4, 1);
    PRINT '  ✓ SSL/TLS configured';
END

-- SMTP Username
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'Email' AND ConfigKey = 'SmtpUsername')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('Email', 'SmtpUsername', 'noreply@oshmanagement.com', 0, 'SMTP authentication username (usually email address)', 1, 5, 1);
    PRINT '  ✓ SMTP Username configured';
END

-- SMTP Password (Encrypted)
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'Email' AND ConfigKey = 'SmtpPassword')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('Email', 'SmtpPassword', '', 1, 'SMTP authentication password (encrypted)', 1, 6, 1);
    PRINT '  ⚠️  SMTP Password - NEEDS TO BE SET via Admin UI';
END

-- From Email
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'Email' AND ConfigKey = 'FromEmail')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('Email', 'FromEmail', 'noreply@oshmanagement.com', 0, 'Email address shown in From field', 1, 7, 1);
    PRINT '  ✓ From Email configured';
END

-- From Name
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'Email' AND ConfigKey = 'FromName')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('Email', 'FromName', 'OSH Management System', 0, 'Display name shown in From field', 1, 8, 1);
    PRINT '  ✓ From Name configured';
END

-- Reply To Email
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'Email' AND ConfigKey = 'ReplyToEmail')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('Email', 'ReplyToEmail', 'support@oshmanagement.com', 0, 'Email address for replies (optional)', 0, 9, 1);
    PRINT '  ✓ Reply-To Email configured';
END

-- Email Enabled
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'Email' AND ConfigKey = 'Enabled')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('Email', 'Enabled', 'true', 0, 'Enable/disable email notifications globally', 1, 0, 1);
    PRINT '  ✓ Email Channel Enabled';
END

-- =============================================================================
-- SMS CHANNEL CONFIGURATION (For Phase 2)
-- =============================================================================
PRINT '';
PRINT 'Configuring SMS Channel (Phase 2)...';

-- SMS Provider
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'SMS' AND ConfigKey = 'Provider')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('SMS', 'Provider', 'Twilio', 0, 'SMS provider (Twilio, AfricasTalking, LocalGateway)', 1, 1, 0);
    PRINT '  ✓ SMS Provider configured (Disabled)';
END

-- Twilio Account SID
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'SMS' AND ConfigKey = 'AccountSid')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('SMS', 'AccountSid', '', 0, 'Twilio Account SID', 1, 2, 0);
    PRINT '  ✓ SMS Account SID configured';
END

-- Twilio Auth Token
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'SMS' AND ConfigKey = 'AuthToken')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('SMS', 'AuthToken', '', 1, 'Twilio Auth Token (encrypted)', 1, 3, 0);
    PRINT '  ✓ SMS Auth Token configured';
END

-- SMS From Number
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'SMS' AND ConfigKey = 'FromNumber')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('SMS', 'FromNumber', '+1234567890', 0, 'Phone number to send SMS from', 1, 4, 0);
    PRINT '  ✓ SMS From Number configured';
END

-- SMS Max Length
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'SMS' AND ConfigKey = 'MaxLength')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('SMS', 'MaxLength', '160', 0, 'Maximum SMS message length', 0, 5, 0);
    PRINT '  ✓ SMS Max Length configured';
END

-- SMS Enabled
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'SMS' AND ConfigKey = 'Enabled')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('SMS', 'Enabled', 'false', 0, 'Enable/disable SMS notifications globally', 1, 0, 0);
    PRINT '  ✓ SMS Channel Disabled (Phase 2)';
END

-- =============================================================================
-- WHATSAPP CHANNEL CONFIGURATION (For Phase 2)
-- =============================================================================
PRINT '';
PRINT 'Configuring WhatsApp Channel (Phase 2)...';

-- WhatsApp Provider
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'WhatsApp' AND ConfigKey = 'Provider')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('WhatsApp', 'Provider', 'TwilioWhatsApp', 0, 'WhatsApp provider (TwilioWhatsApp, WhatsAppBusinessAPI)', 1, 1, 0);
    PRINT '  ✓ WhatsApp Provider configured (Disabled)';
END

-- WhatsApp Account SID
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'WhatsApp' AND ConfigKey = 'AccountSid')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('WhatsApp', 'AccountSid', '', 0, 'Twilio WhatsApp Account SID', 1, 2, 0);
    PRINT '  ✓ WhatsApp Account SID configured';
END

-- WhatsApp Auth Token
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'WhatsApp' AND ConfigKey = 'AuthToken')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('WhatsApp', 'AuthToken', '', 1, 'Twilio WhatsApp Auth Token (encrypted)', 1, 3, 0);
    PRINT '  ✓ WhatsApp Auth Token configured';
END

-- WhatsApp From Number
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'WhatsApp' AND ConfigKey = 'FromNumber')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('WhatsApp', 'FromNumber', 'whatsapp:+1234567890', 0, 'WhatsApp number to send from (format: whatsapp:+1234567890)', 1, 4, 0);
    PRINT '  ✓ WhatsApp From Number configured';
END

-- WhatsApp Enabled
IF NOT EXISTS (SELECT 1 FROM NotificationChannelConfigs WHERE Channel = 'WhatsApp' AND ConfigKey = 'Enabled')
BEGIN
    INSERT INTO NotificationChannelConfigs (Channel, ConfigKey, ConfigValue, IsEncrypted, Description, IsRequired, DisplayOrder, IsActive)
    VALUES ('WhatsApp', 'Enabled', 'false', 0, 'Enable/disable WhatsApp notifications globally', 1, 0, 0);
    PRINT '  ✓ WhatsApp Channel Disabled (Phase 2)';
END

-- =============================================================================
-- Summary
-- =============================================================================
PRINT '';
PRINT '==============================================================================';
PRINT 'Notification Channel Configurations Seeded Successfully';
PRINT '==============================================================================';
PRINT '';
PRINT 'Channel Status:';
SELECT 
    Channel,
    SUM(CASE WHEN ConfigKey = 'Enabled' AND ConfigValue = 'true' THEN 1 ELSE 0 END) AS Enabled,
    COUNT(*) AS TotalConfigs,
    SUM(CASE WHEN IsRequired = 1 THEN 1 ELSE 0 END) AS RequiredConfigs,
    SUM(CASE WHEN IsEncrypted = 1 THEN 1 ELSE 0 END) AS EncryptedConfigs
FROM NotificationChannelConfigs
GROUP BY Channel
ORDER BY 
    CASE Channel 
        WHEN 'Email' THEN 1 
        WHEN 'SMS' THEN 2 
        WHEN 'WhatsApp' THEN 3 
    END;

PRINT '';
PRINT '⚠️  IMPORTANT NEXT STEPS:';
PRINT '  1. Update Email password via Admin UI (NotificationChannelConfigs table)';
PRINT '  2. Test Email configuration by sending a test notification';
PRINT '  3. SMS and WhatsApp are disabled (Phase 2 - enable when ready)';
PRINT '';
PRINT '✓ Phase 1 Database Setup Complete!';
PRINT '  You can now start implementing the notification services.';
PRINT '==============================================================================';
GO
