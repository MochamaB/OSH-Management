using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    /// <summary>
    /// Dynamic notification channel configuration (replaces appsettings.json)
    /// Allows editing Email/SMS/WhatsApp settings via Admin UI
    /// </summary>
    public class NotificationChannelConfig
    {
        [Key]
        public int ConfigId { get; set; }

        /// <summary>
        /// Channel name: Email, SMS, WhatsApp
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Channel { get; set; } = string.Empty;

        /// <summary>
        /// Configuration key (e.g., "SmtpHost", "SmtpPort", "ApiKey")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ConfigKey { get; set; } = string.Empty;

        /// <summary>
        /// Configuration value
        /// </summary>
        [MaxLength(500)]
        public string? ConfigValue { get; set; }

        /// <summary>
        /// Whether this value should be encrypted (passwords, API keys)
        /// </summary>
        public bool IsEncrypted { get; set; } = false;

        /// <summary>
        /// Description/help text for admins
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Whether this config is required for the channel to work
        /// </summary>
        public bool IsRequired { get; set; } = false;

        /// <summary>
        /// Display order in Admin UI
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Whether this config is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When the config was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the config was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Who last updated this config
        /// </summary>
        [MaxLength(50)]
        public string? UpdatedBy { get; set; }
    }
}
