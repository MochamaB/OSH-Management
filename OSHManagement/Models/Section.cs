using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class Section
    {
        [Key]
        public int SectionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SectionName { get; set; } = string.Empty;

        public int StationId { get; set; }

        [MaxLength(20)]
        public string? SectionSupervisorPayroll { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Station Station { get; set; } = null!;
        public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
    }
}
