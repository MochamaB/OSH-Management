using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;

namespace OSHManagement.Services.Notifications
{
    /// <summary>
    /// Implementation of notification preference service with lazy loading and system defaults
    /// Preferences only stored in DB when user customizes - otherwise uses hardcoded defaults
    /// </summary>
    public class NotificationPreferenceService : INotificationPreferenceService
    {
        private readonly OshDbContext _context;
        private readonly ILogger<NotificationPreferenceService> _logger;

        // System-wide default categories
        private static readonly List<string> DefaultCategories = new()
        {
            "Employee",
            "Team",
            "Incident",
            "Training",
            "Safety",
            "Equipment",
            "Inspection"
        };

        public NotificationPreferenceService(
            OshDbContext context,
            ILogger<NotificationPreferenceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get effective preference - custom if exists, otherwise system default
        /// </summary>
        public async Task<NotificationPreference> GetEffectivePreferenceAsync(int employeeId, string category)
        {
            try
            {
                // Try to get custom preference from database
                var customPreference = await _context.NotificationPreferences
                    .FirstOrDefaultAsync(np =>
                        np.EmployeeId == employeeId &&
                        np.Category == category);

                // If custom preference exists, return it
                if (customPreference != null)
                {
                    return customPreference;
                }

                // Otherwise, return system default
                return GetSystemDefault(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting effective preference for Employee {EmployeeId}, Category {Category}",
                    employeeId, category);
                
                // On error, return system default
                return GetSystemDefault(category);
            }
        }

        /// <summary>
        /// Get all effective preferences for a user across all categories
        /// </summary>
        public async Task<List<NotificationPreference>> GetAllEffectivePreferencesAsync(int employeeId)
        {
            try
            {
                var effectivePreferences = new List<NotificationPreference>();

                // Get all custom preferences for this user
                var customPreferences = await _context.NotificationPreferences
                    .Where(np => np.EmployeeId == employeeId)
                    .ToListAsync();

                // For each category, use custom if exists, otherwise default
                foreach (var category in DefaultCategories)
                {
                    var customPref = customPreferences.FirstOrDefault(cp => cp.Category == category);
                    
                    if (customPref != null)
                    {
                        effectivePreferences.Add(customPref);
                    }
                    else
                    {
                        effectivePreferences.Add(GetSystemDefault(category));
                    }
                }

                return effectivePreferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all effective preferences for Employee {EmployeeId}", employeeId);
                
                // On error, return all defaults
                return DefaultCategories.Select(c => GetSystemDefault(c)).ToList();
            }
        }

        /// <summary>
        /// Get system default preference for a category
        /// These are hardcoded business rules
        /// </summary>
        public NotificationPreference GetSystemDefault(string category)
        {
            var defaultPreference = new NotificationPreference
            {
                PreferenceId = 0, // 0 indicates it's a default, not from DB
                EmployeeId = 0,
                Category = category,
                InAppEnabled = true, // InApp always enabled by default
                EmailEnabled = GetDefaultEmailEnabled(category),
                SmsEnabled = false, // SMS disabled by default (costs money)
                WhatsAppEnabled = false, // WhatsApp disabled by default (requires setup)
                MinPriority = GetDefaultMinPriority(category),
                QuietHoursStart = null, // No quiet hours by default
                QuietHoursEnd = null,
                DigestFrequency = null, // Instant delivery by default
                CreatedAt = DateTime.UtcNow
            };

            return defaultPreference;
        }

        /// <summary>
        /// Determine if email should be enabled by default for a category
        /// </summary>
        private bool GetDefaultEmailEnabled(string category)
        {
            return category switch
            {
                "Employee" => true,    // Employee updates via email
                "Team" => false,       // Team notifications too noisy via email
                "Incident" => true,    // Incidents important - email enabled
                "Training" => true,    // Training updates via email
                "Safety" => true,      // Safety alerts via email
                "Equipment" => false,  // Equipment updates - InApp only
                "Inspection" => true,  // Inspection results via email
                _ => true              // Default: Email enabled
            };
        }

        /// <summary>
        /// Determine minimum priority threshold by category
        /// </summary>
        private string GetDefaultMinPriority(string category)
        {
            return category switch
            {
                "Incident" => "Low",   // Show all incident priorities (important)
                "Safety" => "Low",     // Show all safety notifications
                "Employee" => "Normal", // Only Normal and above
                "Team" => "Normal",
                "Training" => "Normal",
                "Equipment" => "Normal",
                "Inspection" => "Normal",
                _ => "Normal"
            };
        }

        /// <summary>
        /// Check if notification should be sent based on preferences
        /// </summary>
        public async Task<bool> ShouldSendNotificationAsync(
            int employeeId,
            string category,
            string channel,
            string priority,
            DateTime scheduledTime)
        {
            try
            {
                // Get effective preference
                var preference = await GetEffectivePreferenceAsync(employeeId, category);

                // Check 1: Is this channel enabled?
                if (!IsChannelEnabled(preference, channel))
                {
                    _logger.LogDebug("Channel {Channel} disabled for Employee {EmployeeId}, Category {Category}",
                        channel, employeeId, category);
                    return false;
                }

                // Check 2: Does notification meet minimum priority threshold?
                if (!MeetsPriorityThreshold(priority, preference.MinPriority))
                {
                    _logger.LogDebug("Priority {Priority} below threshold {MinPriority} for Employee {EmployeeId}",
                        priority, preference.MinPriority, employeeId);
                    return false;
                }

                // Check 3: Are we in quiet hours? (Only applies to Email, SMS, WhatsApp)
                if (channel != "InApp" && IsInQuietHours(preference, scheduledTime))
                {
                    _logger.LogDebug("In quiet hours for Employee {EmployeeId}, skipping {Channel}",
                        employeeId, channel);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if notification should be sent");
                
                // On error, default to sending (fail open)
                return true;
            }
        }

        /// <summary>
        /// Check if specific channel is enabled in preference
        /// </summary>
        private bool IsChannelEnabled(NotificationPreference preference, string channel)
        {
            return channel switch
            {
                "InApp" => preference.InAppEnabled,
                "Email" => preference.EmailEnabled,
                "SMS" => preference.SmsEnabled,
                "WhatsApp" => preference.WhatsAppEnabled,
                _ => false
            };
        }

        /// <summary>
        /// Check if notification priority meets minimum threshold
        /// </summary>
        private bool MeetsPriorityThreshold(string notificationPriority, string minPriority)
        {
            var priorityLevels = new Dictionary<string, int>
            {
                { "Low", 1 },
                { "Normal", 2 },
                { "High", 3 },
                { "Urgent", 4 }
            };

            // Default to Normal if priority not recognized
            if (!priorityLevels.ContainsKey(notificationPriority))
                notificationPriority = "Normal";
            
            if (!priorityLevels.ContainsKey(minPriority))
                minPriority = "Normal";

            return priorityLevels[notificationPriority] >= priorityLevels[minPriority];
        }

        /// <summary>
        /// Check if current time is within quiet hours
        /// </summary>
        private bool IsInQuietHours(NotificationPreference preference, DateTime scheduledTime)
        {
            // No quiet hours configured
            if (!preference.QuietHoursStart.HasValue || !preference.QuietHoursEnd.HasValue)
                return false;

            var currentTime = scheduledTime.TimeOfDay;
            var start = preference.QuietHoursStart.Value;
            var end = preference.QuietHoursEnd.Value;

            // Handle overnight quiet hours (e.g., 22:00 - 07:00)
            if (start > end)
            {
                return currentTime >= start || currentTime <= end;
            }

            // Normal quiet hours (e.g., 13:00 - 14:00)
            return currentTime >= start && currentTime <= end;
        }

        /// <summary>
        /// Save or update user's custom preference
        /// </summary>
        public async Task<bool> SavePreferenceAsync(NotificationPreference preference)
        {
            try
            {
                // Check if custom preference already exists
                var existing = await _context.NotificationPreferences
                    .FirstOrDefaultAsync(np =>
                        np.EmployeeId == preference.EmployeeId &&
                        np.Category == preference.Category);

                if (existing != null)
                {
                    // Update existing preference
                    existing.InAppEnabled = preference.InAppEnabled;
                    existing.EmailEnabled = preference.EmailEnabled;
                    existing.SmsEnabled = preference.SmsEnabled;
                    existing.WhatsAppEnabled = preference.WhatsAppEnabled;
                    existing.MinPriority = preference.MinPriority;
                    existing.QuietHoursStart = preference.QuietHoursStart;
                    existing.QuietHoursEnd = preference.QuietHoursEnd;
                    existing.DigestFrequency = preference.DigestFrequency;
                    existing.UpdatedAt = DateTime.UtcNow;

                    _context.NotificationPreferences.Update(existing);
                }
                else
                {
                    // Create new custom preference
                    preference.CreatedAt = DateTime.UtcNow;
                    preference.UpdatedAt = null;
                    await _context.NotificationPreferences.AddAsync(preference);
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Saved notification preference for Employee {EmployeeId}, Category {Category}",
                    preference.EmployeeId, preference.Category);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving notification preference");
                return false;
            }
        }

        /// <summary>
        /// Reset preference to system default by deleting custom preference
        /// </summary>
        public async Task<bool> ResetToDefaultAsync(int employeeId, string category)
        {
            try
            {
                var preference = await _context.NotificationPreferences
                    .FirstOrDefaultAsync(np =>
                        np.EmployeeId == employeeId &&
                        np.Category == category);

                if (preference != null)
                {
                    _context.NotificationPreferences.Remove(preference);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Reset preference to default for Employee {EmployeeId}, Category {Category}",
                        employeeId, category);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting preference to default");
                return false;
            }
        }

        /// <summary>
        /// Check if user has customized this category
        /// </summary>
        public async Task<bool> HasCustomPreferenceAsync(int employeeId, string category)
        {
            try
            {
                return await _context.NotificationPreferences
                    .AnyAsync(np =>
                        np.EmployeeId == employeeId &&
                        np.Category == category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if custom preference exists");
                return false;
            }
        }

        /// <summary>
        /// Get all available notification categories
        /// </summary>
        public List<string> GetAvailableCategories()
        {
            return new List<string>(DefaultCategories);
        }
    }
}
