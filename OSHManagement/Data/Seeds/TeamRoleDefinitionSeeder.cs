using OSHManagement.Models;

namespace OSHManagement.Data.Seeds
{
    /// <summary>
    /// Seeds predefined team role definitions for all team types
    /// </summary>
    public static class TeamRoleDefinitionSeeder
    {
        public static void SeedTeamRoleDefinitions(OshDbContext context)
        {
            // Check if already seeded
        //    if (context.TeamRoleDefinitions.Any())
       //     {
       //         return;
       //     }

            var roleDefinitions = new List<TeamRoleDefinition>
            {
                // ========== OSH COMMITTEE ROLES ==========
                new TeamRoleDefinition
                {
                    TeamType = "OSH Committee",
                    RoleName = "Chairperson",
                    Description = "Leads committee meetings, sets agenda, and ensures compliance with OSH Act requirements",
                    RequiresVotingRights = true,
                    MaxOccurrences = 1,
                    MinOccurrences = 1,
                    RequiredQualifications = "Management representative with authority to implement decisions",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamRoleDefinition
                {
                    TeamType = "OSH Committee",
                    RoleName = "Secretary",
                    Description = "Records minutes, maintains documentation, and handles committee correspondence",
                    RequiresVotingRights = true,
                    MaxOccurrences = 1,
                    MinOccurrences = 1,
                    RequiredQualifications = "Good documentation and organizational skills",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamRoleDefinition
                {
                    TeamType = "OSH Committee",
                    RoleName = "Safety Representative",
                    Description = "Worker representative responsible for workplace safety concerns and reporting hazards",
                    RequiresVotingRights = true,
                    MaxOccurrences = null,
                    MinOccurrences = 2,
                    RequiredQualifications = "Employee with at least 1 year experience in the workplace",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamRoleDefinition
                {
                    TeamType = "OSH Committee",
                    RoleName = "Management Representative",
                    Description = "Represents management interests and has authority to implement safety decisions",
                    RequiresVotingRights = true,
                    MaxOccurrences = null,
                    MinOccurrences = 2,
                    RequiredQualifications = "Management or supervisory position",
                    DisplayOrder = 4,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamRoleDefinition
                {
                    TeamType = "OSH Committee",
                    RoleName = "Worker Representative",
                    Description = "Represents worker concerns and participates in safety decision-making",
                    RequiresVotingRights = true,
                    MaxOccurrences = null,
                    MinOccurrences = null,
                    RequiredQualifications = "Active employee at the station",
                    DisplayOrder = 5,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamRoleDefinition
                {
                    TeamType = "OSH Committee",
                    RoleName = "Observer",
                    Description = "Attends meetings and provides input but does not have voting rights",
                    RequiresVotingRights = false,
                    MaxOccurrences = null,
                    MinOccurrences = null,
                    RequiredQualifications = null,
                    DisplayOrder = 6,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                // ========== RISK ASSESSMENT TEAM ROLES ==========
                new TeamRoleDefinition
                {
                    TeamType = "Risk Assessment",
                    RoleName = "Team Leader",
                    Description = "Leads risk assessment activities and coordinates team efforts",
                    RequiresVotingRights = true,
                    MaxOccurrences = 1,
                    MinOccurrences = 1,
                    RequiredQualifications = "Training in risk assessment methodology",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamRoleDefinition
                {
                    TeamType = "Risk Assessment",
                    RoleName = "Risk Assessor",
                    Description = "Conducts risk assessments and identifies hazards in the workplace",
                    RequiresVotingRights = true,
                    MaxOccurrences = null,
                    MinOccurrences = 2,
                    RequiredQualifications = "Risk assessment training or relevant experience",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamRoleDefinition
                {
                    TeamType = "Risk Assessment",
                    RoleName = "Subject Matter Expert",
                    Description = "Provides technical expertise for specific hazards or processes",
                    RequiresVotingRights = true,
                    MaxOccurrences = null,
                    MinOccurrences = null,
                    RequiredQualifications = "Technical expertise in relevant area",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamRoleDefinition
                {
                    TeamType = "Risk Assessment",
                    RoleName = "Documentation Officer",
                    Description = "Records findings and maintains risk assessment documentation",
                    RequiresVotingRights = true,
                    MaxOccurrences = 1,
                    MinOccurrences = null,
                    RequiredQualifications = "Good documentation and analytical skills",
                    DisplayOrder = 4,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamRoleDefinition
                {
                    TeamType = "Risk Assessment",
                    RoleName = "Reviewer",
                    Description = "Reviews and validates risk assessment findings",
                    RequiresVotingRights = false,
                    MaxOccurrences = null,
                    MinOccurrences = null,
                    RequiredQualifications = "Experience in risk assessment or safety management",
                    DisplayOrder = 5,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                // ========== INVESTIGATION TEAM ROLES ==========
                new TeamRoleDefinition
                {
                    TeamType = "Investigation",
                    RoleName = "Lead Investigator",
                    Description = "Leads incident investigation and coordinates investigation activities",
                    RequiresVotingRights = true,
                    MaxOccurrences = 1,
                    MinOccurrences = 1,
                    RequiredQualifications = "Training in incident investigation techniques",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamRoleDefinition
                {
                    TeamType = "Investigation",
                    RoleName = "Investigator",
                    Description = "Participates in incident investigation and evidence collection",
                    RequiresVotingRights = true,
                    MaxOccurrences = null,
                    MinOccurrences = 2,
                    RequiredQualifications = "Basic investigation training",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamRoleDefinition
                {
                    TeamType = "Investigation",
                    RoleName = "Technical Specialist",
                    Description = "Provides technical analysis for incident investigation",
                    RequiresVotingRights = true,
                    MaxOccurrences = null,
                    MinOccurrences = null,
                    RequiredQualifications = "Technical expertise relevant to incident type",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamRoleDefinition
                {
                    TeamType = "Investigation",
                    RoleName = "Witness Liaison",
                    Description = "Conducts witness interviews and maintains witness records",
                    RequiresVotingRights = true,
                    MaxOccurrences = null,
                    MinOccurrences = null,
                    RequiredQualifications = "Good interpersonal and communication skills",
                    DisplayOrder = 4,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamRoleDefinition
                {
                    TeamType = "Investigation",
                    RoleName = "Documentation Officer",
                    Description = "Records investigation findings and prepares investigation reports",
                    RequiresVotingRights = true,
                    MaxOccurrences = 1,
                    MinOccurrences = 1,
                    RequiredQualifications = "Strong documentation and report writing skills",
                    DisplayOrder = 5,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TeamRoleDefinition
                {
                    TeamType = "Investigation",
                    RoleName = "Observer",
                    Description = "Observes investigation process for oversight purposes",
                    RequiresVotingRights = false,
                    MaxOccurrences = null,
                    MinOccurrences = null,
                    RequiredQualifications = null,
                    DisplayOrder = 6,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

         //   context.TeamRoleDefinitions.AddRange(roleDefinitions);
            context.SaveChanges();
        }
    }
}
