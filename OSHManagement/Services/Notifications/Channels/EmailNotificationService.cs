using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Models.Enums;
using System.Net;
using System.Net.Mail;

namespace OSHManagement.Services.Notifications.Channels
{
    /// <summary>
    /// Email notification service using SMTP
    /// Configuration loaded from NotificationChannelConfigs table via ChannelConfigService
    /// Automatically decrypts passwords and sensitive data
    /// </summary>
    public class EmailNotificationService : INotificationChannelService
    {
        private readonly OshDbContext _context;
        private readonly IChannelConfigService _channelConfigService;
        private readonly ILogger<EmailNotificationService> _logger;

        public NotificationChannel Channel => NotificationChannel.Email;

        public EmailNotificationService(
            OshDbContext context,
            IChannelConfigService channelConfigService,
            ILogger<EmailNotificationService> logger)
        {
            _context = context;
            _channelConfigService = channelConfigService;
            _logger = logger;
        }

        /// <summary>
        /// Send email notification
        /// </summary>
        public async Task<bool> SendAsync(Notification notification, string? recipientAddress = null)
        {
            try
            {
                if (string.IsNullOrEmpty(recipientAddress))
                {
                    _logger.LogWarning("No recipient email address provided for notification {NotificationId}", 
                        notification.NotificationId);
                    return false;
                }

                // Load email configuration from database
                var config = await LoadEmailConfigAsync();
                if (config == null)
                {
                    _logger.LogError("Email configuration not found or incomplete");
                    return false;
                }

                // Create email message
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(config.FromEmail, config.FromName),
                    Subject = notification.Title,
                    Body = notification.Message,
                    IsBodyHtml = true // Templates support HTML
                };

                mailMessage.To.Add(recipientAddress);

                // Add reply-to if configured
                if (!string.IsNullOrEmpty(config.ReplyToEmail))
                {
                    mailMessage.ReplyToList.Add(config.ReplyToEmail);
                }

                // Send via SMTP
                using var smtpClient = new SmtpClient(config.SmtpHost, config.SmtpPort)
                {
                    Credentials = new NetworkCredential(config.SmtpUsername, config.SmtpPassword),
                    EnableSsl = config.EnableSsl
                };

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Email sent successfully to {RecipientAddress} for notification {NotificationId}",
                    recipientAddress, notification.NotificationId);

                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error sending email for notification {NotificationId}: {Error}",
                    notification.NotificationId, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email for notification {NotificationId}",
                    notification.NotificationId);
                return false;
            }
        }

        /// <summary>
        /// Check if email channel is enabled
        /// </summary>
        public async Task<bool> IsEnabledAsync()
        {
            try
            {
                var enabledConfig = await _context.NotificationChannelConfigs
                    .Where(c => c.Channel == "Email" && c.ConfigKey == "Enabled")
                    .FirstOrDefaultAsync();

                return enabledConfig?.ConfigValue?.ToLower() == "true";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if email channel is enabled");
                return false;
            }
        }

        /// <summary>
        /// Load email configuration from database using ChannelConfigService
        /// Automatically decrypts sensitive values (passwords)
        /// </summary>
        private async Task<EmailConfig?> LoadEmailConfigAsync()
        {
            try
            {
                // Use ChannelConfigService to get configs with automatic decryption
                var configs = await _channelConfigService.GetChannelConfigsAsync("Email");

                // Validate required configs
                var requiredKeys = new[] { "SmtpHost", "SmtpPort", "SmtpUsername", "SmtpPassword", "FromEmail" };
                var missingKeys = requiredKeys.Where(key => !configs.ContainsKey(key) || string.IsNullOrEmpty(configs[key])).ToList();

                if (missingKeys.Any())
                {
                    _logger.LogError("Email configuration is incomplete. Missing required settings: {MissingKeys}",
                        string.Join(", ", missingKeys));
                    return null;
                }

                return new EmailConfig
                {
                    SmtpHost = configs["SmtpHost"],
                    SmtpPort = int.TryParse(configs["SmtpPort"], out var port) ? port : 587,
                    SmtpUsername = configs["SmtpUsername"],
                    SmtpPassword = configs["SmtpPassword"], // Already decrypted by ChannelConfigService
                    FromEmail = configs["FromEmail"],
                    FromName = configs.GetValueOrDefault("FromName", "OSH Management System"),
                    ReplyToEmail = configs.GetValueOrDefault("ReplyToEmail"),
                    EnableSsl = configs.GetValueOrDefault("EnableSsl", "true")?.ToLower() == "true"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading email configuration from database");
                return null;
            }
        }

        /// <summary>
        /// Internal email configuration class
        /// </summary>
        private class EmailConfig
        {
            public string SmtpHost { get; set; } = string.Empty;
            public int SmtpPort { get; set; }
            public string SmtpUsername { get; set; } = string.Empty;
            public string SmtpPassword { get; set; } = string.Empty;
            public string FromEmail { get; set; } = string.Empty;
            public string FromName { get; set; } = string.Empty;
            public string? ReplyToEmail { get; set; }
            public bool EnableSsl { get; set; } = true;
        }
    }
}
