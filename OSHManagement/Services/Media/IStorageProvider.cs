namespace OSHManagement.Services.Media
{
    /// <summary>
    /// Abstraction for file storage providers (local, cloud, etc.)
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>
        /// Provider name (Local, Azure, AWS, etc.)
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Store file and return storage path
        /// </summary>
        /// <param name="fileStream">File content stream</param>
        /// <param name="fileName">Original or system filename</param>
        /// <param name="contentType">MIME type</param>
        /// <param name="context">Storage context (module, station, collection info)</param>
        /// <returns>Relative path to stored file</returns>
        Task<string> StoreAsync(Stream fileStream, string fileName, string contentType, StorageContext context);

        /// <summary>
        /// Retrieve file as stream
        /// </summary>
        /// <param name="filePath">Relative file path</param>
        /// <returns>File stream</returns>
        Task<Stream> RetrieveAsync(string filePath);

        /// <summary>
        /// Delete file from storage
        /// </summary>
        /// <param name="filePath">Relative file path</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteAsync(string filePath);

        /// <summary>
        /// Check if file exists
        /// </summary>
        /// <param name="filePath">Relative file path</param>
        /// <returns>True if file exists</returns>
        Task<bool> ExistsAsync(string filePath);

        /// <summary>
        /// Get public URL (with optional expiry for signed URLs)
        /// </summary>
        /// <param name="filePath">Relative file path</param>
        /// <param name="expiry">Optional expiration time for signed URLs</param>
        /// <returns>Public URL to access the file</returns>
        Task<string> GetPublicUrlAsync(string filePath, TimeSpan? expiry = null);

        /// <summary>
        /// Get storage statistics
        /// </summary>
        /// <returns>Storage usage stats</returns>
        Task<StorageStats> GetStatsAsync();
    }
}
