using OSHManagement.Models;
using OSHManagement.Models.Enums;

namespace OSHManagement.Services.Notifications
{
    /// <summary>
    /// Manages notification templates and rendering
    /// </summary>
    public interface INotificationTemplateService
    {
        /// <summary>
        /// Get a template by event type and channel
        /// </summary>
        Task<NotificationTemplate?> GetTemplateAsync(string eventType, NotificationChannel channel);

        /// <summary>
        /// Render a template with data (replace placeholders)
        /// Example: "{EmployeeName} has been added" → "John Doe has been added"
        /// </summary>
        string RenderTemplate(NotificationTemplate template, Dictionary<string, string> data);

        /// <summary>
        /// Render subject template (for Email/WhatsApp)
        /// </summary>
        string RenderSubject(NotificationTemplate template, Dictionary<string, string> data);

        /// <summary>
        /// Get all active templates for a category
        /// </summary>
        Task<List<NotificationTemplate>> GetTemplatesByCategoryAsync(string category);
    }
}
