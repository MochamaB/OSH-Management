using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Models.Authorization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OSHManagement.Services.Media
{
    /// <summary>
    /// Core media management service implementation
    /// </summary>
    public class MediaService : IMediaService
    {
        private readonly OshDbContext _context;
        private readonly IStorageProvider _storageProvider;
        private readonly IUserScopeService _userScopeService;
        private readonly ILogger<MediaService> _logger;

        public MediaService(
            OshDbContext context,
            IStorageProvider storageProvider,
            IUserScopeService userScopeService,
            ILogger<MediaService> logger)
        {
            _context = context;
            _storageProvider = storageProvider;
            _userScopeService = userScopeService;
            _logger = logger;
        }

        // ========================================
        // File Upload & Management
        // ========================================

        public async Task<MediaFile> UploadAsync(
            IFormFile file,
            string collectionName,
            MediaUploadOptions? options = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null");

            options ??= new MediaUploadOptions();

            // Get collection
            var collection = await _context.MediaCollections
                .FirstOrDefaultAsync(c => c.CollectionName == collectionName && c.IsActive);

            if (collection == null)
                throw new InvalidOperationException($"Collection '{collectionName}' not found");

            // Validate file size
            var fileSizeMb = file.Length / (1024.0 * 1024.0);
            if (fileSizeMb > collection.MaxFileSizeMb)
            {
                throw new InvalidOperationException(
                    $"File size ({fileSizeMb:F2} MB) exceeds maximum allowed ({collection.MaxFileSizeMb} MB)");
            }

            // Validate file type
            if (!string.IsNullOrEmpty(collection.AllowedFileTypes))
            {
                var allowedTypes = JsonSerializer.Deserialize<List<string>>(collection.AllowedFileTypes);
                if (allowedTypes != null && !allowedTypes.Contains(file.ContentType))
                {
                    throw new InvalidOperationException(
                        $"File type '{file.ContentType}' is not allowed for this collection");
                }
            }

            // Calculate file hash for duplicate detection
            string fileHash;
            using (var stream = file.OpenReadStream())
            {
                fileHash = await CalculateFileHashAsync(stream);
                stream.Position = 0; // Reset stream for storage
            }

            // Check for duplicates if not allowed
            if (!options.AllowDuplicates)
            {
                var duplicate = await _context.MediaFiles
                    .FirstOrDefaultAsync(mf => mf.FileHash == fileHash && mf.IsActive);

                if (duplicate != null)
                {
                    _logger.LogInformation("Duplicate file detected: {Hash}", fileHash);
                    return duplicate;
                }
            }

            // Determine station ID (from options or current user scope)
            var stationId = options.StationId ?? GetCurrentUserStationId() ?? 0;
            if (stationId == 0)
            {
                throw new InvalidOperationException("Station ID is required for file storage. Please provide it in upload options or ensure user has station context.");
            }

            // Build storage context for hybrid path structure
            var storageContext = new StorageContext
            {
                ModuleName = collection.ModuleName ?? "general",
                StationId = stationId,
                CollectionName = collection.CollectionName,
                Timestamp = DateTime.UtcNow
            };

            // Store file
            string filePath;
            using (var stream = file.OpenReadStream())
            {
                filePath = await _storageProvider.StoreAsync(stream, file.FileName, file.ContentType, storageContext);
            }

            // Create database record
            var mediaFile = new MediaFile
            {
                CollectionId = collection.CollectionId,
                OriginalFilename = file.FileName,
                SystemFilename = Path.GetFileName(filePath),
                FileHash = fileHash,
                MimeType = file.ContentType,
                FileSizeBytes = file.Length,
                FileExtension = Path.GetExtension(file.FileName),
                FilePath = filePath,
                StorageProvider = _storageProvider.ProviderName,
                Title = options.Title ?? Path.GetFileNameWithoutExtension(file.FileName),
                Description = options.Description,
                AltText = options.AltText,
                CustomProperties = options.CustomProperties != null
                    ? JsonSerializer.Serialize(options.CustomProperties)
                    : null,
                UploadStatus = "Complete",
                UploadedByPayroll = options.UploadedByPayroll ?? "SYSTEM",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.MediaFiles.Add(mediaFile);
            await _context.SaveChangesAsync();

            _logger.LogInformation("File uploaded successfully: {MediaId} - {FileName}",
                mediaFile.MediaId, mediaFile.OriginalFilename);

            return mediaFile;
        }

        public async Task<List<MediaFile>> UploadMultipleAsync(
            IFormFileCollection files,
            string collectionName,
            MediaUploadOptions? options = null)
        {
            var uploadedFiles = new List<MediaFile>();

            foreach (var file in files)
            {
                try
                {
                    var mediaFile = await UploadAsync(file, collectionName, options);
                    uploadedFiles.Add(mediaFile);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
                    // Continue with other files
                }
            }

            return uploadedFiles;
        }

        public async Task<MediaFile?> GetByIdAsync(int mediaId)
        {
            return await _context.MediaFiles
                .Include(mf => mf.Collection)
                .Include(mf => mf.UploadedBy)
                .Include(mf => mf.Associations)
                .FirstOrDefaultAsync(mf => mf.MediaId == mediaId);
        }

        public async Task<Stream> GetFileStreamAsync(int mediaId)
        {
            var mediaFile = await GetByIdAsync(mediaId);

            if (mediaFile == null)
                throw new FileNotFoundException($"Media file with ID {mediaId} not found");

            if (!mediaFile.IsActive)
                throw new InvalidOperationException("File has been deleted");

            return await _storageProvider.RetrieveAsync(mediaFile.FilePath!);
        }

        public async Task<MediaFile> UpdateMetadataAsync(int mediaId, MediaMetadataUpdate metadata)
        {
            var mediaFile = await GetByIdAsync(mediaId);

            if (mediaFile == null)
                throw new InvalidOperationException($"Media file with ID {mediaId} not found");

            // Update fields
            if (metadata.Title != null)
                mediaFile.Title = metadata.Title;

            if (metadata.Description != null)
                mediaFile.Description = metadata.Description;

            if (metadata.AltText != null)
                mediaFile.AltText = metadata.AltText;

            if (metadata.CustomProperties != null)
                mediaFile.CustomProperties = JsonSerializer.Serialize(metadata.CustomProperties);

            mediaFile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Metadata updated for file: {MediaId}", mediaId);

            return mediaFile;
        }

        public async Task<bool> SoftDeleteAsync(int mediaId, string deletedByPayroll)
        {
            var mediaFile = await GetByIdAsync(mediaId);

            if (mediaFile == null)
                return false;

            mediaFile.IsActive = false;
            mediaFile.DeletedAt = DateTime.UtcNow;
            mediaFile.DeletedByPayroll = deletedByPayroll;

            await _context.SaveChangesAsync();

            _logger.LogInformation("File soft deleted: {MediaId} by {User}", mediaId, deletedByPayroll);

            return true;
        }

        public async Task<bool> HardDeleteAsync(int mediaId)
        {
            var mediaFile = await GetByIdAsync(mediaId);

            if (mediaFile == null)
                return false;

            // Delete from storage
            await _storageProvider.DeleteAsync(mediaFile.FilePath!);

            // Delete from database
            _context.MediaFiles.Remove(mediaFile);
            await _context.SaveChangesAsync();

            _logger.LogInformation("File hard deleted: {MediaId}", mediaId);

            return true;
        }

        public async Task<bool> RestoreAsync(int mediaId)
        {
            var mediaFile = await _context.MediaFiles
                .FirstOrDefaultAsync(mf => mf.MediaId == mediaId);

            if (mediaFile == null)
                return false;

            mediaFile.IsActive = true;
            mediaFile.DeletedAt = null;
            mediaFile.DeletedByPayroll = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation("File restored: {MediaId}", mediaId);

            return true;
        }

        // ========================================
        // File Associations (Polymorphic)
        // ========================================

        public async Task<MediaAssociation> AssociateAsync(
            int mediaId,
            string tableName,
            string recordId,
            string associationType,
            MediaAssociationOptions? options = null)
        {
            options ??= new MediaAssociationOptions();

            // Check if media file exists
            var mediaFile = await GetByIdAsync(mediaId);
            if (mediaFile == null)
                throw new InvalidOperationException($"Media file with ID {mediaId} not found");

            // Check for existing association
            var existing = await _context.MediaAssociations
                .FirstOrDefaultAsync(ma =>
                    ma.MediaId == mediaId &&
                    ma.AssociatedTable == tableName &&
                    ma.AssociatedRecordId == recordId &&
                    ma.AssociationType == associationType &&
                    ma.IsActive);

            if (existing != null)
            {
                _logger.LogInformation("Association already exists: {AssociationId}", existing.AssociationId);
                return existing;
            }

            // Create new association
            var association = new MediaAssociation
            {
                MediaId = mediaId,
                AssociatedTable = tableName,
                AssociatedRecordId = recordId,
                AssociationType = associationType,
                AssociationLabel = options.AssociationLabel,
                DisplayOrder = options.DisplayOrder,
                IsPrimary = options.IsPrimary,
                IsRequired = options.IsRequired,
                MaxFilesAllowed = options.MaxFilesAllowed,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.MediaAssociations.Add(association);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Association created: {AssociationId} - {Table}.{RecordId}",
                association.AssociationId, tableName, recordId);

            return association;
        }

        public async Task<bool> DisassociateAsync(
            int mediaId,
            string tableName,
            string recordId,
            string? associationType = null)
        {
            var query = _context.MediaAssociations
                .Where(ma =>
                    ma.MediaId == mediaId &&
                    ma.AssociatedTable == tableName &&
                    ma.AssociatedRecordId == recordId &&
                    ma.IsActive);

            if (!string.IsNullOrEmpty(associationType))
            {
                query = query.Where(ma => ma.AssociationType == associationType);
            }

            var associations = await query.ToListAsync();

            if (!associations.Any())
                return false;

            // Soft delete associations
            foreach (var association in associations)
            {
                association.IsActive = false;
                association.DeletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Disassociated {Count} associations for media {MediaId}",
                associations.Count, mediaId);

            return true;
        }

        public async Task<List<MediaFile>> GetByAssociationAsync(
            string tableName,
            string recordId,
            string? associationType = null)
        {
            var query = _context.MediaAssociations
                .Where(ma =>
                    ma.AssociatedTable == tableName &&
                    ma.AssociatedRecordId == recordId &&
                    ma.IsActive)
                .Include(ma => ma.Media)
                    .ThenInclude(m => m.Collection)
                .AsQueryable();

            if (!string.IsNullOrEmpty(associationType))
            {
                query = query.Where(ma => ma.AssociationType == associationType);
            }

            var associations = await query
                .OrderBy(ma => ma.DisplayOrder)
                .ToListAsync();

            return associations
                .Select(ma => ma.Media)
                .Where(m => m.IsActive)
                .ToList();
        }

        public async Task<bool> HasAssociationsAsync(string tableName, string recordId)
        {
            return await _context.MediaAssociations
                .AnyAsync(ma =>
                    ma.AssociatedTable == tableName &&
                    ma.AssociatedRecordId == recordId &&
                    ma.IsActive);
        }

        // ========================================
        // Collection Management
        // ========================================

        public async Task<List<MediaFile>> GetByCollectionAsync(
            string collectionName,
            MediaFileFilter? filter = null)
        {
            var collection = await _context.MediaCollections
                .FirstOrDefaultAsync(c => c.CollectionName == collectionName);

            if (collection == null)
                return new List<MediaFile>();

            var query = _context.MediaFiles
                .Where(mf => mf.CollectionId == collection.CollectionId)
                .Include(mf => mf.UploadedBy)
                .AsQueryable();

            if (filter != null)
            {
                if (filter.IsActive.HasValue)
                    query = query.Where(mf => mf.IsActive == filter.IsActive.Value);

                if (filter.UploadedAfter.HasValue)
                    query = query.Where(mf => mf.CreatedAt >= filter.UploadedAfter.Value);

                if (filter.UploadedBefore.HasValue)
                    query = query.Where(mf => mf.CreatedAt <= filter.UploadedBefore.Value);

                if (!string.IsNullOrEmpty(filter.UploadedByPayroll))
                    query = query.Where(mf => mf.UploadedByPayroll == filter.UploadedByPayroll);

                if (filter.FileExtensions != null && filter.FileExtensions.Any())
                    query = query.Where(mf => filter.FileExtensions.Contains(mf.FileExtension!));

                if (filter.MaxFileSizeBytes.HasValue)
                    query = query.Where(mf => mf.FileSizeBytes <= filter.MaxFileSizeBytes.Value);

                if (filter.Offset.HasValue)
                    query = query.Skip(filter.Offset.Value);

                if (filter.Limit.HasValue)
                    query = query.Take(filter.Limit.Value);
            }

            return await query.OrderByDescending(mf => mf.CreatedAt).ToListAsync();
        }

        public async Task<MediaCollection?> GetCollectionAsync(string collectionName)
        {
            return await _context.MediaCollections
                .FirstOrDefaultAsync(c => c.CollectionName == collectionName && c.IsActive);
        }

        public async Task<List<MediaCollection>> GetAvailableCollectionsAsync()
        {
            return await _context.MediaCollections
                .Where(c => c.IsActive)
                .OrderBy(c => c.ModuleName)
                .ThenBy(c => c.CollectionDisplayName)
                .ToListAsync();
        }

        // ========================================
        // Orphaned Files Management
        // ========================================

        public async Task<List<MediaFile>> GetOrphanedFilesAsync(
            string? collectionName = null,
            int daysOld = 0)
        {
            var query = _context.MediaFiles
                .Where(mf => mf.IsActive && !mf.Associations.Any(a => a.IsActive))
                .AsQueryable();

            if (!string.IsNullOrEmpty(collectionName))
            {
                var collection = await GetCollectionAsync(collectionName);
                if (collection != null)
                {
                    query = query.Where(mf => mf.CollectionId == collection.CollectionId);
                }
            }

            if (daysOld > 0)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
                query = query.Where(mf => mf.CreatedAt < cutoffDate);
            }

            return await query
                .Include(mf => mf.Collection)
                .OrderBy(mf => mf.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> CleanupOrphanedFilesAsync(int daysOld = 90)
        {
            var orphanedFiles = await GetOrphanedFilesAsync(null, daysOld);
            var deletedCount = 0;

            foreach (var file in orphanedFiles)
            {
                try
                {
                    await HardDeleteAsync(file.MediaId);
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting orphaned file: {MediaId}", file.MediaId);
                }
            }

            _logger.LogInformation("Cleaned up {Count} orphaned files", deletedCount);

            return deletedCount;
        }

        // ========================================
        // Version Control
        // ========================================

        public async Task<MediaFile> CreateVersionAsync(
            int parentMediaId,
            IFormFile newFile,
            string? versionNote = null)
        {
            var parentFile = await GetByIdAsync(parentMediaId);

            if (parentFile == null)
                throw new InvalidOperationException($"Parent media file with ID {parentMediaId} not found");

            // Mark parent as not latest
            parentFile.IsLatestVersion = false;
            parentFile.UpdatedAt = DateTime.UtcNow;

            // Get next version number
            var maxVersion = await _context.MediaFiles
                .Where(mf => mf.ParentMediaId == parentMediaId || mf.MediaId == parentMediaId)
                .MaxAsync(mf => (int?)mf.VersionNumber) ?? parentFile.VersionNumber;

            // Upload new version (preserve station from parent file's uploader)
            var options = new MediaUploadOptions
            {
                Title = parentFile.Title,
                Description = versionNote ?? parentFile.Description,
                UploadedByPayroll = parentFile.UploadedByPayroll,
                StationId = parentFile.UploadedBy.StationId  // Preserve station from parent
            };

            var newVersion = await UploadAsync(newFile, parentFile.Collection.CollectionName, options);

            // Set version info
            newVersion.ParentMediaId = parentMediaId;
            newVersion.VersionNumber = maxVersion + 1;
            newVersion.IsLatestVersion = true;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Created version {Version} for media {ParentId}",
                newVersion.VersionNumber, parentMediaId);

            return newVersion;
        }

        public async Task<List<MediaFile>> GetVersionHistoryAsync(int mediaId)
        {
            var file = await GetByIdAsync(mediaId);

            if (file == null)
                return new List<MediaFile>();

            // Get root parent
            var rootId = file.ParentMediaId ?? file.MediaId;

            return await _context.MediaFiles
                .Where(mf => mf.MediaId == rootId || mf.ParentMediaId == rootId)
                .OrderByDescending(mf => mf.VersionNumber)
                .ToListAsync();
        }

        public async Task<MediaFile> RevertToVersionAsync(int versionMediaId)
        {
            var version = await GetByIdAsync(versionMediaId);

            if (version == null)
                throw new InvalidOperationException($"Version with ID {versionMediaId} not found");

            // Mark all versions as not latest
            var rootId = version.ParentMediaId ?? version.MediaId;
            var allVersions = await _context.MediaFiles
                .Where(mf => mf.MediaId == rootId || mf.ParentMediaId == rootId)
                .ToListAsync();

            foreach (var v in allVersions)
            {
                v.IsLatestVersion = false;
            }

            // Mark specified version as latest
            version.IsLatestVersion = true;
            version.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reverted to version {MediaId}", versionMediaId);

            return version;
        }

        // ========================================
        // Search & Filtering
        // ========================================

        public async Task<List<MediaFile>> SearchAsync(MediaSearchCriteria criteria)
        {
            var query = _context.MediaFiles
                .Include(mf => mf.Collection)
                .Include(mf => mf.UploadedBy)
                .AsQueryable();

            // Apply filters
            if (!criteria.IncludeInactive)
                query = query.Where(mf => mf.IsActive);

            if (!string.IsNullOrEmpty(criteria.SearchTerm))
            {
                var searchLower = criteria.SearchTerm.ToLower();
                query = query.Where(mf =>
                    mf.OriginalFilename.ToLower().Contains(searchLower) ||
                    (mf.Title != null && mf.Title.ToLower().Contains(searchLower)) ||
                    (mf.Description != null && mf.Description.ToLower().Contains(searchLower)));
            }

            if (!string.IsNullOrEmpty(criteria.CollectionName))
            {
                query = query.Where(mf => mf.Collection.CollectionName == criteria.CollectionName);
            }

            if (!string.IsNullOrEmpty(criteria.FileExtension))
            {
                query = query.Where(mf => mf.FileExtension == criteria.FileExtension);
            }

            if (criteria.UploadedAfter.HasValue)
            {
                query = query.Where(mf => mf.CreatedAt >= criteria.UploadedAfter.Value);
            }

            if (criteria.UploadedBefore.HasValue)
            {
                query = query.Where(mf => mf.CreatedAt <= criteria.UploadedBefore.Value);
            }

            if (!string.IsNullOrEmpty(criteria.UploadedByPayroll))
            {
                query = query.Where(mf => mf.UploadedByPayroll == criteria.UploadedByPayroll);
            }

            // Pagination
            var skip = (criteria.PageNumber - 1) * criteria.PageSize;
            query = query.Skip(skip).Take(criteria.PageSize);

            return await query.OrderByDescending(mf => mf.CreatedAt).ToListAsync();
        }

        public async Task<List<MediaFile>> GetUserUploadsAsync(string payrollNo, int? limit = null)
        {
            var query = _context.MediaFiles
                .Where(mf => mf.UploadedByPayroll == payrollNo && mf.IsActive)
                .Include(mf => mf.Collection)
                .OrderByDescending(mf => mf.CreatedAt)
                .AsQueryable();

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            return await query.ToListAsync();
        }

        // ========================================
        // Helper Methods
        // ========================================

        private async Task<string> CalculateFileHashAsync(Stream stream)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            return Convert.ToHexString(hashBytes);
        }

        /// <summary>
        /// Get station ID from current user scope (if available)
        /// </summary>
        private int? GetCurrentUserStationId()
        {
            try
            {
                var userScope = _userScopeService.GetCurrentUserScope();
                return userScope?.StationId;
            }
            catch
            {
                return null;
            }
        }
    }
}
