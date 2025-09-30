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

        [Required]
        [MaxLength(50)]
        public string MemberRole { get; set; } = string.Empty;

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
        public Section? Section { get; set; }
        public ICollection<CommitteeIssue> RaisedIssues { get; set; } = new List<CommitteeIssue>();
        public ICollection<CommitteeRecommendation> Recommendations { get; set; } = new List<CommitteeRecommendation>();
    }
}
