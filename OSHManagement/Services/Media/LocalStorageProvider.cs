using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;

namespace OSHManagement.Services.Media
{
    /// <summary>
    /// Local file system storage provider
    /// Stores files in wwwroot/uploads/ directory
    /// </summary>
    public class LocalStorageProvider : IStorageProvider
    {
        private readonly IWebHostEnvironment _environment;
        private readonly OshDbContext _context;
        private readonly ILogger<LocalStorageProvider> _logger;
        private const string UploadFolder = "uploads";

        public string ProviderName => "Local";

        public LocalStorageProvider(
            IWebHostEnvironment environment,
            OshDbContext context,
            ILogger<LocalStorageProvider> logger)
        {
            _environment = environment;
            _context = context;
            _logger = logger;
        }

        public async Task<string> StoreAsync(Stream fileStream, string fileName, string contentType, StorageContext context)
        {
            try
            {
                // Generate storage path following hybrid approach:
                // uploads/{ModuleName}/{StationId}/{YYYY-MM}/{Collection}/{guid}_{sanitized_filename}
                var datePart = context.Timestamp.ToString("yyyy-MM");
                var sanitizedFileName = SanitizeFilename(Path.GetFileName(fileName));
                var uniqueFileName = $"{Guid.NewGuid()}_{sanitizedFileName}";

                var relativePath = Path.Combine(
                    UploadFolder,
                    context.ModuleName,
                    context.StationId.ToString(),
                    datePart,
                    context.CollectionName,
                    uniqueFileName
                );

                var absolutePath = Path.Combine(_environment.WebRootPath, relativePath);

                // Create directory if it doesn't exist
                var directory = Path.GetDirectoryName(absolutePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Save file to disk
                using (var fileStreamOut = new FileStream(absolutePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(fileStreamOut);
                }

                _logger.LogInformation("File stored successfully: {Path} (Module: {Module}, Station: {Station})", 
                    relativePath, context.ModuleName, context.StationId);

                // Return relative path for database storage (normalize path separators)
                return relativePath.Replace("\\", "/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing file: {FileName} in {Module}/{Station}/{Collection}", 
                    fileName, context.ModuleName, context.StationId, context.CollectionName);
                throw;
            }
        }

        /// <summary>
        /// Sanitize filename by removing invalid characters
        /// </summary>
        private string SanitizeFilename(string filename)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(filename
                .Select(ch => invalidChars.Contains(ch) ? '_' : ch)
                .ToArray());

            // Also replace spaces with underscores and limit length
            sanitized = sanitized.Replace(" ", "_");
            
            // Preserve extension
            var extension = Path.GetExtension(sanitized);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(sanitized);
            
            // Limit name length to 100 characters
            if (nameWithoutExt.Length > 100)
            {
                nameWithoutExt = nameWithoutExt.Substring(0, 100);
            }

            return nameWithoutExt + extension;
        }

        public async Task<Stream> RetrieveAsync(string filePath)
        {
            try
            {
                var absolutePath = Path.Combine(_environment.WebRootPath, filePath.Replace("/", "\\"));

                if (!File.Exists(absolutePath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                // Return a memory stream copy to avoid file locking
                var memoryStream = new MemoryStream();
                using (var fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }
                memoryStream.Position = 0;

                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file: {FilePath}", filePath);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string filePath)
        {
            try
            {
                var absolutePath = Path.Combine(_environment.WebRootPath, filePath.Replace("/", "\\"));

                if (File.Exists(absolutePath))
                {
                    File.Delete(absolutePath);
                    _logger.LogInformation("File deleted: {FilePath}", filePath);
                    return await Task.FromResult(true);
                }

                _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
                return await Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> ExistsAsync(string filePath)
        {
            try
            {
                var absolutePath = Path.Combine(_environment.WebRootPath, filePath.Replace("/", "\\"));
                return await Task.FromResult(File.Exists(absolutePath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking file existence: {FilePath}", filePath);
                return await Task.FromResult(false);
            }
        }

        public async Task<string> GetPublicUrlAsync(string filePath, TimeSpan? expiry = null)
        {
            // For local storage, return relative URL
            // In production with cloud storage, this would return a signed URL
            var url = $"/{filePath.Replace("\\", "/")}";
            return await Task.FromResult(url);
        }

        public async Task<StorageStats> GetStatsAsync()
        {
            try
            {
                var stats = new StorageStats();

                // Get file counts and sizes from database
                var allFiles = await _context.MediaFiles
                    .Include(mf => mf.Collection)
                    .ToListAsync();

                stats.TotalFilesCount = allFiles.Count;
                stats.TotalSizeBytes = allFiles.Sum(f => f.FileSizeBytes);
                stats.ActiveFilesCount = allFiles.Count(f => f.IsActive);
                stats.ActiveSizeBytes = allFiles.Where(f => f.IsActive).Sum(f => f.FileSizeBytes);

                // Group by collection
                stats.SizeByCollection = allFiles
                    .GroupBy(f => f.Collection.CollectionName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(f => f.FileSizeBytes)
                    );

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating storage stats");
                return new StorageStats();
            }
        }
    }
}
