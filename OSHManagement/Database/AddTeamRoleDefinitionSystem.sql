-- =============================================================================
-- TeamRoleDefinition System Implementation
-- =============================================================================
-- This script creates the TeamRoleDefinition table and modifies TeamMembers
-- to use the new role definition FK instead of string-based MemberRole
-- =============================================================================

USE OSH_Management; -- Change to your database name if different
GO

PRINT 'Starting TeamRoleDefinition system setup...';
PRINT '';

-- =============================================================================
-- STEP 1: Create TeamRoleDefinitions Table
-- =============================================================================
PRINT 'Step 1: Creating TeamRoleDefinitions table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TeamRoleDefinitions')
BEGIN
    CREATE TABLE TeamRoleDefinitions (
        TeamRoleDefinitionId INT IDENTITY(1,1) PRIMARY KEY,
        TeamType NVARCHAR(50) NOT NULL,
        RoleName NVARCHAR(50) NOT NULL,
        Description NVARCHAR(500) NULL,
        RequiresVotingRights BIT NOT NULL DEFAULT 1,
        MaxOccurrences INT NULL,
        MinOccurrences INT NULL,
        RequiredQualifications NVARCHAR(500) NULL,
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,

        -- Indexes
        INDEX IX_TeamRoleDefinitions_TeamType (TeamType),
        CONSTRAINT UQ_TeamRoleDefinitions_TeamType_RoleName UNIQUE (TeamType, RoleName)
    );

    PRINT '  ✓ TeamRoleDefinitions table created';
END
ELSE
BEGIN
    PRINT '  ⚠ TeamRoleDefinitions table already exists, skipping...';
END
GO

-- =============================================================================
-- STEP 2: Seed TeamRoleDefinitions with Predefined Roles
-- =============================================================================
PRINT '';
PRINT 'Step 2: Seeding TeamRoleDefinitions...';

IF NOT EXISTS (SELECT * FROM TeamRoleDefinitions)
BEGIN
    -- OSH COMMITTEE ROLES
    INSERT INTO TeamRoleDefinitions (TeamType, RoleName, Description, RequiresVotingRights, MaxOccurrences, MinOccurrences, RequiredQualifications, DisplayOrder, IsActive, CreatedAt)
    VALUES
    -- OSH Committee
    ('OSH Committee', 'Chairperson', 'Leads committee meetings, sets agenda, and ensures compliance with OSH Act requirements', 1, 1, 1, 'Management representative with authority to implement decisions', 1, 1, GETUTCDATE()),
    ('OSH Committee', 'Secretary', 'Records minutes, maintains documentation, and handles committee correspondence', 1, 1, 1, 'Good documentation and organizational skills', 2, 1, GETUTCDATE()),
    ('OSH Committee', 'Safety Representative', 'Worker representative responsible for workplace safety concerns and reporting hazards', 1, NULL, 2, 'Employee with at least 1 year experience in the workplace', 3, 1, GETUTCDATE()),
    ('OSH Committee', 'Management Representative', 'Represents management interests and has authority to implement safety decisions', 1, NULL, 2, 'Management or supervisory position', 4, 1, GETUTCDATE()),
    ('OSH Committee', 'Worker Representative', 'Represents worker concerns and participates in safety decision-making', 1, NULL, NULL, 'Active employee at the station', 5, 1, GETUTCDATE()),
    ('OSH Committee', 'Observer', 'Attends meetings and provides input but does not have voting rights', 0, NULL, NULL, NULL, 6, 1, GETUTCDATE()),

    -- Risk Assessment Team
    ('Risk Assessment', 'Team Leader', 'Leads risk assessment activities and coordinates team efforts', 1, 1, 1, 'Training in risk assessment methodology', 1, 1, GETUTCDATE()),
    ('Risk Assessment', 'Risk Assessor', 'Conducts risk assessments and identifies hazards in the workplace', 1, NULL, 2, 'Risk assessment training or relevant experience', 2, 1, GETUTCDATE()),
    ('Risk Assessment', 'Subject Matter Expert', 'Provides technical expertise for specific hazards or processes', 1, NULL, NULL, 'Technical expertise in relevant area', 3, 1, GETUTCDATE()),
    ('Risk Assessment', 'Documentation Officer', 'Records findings and maintains risk assessment documentation', 1, 1, NULL, 'Good documentation and analytical skills', 4, 1, GETUTCDATE()),
    ('Risk Assessment', 'Reviewer', 'Reviews and validates risk assessment findings', 0, NULL, NULL, 'Experience in risk assessment or safety management', 5, 1, GETUTCDATE()),

    -- Investigation Team
    ('Investigation', 'Lead Investigator', 'Leads incident investigation and coordinates investigation activities', 1, 1, 1, 'Training in incident investigation techniques', 1, 1, GETUTCDATE()),
    ('Investigation', 'Investigator', 'Participates in incident investigation and evidence collection', 1, NULL, 2, 'Basic investigation training', 2, 1, GETUTCDATE()),
    ('Investigation', 'Technical Specialist', 'Provides technical analysis for incident investigation', 1, NULL, NULL, 'Technical expertise relevant to incident type', 3, 1, GETUTCDATE()),
    ('Investigation', 'Witness Liaison', 'Conducts witness interviews and maintains witness records', 1, NULL, NULL, 'Good interpersonal and communication skills', 4, 1, GETUTCDATE()),
    ('Investigation', 'Documentation Officer', 'Records investigation findings and prepares investigation reports', 1, 1, 1, 'Strong documentation and report writing skills', 5, 1, GETUTCDATE()),
    ('Investigation', 'Observer', 'Observes investigation process for oversight purposes', 0, NULL, NULL, NULL, 6, 1, GETUTCDATE());

    PRINT '  ✓ 17 role definitions seeded successfully';
