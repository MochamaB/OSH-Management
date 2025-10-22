using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSHManagement.Models
{
    /// <summary>
    /// User-specific notification preferences per category
    /// </summary>
    public class NotificationPreference
    {
        [Key]
        public int PreferenceId { get; set; }

        /// <summary>
        /// Foreign key to Employee
        /// </summary>
        [Required]
        public int EmployeeId { get; set; }

        /// <summary>
        /// Category for which these preferences apply
        /// Examples: "Employee", "Team", "Incident", "All"
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Enable in-app notifications
        /// </summary>
        public bool InAppEnabled { get; set; } = true;

        /// <summary>
        /// Enable email notifications
        /// </summary>
        public bool EmailEnabled { get; set; } = true;

        /// <summary>
        /// Enable SMS notifications
        /// </summary>
        public bool SmsEnabled { get; set; } = false;

        /// <summary>
        /// Enable WhatsApp notifications
        /// </summary>
        public bool WhatsAppEnabled { get; set; } = false;

        /// <summary>
        /// Minimum priority level to receive: Low, Normal, High, Urgent
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string MinPriority { get; set; } = "Normal";

        /// <summary>
        /// Quiet hours start time (no Email/SMS/WhatsApp during this period)
        /// InApp notifications always work
        /// </summary>
        public TimeSpan? QuietHoursStart { get; set; }

        /// <summary>
        /// Quiet hours end time
        /// </summary>
        public TimeSpan? QuietHoursEnd { get; set; }

        /// <summary>
        /// Digest frequency: Instant, Hourly, Daily, Weekly
        /// </summary>
        [MaxLength(20)]
        public string? DigestFrequency { get; set; }

        /// <summary>
        /// When the preference was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the preference was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(EmployeeId))]
        public Employee Employee { get; set; } = null!;
    }
}
