# Team Type Definition - Complete Implementation Plan

## Overview
This document outlines the complete implementation plan to refactor the team management system from string-based TeamType to entity-based TeamTypeDefinition architecture.

---

## Table of Contents
1. [Architecture Changes](#architecture-changes)
2. [Implementation Phases](#implementation-phases)
3. [Database Migration Scripts](#database-migration-scripts)
4. [Post-Migration Tasks](#post-migration-tasks)
5. [Testing Plan](#testing-plan)
6. [Rollback Plan](#rollback-plan)

---

## Architecture Changes

### Before (Current State)
```
Team
├── TeamType (string) ❌ "OshCommittee", "RiskAssessment", etc.
├── RequiredMemberCount (nullable int) ❌
├── MaxMemberCount (nullable int) ❌
└── RequiresSectionRepresentation (bool) ❌

TeamRoleDefinition
├── TeamType (string) ❌ "OshCommittee"
└── RoleName, Description, etc.
```

### After (New Architecture)
```
TeamTypeDefinition (NEW - Template/Blueprint)
├── TeamTypeDefinitionId
├── TypeName: "OSH Committee"
├── TypeCode: "OshCommittee"
├── Compliance Rules (statutory ratios, quorum, etc.)
├── Member Constraints (min/max, section representation)
└── TeamRoleDefinitions (Collection of roles)
    ├── Chairperson (min: 1, max: 1, IsEmployeeRep: true)
    ├── Secretary (min: 1, max: 1)
    └── Employee Rep (min: 4, IsEmployeeRep: true)

Team (Actual team instances)
├── TeamId
├── TeamTypeDefinitionId (FK) ✅ Points to template
├── StationId
├── TeamName: "Chelal OSH Committee"
└── TeamMembers (Collection)
    ├── Employee 001 → Role: Chairperson
    ├── Employee 002 → Role: Secretary
    └── Employee 003 → Role: Employee Rep

TeamRoleDefinition
├── TeamRoleDefinitionId
├── TeamTypeDefinitionId (FK) ✅ Points to parent team type
├── RoleName: "Chairperson"
├── IsEmployeeRepresentative, IsEmployerRepresentative
└── MinOccurrences, MaxOccurrences

TeamMember (Simplified junction table)
├── MemberId
├── TeamId (FK)
├── EmployeePayroll (FK)
├── TeamRoleDefinitionId (FK)
└── Term fields (TermEndDate, TermNumber, IsElected, etc.)
```

**Key Benefits:**
- ✅ Multiple teams of same type per station (e.g., multiple Risk Assessment teams)
- ✅ Centralized rules and validation
- ✅ Easy to add custom team types
- ✅ Statutory compliance built-in
- ✅ No more string-based TeamType confusion

---

## Implementation Phases

### Phase 1: Database Schema Changes (Run in SSMS)
Execute SQL scripts in order to create new tables, migrate data, and add constraints.

**Duration:** 30-60 minutes
**Downtime Required:** No (additive changes)

### Phase 2: Code Changes (C# Models, Controllers, Views)
Update application code to use new architecture.

**Duration:** 2-3 days
**Downtime Required:** Yes (during deployment)

### Phase 3: Testing & Validation
Test all team management workflows.

**Duration:** 2-3 days
**Downtime Required:** No

---

## Database Migration Scripts

### Prerequisites
- [ ] Backup production database: `BACKUP DATABASE OSHManagement TO DISK = 'C:\Backups\OSHManagement_PreTeamTypeMigration.bak'`
- [ ] Verify backup integrity
- [ ] Test scripts on development environment first
- [ ] Schedule maintenance window if needed

---

### Script 1: Create TeamTypeDefinitions Table

**Purpose:** Create the new TeamTypeDefinition table to store team type templates.

**File:** Save as `Database/Migrations/01_Create_TeamTypeDefinitions_Table.sql`

```sql
-- =============================================================================
-- SCRIPT 1: Create TeamTypeDefinitions Table
-- =============================================================================
-- Creates the central table for team type templates with rules and constraints
-- =============================================================================

USE OSHManagement;
GO

PRINT '========================================';
PRINT 'SCRIPT 1: Creating TeamTypeDefinitions Table';
PRINT '========================================';
PRINT '';

-- Check if table already exists
IF OBJECT_ID('TeamTypeDefinitions', 'U') IS NOT NULL
BEGIN
    PRINT 'WARNING: TeamTypeDefinitions table already exists. Skipping creation.';
    PRINT '';
END
ELSE
BEGIN
    PRINT 'Creating TeamTypeDefinitions table...';

    CREATE TABLE TeamTypeDefinitions (
        TeamTypeDefinitionId INT IDENTITY(1,1) PRIMARY KEY,

        -- Basic Information
        TypeName NVARCHAR(50) NOT NULL,
        TypeCode NVARCHAR(30) NOT NULL UNIQUE,
        Description NVARCHAR(500) NULL,

        -- Team Limits
        MaxTeamsPerStation INT NULL,  -- NULL = unlimited, 1 = only one per station

        -- Statutory Compliance Requirements
        RequiresStatutoryCompliance BIT NOT NULL DEFAULT 0,
        RequiredEmployeeRepRatio DECIMAL(5,4) NULL,  -- e.g., 0.6667 for 2/3
        RequiredEmployerRepRatio DECIMAL(5,4) NULL,  -- e.g., 0.3333 for 1/3
        MinFemaleRatio DECIMAL(5,4) NULL,  -- e.g., 0.30 for 30%
        MinMaleRatio DECIMAL(5,4) NULL,

        -- Meeting Requirements
        MinMeetingsPerYear INT NULL,  -- e.g., 4 for quarterly
        QuorumPercentage DECIMAL(5,4) NULL,  -- e.g., 0.50 for 50%+

        -- Member Constraints
        MinMemberCount INT NULL,
        MaxMemberCount INT NULL,
        RequiresSectionRepresentation BIT NOT NULL DEFAULT 0,

        -- Term Management
        DefaultTermMonths INT NULL,  -- e.g., 24 for 2 years
        MaxConsecutiveTerms INT NULL,  -- e.g., 2

        -- System Flags
        IsActive BIT NOT NULL DEFAULT 1,
        IsSystemType BIT NOT NULL DEFAULT 0,  -- TRUE for OSH, Risk, Investigation (protected from deletion)

        -- Audit
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,

        -- Indexes
        INDEX IX_TeamTypeDefinitions_TypeCode (TypeCode),
        INDEX IX_TeamTypeDefinitions_IsActive (IsActive)
    );

    PRINT '✓ TeamTypeDefinitions table created successfully.';
    PRINT '';

    -- Show table structure
    PRINT 'Table structure:';
    EXEC sp_help 'TeamTypeDefinitions';
    PRINT '';
END

PRINT '========================================';
PRINT 'SCRIPT 1: COMPLETED';
PRINT '========================================';
PRINT '';
GO
```

**Expected Output:** Table created with 20+ columns for team type configuration.

---

### Script 2: Add Foreign Key Columns

**Purpose:** Add FK columns to existing tables without breaking current functionality.

**File:** Save as `Database/Migrations/02_Add_Foreign_Key_Columns.sql`

```sql
-- =============================================================================
-- SCRIPT 2: Add Foreign Key Columns
-- =============================================================================
-- Adds TeamTypeDefinitionId to Teams and TeamRoleDefinitions
-- Keeps old columns temporarily for safe migration
-- =============================================================================

USE OSHManagement;
GO

PRINT '========================================';
PRINT 'SCRIPT 2: Adding Foreign Key Columns';
PRINT '========================================';
PRINT '';

-- ========================================
-- ADD COLUMN TO TEAMS TABLE
-- ========================================
PRINT 'Checking Teams table...';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Teams') AND name = 'TeamTypeDefinitionId')
BEGIN
    PRINT '  Adding TeamTypeDefinitionId column to Teams...';
    ALTER TABLE Teams ADD TeamTypeDefinitionId INT NULL;
    PRINT '  ✓ TeamTypeDefinitionId column added to Teams.';
END
ELSE
BEGIN
    PRINT '  TeamTypeDefinitionId column already exists in Teams. Skipping.';
END

PRINT '';

-- ========================================
-- ADD COLUMN TO TEAMROLEDEFINITIONS TABLE
-- ========================================
PRINT 'Checking TeamRoleDefinitions table...';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamRoleDefinitions') AND name = 'TeamTypeDefinitionId')
BEGIN
    PRINT '  Adding TeamTypeDefinitionId column to TeamRoleDefinitions...';
    ALTER TABLE TeamRoleDefinitions ADD TeamTypeDefinitionId INT NULL;
    PRINT '  ✓ TeamTypeDefinitionId column added to TeamRoleDefinitions.';
END
ELSE
BEGIN
    PRINT '  TeamTypeDefinitionId column already exists. Skipping.';
END

PRINT '';

-- ========================================
-- ADD CLASSIFICATION COLUMNS TO TEAMROLEDEFINITIONS
-- ========================================
PRINT 'Adding role classification columns to TeamRoleDefinitions...';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamRoleDefinitions') AND name = 'IsEmployeeRepresentative')
BEGIN
    ALTER TABLE TeamRoleDefinitions ADD IsEmployeeRepresentative BIT NOT NULL DEFAULT 0;
    PRINT '  ✓ IsEmployeeRepresentative column added.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamRoleDefinitions') AND name = 'IsEmployerRepresentative')
BEGIN
    ALTER TABLE TeamRoleDefinitions ADD IsEmployerRepresentative BIT NOT NULL DEFAULT 0;
    PRINT '  ✓ IsEmployerRepresentative column added.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamRoleDefinitions') AND name = 'IsExOfficio')
BEGIN
    ALTER TABLE TeamRoleDefinitions ADD IsExOfficio BIT NOT NULL DEFAULT 0;
    PRINT '  ✓ IsExOfficio column added.';
END

PRINT '';

-- ========================================
-- ADD TERM MANAGEMENT COLUMNS TO TEAMMEMBERS
-- ========================================
PRINT 'Adding term management columns to TeamMembers...';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamMembers') AND name = 'TermEndDate')
BEGIN
    ALTER TABLE TeamMembers ADD TermEndDate DATETIME2 NULL;
    PRINT '  ✓ TermEndDate column added.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamMembers') AND name = 'TermNumber')
BEGIN
    ALTER TABLE TeamMembers ADD TermNumber INT NOT NULL DEFAULT 1;
    PRINT '  ✓ TermNumber column added.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamMembers') AND name = 'IsElected')
BEGIN
    ALTER TABLE TeamMembers ADD IsElected BIT NOT NULL DEFAULT 0;
    PRINT '  ✓ IsElected column added.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamMembers') AND name = 'ElectionReference')
BEGIN
    ALTER TABLE TeamMembers ADD ElectionReference NVARCHAR(100) NULL;
    PRINT '  ✓ ElectionReference column added.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamMembers') AND name = 'AppointmentLetterRef')
BEGIN
    ALTER TABLE TeamMembers ADD AppointmentLetterRef NVARCHAR(100) NULL;
    PRINT '  ✓ AppointmentLetterRef column added.';
END

PRINT '';

PRINT '========================================';
PRINT 'SCRIPT 2: COMPLETED';
PRINT '========================================';
PRINT '';
PRINT 'Summary:';
PRINT '  - Teams.TeamTypeDefinitionId: Ready';
PRINT '  - TeamRoleDefinitions.TeamTypeDefinitionId: Ready';
PRINT '  - Role classification columns: Added';
PRINT '  - Term management columns: Added';
PRINT '';
GO
```

**Expected Output:** New columns added, no data loss.

---

### Script 3: Seed Standard Team Types

**Purpose:** Create standard team type templates (OSH Committee, Risk Assessment, Investigation).

**File:** Save as `Database/Migrations/03_Seed_TeamTypeDefinitions.sql`

```sql
-- =============================================================================
-- SCRIPT 3: Seed Standard Team Type Definitions
-- =============================================================================
-- Seeds OSH Committee, Risk Assessment, Investigation, and Custom team types
-- =============================================================================

USE OSHManagement;
GO

PRINT '========================================';
PRINT 'SCRIPT 3: Seeding Team Type Definitions';
PRINT '========================================';
PRINT '';

-- ========================================
-- 1. OSH COMMITTEE
-- ========================================
PRINT '1. Creating OSH Committee team type...';

IF NOT EXISTS (SELECT 1 FROM TeamTypeDefinitions WHERE TypeCode = 'OshCommittee')
BEGIN
    INSERT INTO TeamTypeDefinitions (
        TypeName,
        TypeCode,
        Description,
        MaxTeamsPerStation,
        RequiresStatutoryCompliance,
        RequiredEmployeeRepRatio,
        RequiredEmployerRepRatio,
        MinFemaleRatio,
        MinMaleRatio,
        MinMeetingsPerYear,
        QuorumPercentage,
        MinMemberCount,
        MaxMemberCount,
        RequiresSectionRepresentation,
        DefaultTermMonths,
        MaxConsecutiveTerms,
        IsActive,
        IsSystemType
    ) VALUES (
        'OSH Committee',
        'OshCommittee',
        'Statutory OSH Committee as per OSHA 2007 and WSHA 2007. Responsible for workplace safety oversight, inspections, and worker consultation.',
        1,  -- Only 1 OSH Committee per station
        1,  -- Requires statutory compliance
        0.6667,  -- 2/3 employee representatives
        0.3333,  -- 1/3 employer representatives
        0.30,  -- Minimum 30% female
        0.30,  -- Minimum 30% male
        4,  -- Quarterly meetings (4 per year)
        0.50,  -- 50% quorum
        7,  -- Minimum 7 members (recommended)
        15,  -- Maximum 15 members (recommended)
        1,  -- Requires section representation
        24,  -- 2-year term
        2,  -- Maximum 2 consecutive terms
        1,  -- Active
        1   -- System type (cannot be deleted)
    );

    DECLARE @OshCommitteeId INT = SCOPE_IDENTITY();
    PRINT '   ✓ OSH Committee created (ID: ' + CAST(@OshCommitteeId AS VARCHAR(10)) + ')';
END
ELSE
BEGIN
    PRINT '   ⚠ OSH Committee already exists. Skipping.';
END

PRINT '';

-- ========================================
-- 2. RISK ASSESSMENT TEAM
-- ========================================
PRINT '2. Creating Risk Assessment Team type...';

IF NOT EXISTS (SELECT 1 FROM TeamTypeDefinitions WHERE TypeCode = 'RiskAssessment')
BEGIN
    INSERT INTO TeamTypeDefinitions (
        TypeName,
        TypeCode,
        Description,
        MaxTeamsPerStation,
        RequiresStatutoryCompliance,
        MinMeetingsPerYear,
        MinMemberCount,
        MaxMemberCount,
        RequiresSectionRepresentation,
        IsActive,
        IsSystemType
    ) VALUES (
        'Risk Assessment Team',
        'RiskAssessment',
        'Technical team responsible for identifying, assessing, and controlling workplace hazards and risks.',
        NULL,  -- Unlimited - can have multiple risk assessment teams
        0,  -- No statutory compliance requirements
        NULL,  -- No fixed meeting requirement
        3,  -- Minimum 3 members
        10,  -- Maximum 10 members
        0,  -- Section representation not mandatory
        1,  -- Active
        1   -- System type
    );

    DECLARE @RiskAssessmentId INT = SCOPE_IDENTITY();
    PRINT '   ✓ Risk Assessment Team created (ID: ' + CAST(@RiskAssessmentId AS VARCHAR(10)) + ')';
END
ELSE
BEGIN
    PRINT '   ⚠ Risk Assessment Team already exists. Skipping.';
END

PRINT '';

-- ========================================
-- 3. INVESTIGATION TEAM
-- ========================================
PRINT '3. Creating Investigation Team type...';

IF NOT EXISTS (SELECT 1 FROM TeamTypeDefinitions WHERE TypeCode = 'Investigation')
BEGIN
    INSERT INTO TeamTypeDefinitions (
        TypeName,
        TypeCode,
        Description,
        MaxTeamsPerStation,
        RequiresStatutoryCompliance,
        MinMemberCount,
        MaxMemberCount,
        IsActive,
        IsSystemType
    ) VALUES (
        'Investigation Team',
        'Investigation',
        'Incident investigation team responsible for root cause analysis and corrective action recommendations.',
        NULL,  -- Unlimited - can have multiple investigation teams
        0,  -- No statutory requirements
        3,  -- Minimum 3 members
        8,  -- Maximum 8 members
        1,  -- Active
        1   -- System type
    );

    DECLARE @InvestigationId INT = SCOPE_IDENTITY();
    PRINT '   ✓ Investigation Team created (ID: ' + CAST(@InvestigationId AS VARCHAR(10)) + ')';
END
ELSE
BEGIN
    PRINT '   ⚠ Investigation Team already exists. Skipping.';
END

PRINT '';

-- ========================================
-- 4. CUSTOM TEAM TYPE (Template)
-- ========================================
PRINT '4. Creating Custom Team type template...';

IF NOT EXISTS (SELECT 1 FROM TeamTypeDefinitions WHERE TypeCode = 'Custom')
BEGIN
    INSERT INTO TeamTypeDefinitions (
        TypeName,
        TypeCode,
        Description,
        MaxTeamsPerStation,
        RequiresStatutoryCompliance,
        IsActive,
        IsSystemType
    ) VALUES (
        'Custom Team',
        'Custom',
        'User-defined custom team type for specialized purposes. Can be modified or deleted.',
        NULL,  -- Unlimited
        0,  -- No statutory requirements
        1,  -- Active
        0   -- NOT a system type (users can modify/delete)
    );

    DECLARE @CustomId INT = SCOPE_IDENTITY();
    PRINT '   ✓ Custom Team created (ID: ' + CAST(@CustomId AS VARCHAR(10)) + ')';
END
ELSE
BEGIN
    PRINT '   ⚠ Custom Team already exists. Skipping.';
END

PRINT '';

PRINT '========================================';
PRINT 'SCRIPT 3: COMPLETED';
PRINT '========================================';
PRINT '';
PRINT 'Summary - Team Types Created:';
SELECT
    TeamTypeDefinitionId AS ID,
    TypeName,
    TypeCode,
    MaxTeamsPerStation AS MaxPerStation,
    RequiresStatutoryCompliance AS Statutory,
    IsSystemType AS SystemType
FROM TeamTypeDefinitions
ORDER BY TeamTypeDefinitionId;
PRINT '';
GO
```

**Expected Output:** 4 team types created (OSH Committee, Risk Assessment, Investigation, Custom).

---

### Script 4: Seed Role Definitions

**Purpose:** Create role definitions for each team type.

**File:** Save as `Database/Migrations/04_Seed_TeamRoleDefinitions.sql`

```sql
-- =============================================================================
-- SCRIPT 4: Seed Team Role Definitions
-- =============================================================================
-- Creates role definitions for each team type
-- =============================================================================

USE OSHManagement;
GO

PRINT '========================================';
PRINT 'SCRIPT 4: Seeding Team Role Definitions';
PRINT '========================================';
PRINT '';

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
        TeamTypeDefinitionId, RoleName, Description,
        IsEmployeeRepresentative, IsEmployerRepresentative, IsExOfficio,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive
    ) VALUES (
        @OshCommitteeId, 'Chairperson',
        'Leads committee meetings, sets agendas, and ensures effective functioning. Must be elected from employee representatives.',
        1, 0, 0,  -- Employee representative
        1, 1, 1,  -- Voting rights, Min: 1, Max: 1
        'Leadership skills, OSH training preferred',
        1, 1
    );
    PRINT '   ✓ Chairperson';
END

-- Vice Chairperson
IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @OshCommitteeId AND RoleName = 'Vice Chairperson')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, RoleName, Description,
        IsEmployeeRepresentative, IsEmployerRepresentative, IsExOfficio,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        DisplayOrder, IsActive
    ) VALUES (
        @OshCommitteeId, 'Vice Chairperson',
        'Supports chairperson and acts in their absence.',
        0, 0, 0,
        1, 0, 1,  -- Optional, Max: 1
        2, 1
    );
    PRINT '   ✓ Vice Chairperson';
END

-- Secretary
IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @OshCommitteeId AND RoleName = 'Secretary')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, RoleName, Description,
        IsEmployeeRepresentative, IsEmployerRepresentative, IsExOfficio,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive
    ) VALUES (
        @OshCommitteeId, 'Secretary',
        'Records minutes, maintains documentation, handles correspondence.',
        0, 0, 0,
        1, 1, 1,  -- Required, Max: 1
        'Good writing and organizational skills',
        3, 1
    );
    PRINT '   ✓ Secretary';
END

-- Employee Representatives
IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @OshCommitteeId AND RoleName = 'Employee Representative')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, RoleName, Description,
        IsEmployeeRepresentative, IsEmployerRepresentative, IsExOfficio,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        DisplayOrder, IsActive
    ) VALUES (
        @OshCommitteeId, 'Employee Representative',
        'Elected representatives from workforce. Must constitute at least 2/3 of committee.',
        1, 0, 0,  -- Employee representative
        1, 4, NULL,  -- Min: 4 (to meet 2/3 ratio), No max
        4, 1
    );
    PRINT '   ✓ Employee Representative';
END

-- Employer Representatives
IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @OshCommitteeId AND RoleName = 'Employer Representative')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, RoleName, Description,
        IsEmployeeRepresentative, IsEmployerRepresentative, IsExOfficio,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        DisplayOrder, IsActive
    ) VALUES (
        @OshCommitteeId, 'Employer Representative',
        'Management-appointed representatives. Must constitute approximately 1/3 of committee.',
        0, 1, 0,  -- Employer representative
        1, 2, NULL,  -- Min: 2, No max
        5, 1
    );
    PRINT '   ✓ Employer Representative';
END

-- Safety Officer (Ex-Officio)
IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @OshCommitteeId AND RoleName = 'Safety Officer')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, RoleName, Description,
        IsEmployeeRepresentative, IsEmployerRepresentative, IsExOfficio,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive
    ) VALUES (
        @OshCommitteeId, 'Safety Officer',
        'Technical advisor to the committee. Non-voting member.',
        0, 0, 1,  -- Ex-officio
        0, 0, 1,  -- Non-voting, Optional, Max: 1
        'OSH professional qualification (Level 2 or higher)',
        6, 1
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
        TeamTypeDefinitionId, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive
    ) VALUES
    (
        @RiskAssessmentId, 'Team Leader',
        'Coordinates risk assessment activities and finalizes reports.',
        1, 1, 1,
        'Risk assessment training, technical expertise',
        1, 1
    );
    PRINT '   ✓ Team Leader';
END

IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @RiskAssessmentId AND RoleName = 'Risk Assessor')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive
    ) VALUES
    (
        @RiskAssessmentId, 'Risk Assessor',
        'Conducts hazard identification and risk analysis.',
        1, 2, NULL,
        'Risk assessment competency',
        2, 1
    );
    PRINT '   ✓ Risk Assessor';
END

IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @RiskAssessmentId AND RoleName = 'Technical Expert')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive
    ) VALUES
    (
        @RiskAssessmentId, 'Technical Expert',
        'Provides specialized technical input (machinery, chemical, ergonomic, etc.).',
        1, 0, NULL,
        'Technical expertise in specific domain',
        3, 1
    );
    PRINT '   ✓ Technical Expert';
END

IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @RiskAssessmentId AND RoleName = 'Worker Representative')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive
    ) VALUES
    (
        @RiskAssessmentId, 'Worker Representative',
        'Represents workers from assessed areas, provides practical insights.',
        1, 1, NULL,
        'Familiarity with work processes',
        4, 1
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
        TeamTypeDefinitionId, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive
    ) VALUES
    (
        @InvestigationId, 'Lead Investigator',
        'Leads incident investigation and root cause analysis.',
        1, 1, 1,
        'Incident investigation training (e.g., ICAM, TapRooT, 5-Why)',
        1, 1
    );
    PRINT '   ✓ Lead Investigator';
END

IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @InvestigationId AND RoleName = 'Investigator')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive
    ) VALUES
    (
        @InvestigationId, 'Investigator',
        'Supports investigation process, gathers evidence, interviews witnesses.',
        1, 1, NULL,
        'Investigation methodology knowledge',
        2, 1
    );
    PRINT '   ✓ Investigator';
END

IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @InvestigationId AND RoleName = 'Technical Specialist')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive
    ) VALUES
    (
        @InvestigationId, 'Technical Specialist',
        'Provides technical analysis (engineering, medical, etc.).',
        1, 0, NULL,
        'Technical expertise relevant to incident type',
        3, 1
    );
    PRINT '   ✓ Technical Specialist';
END

IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @InvestigationId AND RoleName = 'Safety Representative')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        RequiredQualifications, DisplayOrder, IsActive
    ) VALUES
    (
        @InvestigationId, 'Safety Representative',
        'Ensures worker perspective and safety focus.',
        1, 1, NULL,
        'OSH Committee member or safety rep',
        4, 1
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
        TeamTypeDefinitionId, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        DisplayOrder, IsActive
    ) VALUES
    (
        @CustomId, 'Team Captain',
        'Leads the team activities.',
        1, 0, 1,
        1, 1
    );
    PRINT '   ✓ Team Captain';
END

IF NOT EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId = @CustomId AND RoleName = 'Member')
BEGIN
    INSERT INTO TeamRoleDefinitions (
        TeamTypeDefinitionId, RoleName, Description,
        RequiresVotingRights, MinOccurrences, MaxOccurrences,
        DisplayOrder, IsActive
    ) VALUES
    (
        @CustomId, 'Member',
        'General team member.',
        1, 1, NULL,
        2, 1
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
GROUP BY ttd.TypeName, ttd.TeamTypeDefinitionId
ORDER BY ttd.TeamTypeDefinitionId;
PRINT '';
GO
```

**Expected Output:** 16 roles created across 4 team types.

---

### Script 5: Migrate Existing Data

**Purpose:** Map existing teams and roles to new structure.

**File:** Save as `Database/Migrations/05_Migrate_Existing_Data.sql`

```sql
-- =============================================================================
-- SCRIPT 5: Migrate Existing Data
-- =============================================================================
-- Migrates existing Teams and TeamRoleDefinitions to new structure
-- =============================================================================

USE OSHManagement;
GO

PRINT '========================================';
PRINT 'SCRIPT 5: Migrating Existing Data';
PRINT '========================================';
PRINT '';

-- ========================================
-- MIGRATE TEAMS
-- ========================================
PRINT '1. Migrating Teams to TeamTypeDefinitions...';

UPDATE t
SET t.TeamTypeDefinitionId = ttd.TeamTypeDefinitionId,
    t.UpdatedAt = GETUTCDATE()
FROM Teams t
INNER JOIN TeamTypeDefinitions ttd ON t.TeamType = ttd.TypeCode
WHERE t.TeamTypeDefinitionId IS NULL;

DECLARE @TeamsUpdated INT = @@ROWCOUNT;
PRINT '   ✓ Teams migrated: ' + CAST(@TeamsUpdated AS VARCHAR(10));

-- Check for unmigrated teams
IF EXISTS (SELECT 1 FROM Teams WHERE TeamTypeDefinitionId IS NULL)
BEGIN
    PRINT '';
    PRINT '   ⚠ WARNING: Some teams could not be migrated. Review TeamType values:';
    SELECT TeamId, TeamName, TeamType, StationId
    FROM Teams
    WHERE TeamTypeDefinitionId IS NULL;
    PRINT '';
END
ELSE
BEGIN
    PRINT '   ✓ All teams migrated successfully.';
END

PRINT '';

-- ========================================
-- MIGRATE TEAM ROLE DEFINITIONS
-- ========================================
PRINT '2. Migrating TeamRoleDefinitions to TeamTypeDefinitions...';

-- Check if there are existing role definitions with old structure
IF EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamType IS NOT NULL AND TeamTypeDefinitionId IS NULL)
BEGIN
    PRINT '   Found existing role definitions to migrate...';

    -- Update TeamRoleDefinitions.TeamTypeDefinitionId based on old TeamType string
    UPDATE trd
    SET trd.TeamTypeDefinitionId = ttd.TeamTypeDefinitionId,
        trd.UpdatedAt = GETUTCDATE()
    FROM TeamRoleDefinitions trd
    INNER JOIN TeamTypeDefinitions ttd ON trd.TeamType = ttd.TypeCode
    WHERE trd.TeamTypeDefinitionId IS NULL;

    DECLARE @RolesUpdated INT = @@ROWCOUNT;
    PRINT '   ✓ TeamRoleDefinitions migrated: ' + CAST(@RolesUpdated AS VARCHAR(10));

    -- Check for unmigrated role definitions
    IF EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId IS NULL)
    BEGIN
        PRINT '';
        PRINT '   ⚠ WARNING: Some role definitions could not be migrated:';
        SELECT TeamRoleDefinitionId, RoleName, TeamType
        FROM TeamRoleDefinitions
        WHERE TeamTypeDefinitionId IS NULL;
        PRINT '';
    END
    ELSE
    BEGIN
        PRINT '   ✓ All role definitions migrated successfully.';
    END
END
ELSE
BEGIN
    PRINT '   ℹ No old role definitions to migrate (already using new structure).';
END

PRINT '';

-- ========================================
-- CLASSIFY EXISTING ROLES
-- ========================================
PRINT '3. Classifying existing roles (employee/employer representatives)...';

DECLARE @ClassifiedCount INT = 0;

-- Mark roles as Employee Representatives
UPDATE TeamRoleDefinitions
SET IsEmployeeRepresentative = 1
WHERE RoleName IN (
    'Chairperson',
    'Employee Representative',
    'Worker Representative',
    'Section Representative',
    'Department Representative'
)
AND TeamTypeDefinitionId = (SELECT TeamTypeDefinitionId FROM TeamTypeDefinitions WHERE TypeCode = 'OshCommittee')
AND IsEmployeeRepresentative = 0;

SET @ClassifiedCount = @ClassifiedCount + @@ROWCOUNT;

-- Mark roles as Employer Representatives
UPDATE TeamRoleDefinitions
SET IsEmployerRepresentative = 1
WHERE RoleName IN (
    'Employer Representative',
    'Management Representative',
    'HR Representative'
)
AND TeamTypeDefinitionId = (SELECT TeamTypeDefinitionId FROM TeamTypeDefinitions WHERE TypeCode = 'OshCommittee')
AND IsEmployerRepresentative = 0;

SET @ClassifiedCount = @ClassifiedCount + @@ROWCOUNT;

-- Mark Safety Officer as Ex-Officio
UPDATE TeamRoleDefinitions
SET IsExOfficio = 1,
    RequiresVotingRights = 0
WHERE RoleName IN ('Safety Officer', 'OSH Officer')
AND TeamTypeDefinitionId = (SELECT TeamTypeDefinitionId FROM TeamTypeDefinitions WHERE TypeCode = 'OshCommittee')
AND IsExOfficio = 0;

SET @ClassifiedCount = @ClassifiedCount + @@ROWCOUNT;

PRINT '   ✓ Roles classified: ' + CAST(@ClassifiedCount AS VARCHAR(10));

PRINT '';

PRINT '========================================';
PRINT 'SCRIPT 5: COMPLETED';
PRINT '========================================';
PRINT '';
PRINT 'Migration Summary:';
PRINT '  - Teams migrated: ' + CAST(@TeamsUpdated AS VARCHAR(10));
PRINT '  - Roles classified: ' + CAST(@ClassifiedCount AS VARCHAR(10));
PRINT '';
GO
```

**Expected Output:** Existing teams and roles mapped to new structure.

---

### Script 6: Add Foreign Key Constraints

**Purpose:** Enforce referential integrity with FK constraints.

**File:** Save as `Database/Migrations/06_Add_Foreign_Key_Constraints.sql`

```sql
-- =============================================================================
-- SCRIPT 6: Add Foreign Key Constraints
-- =============================================================================
-- Creates foreign key relationships after data migration
-- =============================================================================

USE OSHManagement;
GO

PRINT '========================================';
PRINT 'SCRIPT 6: Adding Foreign Key Constraints';
PRINT '========================================';
PRINT '';

-- ========================================
-- VALIDATION CHECKS
-- ========================================
PRINT '1. Running validation checks...';

-- Check if all Teams have valid TeamTypeDefinitionId
IF EXISTS (SELECT 1 FROM Teams WHERE TeamTypeDefinitionId IS NULL)
BEGIN
    PRINT '   ❌ ERROR: Some Teams have NULL TeamTypeDefinitionId.';
    PRINT '   Cannot add FK constraint. Resolve these records first:';
    SELECT TeamId, TeamName, TeamType FROM Teams WHERE TeamTypeDefinitionId IS NULL;
    PRINT '';
    PRINT 'SCRIPT ABORTED. Fix data issues and re-run.';
    RETURN;
END
ELSE
BEGIN
    PRINT '   ✓ All Teams have valid TeamTypeDefinitionId';
END

-- Check if all TeamRoleDefinitions have valid TeamTypeDefinitionId
IF EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId IS NULL)
BEGIN
    PRINT '   ❌ ERROR: Some TeamRoleDefinitions have NULL TeamTypeDefinitionId.';
    PRINT '   Cannot add FK constraint. Resolve these records first:';
    SELECT TeamRoleDefinitionId, RoleName, TeamType FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId IS NULL;
    PRINT '';
    PRINT 'SCRIPT ABORTED. Fix data issues and re-run.';
    RETURN;
END
ELSE
BEGIN
    PRINT '   ✓ All TeamRoleDefinitions have valid TeamTypeDefinitionId';
END

PRINT '';

-- ========================================
-- ADD FK: TEAMS -> TEAMTYPEDEFINITIONS
-- ========================================
PRINT '2. Adding FK constraint: Teams -> TeamTypeDefinitions...';

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Teams_TeamTypeDefinitions')
BEGIN
    ALTER TABLE Teams
    ADD CONSTRAINT FK_Teams_TeamTypeDefinitions
    FOREIGN KEY (TeamTypeDefinitionId)
    REFERENCES TeamTypeDefinitions(TeamTypeDefinitionId);

    PRINT '   ✓ FK_Teams_TeamTypeDefinitions created.';
END
ELSE
BEGIN
    PRINT '   ℹ FK_Teams_TeamTypeDefinitions already exists.';
END

-- Add index for performance
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Teams_TeamTypeDefinitionId' AND object_id = OBJECT_ID('Teams'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Teams_TeamTypeDefinitionId
    ON Teams(TeamTypeDefinitionId);

    PRINT '   ✓ Index IX_Teams_TeamTypeDefinitionId created.';
END
ELSE
BEGIN
    PRINT '   ℹ Index IX_Teams_TeamTypeDefinitionId already exists.';
END

PRINT '';

-- ========================================
-- ADD FK: TEAMROLEDEFINITIONS -> TEAMTYPEDEFINITIONS
-- ========================================
PRINT '3. Adding FK constraint: TeamRoleDefinitions -> TeamTypeDefinitions...';

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TeamRoleDefinitions_TeamTypeDefinitions')
BEGIN
    ALTER TABLE TeamRoleDefinitions
    ADD CONSTRAINT FK_TeamRoleDefinitions_TeamTypeDefinitions
    FOREIGN KEY (TeamTypeDefinitionId)
    REFERENCES TeamTypeDefinitions(TeamTypeDefinitionId);

    PRINT '   ✓ FK_TeamRoleDefinitions_TeamTypeDefinitions created.';
END
ELSE
BEGIN
    PRINT '   ℹ FK_TeamRoleDefinitions_TeamTypeDefinitions already exists.';
END

-- Add index for performance
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TeamRoleDefinitions_TeamTypeDefinitionId' AND object_id = OBJECT_ID('TeamRoleDefinitions'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TeamRoleDefinitions_TeamTypeDefinitionId
    ON TeamRoleDefinitions(TeamTypeDefinitionId);

    PRINT '   ✓ Index IX_TeamRoleDefinitions_TeamTypeDefinitionId created.';
END
ELSE
BEGIN
    PRINT '   ℹ Index IX_TeamRoleDefinitions_TeamTypeDefinitionId already exists.';
END

PRINT '';

PRINT '========================================';
PRINT 'SCRIPT 6: COMPLETED';
PRINT '========================================';
PRINT '';
PRINT 'Foreign Key Constraints Summary:';
PRINT '  ✓ Teams -> TeamTypeDefinitions';
PRINT '  ✓ TeamRoleDefinitions -> TeamTypeDefinitions';
PRINT '  ✓ Indexes created for performance';
PRINT '';
GO
```

**Expected Output:** FK constraints added, referential integrity enforced.

---

### Script 7: Validation Queries

**Purpose:** Verify migration was successful.

**File:** Save as `Database/Migrations/07_Validation_Queries.sql`

```sql
-- =============================================================================
-- SCRIPT 7: Validation Queries
-- =============================================================================
-- Run these queries to verify the migration was successful
-- =============================================================================

USE OSHManagement;
GO

PRINT '========================================';
PRINT 'SCRIPT 7: VALIDATION QUERIES';
PRINT '========================================';
PRINT '';

-- ========================================
-- 1. CHECK TEAM TYPE DEFINITIONS
-- ========================================
PRINT '1. Team Type Definitions:';
PRINT '---';
SELECT
    TeamTypeDefinitionId AS ID,
    TypeName,
    TypeCode,
    MaxTeamsPerStation AS MaxPerStation,
    RequiresStatutoryCompliance AS Statutory,
    MinMemberCount AS MinMembers,
    MaxMemberCount AS MaxMembers,
    IsSystemType AS SystemType,
    IsActive AS Active
FROM TeamTypeDefinitions
ORDER BY TeamTypeDefinitionId;
PRINT '';

-- ========================================
-- 2. CHECK ROLE DEFINITIONS PER TEAM TYPE
-- ========================================
PRINT '2. Role Definitions per Team Type:';
PRINT '---';
SELECT
    ttd.TypeName,
    trd.RoleName,
    trd.MinOccurrences AS Min,
    trd.MaxOccurrences AS Max,
    CASE WHEN trd.IsEmployeeRepresentative = 1 THEN 'Y' ELSE '' END AS EmpRep,
    CASE WHEN trd.IsEmployerRepresentative = 1 THEN 'Y' ELSE '' END AS EmplRep,
    CASE WHEN trd.IsExOfficio = 1 THEN 'Y' ELSE '' END AS ExOff,
    CASE WHEN trd.RequiresVotingRights = 1 THEN 'Y' ELSE 'N' END AS Voting
FROM TeamTypeDefinitions ttd
LEFT JOIN TeamRoleDefinitions trd ON ttd.TeamTypeDefinitionId = trd.TeamTypeDefinitionId
WHERE trd.IsActive = 1
ORDER BY ttd.TypeName, trd.DisplayOrder;
PRINT '';

-- ========================================
-- 3. CHECK MIGRATED TEAMS
-- ========================================
PRINT '3. Migrated Teams:';
PRINT '---';
SELECT
    t.TeamId,
    t.TeamName,
    ttd.TypeName AS TeamType,
    s.StationName,
    t.TeamStatus,
    COUNT(tm.MemberId) AS MemberCount
FROM Teams t
INNER JOIN TeamTypeDefinitions ttd ON t.TeamTypeDefinitionId = ttd.TeamTypeDefinitionId
INNER JOIN Stations s ON t.StationId = s.StationId
LEFT JOIN TeamMembers tm ON t.TeamId = tm.TeamId AND tm.IsActive = 1
GROUP BY t.TeamId, t.TeamName, ttd.TypeName, s.StationName, t.TeamStatus, ttd.TeamTypeDefinitionId
ORDER BY ttd.TypeName, s.StationName;
PRINT '';

-- ========================================
-- 4. CHECK TEAM MEMBERS WITH ROLES
-- ========================================
PRINT '4. Team Members and Roles (Sample):';
PRINT '---';
SELECT TOP 20
    t.TeamName,
    ttd.TypeName AS TeamType,
    CONCAT(e.FirstName, ' ', e.LastName) AS EmployeeName,
    trd.RoleName,
    CASE WHEN tm.IsVotingMember = 1 THEN 'Y' ELSE 'N' END AS Voting,
    tm.AppointmentDate,
    CASE WHEN tm.IsActive = 1 THEN 'Active' ELSE 'Inactive' END AS Status
FROM TeamMembers tm
INNER JOIN Teams t ON tm.TeamId = t.TeamId
INNER JOIN TeamTypeDefinitions ttd ON t.TeamTypeDefinitionId = ttd.TeamTypeDefinitionId
INNER JOIN Employees e ON tm.EmployeePayroll = e.PayrollNo
LEFT JOIN TeamRoleDefinitions trd ON tm.TeamRoleDefinitionId = trd.TeamRoleDefinitionId
WHERE tm.IsActive = 1
ORDER BY t.TeamName, trd.DisplayOrder;
PRINT '';

-- ========================================
-- 5. REFERENTIAL INTEGRITY CHECKS
-- ========================================
PRINT '5. Referential Integrity Checks:';
PRINT '---';

-- Check orphaned teams
IF EXISTS (SELECT 1 FROM Teams WHERE TeamTypeDefinitionId IS NULL)
BEGIN
    PRINT '❌ ERROR: Found teams with NULL TeamTypeDefinitionId:';
    SELECT TeamId, TeamName FROM Teams WHERE TeamTypeDefinitionId IS NULL;
END
ELSE
BEGIN
    PRINT '✓ All teams have valid TeamTypeDefinitionId';
END

-- Check orphaned role definitions
IF EXISTS (SELECT 1 FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId IS NULL)
BEGIN
    PRINT '❌ ERROR: Found role definitions with NULL TeamTypeDefinitionId:';
    SELECT TeamRoleDefinitionId, RoleName FROM TeamRoleDefinitions WHERE TeamTypeDefinitionId IS NULL;
END
ELSE
BEGIN
    PRINT '✓ All role definitions have valid TeamTypeDefinitionId';
END

PRINT '';

-- ========================================
-- 6. OSH COMMITTEE COMPOSITION CHECK
-- ========================================
PRINT '6. OSH Committee Composition Check:';
PRINT '---';
SELECT
    t.TeamName,
    s.StationName,
    COUNT(CASE WHEN trd.IsEmployeeRepresentative = 1 THEN 1 END) AS EmployeeReps,
    COUNT(CASE WHEN trd.IsEmployerRepresentative = 1 THEN 1 END) AS EmployerReps,
    COUNT(CASE WHEN trd.IsExOfficio = 1 THEN 1 END) AS ExOfficio,
    COUNT(CASE WHEN tm.IsVotingMember = 1 THEN 1 END) AS VotingMembers,
    COUNT(tm.MemberId) AS TotalMembers,
    CAST(ROUND(
        CAST(COUNT(CASE WHEN trd.IsEmployeeRepresentative = 1 THEN 1 END) AS FLOAT) /
        NULLIF(COUNT(CASE WHEN tm.IsVotingMember = 1 THEN 1 END), 0) * 100,
    0) AS INT) AS EmpRepPercent,
    CAST(ROUND(
        CAST(COUNT(CASE WHEN trd.IsEmployerRepresentative = 1 THEN 1 END) AS FLOAT) /
        NULLIF(COUNT(CASE WHEN tm.IsVotingMember = 1 THEN 1 END), 0) * 100,
    0) AS INT) AS EmplRepPercent
FROM Teams t
INNER JOIN TeamTypeDefinitions ttd ON t.TeamTypeDefinitionId = ttd.TeamTypeDefinitionId
INNER JOIN Stations s ON t.StationId = s.StationId
LEFT JOIN TeamMembers tm ON t.TeamId = tm.TeamId AND tm.IsActive = 1
LEFT JOIN TeamRoleDefinitions trd ON tm.TeamRoleDefinitionId = trd.TeamRoleDefinitionId
WHERE ttd.TypeCode = 'OshCommittee'
AND t.TeamStatus = 'Active'
GROUP BY t.TeamName, s.StationName, t.TeamId
ORDER BY s.StationName;

PRINT '';
PRINT 'Note: OSH Committees should have ~67% Employee Reps, ~33% Employer Reps';
PRINT '';

PRINT '========================================';
PRINT 'SCRIPT 7: VALIDATION COMPLETE';
PRINT '========================================';
PRINT '';
GO
```

**Expected Output:** Comprehensive validation report showing all migrated data.

---

## Post-Migration Tasks

### Task 1: Update C# Models

**Files to create/update:**
1. `Models/TeamTypeDefinition.cs` (NEW)
2. `Models/Team.cs` (UPDATE - remove TeamType string, add TeamTypeDefinitionId FK)
3. `Models/TeamRoleDefinition.cs` (UPDATE - remove TeamType string, add TeamTypeDefinitionId FK)
4. `Models/TeamMember.cs` (UPDATE - add term fields)
5. `Data/OshDbContext.cs` (UPDATE - add TeamTypeDefinitions DbSet, configure relationships)

### Task 2: Create Validation Service

**File:** `Services/TeamValidationService.cs` (NEW)
- Validates team composition against TeamTypeDefinition rules
- Checks statutory compliance
- Validates role min/max occurrences

### Task 3: Update Controllers

**Files to update:**
1. `Controllers/TeamController.cs`
   - Load TeamTypeDefinitions instead of hardcoded types
   - Use validation service
   - Update Create/Edit logic
2. `Controllers/TeamRoleDefinitionController.cs`
   - Load TeamTypeDefinitions
   - Filter roles by TeamTypeDefinitionId

### Task 4: Update Views

**Files to update:**
1. `Views/Team/Index.cshtml`
2. `Views/Team/Create.cshtml`
3. `Views/Team/Edit.cshtml`
4. `Views/TeamRoleDefinition/Index.cshtml`
5. `Views/TeamRoleDefinition/Create.cshtml`

---

## Testing Plan

### 1. Database Testing
- [ ] Run all scripts on dev environment
- [ ] Verify data migration
- [ ] Check FK constraints
- [ ] Run validation queries

### 2. Application Testing
- [ ] Team creation (all types)
- [ ] Team editing
- [ ] Member addition/removal
- [ ] Role validation
- [ ] Statutory compliance checks

### 3. Edge Cases
- [ ] Multiple teams of same type per station
- [ ] Invalid role combinations
- [ ] Gender ratio warnings
- [ ] Term expiry scenarios

---

## Rollback Plan

**File:** Save as `Database/Migrations/99_Rollback_Script.sql`

```sql
-- =============================================================================
-- ROLLBACK SCRIPT (Emergency Use Only)
-- =============================================================================
-- WARNING: This will lose data entered after migration!
-- =============================================================================

USE OSHManagement;
GO

PRINT 'WARNING: Starting rollback process...';
PRINT '';

-- 1. Drop FK constraints
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Teams_TeamTypeDefinitions')
BEGIN
    ALTER TABLE Teams DROP CONSTRAINT FK_Teams_TeamTypeDefinitions;
    PRINT '✓ Dropped FK_Teams_TeamTypeDefinitions';
END

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TeamRoleDefinitions_TeamTypeDefinitions')
BEGIN
    ALTER TABLE TeamRoleDefinitions DROP CONSTRAINT FK_TeamRoleDefinitions_TeamTypeDefinitions;
    PRINT '✓ Dropped FK_TeamRoleDefinitions_TeamTypeDefinitions';
END

-- 2. Re-add old TeamType columns
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Teams') AND name = 'TeamType')
BEGIN
    ALTER TABLE Teams ADD TeamType NVARCHAR(50) NULL;

    -- Restore TeamType from TeamTypeDefinition
    UPDATE t
    SET t.TeamType = ttd.TypeCode
    FROM Teams t
    INNER JOIN TeamTypeDefinitions ttd ON t.TeamTypeDefinitionId = ttd.TeamTypeDefinitionId;

    PRINT '✓ Restored Teams.TeamType column';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeamRoleDefinitions') AND name = 'TeamType')
BEGIN
    ALTER TABLE TeamRoleDefinitions ADD TeamType NVARCHAR(50) NULL;

    -- Restore TeamType from TeamTypeDefinition
    UPDATE trd
    SET trd.TeamType = ttd.TypeCode
    FROM TeamRoleDefinitions trd
    INNER JOIN TeamTypeDefinitions ttd ON trd.TeamTypeDefinitionId = ttd.TeamTypeDefinitionId;

    PRINT '✓ Restored TeamRoleDefinitions.TeamType column';
END

-- 3. Drop new columns
ALTER TABLE Teams DROP COLUMN IF EXISTS TeamTypeDefinitionId;
ALTER TABLE TeamRoleDefinitions DROP COLUMN IF EXISTS TeamTypeDefinitionId;
ALTER TABLE TeamRoleDefinitions DROP COLUMN IF EXISTS IsEmployeeRepresentative;
ALTER TABLE TeamRoleDefinitions DROP COLUMN IF EXISTS IsEmployerRepresentative;
ALTER TABLE TeamRoleDefinitions DROP COLUMN IF EXISTS IsExOfficio;
ALTER TABLE TeamMembers DROP COLUMN IF EXISTS TermEndDate;
ALTER TABLE TeamMembers DROP COLUMN IF EXISTS TermNumber;
ALTER TABLE TeamMembers DROP COLUMN IF EXISTS IsElected;
ALTER TABLE TeamMembers DROP COLUMN IF EXISTS ElectionReference;
ALTER TABLE TeamMembers DROP COLUMN IF EXISTS AppointmentLetterRef;

PRINT '✓ Dropped new columns';

-- 4. Drop new table
DROP TABLE IF EXISTS TeamTypeDefinitions;

PRINT '✓ Dropped TeamTypeDefinitions table';
PRINT '';
PRINT 'Rollback completed. Database reverted to pre-migration state.';
GO
```

---

## Execution Checklist

### Pre-Execution
- [ ] Backup database: `BACKUP DATABASE OSHManagement TO DISK = 'C:\Backups\OSHManagement_PreMigration.bak'`
- [ ] Test scripts on dev environment
- [ ] Review validation queries
- [ ] Schedule maintenance window (if needed)

### Execution (in SSMS)
1. [ ] Execute Script 1: Create TeamTypeDefinitions Table
2. [ ] Execute Script 2: Add Foreign Key Columns
3. [ ] Execute Script 3: Seed Standard Team Types
4. [ ] Execute Script 4: Seed Role Definitions
5. [ ] Execute Script 5: Migrate Existing Data
6. [ ] Execute Script 6: Add Foreign Key Constraints
7. [ ] Execute Script 7: Validation Queries (verify results)

### Post-Execution
- [ ] Review validation output
- [ ] Verify no errors in SSMS messages
- [ ] Check team counts match expectations
- [ ] Update C# code (models, controllers, views)
- [ ] Deploy application
- [ ] Test all team management workflows
- [ ] Monitor application logs

---

## Support

For issues or questions:
1. Review validation queries output
2. Check SSMS messages for errors
3. Refer to rollback plan if critical issues found
4. Contact development team

---

**Document Version:** 1.0
**Created:** 2025-10-22
**Status:** Ready for Execution
