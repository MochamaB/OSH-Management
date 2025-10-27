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
        public string? AllowedFileTypes { get; set; }  // JSON array
        public int? RetentionPolicyDays { get; set; }

        public bool IsPublic { get; set; }
        public bool RequiresAuthentication { get; set; }
        public string? AllowedRoles { get; set; }  // JSON array

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Audit fields
        [MaxLength(20)]
        public string? CreatedByPayroll { get; set; }

        // Soft delete support
        public bool IsActive { get; set; } = true;
        public DateTime? DeletedAt { get; set; }

        [MaxLength(20)]
        public string? DeletedByPayroll { get; set; }

        // Navigation properties
        public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();
        public Employee? CreatedBy { get; set; }
        public Employee? DeletedBy { get; set; }
    }
}
