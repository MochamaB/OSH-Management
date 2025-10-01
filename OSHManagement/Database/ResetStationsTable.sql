-- =============================================
-- Script to reset Stations table
-- WARNING: This will delete all station data!
-- =============================================

-- Disable foreign key constraints temporarily
ALTER TABLE Employees NOCHECK CONSTRAINT ALL;
ALTER TABLE Departments NOCHECK CONSTRAINT ALL;

-- Delete all stations
DELETE FROM Stations;

-- Reset identity seed to start from 1
DBCC CHECKIDENT ('Stations', RESEED, 0);

-- Re-enable foreign key constraints
ALTER TABLE Employees CHECK CONSTRAINT ALL;
ALTER TABLE Departments CHECK CONSTRAINT ALL;

-- Verify the table is empty
SELECT * FROM Stations;

PRINT 'Stations table has been reset. Identity seed is now 0.';
PRINT 'Next insert will start from StationId = 1.';
