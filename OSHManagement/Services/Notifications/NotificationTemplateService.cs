using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Models.Enums;
using System.Text.RegularExpressions;

namespace OSHManagement.Services.Notifications
{
    /// <summary>
    /// Service for managing and rendering notification templates
    /// </summary>
    public class NotificationTemplateService : INotificationTemplateService
    {
        private readonly OshDbContext _context;
        private readonly ILogger<NotificationTemplateService> _logger;

        public NotificationTemplateService(
            OshDbContext context,
            ILogger<NotificationTemplateService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get a template by event type and channel
        /// </summary>
        public async Task<NotificationTemplate?> GetTemplateAsync(string eventType, NotificationChannel channel)
        {
            try
            {
                var channelName = channel.ToString();
                
                var template = await _context.NotificationTemplates
                    .Where(t => t.TemplateName == eventType 
                             && t.Channel == channelName 
                             && t.IsActive)
                    .FirstOrDefaultAsync();

                if (template == null)
                {
                    _logger.LogWarning("No active template found for event '{EventType}' and channel '{Channel}'", 
                        eventType, channelName);
                }

                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving template for event '{EventType}' and channel '{Channel}'", 
                    eventType, channel);
                return null;
            }
        }

        /// <summary>
        /// Render a template with data (replace placeholders)
        /// </summary>
        public string RenderTemplate(NotificationTemplate template, Dictionary<string, string> data)
        {
            if (string.IsNullOrEmpty(template.BodyTemplate))
            {
                return string.Empty;
            }

            var result = template.BodyTemplate;

            // Replace placeholders: {EmployeeName}, {StationName}, etc.
            foreach (var kvp in data)
            {
                var placeholder = $"{{{kvp.Key}}}";
                result = result.Replace(placeholder, kvp.Value ?? string.Empty);
            }

            // Log if any placeholders remain unreplaced (for debugging)
            var remainingPlaceholders = Regex.Matches(result, @"\{([^}]+)\}");
            if (remainingPlaceholders.Count > 0)
            {
                var placeholderNames = string.Join(", ", remainingPlaceholders.Select(m => m.Value));
                _logger.LogWarning("Template '{TemplateName}' has unreplaced placeholders: {Placeholders}", 
                    template.TemplateName, placeholderNames);
            }

            return result;
        }

        /// <summary>
        /// Render subject template (for Email/WhatsApp)
        /// </summary>
        public string RenderSubject(NotificationTemplate template, Dictionary<string, string> data)
        {
            if (string.IsNullOrEmpty(template.SubjectTemplate))
            {
                return template.TemplateName; // Fallback to template name
            }

            var result = template.SubjectTemplate;

            // Replace placeholders
            foreach (var kvp in data)
            {
                var placeholder = $"{{{kvp.Key}}}";
                result = result.Replace(placeholder, kvp.Value ?? string.Empty);
            }

            return result;
        }

        /// <summary>
        /// Get all active templates for a category
        /// </summary>
        public async Task<List<NotificationTemplate>> GetTemplatesByCategoryAsync(string category)
        {
            try
            {
                return await _context.NotificationTemplates
                    .Where(t => t.Category == category && t.IsActive)
                    .OrderBy(t => t.TemplateName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving templates for category '{Category}'", category);
                return new List<NotificationTemplate>();
            }
        }
    }
}
