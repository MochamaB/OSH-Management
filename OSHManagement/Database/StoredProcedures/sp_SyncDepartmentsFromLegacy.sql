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
            DepartmentCode NVARCHAR(50),
            DepartmentID NVARCHAR(50),
            DepartmentName NVARCHAR(100),
            DepartmentHD NVARCHAR(50),
            OrgCode INT
        )

        -- Build dynamic SQL to fetch from legacy database
        -- Note: Using DepartmentID as the code (100, 101, 102...)
        SET @SQL = '
        INSERT INTO #LegacyDepartments (DepartmentCode, DepartmentID, DepartmentName, DepartmentHD, OrgCode)
        SELECT
            CAST(DepartmentID AS NVARCHAR(50)) as DepartmentCode,
            CAST(DepartmentID AS NVARCHAR(50)) as DepartmentID,
            RTRIM(LTRIM(DepartmentName)) as DepartmentName,
            DepartmentHD,
            ISNULL(OrgCode, 1) as OrgCode
        FROM OPENROWSET(''MSOLEDBSQL'', ''' + @LegacyConnectionString + ''',
            ''SELECT departmentCode, DepartmentID, DepartmentName, DepartmentHD, OrgCode FROM Department'') d
        WHERE DepartmentID IS NOT NULL'

        EXEC sp_executesql @SQL
        
        -- Insert/Update departments in OSH database
        MERGE Departments AS target
        USING (
            SELECT
                ld.DepartmentCode,
                ld.DepartmentName,
                -- Map OrgCode to StationId
                -- OrgCode 1 = HEAD OFFICE (Station 55 in legacy, which maps to our synced station)
                COALESCE(
                    (SELECT TOP 1 StationId FROM Stations WHERE LegacyStationMapping = '55'),
                    1
                ) as StationId,
                ld.DepartmentHD as DepartmentHeadPayroll,
                ld.DepartmentCode as LegacyDepartmentMapping
            FROM #LegacyDepartments ld
        ) AS source ON target.LegacyDepartmentMapping = source.LegacyDepartmentMapping

        WHEN MATCHED THEN
            UPDATE SET
                DepartmentName = source.DepartmentName,
                DepartmentHeadPayroll = source.DepartmentHeadPayroll,
                UpdatedAt = GETUTCDATE()

        WHEN NOT MATCHED BY TARGET THEN
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
