using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class RiskAssessmentConfig
    {
        [Key]
        public int ConfigId { get; set; }

        public int TeamId { get; set; }

        [Required]
        [MaxLength(20)]
        public string AssessmentFrequency { get; set; } = string.Empty;

        public DateTime? LastAssessmentDate { get; set; }
        public DateTime? NextAssessmentDate { get; set; }

        [MaxLength(500)]
        public string? TeamQualifications { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Team Team { get; set; } = null!;
    }
}
