-- =============================================================================
-- Performance Optimization - Add Missing Indexes
-- =============================================================================
-- This script adds indexes to improve query performance and reduce timeouts
-- =============================================================================

USE OSH_Management; -- Change to your database name if different
GO

PRINT 'Adding performance indexes...';
PRINT '';

-- =============================================================================
-- EMPLOYEES TABLE INDEXES
-- =============================================================================
PRINT 'Adding indexes on Employees table...';

-- Index for StationId (frequently joined)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_StationId' AND object_id = OBJECT_ID(N'Employees'))
BEGIN
    CREATE INDEX IX_Employees_StationId ON Employees(StationId);
    PRINT '  ✓ IX_Employees_StationId created';
END
ELSE
    PRINT '  ⚠ IX_Employees_StationId already exists';

-- Index for DepartmentId (frequently joined)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_DepartmentId' AND object_id = OBJECT_ID(N'Employees'))
BEGIN
    CREATE INDEX IX_Employees_DepartmentId ON Employees(DepartmentId);
    PRINT '  ✓ IX_Employees_DepartmentId created';
END
ELSE
    PRINT '  ⚠ IX_Employees_DepartmentId already exists';

-- Covering index for common employee queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_Station_Department_Cover' AND object_id = OBJECT_ID(N'Employees'))
BEGIN
    CREATE INDEX IX_Employees_Station_Department_Cover
    ON Employees(StationId, DepartmentId)
    INCLUDE (PayrollNo, FirstName, LastName, Designation);
    PRINT '  ✓ IX_Employees_Station_Department_Cover created';
END
ELSE
    PRINT '  ⚠ IX_Employees_Station_Department_Cover already exists';

GO

-- =============================================================================
-- SECTIONS TABLE INDEXES
-- =============================================================================
PRINT '';
PRINT 'Adding indexes on Sections table...';

-- Index for StationId (frequently joined)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Sections_StationId' AND object_id = OBJECT_ID(N'Sections'))
BEGIN
    CREATE INDEX IX_Sections_StationId ON Sections(StationId);
    PRINT '  ✓ IX_Sections_StationId created';
END
ELSE
    PRINT '  ⚠ IX_Sections_StationId already exists';

-- Index for SectionName (frequently ordered by)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Sections_SectionName' AND object_id = OBJECT_ID(N'Sections'))
BEGIN
    CREATE INDEX IX_Sections_SectionName ON Sections(SectionName);
    PRINT '  ✓ IX_Sections_SectionName created';
END
ELSE
    PRINT '  ⚠ IX_Sections_SectionName already exists';

GO

-- =============================================================================
-- DEPARTMENTS TABLE INDEXES
-- =============================================================================
PRINT '';
PRINT 'Adding indexes on Departments table...';

-- Index for StationId (frequently joined)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Departments_StationId' AND object_id = OBJECT_ID(N'Departments'))
BEGIN
    CREATE INDEX IX_Departments_StationId ON Departments(StationId);
    PRINT '  ✓ IX_Departments_StationId created';
END
ELSE
    PRINT '  ⚠ IX_Departments_StationId already exists';

-- Index for DepartmentName (frequently ordered by)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Departments_DepartmentName' AND object_id = OBJECT_ID(N'Departments'))
BEGIN
    CREATE INDEX IX_Departments_DepartmentName ON Departments(DepartmentName);
    PRINT '  ✓ IX_Departments_DepartmentName created';
END
ELSE
    PRINT '  ⚠ IX_Departments_DepartmentName already exists';

GO

-- =============================================================================
-- TEAMS TABLE INDEXES
-- =============================================================================
PRINT '';
PRINT 'Adding indexes on Teams table...';

-- Index for StationId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Teams_StationId' AND object_id = OBJECT_ID(N'Teams'))
BEGIN
    CREATE INDEX IX_Teams_StationId ON Teams(StationId);
    PRINT '  ✓ IX_Teams_StationId created';
END
ELSE
    PRINT '  ⚠ IX_Teams_StationId already exists';

-- Index for TeamType
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Teams_TeamType' AND object_id = OBJECT_ID(N'Teams'))
BEGIN
    CREATE INDEX IX_Teams_TeamType ON Teams(TeamType);
    PRINT '  ✓ IX_Teams_TeamType created';
END
ELSE
    PRINT '  ⚠ IX_Teams_TeamType already exists';

-- Index for TeamStatus
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Teams_TeamStatus' AND object_id = OBJECT_ID(N'Teams'))
BEGIN
    CREATE INDEX IX_Teams_TeamStatus ON Teams(TeamStatus);
    PRINT '  ✓ IX_Teams_TeamStatus created';
END
ELSE
    PRINT '  ⚠ IX_Teams_TeamStatus already exists';

GO

-- =============================================================================
-- TEAMMEMBERS TABLE INDEXES
-- =============================================================================
PRINT '';
PRINT 'Adding indexes on TeamMembers table...';

-- Index for TeamId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TeamMembers_TeamId' AND object_id = OBJECT_ID(N'TeamMembers'))
BEGIN
    CREATE INDEX IX_TeamMembers_TeamId ON TeamMembers(TeamId);
    PRINT '  ✓ IX_TeamMembers_TeamId created';
END
ELSE
    PRINT '  ⚠ IX_TeamMembers_TeamId already exists';

-- Index for IsActive (frequently filtered)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TeamMembers_IsActive' AND object_id = OBJECT_ID(N'TeamMembers'))
BEGIN
    CREATE INDEX IX_TeamMembers_IsActive ON TeamMembers(IsActive);
    PRINT '  ✓ IX_TeamMembers_IsActive created';
END
ELSE
    PRINT '  ⚠ IX_TeamMembers_IsActive already exists';

-- Composite index for common queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TeamMembers_Team_Active' AND object_id = OBJECT_ID(N'TeamMembers'))
BEGIN
    CREATE INDEX IX_TeamMembers_Team_Active ON TeamMembers(TeamId, IsActive);
    PRINT '  ✓ IX_TeamMembers_Team_Active created';
END
ELSE
    PRINT '  ⚠ IX_TeamMembers_Team_Active already exists';

GO

-- =============================================================================
-- UPDATE STATISTICS
-- =============================================================================
PRINT '';
PRINT 'Updating statistics...';

UPDATE STATISTICS Employees WITH FULLSCAN;
PRINT '  ✓ Employees statistics updated';

UPDATE STATISTICS Sections WITH FULLSCAN;
PRINT '  ✓ Sections statistics updated';

UPDATE STATISTICS Departments WITH FULLSCAN;
PRINT '  ✓ Departments statistics updated';

UPDATE STATISTICS Teams WITH FULLSCAN;
PRINT '  ✓ Teams statistics updated';

UPDATE STATISTICS TeamMembers WITH FULLSCAN;
PRINT '  ✓ TeamMembers statistics updated';

UPDATE STATISTICS TeamRoleDefinitions WITH FULLSCAN;
PRINT '  ✓ TeamRoleDefinitions statistics updated';

GO

-- =============================================================================
-- VERIFICATION
-- =============================================================================
PRINT '';
PRINT '=============================================================================';
PRINT 'INDEX VERIFICATION';
PRINT '=============================================================================';

PRINT 'Employees Indexes:';
SELECT
    i.name AS IndexName,
    i.type_desc AS IndexType,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.object_id = OBJECT_ID('Employees')
AND i.name IS NOT NULL
ORDER BY i.name, ic.key_ordinal;

PRINT '';
PRINT 'Performance optimization complete!';
PRINT '';
GO
