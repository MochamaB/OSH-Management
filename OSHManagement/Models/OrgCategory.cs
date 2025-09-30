using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class OrgCategory
    {
        [Key]
        public int OrgCategoryId { get; set; }

        [Required]
        [MaxLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Station> Stations { get; set; } = new List<Station>();
    }
}
