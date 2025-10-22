using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Services.Security;

namespace OSHManagement.Services.Notifications
{
    /// <summary>
    /// Implementation of channel configuration service
    /// Handles reading, caching, and decrypting notification channel settings
    /// </summary>
    public class ChannelConfigService : IChannelConfigService
    {
        private readonly OshDbContext _context;
        private readonly IEncryptionService _encryption;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ChannelConfigService> _logger;

        // Cache configuration values for 5 minutes to reduce DB queries
        private const int CACHE_DURATION_MINUTES = 5;
        private const string CACHE_KEY_PREFIX = "ChannelConfig_";

        public ChannelConfigService(
            OshDbContext context,
            IEncryptionService encryption,
            IMemoryCache cache,
            ILogger<ChannelConfigService> logger)
        {
            _context = context;
            _encryption = encryption;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Get single configuration value
        /// Uses caching and automatic decryption
        /// </summary>
        public async Task<string?> GetConfigValueAsync(string channel, string key)
        {
            var cacheKey = $"{CACHE_KEY_PREFIX}{channel}_{key}";

            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out string? cachedValue))
            {
                _logger.LogDebug("Retrieved config from cache: {Channel}.{Key}", channel, key);
                return cachedValue;
            }

            try
            {
                var config = await _context.NotificationChannelConfigs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Channel == channel &&
                        c.ConfigKey == key &&
                        c.IsActive);

                if (config == null)
                {
                    _logger.LogWarning("Configuration not found: {Channel}.{Key}", channel, key);
                    return null;
                }

                // Decrypt if necessary
                var value = config.IsEncrypted && !string.IsNullOrEmpty(config.ConfigValue)
                    ? _encryption.Decrypt(config.ConfigValue)
                    : config.ConfigValue;

