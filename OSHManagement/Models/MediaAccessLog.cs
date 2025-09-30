using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class MediaAccessLog
    {
        [Key]
        public int AccessLogId { get; set; }

        public int MediaId { get; set; }

        [Required]
        [MaxLength(20)]
        public string ActionType { get; set; } = string.Empty;

        public DateTime AccessTimestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        public string UserPayroll { get; set; } = string.Empty;

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [Required]
        [MaxLength(20)]
        public string RequestStatus { get; set; } = string.Empty;

        public long? ResponseSizeBytes { get; set; }

        // Navigation properties
        public MediaFile Media { get; set; } = null!;
    }
}
