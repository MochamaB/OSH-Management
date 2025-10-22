using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    /// <summary>
    /// Defines a team type template with rules, constraints, and role definitions.
    /// Acts as a blueprint for creating actual teams.
    /// </summary>
    public class TeamTypeDefinition
    {
        [Key]
        public int TeamTypeDefinitionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string TypeCode { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        // Team Limits
        public int? MaxTeamsPerStation { get; set; }

        // Statutory Compliance Requirements
        public bool RequiresStatutoryCompliance { get; set; }
        public decimal? RequiredEmployeeRepRatio { get; set; }
        public decimal? RequiredEmployerRepRatio { get; set; }
        public decimal? MinFemaleRatio { get; set; }
        public decimal? MinMaleRatio { get; set; }

        // Meeting Requirements
        public int? MinMeetingsPerYear { get; set; }
        public decimal? QuorumPercentage { get; set; }

        // Member Constraints
        public int? MinMemberCount { get; set; }
        public int? MaxMemberCount { get; set; }
        public bool RequiresSectionRepresentation { get; set; }

        // Term Management
        public int? DefaultTermMonths { get; set; }
        public int? MaxConsecutiveTerms { get; set; }

        // System Flags
        public bool IsActive { get; set; } = true;
        public bool IsSystemType { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public ICollection<TeamRoleDefinition> TeamRoleDefinitions { get; set; } = new List<TeamRoleDefinition>();
        public ICollection<Team> Teams { get; set; } = new List<Team>();
    }
}
