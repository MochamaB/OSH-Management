using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class MediaCollection
    {
        [Key]
        public int CollectionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CollectionName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string CollectionDisplayName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ModuleName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int MaxFileSizeMb { get; set; }
        public string? AllowedFileTypes { get; set; }
        public int? RetentionPolicyDays { get; set; }

        public bool IsPublic { get; set; }
        public bool RequiresAuthentication { get; set; }
        public string? AllowedRoles { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();
    }
}
