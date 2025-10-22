-- =============================================================================
-- SCRIPT 4: Seed Team Role Definitions (FIXED)
-- =============================================================================
-- Creates role definitions for each team type
-- Handles both old TeamType (string) and new TeamTypeDefinitionId (FK)
-- =============================================================================

USE OSHManagement;
GO

PRINT '========================================';
PRINT 'SCRIPT 4: Seeding Team Role Definitions';
PRINT '========================================';
PRINT '';

-- ========================================
-- STEP 1: Make old TeamType column nullable (if it exists and is NOT NULL)
-- ========================================
IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('TeamRoleDefinitions')
    AND name = 'TeamType'
    AND is_nullable = 0
)
BEGIN
    PRINT 'Making TeamType column nullable temporarily...';
    ALTER TABLE TeamRoleDefinitions ALTER COLUMN TeamType NVARCHAR(50) NULL;
    PRINT '  ✓ TeamType column is now nullable';
    PRINT '';
END

-- Get team type IDs
DECLARE @OshCommitteeId INT = (SELECT TeamTypeDefinitionId FROM TeamTypeDefinitions WHERE TypeCode = 'OshCommittee');
DECLARE @RiskAssessmentId INT = (SELECT TeamTypeDefinitionId FROM TeamTypeDefinitions WHERE TypeCode = 'RiskAssessment');
DECLARE @InvestigationId INT = (SELECT TeamTypeDefinitionId FROM TeamTypeDefinitions WHERE TypeCode = 'Investigation');
DECLARE @CustomId INT = (SELECT TeamTypeDefinitionId FROM TeamTypeDefinitions WHERE TypeCode = 'Custom');

PRINT 'Team Type IDs:';
PRINT '  OSH Committee: ' + CAST(@OshCommitteeId AS VARCHAR(10));
PRINT '  Risk Assessment: ' + CAST(@RiskAssessmentId AS VARCHAR(10));
PRINT '  Investigation: ' + CAST(@InvestigationId AS VARCHAR(10));
PRINT '  Custom: ' + CAST(@CustomId AS VARCHAR(10));
PRINT '';

-- ========================================
-- OSH COMMITTEE ROLES
-- ========================================
PRINT '1. Creating OSH Committee roles...';

-- Chairperson
IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @OshCommitteeId AND RoleName = 'Chairperson')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId,
        TeamType,  -- Populate old column too
        RoleName,
        Description,
        IsEmployeeRepresentative,
        IsEmployerRepresentative,
        IsExOfficio,
        RequiresVotingRights,
        MinOccurrences,
        MaxOccurrences,
        RequiredQualifications,
        DisplayOrder,
        IsActive,
        CreatedAt
    ) VALUES (
        @OshCommitteeId,
        'OshCommittee',  -- Old column value
        'Chairperson',
        'Leads committee meetings, sets agendas, and ensures effective functioning. Must be elected from employee representatives.',
        1, 0, 0,  -- Employee representative
        1, 1, 1,  -- Voting rights, Min: 1, Max: 1
        'Leadership skills, OSH training preferred',
        1, 1,
        GETUTCDATE()
    );
    PRINT '   ✓ Chairperson';
END

-- Vice Chairperson
IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @OshCommitteeId AND RoleName = 'Vice Chairperson')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, TeamType, RoleName, Description,
        IsEmployeeRepresentative, IsEmployerRepresentative, IsExOfficio,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        DisplayOrder, IsActive, CreatedAt
    ) VALUES (
        @OshCommitteeId, 'OshCommittee', 'Vice Chairperson',
        'Supports chairperson and acts in their absence.',
        0, 0, 0,
        1, 0, 1,  -- Optional, Max: 1
        2, 1, GETUTCDATE()
    );
    PRINT '   ✓ Vice Chairperson';
END

