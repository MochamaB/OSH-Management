using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class OshCommitteeConfig
    {
        [Key]
        public int ConfigId { get; set; }

        public int TeamId { get; set; }

        public bool IsCommitteeTrained { get; set; }
        public DateTime? TrainingDate { get; set; }
        public bool HasMeetingSchedule { get; set; }

        [Required]
        [MaxLength(20)]
        public string InspectionFrequency { get; set; } = string.Empty;

        public DateTime? LastInspectionDate { get; set; }
        public DateTime? NextInspectionDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Team Team { get; set; } = null!;
    }
}
