-- =============================================
-- Author: OSH Management System
-- Create date: 2024-09-29
-- Description: Synchronizes roles and employee role assignments from legacy KTDALeave database
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_SyncRolesFromLegacy]
    @LegacyConnectionString NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SQL NVARCHAR(MAX)
    DECLARE @ErrorMessage NVARCHAR(4000)
    DECLARE @ErrorSeverity INT
    DECLARE @ErrorState INT
    
    BEGIN TRY
        -- Create temporary table for legacy role data
        IF OBJECT_ID('tempdb..#LegacyRoles') IS NOT NULL
            DROP TABLE #LegacyRoles
            
        CREATE TABLE #LegacyRoles (
            PayrollNo NVARCHAR(20),
            Role NVARCHAR(20)
        )
        
        -- Build dynamic SQL to fetch from legacy database
        SET @SQL = '
        INSERT INTO #LegacyRoles (PayrollNo, Role)
        SELECT DISTINCT PayrollNo, Role
        FROM OPENROWSET(''SQLNCLI'', ''' + @LegacyConnectionString + ''', 
            ''SELECT PayrollNo, Role FROM Employee_bkp 
              WHERE EmpisCurrActive = 1 AND Role IS NOT NULL AND Role != '''''''')'
        
        EXEC sp_executesql @SQL
        
        -- First, ensure all roles exist in the Roles table
        MERGE Roles AS target
        USING (
            SELECT DISTINCT 
                Role as RoleName,
                CASE Role
                    WHEN 'FieldUser' THEN 'Field User - Basic OSH operations'
                    WHEN 'user' THEN 'Standard User - General system access'
                    WHEN 'Hod' THEN 'Head of Department - Departmental oversight'
                    WHEN 'FieldSupervisor' THEN 'Field Supervisor - Field operations management'
                    ELSE Role + ' - Legacy role'
                END as Description,
                Role as LegacyRoleMapping
            FROM #LegacyRoles
            WHERE Role IS NOT NULL
        ) AS source ON target.LegacyRoleMapping = source.RoleName
        
        WHEN NOT MATCHED THEN
            INSERT (RoleName, Description, LegacyRoleMapping, IsActive, CreatedAt)
            VALUES (source.RoleName, source.Description, source.LegacyRoleMapping, 1, GETUTCDATE());
        
        -- Now assign roles to employees
        MERGE EmployeeRoles AS target
        USING (
            SELECT 
                e.EmployeeId,
                r.RoleId
            FROM #LegacyRoles lr
            INNER JOIN Employees e ON e.PayrollNo = lr.PayrollNo
            INNER JOIN Roles r ON r.LegacyRoleMapping = lr.Role
        ) AS source ON target.EmployeeId = source.EmployeeId AND target.RoleId = source.RoleId
        
        WHEN NOT MATCHED THEN
            INSERT (EmployeeId, RoleId, AssignedAt, IsActive)
            VALUES (source.EmployeeId, source.RoleId, GETUTCDATE(), 1);
        
        -- Log results
        DECLARE @RoleCount INT = (SELECT COUNT(DISTINCT Role) FROM #LegacyRoles)
        DECLARE @AssignmentCount INT = @@ROWCOUNT
        
        PRINT 'Role sync completed. Unique roles: ' + CAST(@RoleCount AS VARCHAR(10)) + 
              ', Role assignments: ' + CAST(@AssignmentCount AS VARCHAR(10))
        
        DROP TABLE #LegacyRoles
        
    END TRY
    BEGIN CATCH
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE()
            
        PRINT 'Error in sp_SyncRolesFromLegacy: ' + @ErrorMessage
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
