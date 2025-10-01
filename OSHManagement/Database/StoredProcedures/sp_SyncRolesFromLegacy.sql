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
        -- Create temp table for employee role assignments from Employee_bkp
        IF OBJECT_ID('tempdb..#LegacyEmployeeRoles') IS NOT NULL
            DROP TABLE #LegacyEmployeeRoles

        CREATE TABLE #LegacyEmployeeRoles (
            PayrollNo NVARCHAR(20),
            Role NVARCHAR(50)
        )

        -- Fetch employee roles directly from Employee_bkp.Role field
        SET @SQL = '
        INSERT INTO #LegacyEmployeeRoles (PayrollNo, Role)
        SELECT DISTINCT PayrollNo, RTRIM(LTRIM(Role)) as Role
        FROM OPENROWSET(''MSOLEDBSQL'', ''' + @LegacyConnectionString + ''',
            ''SELECT PayrollNo, Role FROM Employee_bkp
                    WHERE EmpisCurrActive = 0 AND Role IS NOT NULL AND Role != ''''''''
            '')'

        EXEC sp_executesql @SQL

        -- First, ensure all unique roles from Employee_bkp exist in Roles table
        MERGE Roles AS target
        USING (
            SELECT DISTINCT
                Role as RoleName,
                CASE Role
                    WHEN 'Admin' THEN 'System Administrator - Full system access'
                    WHEN 'HR' THEN 'Human Resources - Employee management'
                    WHEN 'Hod' THEN 'Head of Department - Departmental oversight'
                    WHEN 'supervisor' THEN 'Supervisor - Team supervision'
                    WHEN 'user' THEN 'Standard User - General system access'
                    WHEN '2_step_mgrs' THEN 'Two-Step Manager - Approval authority'
                    WHEN '2_step_secs' THEN 'Two-Step Secretary - Administrative support'
                    WHEN 'FieldUser' THEN 'Field User - Basic field operations'
                    WHEN 'FieldSupervisor' THEN 'Field Supervisor - Field operations management'
                    ELSE Role + ' - Legacy role'
                END as Description,
                Role as LegacyRoleMapping
            FROM #LegacyEmployeeRoles
            WHERE Role IS NOT NULL
        ) AS source ON target.LegacyRoleMapping = source.LegacyRoleMapping

        WHEN NOT MATCHED BY TARGET THEN
            INSERT (RoleName, Description, LegacyRoleMapping, IsActive, CreatedAt)
            VALUES (source.RoleName, source.Description, source.LegacyRoleMapping, 1, GETUTCDATE());
        
        -- Now assign roles to employees
        MERGE EmployeeRoles AS target
        USING (
            SELECT
                e.EmployeeId,
                r.RoleId
            FROM #LegacyEmployeeRoles ler
            INNER JOIN Employees e ON e.PayrollNo = ler.PayrollNo
            INNER JOIN Roles r ON r.LegacyRoleMapping = ler.Role
        ) AS source ON target.EmployeeId = source.EmployeeId AND target.RoleId = source.RoleId

        WHEN NOT MATCHED BY TARGET THEN
            INSERT (EmployeeId, RoleId, AssignedAt, IsActive)
            VALUES (source.EmployeeId, source.RoleId, GETUTCDATE(), 1);

        -- Log results
        DECLARE @RoleCount INT = (SELECT COUNT(DISTINCT Role) FROM #LegacyEmployeeRoles)
        DECLARE @AssignmentCount INT = @@ROWCOUNT

        PRINT 'Role sync completed. Unique roles: ' + CAST(@RoleCount AS VARCHAR(10)) +
              ', Employee role assignments: ' + CAST(@AssignmentCount AS VARCHAR(10))

        DROP TABLE #LegacyEmployeeRoles
        
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