END
ELSE
BEGIN
    PRINT '  ⚠ TeamRoleDefinitions already contain data, skipping seed...';
END
GO

-- =============================================================================
-- STEP 3: Add TeamRoleDefinitionId to TeamMembers
-- =============================================================================
PRINT '';
PRINT 'Step 3: Modifying TeamMembers table...';

-- Add TeamRoleDefinitionId column (nullable for now)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'TeamMembers') AND name = 'TeamRoleDefinitionId')
BEGIN
    ALTER TABLE TeamMembers
    ADD TeamRoleDefinitionId INT NULL;

    PRINT '  ✓ TeamRoleDefinitionId column added to TeamMembers';
END
ELSE
BEGIN
    PRINT '  ⚠ TeamRoleDefinitionId column already exists in TeamMembers';
END
GO

-- Rename MemberRole to MemberRole_Old (preserve data for migration)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'TeamMembers') AND name = 'MemberRole')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'TeamMembers') AND name = 'MemberRole_Old')
BEGIN
    EXEC sp_rename 'TeamMembers.MemberRole', 'MemberRole_Old', 'COLUMN';
    PRINT '  ✓ MemberRole column renamed to MemberRole_Old';
END
ELSE
BEGIN
    PRINT '  ⚠ MemberRole column already renamed or does not exist';
END
GO

-- =============================================================================
-- STEP 4: Create Foreign Key Relationship
-- =============================================================================
PRINT '';
PRINT 'Step 4: Creating foreign key relationships...';

-- FK: TeamMembers.TeamRoleDefinitionId -> TeamRoleDefinitions
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TeamMembers_TeamRoleDefinitions_TeamRoleDefinitionId')
BEGIN
    ALTER TABLE TeamMembers
    ADD CONSTRAINT FK_TeamMembers_TeamRoleDefinitions_TeamRoleDefinitionId
    FOREIGN KEY (TeamRoleDefinitionId)
    REFERENCES TeamRoleDefinitions(TeamRoleDefinitionId)
    ON DELETE SET NULL;

    PRINT '  ✓ Foreign key FK_TeamMembers_TeamRoleDefinitions_TeamRoleDefinitionId created';
END
ELSE
BEGIN
    PRINT '  ⚠ Foreign key already exists';
END
GO

-- Create index for performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TeamMembers_TeamRoleDefinitionId' AND object_id = OBJECT_ID(N'TeamMembers'))
BEGIN
    CREATE INDEX IX_TeamMembers_TeamRoleDefinitionId
    ON TeamMembers(TeamRoleDefinitionId);

    PRINT '  ✓ Index IX_TeamMembers_TeamRoleDefinitionId created';
