using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class IncidentInvestigation
    {
        [Key]
        public int InvestigationId { get; set; }

        public int IncidentId { get; set; }
        public int InvestigationTeamId { get; set; }

        public DateTime InvestigationStartDate { get; set; }
        public DateTime? InvestigationCompletionDate { get; set; }

        [MaxLength(100)]
        public string? InvestigationMethod { get; set; }

        public string? EvidenceCollected { get; set; }
        public string? WitnessesInterviewed { get; set; }
        public string? ImmediateCauses { get; set; }
        public string? RootCauses { get; set; }

        [Required]
        [MaxLength(20)]
        public string InvestigationStatus { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string InvestigationLeadPayroll { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Incident Incident { get; set; } = null!;
        public Team InvestigationTeam { get; set; } = null!;
    }
}
