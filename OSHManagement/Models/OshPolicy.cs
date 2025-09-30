using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class OshPolicy
    {
        [Key]
        public int PolicyId { get; set; }

        public int StationId { get; set; }

        public bool HasTopManagementSignature { get; set; }
        public DateTime? DateSigned { get; set; }

        [MaxLength(20)]
        public string? SignedByPayroll { get; set; }

        public DateTime? LastReviewedDate { get; set; }
        public bool IsPolicyImplemented { get; set; }

        public string? CommunicationMethods { get; set; }
        public string? ManagementResponsibilities { get; set; }
        public string? SupervisorResponsibilities { get; set; }
        public string? OutsourcedResponsibilities { get; set; }
        public string? ContractorResponsibilities { get; set; }

        public bool ContractorCharterExists { get; set; }

        [Required]
        [MaxLength(20)]
        public string PolicyStatus { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Station Station { get; set; } = null!;
    }
}
