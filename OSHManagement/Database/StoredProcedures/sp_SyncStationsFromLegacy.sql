-- =============================================
-- Author: OSH Management System
-- Create date: 2024-09-29
-- Description: Synchronizes stations from legacy KTDALeave database
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_SyncStationsFromLegacy]
    @LegacyConnectionString NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SQL NVARCHAR(MAX)
    DECLARE @ErrorMessage NVARCHAR(4000)
    DECLARE @ErrorSeverity INT
    DECLARE @ErrorState INT
    
    BEGIN TRY
        -- Create temporary table for legacy station data
        IF OBJECT_ID('tempdb..#LegacyStations') IS NOT NULL
            DROP TABLE #LegacyStations
            
        CREATE TABLE #LegacyStations (
            Station_Name NVARCHAR(50),
            StationCode NVARCHAR(20),
            EmployeeCount INT
        )
        
        -- Build dynamic SQL to fetch from legacy database using MSOLEDBSQL
        SET @SQL = '
        INSERT INTO #LegacyStations (Station_Name, StationCode, EmployeeCount)
        SELECT
            s.Station_Name,
            CAST(s.StationID AS NVARCHAR(20)) as StationCode,
            COUNT(DISTINCT e.Station) as EmployeeCount
        FROM OPENROWSET(''MSOLEDBSQL'', ''' + @LegacyConnectionString + ''',
            ''SELECT StationID, Station_Name FROM Station'') s
        LEFT JOIN OPENROWSET(''MSOLEDBSQL'', ''' + @LegacyConnectionString + ''',
            ''SELECT Station FROM Employee_bkp WHERE EmpisCurrActive = 1 AND Station IS NOT NULL'') e
            ON e.Station = s.Station_Name
            OR e.Station = CAST(s.StationID AS VARCHAR(10))
            OR e.Station = UPPER(s.Station_Name)
        WHERE s.StationID IS NOT NULL
        GROUP BY s.StationID, s.Station_Name'

        EXEC sp_executesql @SQL
        
        -- Insert/Update stations in OSH database
        MERGE Stations AS target
        USING (
            SELECT
                StationCode,
                Station_Name as StationName,
                CASE
                    WHEN UPPER(RTRIM(LTRIM(Station_Name))) LIKE 'HEAD OFFICE%' THEN 2 -- Head Office category
                    WHEN UPPER(RTRIM(LTRIM(Station_Name))) LIKE 'ZONAL OFFICE%' THEN 3 -- Regional Office category (if exists)
                    WHEN UPPER(RTRIM(LTRIM(Station_Name))) LIKE 'REGION%' THEN 3 -- Regional Office category (if exists)
                    WHEN UPPER(RTRIM(LTRIM(Station_Name))) IN ('KTDA_POWER','GREENLAND_FEDHA','KETEPA','TEA MACHINERY & ENGINEERING SERVICES','CHAI LOGISTICS CENTER') THEN 4 -- Subsidiary category
                    WHEN UPPER(RTRIM(LTRIM(Station_Name))) IN ('KTDA_HOLDINGS','KTDA MS','EXTERNAL','KIGALI RWANDA OFFICE','SHANGASHA','MULINDI','KIGALI') THEN 5 -- Other category
                    ELSE 1 -- Factory category
                END as OrgCategoryId,
                StationCode as LegacyStationMapping
            FROM #LegacyStations
        ) AS source ON target.LegacyStationMapping = source.StationCode
            OR (target.StationCode = 'HQ' AND source.StationCode = '55') -- Map seeded HQ to Station 55

        WHEN MATCHED THEN
            UPDATE SET
                StationName = source.StationName,
                OrgCategoryId = source.OrgCategoryId,
                LegacyStationMapping = source.LegacyStationMapping,
                UpdatedAt = GETUTCDATE()

        WHEN NOT MATCHED BY TARGET THEN
            INSERT (StationCode, StationName, OrgCategoryId, LegacyStationMapping, IsActive, CreatedAt)
            VALUES (source.StationCode, source.StationName, source.OrgCategoryId, source.LegacyStationMapping, 1, GETUTCDATE());
        
        -- Log results
        DECLARE @InsertedCount INT = @@ROWCOUNT
        PRINT 'Station sync completed. Rows affected: ' + CAST(@InsertedCount AS VARCHAR(10))
        
        DROP TABLE #LegacyStations
        
    END TRY
    BEGIN CATCH
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE()
            
        PRINT 'Error in sp_SyncStationsFromLegacy: ' + @ErrorMessage
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
