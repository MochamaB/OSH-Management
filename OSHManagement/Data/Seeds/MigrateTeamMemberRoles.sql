-- =============================================================================
-- Data Migration Script: Map existing TeamMember.MemberRole to TeamRoleDefinitionId
-- =============================================================================
-- Run this script AFTER:
-- 1. Running the migrations (Add-Migration, Update-Database)
-- 2. Running the TeamRoleDefinitionSeeder
--
-- This script maps old string-based MemberRole values to the new
-- TeamRoleDefinition foreign key
-- =============================================================================

USE OSH_Management; -- Change to your database name if different
GO

PRINT 'Starting TeamMember role migration...';
GO

-- Update TeamMembers to link to TeamRoleDefinitions based on MemberRole_Old and Team.TeamType
UPDATE tm
SET tm.TeamRoleDefinitionId = trd.TeamRoleDefinitionId
FROM TeamMembers tm
INNER JOIN Teams t ON tm.TeamId = t.TeamId
INNER JOIN TeamRoleDefinitions trd ON
    trd.TeamType = t.TeamType
    AND (
        -- Exact match
        trd.RoleName = tm.MemberRole_Old
        OR
        -- Fuzzy matching for common variations
        (tm.MemberRole_Old LIKE '%Chair%' AND trd.RoleName = 'Chairperson')
        OR
        (tm.MemberRole_Old LIKE '%Secretar%' AND trd.RoleName = 'Secretary')
        OR
        (tm.MemberRole_Old LIKE '%Leader%' AND trd.RoleName = 'Team Leader')
        OR
        (tm.MemberRole_Old LIKE '%Lead Investigator%' AND trd.RoleName = 'Lead Investigator')
        OR
        (tm.MemberRole_Old LIKE '%Investigator%' AND trd.RoleName = 'Investigator')
        OR
        (tm.MemberRole_Old LIKE '%Assessor%' AND trd.RoleName = 'Risk Assessor')
        OR
        (tm.MemberRole_Old LIKE '%Safety Rep%' AND trd.RoleName = 'Safety Representative')
        OR
        (tm.MemberRole_Old LIKE '%Worker Rep%' AND trd.RoleName = 'Worker Representative')
        OR
        (tm.MemberRole_Old LIKE '%Management Rep%' AND trd.RoleName = 'Management Representative')
        OR
        (tm.MemberRole_Old LIKE '%Observer%' AND trd.RoleName = 'Observer')
        OR
        (tm.MemberRole_Old LIKE '%Expert%' AND trd.RoleName = 'Subject Matter Expert')
        OR
        (tm.MemberRole_Old LIKE '%Specialist%' AND trd.RoleName = 'Technical Specialist')
        OR
        (tm.MemberRole_Old LIKE '%Documentation%' AND trd.RoleName = 'Documentation Officer')
    )
WHERE tm.TeamRoleDefinitionId IS NULL;

PRINT 'Updated TeamMembers with matched roles';
GO

-- Handle any unmapped roles by assigning a generic role based on team type
-- OSH Committee: assign to "Worker Representative"
UPDATE tm
SET tm.TeamRoleDefinitionId = (
    SELECT TOP 1 trd.TeamRoleDefinitionId
    FROM TeamRoleDefinitions trd
    WHERE trd.TeamType = 'OSH Committee'
    AND trd.RoleName = 'Worker Representative'
)
FROM TeamMembers tm
INNER JOIN Teams t ON tm.TeamId = t.TeamId
WHERE tm.TeamRoleDefinitionId IS NULL
AND t.TeamType = 'OSH Committee';

-- Risk Assessment: assign to "Risk Assessor"
UPDATE tm
SET tm.TeamRoleDefinitionId = (
    SELECT TOP 1 trd.TeamRoleDefinitionId
    FROM TeamRoleDefinitions trd
    WHERE trd.TeamType = 'Risk Assessment'
    AND trd.RoleName = 'Risk Assessor'
)
FROM TeamMembers tm
INNER JOIN Teams t ON tm.TeamId = t.TeamId
WHERE tm.TeamRoleDefinitionId IS NULL
AND t.TeamType = 'Risk Assessment';

-- Investigation: assign to "Investigator"
UPDATE tm
SET tm.TeamRoleDefinitionId = (
    SELECT TOP 1 trd.TeamRoleDefinitionId
    FROM TeamRoleDefinitions trd
    WHERE trd.TeamType = 'Investigation'
    AND trd.RoleName = 'Investigator'
)
FROM TeamMembers tm
INNER JOIN Teams t ON tm.TeamId = t.TeamId
WHERE tm.TeamRoleDefinitionId IS NULL
AND t.TeamType = 'Investigation';

PRINT 'Assigned generic roles to unmapped members';
GO

-- Verify migration
SELECT
    'Total TeamMembers' AS Category,
    COUNT(*) AS Count
FROM TeamMembers
UNION ALL
SELECT
    'Migrated (has TeamRoleDefinitionId)',
    COUNT(*)
FROM TeamMembers
WHERE TeamRoleDefinitionId IS NOT NULL
UNION ALL
SELECT
    'Not Migrated (NULL TeamRoleDefinitionId)',
    COUNT(*)
FROM TeamMembers
WHERE TeamRoleDefinitionId IS NULL;

-- Show unmapped records for manual review
IF EXISTS (SELECT 1 FROM TeamMembers WHERE TeamRoleDefinitionId IS NULL)
BEGIN
    PRINT 'WARNING: The following TeamMembers could not be migrated automatically:';
    SELECT
        tm.MemberId,
        tm.EmployeePayroll,
        t.TeamName,
        t.TeamType,
        tm.MemberRole_Old AS UnmappedRole
    FROM TeamMembers tm
    INNER JOIN Teams t ON tm.TeamId = t.TeamId
    WHERE tm.TeamRoleDefinitionId IS NULL;
END
ELSE
BEGIN
    PRINT 'SUCCESS: All TeamMembers successfully migrated!';
END

GO

PRINT 'Migration complete. Review results above.';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Verify all records migrated successfully';
PRINT '2. Fix any unmapped records manually if needed';
PRINT '3. Run the final migration to make TeamRoleDefinitionId NOT NULL and drop MemberRole_Old column';
GO
