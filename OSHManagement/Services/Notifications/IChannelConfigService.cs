using OSHManagement.Models;

namespace OSHManagement.Services.Notifications
{
    /// <summary>
    /// Service for managing notification channel configurations
    /// Provides access to Email/SMS/WhatsApp settings from database
    /// </summary>
    public interface IChannelConfigService
    {
        /// <summary>
        /// Get single configuration value for a channel
        /// Automatically decrypts if encrypted
        /// </summary>
        Task<string?> GetConfigValueAsync(string channel, string key);

        /// <summary>
        /// Get all configuration key-value pairs for a channel
        /// Returns decrypted values
        /// </summary>
        Task<Dictionary<string, string>> GetChannelConfigsAsync(string channel);

        /// <summary>
        /// Get all configuration records for a channel (including metadata)
        /// Values are decrypted for display
        /// </summary>
        Task<List<NotificationChannelConfig>> GetChannelConfigRecordsAsync(string channel);

        /// <summary>
        /// Get configuration records for all channels
        /// Grouped by channel for admin UI
        /// </summary>
        Task<Dictionary<string, List<NotificationChannelConfig>>> GetAllChannelConfigsAsync();

        /// <summary>
        /// Check if all required configurations are set for a channel
        /// </summary>
        Task<bool> IsChannelConfiguredAsync(string channel);

        /// <summary>
        /// Get list of missing required configurations for a channel
        /// </summary>
        Task<List<string>> GetMissingRequiredConfigsAsync(string channel);

        /// <summary>
        /// Save or update a single configuration value
        /// Automatically encrypts if IsEncrypted flag is set
        /// </summary>
        Task<bool> SaveConfigAsync(string channel, string key, string value);

        /// <summary>
        /// Save multiple configuration values at once
        /// Used by edit form to update all channel configs
        /// </summary>
        Task<bool> SaveChannelConfigsAsync(string channel, Dictionary<string, string> configs);

        /// <summary>
        /// Test channel configuration (attempt connection)
        /// Returns success status and message
        /// </summary>
        Task<(bool Success, string Message)> TestChannelConnectionAsync(string channel);
    }
}
