using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class CommitteeAction
    {
        [Key]
        public int ActionId { get; set; }

        public int TeamId { get; set; }
        public int RecommendationId { get; set; }

        [Required]
        public string ActionDescription { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string AssignedToPayroll { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string ActionStatus { get; set; } = string.Empty;

        public DateTime? CompletionDate { get; set; }

        [MaxLength(500)]
        public string? CompletionNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Team Team { get; set; } = null!;
        public CommitteeRecommendation Recommendation { get; set; } = null!;
    }
}
