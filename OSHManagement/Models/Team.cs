using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }

        public int StationId { get; set; }

        /// <summary>
        /// Foreign key to TeamTypeDefinition (NEW architecture)
        /// </summary>
        public int? TeamTypeDefinitionId { get; set; }

        /// <summary>
        /// Old TeamType string - DEPRECATED, kept for backward compatibility during migration
        /// Use TeamTypeDefinition navigation property instead
        /// </summary>
        [MaxLength(50)]
        public string? TeamType { get; set; }

        [Required]
        [MaxLength(100)]
        public string TeamName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? TeamDescription { get; set; }

        /// <summary>
        /// DEPRECATED: These constraints are now defined in TeamTypeDefinition
        /// Kept for backward compatibility during migration
        /// </summary>
        public int? RequiredMemberCount { get; set; }
        public int? MaxMemberCount { get; set; }
        public bool RequiresSectionRepresentation { get; set; }

        [Required]
        [MaxLength(20)]
        public string TeamStatus { get; set; } = string.Empty;

        public DateTime FormationDate { get; set; }
        public DateTime? DisbandDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Station Station { get; set; } = null!;
        public TeamTypeDefinition? TeamTypeDefinition { get; set; }
        public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
        public ICollection<Hazard> Hazards { get; set; } = new List<Hazard>();
        public ICollection<CommitteeIssue> CommitteeIssues { get; set; } = new List<CommitteeIssue>();
        public OshCommitteeConfig? OshCommitteeConfig { get; set; }
        public RiskAssessmentConfig? RiskAssessmentConfig { get; set; }
        public IncidentInvestigationConfig? IncidentInvestigationConfig { get; set; }
    }
}
