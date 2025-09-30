using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class CommitteeIssue
    {
        [Key]
        public int IssueId { get; set; }

        public int TeamId { get; set; }

        [Required]
        [MaxLength(200)]
        public string IssueTitle { get; set; } = string.Empty;

        public string? IssueDescription { get; set; }

        [Required]
        [MaxLength(50)]
        public string IssueCategory { get; set; } = string.Empty;

        public DateTime RaisedDate { get; set; }
        public int RaisedByMemberId { get; set; }

        [MaxLength(500)]
        public string? PreviousOutcome { get; set; }

        [Required]
        [MaxLength(20)]
        public string IssueStatus { get; set; } = string.Empty;

        public DateTime? ResolutionDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Team Team { get; set; } = null!;
        public TeamMember RaisedByMember { get; set; } = null!;
        public ICollection<CommitteeRecommendation> Recommendations { get; set; } = new List<CommitteeRecommendation>();
    }
}
