using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSHManagement.Models
{
    /// <summary>
    /// Tracks multi-channel notification delivery (Email, SMS, WhatsApp)
    /// </summary>
    public class NotificationDelivery
    {
        [Key]
        public int NotificationDeliveryId { get; set; }

        /// <summary>
        /// Foreign key to the notification
        /// </summary>
        [Required]
        public int NotificationId { get; set; }

        /// <summary>
        /// Delivery channel: InApp, Email, SMS, WhatsApp
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Channel { get; set; } = string.Empty;

        /// <summary>
        /// Recipient address (email, phone number, WhatsApp number)
        /// </summary>
        [MaxLength(255)]
        public string? RecipientAddress { get; set; }

        /// <summary>
        /// Delivery status: Pending, Sending, Sent, Delivered, Failed, Bounced
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// When the notification was sent
        /// </summary>
        public DateTime? SentAt { get; set; }

        /// <summary>
        /// When the notification was delivered (confirmed by provider)
        /// </summary>
        public DateTime? DeliveredAt { get; set; }

        /// <summary>
        /// When the notification was read/opened (if trackable)
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// Error message if delivery failed
        /// </summary>
        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Number of retry attempts
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Next scheduled retry time (for failed deliveries)
        /// </summary>
        public DateTime? NextRetryAt { get; set; }

        /// <summary>
        /// When the delivery record was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(NotificationId))]
        public Notification Notification { get; set; } = null!;
    }
}
