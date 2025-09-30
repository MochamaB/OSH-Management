using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class MediaConversionJob
    {
        [Key]
        public int JobId { get; set; }

        public int MediaId { get; set; }

        [Required]
        [MaxLength(50)]
        public string JobType { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string JobStatus { get; set; } = string.Empty;

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string? JobParameters { get; set; }
        public int? OutputMediaId { get; set; }

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public MediaFile Media { get; set; } = null!;
        public MediaFile? OutputMedia { get; set; }
    }
}
