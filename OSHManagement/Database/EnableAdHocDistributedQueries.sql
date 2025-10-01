-- Enable Ad Hoc Distributed Queries for OPENROWSET
-- Run this script on your SQL Server instance with administrator privileges

-- Show current configuration
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
GO

-- Enable Ad Hoc Distributed Queries
EXEC sp_configure 'Ad Hoc Distributed Queries', 1;
RECONFIGURE;
GO

-- Verify the setting
EXEC sp_configure 'Ad Hoc Distributed Queries';
GO

-- Show advanced options (optional - to hide advanced options again)
-- EXEC sp_configure 'show advanced options', 0;
-- RECONFIGURE;
-- GO
