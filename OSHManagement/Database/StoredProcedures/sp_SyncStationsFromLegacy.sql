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
        
        -- Build dynamic SQL to fetch from legacy database
        SET @SQL = '
        INSERT INTO #LegacyStations (Station_Name, StationCode, EmployeeCount)
        SELECT DISTINCT 
            COALESCE(s.Station_Name, e.Station) as Station_Name,
            e.Station as StationCode,
            COUNT(*) as EmployeeCount
        FROM OPENROWSET(''SQLNCLI'', ''' + @LegacyConnectionString + ''', 
            ''SELECT Station FROM Employee_bkp WHERE EmpisCurrActive = 1 AND Station IS NOT NULL'') e
        LEFT JOIN OPENROWSET(''SQLNCLI'', ''' + @LegacyConnectionString + ''', 
            ''SELECT StationID, Station_Name FROM Station'') s 
            ON e.Station = s.Station_Name OR e.Station = CAST(s.StationID AS VARCHAR(10))
        WHERE e.Station IS NOT NULL AND e.Station != ''''
        GROUP BY COALESCE(s.Station_Name, e.Station), e.Station'
        
        EXEC sp_executesql @SQL
        
        -- Insert/Update stations in OSH database
        MERGE Stations AS target
        USING (
            SELECT 
                StationCode,
                CASE 
                    WHEN Station_Name IS NOT NULL AND Station_Name != StationCode 
                    THEN Station_Name 
                    ELSE 
                        CASE StationCode
                            WHEN 'HQ' THEN 'Head Office'
                            WHEN '005' THEN 'Station 005'
                            WHEN '011' THEN 'Station 011'
                            WHEN '367' THEN 'Kapsara Factory'
                            WHEN '711' THEN 'Station 711'
                            ELSE 'Station ' + StationCode
                        END
                END as StationName,
                1 as OrgCategoryId, -- Default to first category
                StationCode as LegacyStationMapping
            FROM #LegacyStations
        ) AS source ON target.LegacyStationMapping = source.StationCode
        
        WHEN MATCHED THEN
            UPDATE SET 
                StationName = source.StationName,
                UpdatedAt = GETUTCDATE()
                
        WHEN NOT MATCHED THEN
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
