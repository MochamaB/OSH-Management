using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class TeamMember
    {
        [Key]
        public int MemberId { get; set; }

        public int TeamId { get; set; }

        [Required]
        [MaxLength(20)]
        public string EmployeePayroll { get; set; } = string.Empty;

        /// <summary>
        /// Foreign key to TeamRoleDefinition
        /// Defines what role this member holds in the team
        /// </summary>
        public int? TeamRoleDefinitionId { get; set; }

        public int? SectionId { get; set; }

        [MaxLength(50)]
        public string? EducationLevel { get; set; }

        [MaxLength(200)]
        public string? RelevantExperience { get; set; }

        public DateTime AppointmentDate { get; set; }
        public DateTime? DepartureDate { get; set; }

        public bool IsVotingMember { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Team Team { get; set; } = null!;
        public Employee Employee { get; set; } = null!;
        public TeamRoleDefinition? TeamRoleDefinition { get; set; }
        public Section? Section { get; set; }
        public ICollection<CommitteeIssue> RaisedIssues { get; set; } = new List<CommitteeIssue>();
        public ICollection<CommitteeRecommendation> Recommendations { get; set; } = new List<CommitteeRecommendation>();
    }
}
