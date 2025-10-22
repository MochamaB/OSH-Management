using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSHManagement.Models
{
    /// <summary>
    /// Stores all in-app notifications using type-discriminator pattern
    /// </summary>
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        /// <summary>
        /// Type of recipient: Employee, Role, Station, Department, Team
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string RecipientType { get; set; } = string.Empty;

        /// <summary>
        /// ID of the recipient (works with RecipientType to identify target)
        /// Example: RecipientType='Employee', RecipientId=123 means Employee #123
        /// </summary>
        [Required]
        public int RecipientId { get; set; }

        /// <summary>
        /// Notification title (short summary)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Full notification message
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Notification type for UI styling: Info, Success, Warning, Error, ActionRequired
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string NotificationType { get; set; } = "Info";

        /// <summary>
        /// Priority level: Low, Normal, High, Urgent
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = "Normal";

        /// <summary>
        /// Category for grouping: Employee, Team, Incident, Training, Safety, etc.
        /// </summary>
        [MaxLength(50)]
        public string? Category { get; set; }

        /// <summary>
        /// Optional URL to navigate to related entity
        /// </summary>
        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Whether the notification has been read
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Timestamp when notification was read
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// When the notification was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional expiration date for auto-cleanup
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Who created this notification (system or username)
        /// </summary>
        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        // Navigation properties
        public ICollection<NotificationDelivery> Deliveries { get; set; } = new List<NotificationDelivery>();
    }
}
