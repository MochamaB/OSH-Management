using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    /// <summary>
    /// Defines valid roles that team members can hold within specific team types
    /// </summary>
    public class TeamRoleDefinition
    {
        [Key]
        public int TeamRoleDefinitionId { get; set; }

        /// <summary>
        /// Foreign key to TeamTypeDefinition (NEW architecture)
        /// </summary>
        public int? TeamTypeDefinitionId { get; set; }

        /// <summary>
        /// The type of team this role applies to - DEPRECATED
        /// Use TeamTypeDefinition navigation property instead
        /// Examples: "OshCommittee", "Investigation", "RiskAssessment"
        /// </summary>
        [MaxLength(50)]
        public string? TeamType { get; set; }

        /// <summary>
        /// The name of the role
        /// Examples: "Chairperson", "Secretary", "Safety Representative"
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// Description of the role's responsibilities
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Whether members in this role have voting rights
        /// </summary>
        public bool RequiresVotingRights { get; set; } = true;

        /// <summary>
        /// Maximum number of team members that can hold this role in a single team
        /// NULL = unlimited
        /// </summary>
        public int? MaxOccurrences { get; set; }

        /// <summary>
        /// Minimum number of team members required to hold this role
        /// Used for validation (e.g., every OSH Committee must have 1 Chairperson)
        /// </summary>
        public int? MinOccurrences { get; set; }

        /// <summary>
        /// Required qualifications or experience for this role
        /// </summary>
        [MaxLength(500)]
        public string? RequiredQualifications { get; set; }

        /// <summary>
        /// Display order for UI dropdowns
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        // Role Classification (NEW - for statutory compliance)
        /// <summary>
        /// Whether this role represents employee interests (for OSH Committee compliance)
        /// </summary>
        public bool IsEmployeeRepresentative { get; set; } = false;

        /// <summary>
        /// Whether this role represents employer/management interests
        /// </summary>
        public bool IsEmployerRepresentative { get; set; } = false;

        /// <summary>
        /// Whether this is an ex-officio (non-voting advisory) role
        /// </summary>
        public bool IsExOfficio { get; set; } = false;

        /// <summary>
        /// Whether this role definition is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public TeamTypeDefinition? TeamTypeDefinition { get; set; }
        public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
    }
}
