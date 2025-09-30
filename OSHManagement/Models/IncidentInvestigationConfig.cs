using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class IncidentInvestigationConfig
    {
        [Key]
        public int ConfigId { get; set; }

        public int TeamId { get; set; }

        [MaxLength(500)]
        public string? InvestigationScope { get; set; }

        [MaxLength(500)]
        public string? TeamExpertise { get; set; }

        public int ResponseTimeHours { get; set; }

        [Required]
        [MaxLength(20)]
        public string EscalationThreshold { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Team Team { get; set; } = null!;
    }
}
