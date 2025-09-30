using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class ControlAction
    {
        [Key]
        public int ActionId { get; set; }

        public int IncidentId { get; set; }

        [Required]
        [MaxLength(20)]
        public string ActionType { get; set; } = string.Empty;

        [Required]
        public string ActionDescription { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ActionCategory { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string AssignedToPayroll { get; set; } = string.Empty;

        public int? AssignedDepartmentId { get; set; }

        public DateTime? TargetCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string ActionStatus { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? CompletionVerificationBy { get; set; }

        public int? EffectivenessRating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Incident Incident { get; set; } = null!;
        public Department? AssignedDepartment { get; set; }
    }
}