                // Cache the decrypted value
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));

                _cache.Set(cacheKey, value, cacheOptions);

                _logger.LogDebug("Retrieved config from database: {Channel}.{Key}", channel, key);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting config value: {Channel}.{Key}", channel, key);
                return null;
            }
        }

        /// <summary>
        /// Get all configuration key-value pairs for a channel
        /// Returns decrypted dictionary
        /// </summary>
        public async Task<Dictionary<string, string>> GetChannelConfigsAsync(string channel)
        {
            try
            {
                var configs = await _context.NotificationChannelConfigs
                    .AsNoTracking()
                    .Where(c => c.Channel == channel && c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();

                var result = new Dictionary<string, string>();

                foreach (var config in configs)
                {
                    if (string.IsNullOrEmpty(config.ConfigValue))
                    {
                        result[config.ConfigKey] = string.Empty;
                        continue;
                    }

                    var value = config.IsEncrypted
                        ? _encryption.Decrypt(config.ConfigValue)
                        : config.ConfigValue;

                    result[config.ConfigKey] = value;
                }

                _logger.LogInformation("Retrieved {Count} configurations for channel: {Channel}",
                    configs.Count, channel);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting channel configs: {Channel}", channel);
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Get all configuration records for a channel with metadata
        /// Used by admin UI to display full configuration details
        /// </summary>
        public async Task<List<NotificationChannelConfig>> GetChannelConfigRecordsAsync(string channel)
        {
            try
            {
                var configs = await _context.NotificationChannelConfigs
                    .AsNoTracking()
                    .Where(c => c.Channel == channel && c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.ConfigKey)
                    .ToListAsync();

                // Decrypt values for display (but keep IsEncrypted flag)
                foreach (var config in configs)
                {
                    if (config.IsEncrypted && !string.IsNullOrEmpty(config.ConfigValue))
                    {
                        try
                        {
                            config.ConfigValue = _encryption.Decrypt(config.ConfigValue);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to decrypt config: {Channel}.{Key}",
                                channel, config.ConfigKey);
                            config.ConfigValue = "[Decryption Failed]";
                        }
                    }
                }

                return configs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting channel config records: {Channel}", channel);
                return new List<NotificationChannelConfig>();
            }
        }

        /// <summary>
        /// Get all configurations grouped by channel
        /// Used by admin UI to display all channels
        /// </summary>
        public async Task<Dictionary<string, List<NotificationChannelConfig>>> GetAllChannelConfigsAsync()
        {
            try
            {
                var allConfigs = await _context.NotificationChannelConfigs
                    .AsNoTracking()
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Channel)
                    .ThenBy(c => c.DisplayOrder)
                    .ThenBy(c => c.ConfigKey)
                    .ToListAsync();

                // Decrypt values
                foreach (var config in allConfigs)
                {
                    if (config.IsEncrypted && !string.IsNullOrEmpty(config.ConfigValue))
                    {
                        try
                        {
                            config.ConfigValue = _encryption.Decrypt(config.ConfigValue);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to decrypt config: {Channel}.{Key}",
                                config.Channel, config.ConfigKey);
                            config.ConfigValue = "[Decryption Failed]";
                        }
                    }
                }

                // Group by channel
                var grouped = allConfigs
                    .GroupBy(c => c.Channel)
                    .ToDictionary(g => g.Key, g => g.ToList());

                return grouped;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all channel configs");
                return new Dictionary<string, List<NotificationChannelConfig>>();
            }
        }

        /// <summary>
        /// Check if all required configurations are set
        /// </summary>
        public async Task<bool> IsChannelConfiguredAsync(string channel)
        {
            try
            {
                var missingConfigs = await GetMissingRequiredConfigsAsync(channel);
                return missingConfigs.Count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if channel is configured: {Channel}", channel);
                return false;
            }
        }

        /// <summary>
        /// Get list of missing required configurations
        /// </summary>
        public async Task<List<string>> GetMissingRequiredConfigsAsync(string channel)
        {
            try
            {
                var requiredConfigs = await _context.NotificationChannelConfigs
                    .AsNoTracking()
                    .Where(c =>
                        c.Channel == channel &&
                        c.IsRequired &&
                        c.IsActive)
                    .ToListAsync();

                var missing = requiredConfigs
                    .Where(c => string.IsNullOrWhiteSpace(c.ConfigValue))
                    .Select(c => c.ConfigKey)
                    .ToList();

                if (missing.Any())
                {
                    _logger.LogWarning("Channel {Channel} is missing required configs: {MissingConfigs}",
                        channel, string.Join(", ", missing));
                }

                return missing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting missing required configs: {Channel}", channel);
                return new List<string>();
            }
        }

        /// <summary>
        /// Save or update a single configuration value
        /// Automatically encrypts if IsEncrypted flag is set
        /// </summary>
        public async Task<bool> SaveConfigAsync(string channel, string key, string value)
        {
            try
            {
                var config = await _context.NotificationChannelConfigs
                    .FirstOrDefaultAsync(c =>
                        c.Channel == channel &&
                        c.ConfigKey == key);

                if (config == null)
                {
                    _logger.LogWarning("Config not found: {Channel}.{Key}", channel, key);
                    return false;
                }

                // Encrypt if needed
                var valueToStore = config.IsEncrypted && !string.IsNullOrEmpty(value)
                    ? _encryption.Encrypt(value)
                    : value;

                config.ConfigValue = valueToStore;
                config.UpdatedAt = DateTime.UtcNow;

                _context.NotificationChannelConfigs.Update(config);
                await _context.SaveChangesAsync();

                // Clear cache for this config
                _cache.Remove($"{CACHE_KEY_PREFIX}{channel}_{key}");

                _logger.LogInformation("Saved config: {Channel}.{Key}", channel, key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving config: {Channel}.{Key}", channel, key);
                return false;
            }
        }

        /// <summary>
        /// Save multiple configuration values at once
        /// Used by edit form to update all channel configs
        /// </summary>
        public async Task<bool> SaveChannelConfigsAsync(string channel, Dictionary<string, string> configs)
        {
            try
            {
                // Get all existing configs for this channel
                var existingConfigs = await _context.NotificationChannelConfigs
                    .Where(c => c.Channel == channel)
                    .ToListAsync();

                foreach (var kvp in configs)
                {
                    var config = existingConfigs.FirstOrDefault(c => c.ConfigKey == kvp.Key);
                    if (config == null)
                    {
                        _logger.LogWarning("Config not found, skipping: {Channel}.{Key}", channel, kvp.Key);
                        continue;
                    }

                    // Encrypt if needed
                    var valueToStore = config.IsEncrypted && !string.IsNullOrEmpty(kvp.Value)
                        ? _encryption.Encrypt(kvp.Value)
                        : kvp.Value;

                    config.ConfigValue = valueToStore;
                    config.UpdatedAt = DateTime.UtcNow;

                    _context.NotificationChannelConfigs.Update(config);

                    // Clear cache
                    _cache.Remove($"{CACHE_KEY_PREFIX}{channel}_{kvp.Key}");
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Saved {Count} configurations for channel: {Channel}",
                    configs.Count, channel);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving channel configs: {Channel}", channel);
                return false;
            }
        }

        /// <summary>
        /// Test channel configuration (attempt connection)
        /// Returns success status and message
        /// </summary>
        public async Task<(bool Success, string Message)> TestChannelConnectionAsync(string channel)
        {
            return channel switch
            {
                "Email" => await TestEmailConfigAsync(),
                "SMS" => await TestSmsConfigAsync(),
                "WhatsApp" => await TestWhatsAppConfigAsync(),
                _ => (false, $"Unknown channel: {channel}")
            };
        }

        /// <summary>
        /// Test email configuration by attempting SMTP connection
        /// </summary>
        private async Task<(bool Success, string Message)> TestEmailConfigAsync()
        {
            try
            {
                var configs = await GetChannelConfigsAsync("Email");

                // Check required configs
                var required = new[] { "SmtpHost", "SmtpPort", "SmtpUsername", "SmtpPassword", "FromEmail" };
                var missing = required.Where(key => !configs.ContainsKey(key) || string.IsNullOrEmpty(configs[key])).ToList();

                if (missing.Any())
                {
                    return (false, $"Missing required configurations: {string.Join(", ", missing)}");
                }

                // Try to connect to SMTP server
                using var client = new System.Net.Mail.SmtpClient(configs["SmtpHost"], int.Parse(configs["SmtpPort"]));
                client.Credentials = new System.Net.NetworkCredential(configs["SmtpUsername"], configs["SmtpPassword"]);
                client.EnableSsl = configs.GetValueOrDefault("EnableSsl", "true")?.ToLower() == "true";
                client.Timeout = 10000; // 10 seconds

                // Just test connection, don't send email
                // Note: Some SMTP servers may not allow testing without sending
                var testMessage = new System.Net.Mail.MailMessage(
                    configs["FromEmail"],
                    configs["FromEmail"],
                    "Test Connection",
                    "This is a test message from OSH Management System to verify SMTP configuration."
                );

                await client.SendMailAsync(testMessage);

                return (true, "Email configuration is valid! Test email sent successfully.");
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                return (false, $"SMTP Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Connection failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Test SMS configuration (placeholder for future implementation)
        /// </summary>
        private async Task<(bool Success, string Message)> TestSmsConfigAsync()
        {
            await Task.CompletedTask;
            return (false, "SMS testing is not yet implemented");
        }

        /// <summary>
        /// Test WhatsApp configuration (placeholder for future implementation)
        /// </summary>
        private async Task<(bool Success, string Message)> TestWhatsAppConfigAsync()
        {
            await Task.CompletedTask;
            return (false, "WhatsApp testing is not yet implemented");
        }
    }
}
