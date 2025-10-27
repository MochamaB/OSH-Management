using Microsoft.AspNetCore.Http;
using OSHManagement.Models;

namespace OSHManagement.Services.Media
{
    /// <summary>
    /// Service interface for media file management
    /// </summary>
    public interface IMediaService
    {
        // ========================================
        // File Upload & Management
        // ========================================

        /// <summary>
        /// Upload a file to a specified collection
        /// </summary>
        Task<MediaFile> UploadAsync(
            IFormFile file,
            string collectionName,
            MediaUploadOptions? options = null);

        /// <summary>
        /// Upload multiple files at once
        /// </summary>
        Task<List<MediaFile>> UploadMultipleAsync(
            IFormFileCollection files,
            string collectionName,
            MediaUploadOptions? options = null);

        /// <summary>
        /// Get media file by ID
        /// </summary>
        Task<MediaFile?> GetByIdAsync(int mediaId);

        /// <summary>
        /// Get file stream for download
        /// </summary>
        Task<Stream> GetFileStreamAsync(int mediaId);

        /// <summary>
        /// Update file metadata (title, description, etc.)
        /// </summary>
        Task<MediaFile> UpdateMetadataAsync(
            int mediaId,
            MediaMetadataUpdate metadata);

        /// <summary>
        /// Soft delete a file (sets IsActive = false)
        /// </summary>
        Task<bool> SoftDeleteAsync(int mediaId, string deletedByPayroll);

        /// <summary>
        /// Permanently delete a file (removes from storage)
        /// </summary>
        Task<bool> HardDeleteAsync(int mediaId);

        /// <summary>
        /// Restore a soft-deleted file
        /// </summary>
        Task<bool> RestoreAsync(int mediaId);

        // ========================================
        // File Associations (Polymorphic)
        // ========================================

        /// <summary>
        /// Associate a file with any entity
        /// </summary>
        Task<MediaAssociation> AssociateAsync(
            int mediaId,
            string tableName,
            string recordId,
            string associationType,
            MediaAssociationOptions? options = null);

        /// <summary>
        /// Remove association (does not delete file)
        /// </summary>
        Task<bool> DisassociateAsync(
            int mediaId,
            string tableName,
            string recordId,
            string? associationType = null);

        /// <summary>
        /// Get all files associated with an entity
        /// </summary>
        Task<List<MediaFile>> GetByAssociationAsync(
            string tableName,
            string recordId,
            string? associationType = null);

        /// <summary>
        /// Check if entity has associated files
        /// </summary>
        Task<bool> HasAssociationsAsync(
            string tableName,
            string recordId);

        // ========================================
        // Collection Management
        // ========================================

        /// <summary>
        /// Get all files in a collection
        /// </summary>
        Task<List<MediaFile>> GetByCollectionAsync(
            string collectionName,
            MediaFileFilter? filter = null);

        /// <summary>
        /// Get collection configuration
        /// </summary>
        Task<MediaCollection?> GetCollectionAsync(string collectionName);

        /// <summary>
        /// Get all available collections for current user
        /// </summary>
        Task<List<MediaCollection>> GetAvailableCollectionsAsync();

        // ========================================
        // Orphaned Files Management
        // ========================================

        /// <summary>
        /// Get files that have no associations
        /// </summary>
        Task<List<MediaFile>> GetOrphanedFilesAsync(
            string? collectionName = null,
            int daysOld = 0);

        /// <summary>
        /// Cleanup orphaned files older than specified days
        /// </summary>
        Task<int> CleanupOrphanedFilesAsync(int daysOld = 90);

        // ========================================
        // Version Control
        // ========================================

        /// <summary>
        /// Create new version of existing file
        /// </summary>
        Task<MediaFile> CreateVersionAsync(
            int parentMediaId,
            IFormFile newFile,
            string? versionNote = null);

        /// <summary>
        /// Get all versions of a file
        /// </summary>
        Task<List<MediaFile>> GetVersionHistoryAsync(int mediaId);

        /// <summary>
        /// Revert to a previous version (makes it the latest)
        /// </summary>
        Task<MediaFile> RevertToVersionAsync(int versionMediaId);

        // ========================================
        // Search & Filtering
        // ========================================

        /// <summary>
        /// Search files by criteria (with scope filtering)
        /// </summary>
        Task<List<MediaFile>> SearchAsync(MediaSearchCriteria criteria);

        /// <summary>
        /// Get user's upload history
        /// </summary>
        Task<List<MediaFile>> GetUserUploadsAsync(
            string payrollNo,
            int? limit = null);
    }
}
