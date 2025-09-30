-- =============================================
-- Author: OSH Management System
-- Create date: 2024-09-29
-- Description: Synchronizes departments from legacy KTDALeave database
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_SyncDepartmentsFromLegacy]
    @LegacyConnectionString NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SQL NVARCHAR(MAX)
    DECLARE @ErrorMessage NVARCHAR(4000)
    DECLARE @ErrorSeverity INT
    DECLARE @ErrorState INT
    
    BEGIN TRY
        -- Create temporary table for legacy department data
        IF OBJECT_ID('tempdb..#LegacyDepartments') IS NOT NULL
            DROP TABLE #LegacyDepartments
            
        CREATE TABLE #LegacyDepartments (
            DepartmentID NVARCHAR(50),
            DepartmentName NVARCHAR(100),
            DepartmentHD NVARCHAR(50),
            StationMapping NVARCHAR(50)
        )
        
        -- Build dynamic SQL to fetch from legacy database
        SET @SQL = '
        INSERT INTO #LegacyDepartments (DepartmentID, DepartmentName, DepartmentHD, StationMapping)
        SELECT DISTINCT 
            d.DepartmentID,
            d.DepartmentName,
            d.DepartmentHD,
            e.Station
        FROM OPENROWSET(''SQLNCLI'', ''' + @LegacyConnectionString + ''', 
            ''SELECT DepartmentID, DepartmentName, DepartmentHD FROM Department'') d
        LEFT JOIN OPENROWSET(''SQLNCLI'', ''' + @LegacyConnectionString + ''', 
            ''SELECT DISTINCT Department, Station FROM Employee_bkp WHERE EmpisCurrActive = 1'') e 
            ON d.DepartmentID = e.Department
        WHERE d.DepartmentID IS NOT NULL'
        
        EXEC sp_executesql @SQL
        
        -- Insert/Update departments in OSH database
        MERGE Departments AS target
        USING (
            SELECT 
                ld.DepartmentID as DepartmentCode,
                ld.DepartmentName,
                COALESCE(s.StationId, 1) as StationId, -- Default to station 1 if not found
                ld.DepartmentHD as DepartmentHeadPayroll,
                ld.DepartmentID as LegacyDepartmentMapping
            FROM #LegacyDepartments ld
            LEFT JOIN Stations s ON s.LegacyStationMapping = ld.StationMapping
        ) AS source ON target.LegacyDepartmentMapping = source.DepartmentCode
        
        WHEN MATCHED THEN
            UPDATE SET 
                DepartmentName = source.DepartmentName,
                DepartmentHeadPayroll = source.DepartmentHeadPayroll,
                UpdatedAt = GETUTCDATE()
                
        WHEN NOT MATCHED THEN
            INSERT (DepartmentCode, DepartmentName, StationId, DepartmentHeadPayroll, LegacyDepartmentMapping, IsActive, CreatedAt)
            VALUES (source.DepartmentCode, source.DepartmentName, source.StationId, source.DepartmentHeadPayroll, source.LegacyDepartmentMapping, 1, GETUTCDATE());
        
        -- Log results
        DECLARE @InsertedCount INT = @@ROWCOUNT
        PRINT 'Department sync completed. Rows affected: ' + CAST(@InsertedCount AS VARCHAR(10))
        
        DROP TABLE #LegacyDepartments
        
    END TRY
    BEGIN CATCH
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE()
            
        PRINT 'Error in sp_SyncDepartmentsFromLegacy: ' + @ErrorMessage
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
