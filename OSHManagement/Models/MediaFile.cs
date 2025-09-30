using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class MediaFile
    {
        [Key]
        public int MediaId { get; set; }

        public int CollectionId { get; set; }

        [Required]
        [MaxLength(255)]
        public string OriginalFilename { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string SystemFilename { get; set; } = string.Empty;

        [MaxLength(64)]
        public string? FileHash { get; set; }

        [MaxLength(100)]
        public string? MimeType { get; set; }

        public long FileSizeBytes { get; set; }

        [MaxLength(10)]
        public string? FileExtension { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [Required]
        [MaxLength(20)]
        public string StorageProvider { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int VersionNumber { get; set; } = 1;
        public int? ParentMediaId { get; set; }
        public bool IsLatestVersion { get; set; } = true;

        [Required]
        [MaxLength(20)]
        public string UploadStatus { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string UploadedByPayroll { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public MediaCollection Collection { get; set; } = null!;
        public MediaFile? ParentMedia { get; set; }
        public ICollection<MediaFile> ChildVersions { get; set; } = new List<MediaFile>();
        public ICollection<MediaAssociation> Associations { get; set; } = new List<MediaAssociation>();
        public ICollection<MediaAccessLog> AccessLogs { get; set; } = new List<MediaAccessLog>();
    }
}