-- Secretary
IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @OshCommitteeId AND RoleName = 'Secretary')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, TeamType, RoleName, Description,
        IsEmployeeRepresentative, IsEmployerRepresentative, IsExOfficio,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive, CreatedAt
    ) VALUES (
        @OshCommitteeId, 'OshCommittee', 'Secretary',
        'Records minutes, maintains documentation, handles correspondence.',
        0, 0, 0,
        1, 1, 1,  -- Required, Max: 1
        'Good writing and organizational skills',
        3, 1, GETUTCDATE()
    );
    PRINT '   ✓ Secretary';
END

-- Employee Representatives
IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @OshCommitteeId AND RoleName = 'Employee Representative')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, TeamType, RoleName, Description,
        IsEmployeeRepresentative, IsEmployerRepresentative, IsExOfficio,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        DisplayOrder, IsActive, CreatedAt
    ) VALUES (
        @OshCommitteeId, 'OshCommittee', 'Employee Representative',
        'Elected representatives from workforce. Must constitute at least 2/3 of committee.',
        1, 0, 0,  -- Employee representative
        1, 4, NULL,  -- Min: 4 (to meet 2/3 ratio), No max
        4, 1, GETUTCDATE()
    );
    PRINT '   ✓ Employee Representative';
END

-- Employer Representatives
IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @OshCommitteeId AND RoleName = 'Employer Representative')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, TeamType, RoleName, Description,
        IsEmployeeRepresentative, IsEmployerRepresentative, IsExOfficio,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        DisplayOrder, IsActive, CreatedAt
    ) VALUES (
        @OshCommitteeId, 'OshCommittee', 'Employer Representative',
        'Management-appointed representatives. Must constitute approximately 1/3 of committee.',
        0, 1, 0,  -- Employer representative
        1, 2, NULL,  -- Min: 2, No max
        5, 1, GETUTCDATE()
    );
    PRINT '   ✓ Employer Representative';
END

-- Safety Officer (Ex-Officio)
IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @OshCommitteeId AND RoleName = 'Safety Officer')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, TeamType, RoleName, Description,
        IsEmployeeRepresentative, IsEmployerRepresentative, IsExOfficio,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive, CreatedAt
    ) VALUES (
        @OshCommitteeId, 'OshCommittee', 'Safety Officer',
        'Technical advisor to the committee. Non-voting member.',
        0, 0, 1,  -- Ex-officio
        0, 0, 1,  -- Non-voting, Optional, Max: 1
        'OSH professional qualification (Level 2 or higher)',
        6, 1, GETUTCDATE()
    );
    PRINT '   ✓ Safety Officer';
END

PRINT '';

-- ========================================
-- RISK ASSESSMENT TEAM ROLES
-- ========================================
PRINT '2. Creating Risk Assessment Team roles...';

IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @RiskAssessmentId AND RoleName = 'Team Leader')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, TeamType, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive, CreatedAt
    ) VALUES (
        @RiskAssessmentId, 'RiskAssessment', 'Team Leader',
        'Coordinates risk assessment activities and finalizes reports.',
        1, 1, 1,
        'Risk assessment training, technical expertise',
        1, 1, GETUTCDATE()
    );
    PRINT '   ✓ Team Leader';
END

IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @RiskAssessmentId AND RoleName = 'Risk Assessor')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, TeamType, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive, CreatedAt
    ) VALUES (
        @RiskAssessmentId, 'RiskAssessment', 'Risk Assessor',
        'Conducts hazard identification and risk analysis.',
        1, 2, NULL,
        'Risk assessment competency',
        2, 1, GETUTCDATE()
    );
    PRINT '   ✓ Risk Assessor';
END

IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @RiskAssessmentId AND RoleName = 'Technical Expert')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, TeamType, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive, CreatedAt
    ) VALUES (
        @RiskAssessmentId, 'RiskAssessment', 'Technical Expert',
        'Provides specialized technical input (machinery, chemical, ergonomic, etc.).',
        1, 0, NULL,
        'Technical expertise in specific domain',
        3, 1, GETUTCDATE()
    );
    PRINT '   ✓ Technical Expert';
END

IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @RiskAssessmentId AND RoleName = 'Worker Representative')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, TeamType, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive, CreatedAt
    ) VALUES (
        @RiskAssessmentId, 'RiskAssessment', 'Worker Representative',
        'Represents workers from assessed areas, provides practical insights.',
        1, 1, NULL,
        'Familiarity with work processes',
        4, 1, GETUTCDATE()
    );
    PRINT '   ✓ Worker Representative';
END

PRINT '';

-- ========================================
-- INVESTIGATION TEAM ROLES
-- ========================================
PRINT '3. Creating Investigation Team roles...';

IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @InvestigationId AND RoleName = 'Lead Investigator')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, TeamType, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive, CreatedAt
    ) VALUES (
        @InvestigationId, 'Investigation', 'Lead Investigator',
        'Leads incident investigation and root cause analysis.',
        1, 1, 1,
        'Incident investigation training (e.g., ICAM, TapRooT, 5-Why)',
        1, 1, GETUTCDATE()
    );
    PRINT '   ✓ Lead Investigator';
END

IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @InvestigationId AND RoleName = 'Investigator')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, TeamType, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive, CreatedAt
    ) VALUES (
        @InvestigationId, 'Investigation', 'Investigator',
        'Supports investigation process, gathers evidence, interviews witnesses.',
        1, 1, NULL,
        'Investigation methodology knowledge',
        2, 1, GETUTCDATE()
    );
    PRINT '   ✓ Investigator';
END

IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @InvestigationId AND RoleName = 'Technical Specialist')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, TeamType, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive, CreatedAt
    ) VALUES (
        @InvestigationId, 'Investigation', 'Technical Specialist',
        'Provides technical analysis (engineering, medical, etc.).',
        1, 0, NULL,
        'Technical expertise relevant to incident type',
        3, 1, GETUTCDATE()
    );
    PRINT '   ✓ Technical Specialist';
END

IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @InvestigationId AND RoleName = 'Safety Representative')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, TeamType, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive, CreatedAt
    ) VALUES (
        @InvestigationId, 'Investigation', 'Safety Representative',
        'Ensures worker perspective and safety focus.',
        1, 1, NULL,
        'OSH Committee member or safety rep',
        4, 1, GETUTCDATE()
    );
    PRINT '   ✓ Safety Representative';
END

PRINT '';

-- ========================================
-- CUSTOM TEAM ROLES (Examples)
-- ========================================
PRINT '4. Creating Custom Team role examples...';

IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @CustomId AND RoleName = 'Team Captain')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, TeamType, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        DisplayOrder, IsActive, CreatedAt
    ) VALUES (
        @CustomId, 'Custom', 'Team Captain',
        'Leads the team activities.',
        1, 0, 1,
        1, 1, GETUTCDATE()
    );
    PRINT '   ✓ Team Captain';
END

IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @CustomId AND RoleName = 'Member')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, TeamType, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        DisplayOrder, IsActive, CreatedAt
    ) VALUES (
        @CustomId, 'Custom', 'Member',
        'General team member.',
        1, 1, NULL,
        2, 1, GETUTCDATE()
    );
    PRINT '   ✓ Member';
END

PRINT '';

PRINT '========================================';
PRINT 'SCRIPT 4: COMPLETED';
PRINT '========================================';
PRINT '';
PRINT 'Summary - Roles per Team Type:';
SELECT
    ttd.TypeName,
    COUNT(trd.TeamRoleDefinitionId) AS RoleCount,
    STRING_AGG(trd.RoleName, ', ') WITHIN GROUP (ORDER BY trd.DisplayOrder) AS Roles
FROM TeamTypeDefinitions ttd
LEFT JOIN TeamRoleDefinitions trd ON ttd.TeamTypeDefinitionId = trd.TeamTypeDefinitionId
WHERE trd.IsActive = 1
GROUP BY ttd.TypeName, ttd.TeamTypeDefinitionId
ORDER BY ttd.TeamTypeDefinitionId;
PRINT '';
GO
