using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class Hazard
    {
        [Key]
        public int HazardId { get; set; }

        public int TeamId { get; set; }
        public int StationId { get; set; }
        public int? SectionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string HazardCategory { get; set; } = string.Empty;

        [Required]
        public string HazardDescription { get; set; } = string.Empty;

        public string? AffectedDescription { get; set; }
        public string? ImpactDescription { get; set; }
        public string? CircumstancesDescription { get; set; }
        public string? ExistingPrecautions { get; set; }

        public int SeverityLevel { get; set; }
        public int LikelihoodLevel { get; set; }
        public int RiskRating { get; set; }

        [Required]
        [MaxLength(10)]
        public string PriorityLevel { get; set; } = string.Empty;

        public DateTime IdentifiedDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Team Team { get; set; } = null!;
        public Station Station { get; set; } = null!;
        public Section? Section { get; set; }
        public ICollection<RiskMitigationPlan> MitigationPlans { get; set; } = new List<RiskMitigationPlan>();
    }
}
