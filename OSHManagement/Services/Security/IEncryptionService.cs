namespace OSHManagement.Services.Security
{
    /// <summary>
    /// Service for encrypting and decrypting sensitive configuration data
    /// Used for protecting passwords, API keys, auth tokens, etc.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypt plain text string
        /// </summary>
        string Encrypt(string plainText);

        /// <summary>
        /// Decrypt cipher text back to plain text
        /// </summary>
        string Decrypt(string cipherText);

        /// <summary>
        /// Check if a string is already encrypted (basic validation)
        /// </summary>
        bool IsEncrypted(string text);
    }
}
