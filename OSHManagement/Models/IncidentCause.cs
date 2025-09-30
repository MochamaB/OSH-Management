using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class IncidentCause
    {
        [Key]
        public int CauseId { get; set; }

        public int IncidentId { get; set; }

        public bool NotUsingPpes { get; set; }
        public bool IgnoringSops { get; set; }
        public bool TakingShortcuts { get; set; }
        public bool WorkingWithoutAuthorization { get; set; }
        public bool PoorLighting { get; set; }
        public bool SlipperyFloor { get; set; }
        public bool DamagedEquipment { get; set; }
        public bool UnguardedMachine { get; set; }
        public bool ExposedElectricalSystem { get; set; }

        public string? HumanFactors { get; set; }
        public string? OrganizationalFactors { get; set; }
        public string? EnvironmentalFactors { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Incident Incident { get; set; } = null!;
    }
}