END
ELSE
BEGIN
    PRINT '  ⚠ Index already exists';
END
GO

-- =============================================================================
-- STEP 5: Add Unique Constraint on Employees.PayrollNo (if needed)
-- =============================================================================
PRINT '';
PRINT 'Step 5: Adding unique constraint on Employees.PayrollNo...';

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'AK_Employees_PayrollNo')
BEGIN
    ALTER TABLE Employees
    ADD CONSTRAINT AK_Employees_PayrollNo UNIQUE (PayrollNo);

    PRINT '  ✓ Unique constraint AK_Employees_PayrollNo created';
END
ELSE
BEGIN
    PRINT '  ⚠ Unique constraint already exists';
END
GO

-- =============================================================================
-- STEP 6: Create FK for TeamMembers.EmployeePayroll (if not exists)
-- =============================================================================
PRINT '';
PRINT 'Step 6: Creating FK for TeamMembers.EmployeePayroll...';

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TeamMembers_Employees_EmployeePayroll')
BEGIN
    -- Create index first for performance
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TeamMembers_EmployeePayroll' AND object_id = OBJECT_ID(N'TeamMembers'))
    BEGIN
        CREATE INDEX IX_TeamMembers_EmployeePayroll
        ON TeamMembers(EmployeePayroll);

        PRINT '  ✓ Index IX_TeamMembers_EmployeePayroll created';
    END

    ALTER TABLE TeamMembers
    ADD CONSTRAINT FK_TeamMembers_Employees_EmployeePayroll
    FOREIGN KEY (EmployeePayroll)
    REFERENCES Employees(PayrollNo)
    ON DELETE NO ACTION;

    PRINT '  ✓ Foreign key FK_TeamMembers_Employees_EmployeePayroll created';
END
ELSE
BEGIN
    PRINT '  ⚠ Foreign key already exists';
END
GO

-- =============================================================================
-- VERIFICATION
-- =============================================================================
PRINT '';
PRINT '=============================================================================';
PRINT 'VERIFICATION';
PRINT '=============================================================================';

-- Check TeamRoleDefinitions
PRINT 'TeamRoleDefinitions:';
SELECT
    COUNT(*) AS TotalRoles,
    COUNT(CASE WHEN TeamType = 'OSH Committee' THEN 1 END) AS OSHCommitteeRoles,
    COUNT(CASE WHEN TeamType = 'Risk Assessment' THEN 1 END) AS RiskAssessmentRoles,
    COUNT(CASE WHEN TeamType = 'Investigation' THEN 1 END) AS InvestigationRoles
FROM TeamRoleDefinitions;

-- Check TeamMembers structure
PRINT '';
PRINT 'TeamMembers columns:';
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TeamMembers'
AND COLUMN_NAME IN ('EmployeePayroll', 'TeamRoleDefinitionId', 'MemberRole', 'MemberRole_Old')
ORDER BY COLUMN_NAME;

-- Check existing TeamMembers
PRINT '';
PRINT 'TeamMembers status:';
SELECT
    'Total TeamMembers' AS Category,
    COUNT(*) AS Count
FROM TeamMembers
UNION ALL
SELECT
    'With TeamRoleDefinitionId',
    COUNT(*)
FROM TeamMembers
WHERE TeamRoleDefinitionId IS NOT NULL
UNION ALL
SELECT
    'Without TeamRoleDefinitionId (needs migration)',
    COUNT(*)
FROM TeamMembers
WHERE TeamRoleDefinitionId IS NULL;

PRINT '';
PRINT '=============================================================================';
PRINT 'TeamRoleDefinition System Setup Complete!';
PRINT '=============================================================================';
PRINT '';
PRINT 'NEXT STEPS:';
PRINT '1. Run the data migration script: MigrateTeamMemberRoles.sql';
PRINT '2. Verify all TeamMembers have TeamRoleDefinitionId populated';
PRINT '3. Optionally, make TeamRoleDefinitionId NOT NULL (requires all data migrated)';
PRINT '4. Update your application code to use TeamRoleDefinitionId instead of MemberRole';
PRINT '';
GO
