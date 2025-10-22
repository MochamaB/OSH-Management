using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    /// <summary>
    /// Notification templates for different events and channels
    /// </summary>
    public class NotificationTemplate
    {
        [Key]
        public int TemplateId { get; set; }

        /// <summary>
        /// Template name/identifier (e.g., "EmployeeCreated", "TeamMemberAdded")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// Category for grouping (e.g., "Employee", "Team", "Incident")
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Delivery channel: InApp, Email, SMS, WhatsApp
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Channel { get; set; } = string.Empty;

        /// <summary>
        /// Subject template for Email/WhatsApp (optional for InApp)
        /// Supports placeholders: {EmployeeName}, {StationName}, etc.
        /// </summary>
        [MaxLength(200)]
        public string? SubjectTemplate { get; set; }

        /// <summary>
        /// Body template with placeholders
        /// Supports HTML for Email, plain text for SMS/InApp
        /// Example: "New employee {EmployeeName} has been added to {StationName}"
        /// </summary>
        [Required]
        public string BodyTemplate { get; set; } = string.Empty;

        /// <summary>
        /// Description of the template for admin reference
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Whether this template is active and should be used
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When the template was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the template was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Who created this template
        /// </summary>
        [MaxLength(50)]
        public string? CreatedBy { get; set; }
    }
}
