using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class OrgMetadata
    {
        [Key]
        public int MetadataId { get; set; }

        public int StationId { get; set; }

        [MaxLength(20)]
        public string? FactoryManagerPayroll { get; set; }

        public int? TotalEmployeesMale { get; set; }
        public int? TotalEmployeesFemale { get; set; }
        public int? OutsourcedEmployeesMale { get; set; }
        public int? OutsourcedEmployeesFemale { get; set; }

        public bool OshPolicyExists { get; set; }
        public string? ComplianceStatus { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Station Station { get; set; } = null!;
    }
}
