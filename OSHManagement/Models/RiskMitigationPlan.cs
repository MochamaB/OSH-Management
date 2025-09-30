using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class RiskMitigationPlan
    {
        [Key]
        public int MitigationId { get; set; }

        public int HazardId { get; set; }

        [Required]
        public string MitigationMeasures { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ImplementationPeriod { get; set; }

        [Required]
        [MaxLength(20)]
        public string ResponsiblePersonPayroll { get; set; } = string.Empty;

        public string? MonitoringSystem { get; set; }

        [Required]
        [MaxLength(20)]
        public string ImplementationStatus { get; set; } = string.Empty;

        public DateTime? TargetCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Hazard Hazard { get; set; } = null!;
    }
}
