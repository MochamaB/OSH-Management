using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class Station
    {
        [Key]
        public int StationId { get; set; }

        [Required]
        [MaxLength(20)]
        public string StationCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string StationName { get; set; } = string.Empty;

        public int OrgCategoryId { get; set; }
        public int? ParentStationId { get; set; }

        [MaxLength(50)]
        public string? LegacyStationMapping { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public OrgCategory OrgCategory { get; set; } = null!;
        public Station? ParentStation { get; set; }
        public ICollection<Station> ChildStations { get; set; } = new List<Station>();
        public ICollection<Department> Departments { get; set; } = new List<Department>();
        public ICollection<Section> Sections { get; set; } = new List<Section>();
        public ICollection<Team> Teams { get; set; } = new List<Team>();
        public OrgMetadata? OrgMetadata { get; set; }
    }
}
