-- =============================================
-- Author: OSH Management System
-- Create date: 2024-09-29
-- Description: Synchronizes employees from legacy KTDALeave database
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_SyncEmployeesFromLegacy]
    @LegacyConnectionString NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SQL NVARCHAR(MAX)
    DECLARE @ErrorMessage NVARCHAR(4000)
    DECLARE @ErrorSeverity INT
    DECLARE @ErrorState INT
    
    BEGIN TRY
        -- Create temporary table for legacy employee data
        IF OBJECT_ID('tempdb..#LegacyEmployees') IS NOT NULL
            DROP TABLE #LegacyEmployees
            
        CREATE TABLE #LegacyEmployees (
            PayrollNo NVARCHAR(20),
            RollNo NVARCHAR(20),
            SurName NVARCHAR(50),
            OtherNames NVARCHAR(50),
            Email_address NVARCHAR(100),
            Station NVARCHAR(50),
            Department NVARCHAR(50),
            Designation NVARCHAR(50),
            Hod NVARCHAR(50),
            supervisor NVARCHAR(50),
            Role NVARCHAR(20),
            EmpisCurrActive INT,
            hire_date DATETIME,
            contractEnd DATETIME,
            Service_years INT,
            username NVARCHAR(50),
            pass VARBINARY(50)
        )
        
        -- Build dynamic SQL to fetch from legacy database
        SET @SQL = '
        INSERT INTO #LegacyEmployees
        SELECT
            PayrollNo, RollNo, SurName, OtherNames, Email_address, Station, Department,
            Designation, Hod, supervisor, Role, EmpisCurrActive, hire_date, contractEnd,
            Service_years, username, pass
        FROM OPENROWSET(''MSOLEDBSQL'', ''' + @LegacyConnectionString + ''',
            ''SELECT PayrollNo, RollNo, SurName, OtherNames, Email_address, Station, Department,
                    Designation, Hod, supervisor, Role, EmpisCurrActive, hire_date, contractEnd,
                    Service_years, username, pass
                    FROM Employee_bkp
                    WHERE PayrollNo IS NOT NULL AND PayrollNo != ''''''''
                    AND (EmpisCurrActive = 0 OR EmpisCurrActive = 1)
            '')'

        EXEC sp_executesql @SQL
        
        -- Insert/Update employees in OSH database
        MERGE Employees AS target
        USING (
            SELECT 
                le.PayrollNo,
                le.RollNo,
                COALESCE(le.OtherNames, 'Unknown') as FirstName,
                COALESCE(le.SurName, 'Employee') as LastName,
                le.Email_address as EmailAddress,
                le.username as Username,
                le.pass as LegacyPassword,
                COALESCE(s.StationId, 1) as StationId,
                d.DepartmentId,
                le.Designation,
                le.supervisor as SupervisorPayroll,
                le.Hod as HodPayroll,
                CASE le.EmpisCurrActive
                    WHEN 0 THEN 'Active'
                    WHEN 1 THEN 'Inactive'
                    ELSE 'Inactive'
                END as EmploymentStatus,
                le.hire_date as HireDate,
                le.contractEnd as ContractEndDate,
                le.Service_years as ServiceYears
            FROM #LegacyEmployees le
            LEFT JOIN Stations s ON s.LegacyStationMapping = le.Station
            LEFT JOIN Departments d ON d.LegacyDepartmentMapping = le.Department
            WHERE le.PayrollNo IS NOT NULL
        ) AS source ON target.PayrollNo = source.PayrollNo
        
        WHEN MATCHED THEN
            UPDATE SET 
                FirstName = source.FirstName,
                LastName = source.LastName,
                EmailAddress = source.EmailAddress,
                Username = source.Username,
                LegacyPassword = source.LegacyPassword,
                StationId = source.StationId,
                DepartmentId = source.DepartmentId,
                Designation = source.Designation,
                SupervisorPayroll = source.SupervisorPayroll,
                HodPayroll = source.HodPayroll,
                EmploymentStatus = source.EmploymentStatus,
                HireDate = source.HireDate,
                ContractEndDate = source.ContractEndDate,
                ServiceYears = source.ServiceYears,
                UpdatedAt = GETUTCDATE()
                
        WHEN NOT MATCHED THEN
            INSERT (PayrollNo, RollNo, FirstName, LastName, EmailAddress, Username, LegacyPassword,
                   StationId, DepartmentId, Designation, SupervisorPayroll, HodPayroll, 
                   EmploymentStatus, HireDate, ContractEndDate, ServiceYears, CreatedAt)
            VALUES (source.PayrollNo, source.RollNo, source.FirstName, source.LastName, 
                   source.EmailAddress, source.Username, source.LegacyPassword, source.StationId, 
                   source.DepartmentId, source.Designation, source.SupervisorPayroll, 
                   source.HodPayroll, source.EmploymentStatus, source.HireDate, 
                   source.ContractEndDate, source.ServiceYears, GETUTCDATE());
        
        -- Log results
        DECLARE @InsertedCount INT = @@ROWCOUNT
        PRINT 'Employee sync completed. Rows affected: ' + CAST(@InsertedCount AS VARCHAR(10))
        
        DROP TABLE #LegacyEmployees
        
    END TRY
    BEGIN CATCH
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE()
            
        PRINT 'Error in sp_SyncEmployeesFromLegacy: ' + @ErrorMessage
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
