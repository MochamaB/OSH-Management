using OSHManagement.Models;

namespace OSHManagement.Services.Notifications
{
    /// <summary>
    /// Service for managing user notification preferences with system defaults
    /// Uses lazy loading - preferences only stored in DB when user customizes
    /// </summary>
    public interface INotificationPreferenceService
    {
        /// <summary>
        /// Get effective preference for user and category
        /// Returns custom preference if exists, otherwise system default
        /// </summary>
        Task<NotificationPreference> GetEffectivePreferenceAsync(int employeeId, string category);

        /// <summary>
        /// Get all effective preferences for a user (all categories)
        /// Merges custom preferences with system defaults
        /// </summary>
        Task<List<NotificationPreference>> GetAllEffectivePreferencesAsync(int employeeId);

        /// <summary>
        /// Get system default preference for a category
        /// These are hardcoded defaults, not from database
        /// </summary>
        NotificationPreference GetSystemDefault(string category);

        /// <summary>
        /// Check if notification should be sent based on user preferences
        /// Validates channel enabled, priority threshold, quiet hours
        /// </summary>
        Task<bool> ShouldSendNotificationAsync(
            int employeeId,
            string category,
            string channel,
            string priority,
            DateTime scheduledTime);

        /// <summary>
        /// Save or update user's custom preference
        /// Creates DB row only when user customizes settings
        /// </summary>
        Task<bool> SavePreferenceAsync(NotificationPreference preference);

        /// <summary>
        /// Reset user preference to system default
        /// Deletes custom preference from database
        /// </summary>
        Task<bool> ResetToDefaultAsync(int employeeId, string category);

        /// <summary>
        /// Check if user has customized preference for category
        /// </summary>
        Task<bool> HasCustomPreferenceAsync(int employeeId, string category);

        /// <summary>
        /// Get all available notification categories
        /// </summary>
        List<string> GetAvailableCategories();
    }
}
