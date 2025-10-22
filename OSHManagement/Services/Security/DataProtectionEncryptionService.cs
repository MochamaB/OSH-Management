using Microsoft.AspNetCore.DataProtection;

namespace OSHManagement.Services.Security
{
    /// <summary>
    /// Encryption service using ASP.NET Core Data Protection API
    /// Provides automatic key management, rotation, and secure encryption
    /// </summary>
    public class DataProtectionEncryptionService : IEncryptionService
    {
        private readonly IDataProtector _protector;
        private readonly ILogger<DataProtectionEncryptionService> _logger;

        // Prefix to identify encrypted strings
        private const string ENCRYPTED_PREFIX = "ENC:";

        public DataProtectionEncryptionService(
            IDataProtectionProvider provider,
            ILogger<DataProtectionEncryptionService> logger)
        {
            // Create a protector with a specific purpose string
            // This ensures encrypted data is only decryptable by this purpose
            _protector = provider.CreateProtector("OSHManagement.NotificationChannelConfig.Protection.v1");
            _logger = logger;
        }

        /// <summary>
        /// Encrypt plain text string
        /// Returns encrypted text with prefix for identification
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            // If already encrypted, return as-is
            if (IsEncrypted(plainText))
            {
                _logger.LogDebug("Text is already encrypted, skipping");
                return plainText;
            }

            try
            {
                var encrypted = _protector.Protect(plainText);
                return ENCRYPTED_PREFIX + encrypted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting data");
                throw new InvalidOperationException("Failed to encrypt data", ex);
            }
        }

        /// <summary>
        /// Decrypt cipher text back to plain text
        /// Automatically detects encrypted prefix
        /// </summary>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return cipherText;
            }

            // If not encrypted (no prefix), return as-is
            if (!IsEncrypted(cipherText))
            {
                _logger.LogWarning("Attempting to decrypt non-encrypted text");
                return cipherText;
            }

            try
            {
                // Remove prefix
                var encrypted = cipherText.Substring(ENCRYPTED_PREFIX.Length);
                return _protector.Unprotect(encrypted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting data");
                throw new InvalidOperationException("Failed to decrypt data. The encryption key may have changed.", ex);
            }
        }

        /// <summary>
        /// Check if a string is encrypted by looking for the prefix
        /// </summary>
        public bool IsEncrypted(string text)
        {
            return !string.IsNullOrEmpty(text) && text.StartsWith(ENCRYPTED_PREFIX);
        }
    }
}
