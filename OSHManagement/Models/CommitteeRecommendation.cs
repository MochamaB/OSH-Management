using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class CommitteeRecommendation
    {
        [Key]
        public int RecommendationId { get; set; }

        public int TeamId { get; set; }
        public int IssueId { get; set; }

        [Required]
        public string RecommendationDescription { get; set; } = string.Empty;

        public DateTime RecommendationDate { get; set; }
        public int RecommendedByMemberId { get; set; }

        [Required]
        [MaxLength(20)]
        public string ImplementationStatus { get; set; } = string.Empty;

        public DateTime? TargetCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Team Team { get; set; } = null!;
        public CommitteeIssue Issue { get; set; } = null!;
        public TeamMember RecommendedByMember { get; set; } = null!;
        public ICollection<CommitteeAction> Actions { get; set; } = new List<CommitteeAction>();
    }
}
