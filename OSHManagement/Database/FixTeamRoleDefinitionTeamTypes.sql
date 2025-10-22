-- =============================================================================
-- Fix TeamRoleDefinition TeamType Values - Use Enum Values
-- =============================================================================
-- This script updates TeamRoleDefinitions to use enum values instead of display names
-- This ensures consistency with Teams.TeamType which stores enum values
-- =============================================================================

USE OSHManagement;
GO

PRINT 'Updating TeamRoleDefinition TeamType values to use enum values...';
PRINT '';

-- Show current values
PRINT 'BEFORE - Current TeamType values:';
SELECT DISTINCT TeamType, COUNT(*) as RoleCount
FROM TeamRoleDefinitions
GROUP BY TeamType
ORDER BY TeamType;
PRINT '';

-- Update "OSH Committee" -> "OshCommittee"
UPDATE TeamRoleDefinitions
SET TeamType = 'OshCommittee',
    UpdatedAt = GETUTCDATE()
WHERE TeamType = 'OSH Committee';
PRINT '  Updated "OSH Committee" -> "OshCommittee": ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' roles';

-- Update "Risk Assessment" -> "RiskAssessment"
UPDATE TeamRoleDefinitions
SET TeamType = 'RiskAssessment',
    UpdatedAt = GETUTCDATE()
WHERE TeamType = 'Risk Assessment';
PRINT '  Updated "Risk Assessment" -> "RiskAssessment": ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' roles';

-- Update "Investigation" -> "Investigation" (already correct)
-- No change needed for Investigation

-- Update "Custom" -> "Custom" (already correct if exists)
-- No change needed for Custom

PRINT '';
PRINT 'AFTER - Updated TeamType values:';
SELECT DISTINCT TeamType, COUNT(*) as RoleCount
FROM TeamRoleDefinitions
GROUP BY TeamType
ORDER BY TeamType;

PRINT '';
PRINT 'TeamRoleDefinition TeamType values fixed successfully!';
PRINT 'All TeamType values now use enum format (OshCommittee, RiskAssessment, Investigation, Custom)';
GO
