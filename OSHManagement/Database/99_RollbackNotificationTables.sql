/*
==============================================================================
OSH Management System - Rollback Notification System
CAUTION: This script DELETES all notification tables and data
==============================================================================
Description: Removes all notification system tables (use carefully!)
Author: OSH Management Development Team
Date: 2025-10-21
==============================================================================
*/

USE OSHManagement;
GO

PRINT '==============================================================================';
PRINT '⚠️  WARNING: NOTIFICATION SYSTEM ROLLBACK';
PRINT '==============================================================================';
PRINT 'This script will DELETE the following tables and ALL their data:';
PRINT '  - NotificationDeliveries';
PRINT '  - Notifications';
PRINT '  - NotificationPreferences';
PRINT '  - NotificationTemplates';
PRINT '  - NotificationChannelConfigs';
PRINT '';
PRINT 'This action CANNOT be undone!';
PRINT '==============================================================================';
PRINT '';

-- Uncomment the line below ONLY if you are ABSOLUTELY SURE you want to delete everything
-- DECLARE @ConfirmDelete BIT = 1;

-- Safety check
IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'Notifications')
BEGIN
    PRINT '✓ Notification tables do not exist. Nothing to rollback.';
    RETURN;
END

-- Uncomment to enable deletion
/*
IF @ConfirmDelete = 1
BEGIN
    PRINT 'Starting rollback...';
    PRINT '';

    -- Drop tables in reverse order (respecting foreign keys)
    
    -- 1. NotificationDeliveries (depends on Notifications)
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationDeliveries')
    BEGIN
        DROP TABLE NotificationDeliveries;
        PRINT '✓ NotificationDeliveries table dropped';
    END

    -- 2. NotificationPreferences (depends on Employees via FK)
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationPreferences')
    BEGIN
        DROP TABLE NotificationPreferences;
        PRINT '✓ NotificationPreferences table dropped';
    END

    -- 3. Notifications (no dependencies)
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
    BEGIN
        DROP TABLE Notifications;
        PRINT '✓ Notifications table dropped';
    END

    -- 4. NotificationTemplates (no dependencies)
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationTemplates')
    BEGIN
        DROP TABLE NotificationTemplates;
        PRINT '✓ NotificationTemplates table dropped';
    END

    -- 5. NotificationChannelConfigs (no dependencies)
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationChannelConfigs')
    BEGIN
        DROP TABLE NotificationChannelConfigs;
        PRINT '✓ NotificationChannelConfigs table dropped';
    END

    PRINT '';
    PRINT '==============================================================================';
    PRINT '✓ Rollback completed successfully';
    PRINT '==============================================================================';
    PRINT 'All notification tables have been removed.';
    PRINT '';
    PRINT 'To recreate the system, run:';
    PRINT '  1. 01_CreateNotificationTables.sql';
    PRINT '  2. 02_SeedNotificationTemplates.sql';
    PRINT '  3. 03_SeedNotificationChannelConfigs.sql';
    PRINT '==============================================================================';
END
ELSE
BEGIN
    PRINT '❌ Rollback NOT executed.';
    PRINT '';
    PRINT 'To execute this rollback:';
    PRINT '  1. Backup your database first!';
    PRINT '  2. Uncomment the line: DECLARE @ConfirmDelete BIT = 1;';
    PRINT '  3. Run this script again.';
    PRINT '';
END
*/

-- Default message if not uncommented
PRINT '❌ Rollback script is DISABLED for safety.';
PRINT '';
PRINT 'To enable:';
PRINT '  1. BACKUP YOUR DATABASE FIRST!';
PRINT '  2. Open this file in SSMS';
PRINT '  3. Uncomment the /* */ block starting at line 36';
PRINT '  4. Uncomment: DECLARE @ConfirmDelete BIT = 1;';
PRINT '  5. Run the script';
PRINT '';
PRINT '⚠️  This will permanently delete all notification data!';
GO
