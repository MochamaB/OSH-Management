using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models;
using OSHManagement.Models.ViewModels;
using OSHManagement.Services;
using OSHManagement.Services.Media;

namespace OSHManagement.Controllers
{
    [Authorize]
    public class MediaController : ScopedController
    {
        private readonly IMediaService _mediaService;
        private readonly IStorageProvider _storageProvider;

        public MediaController(
            OshDbContext context,
            IScopeFilterService scopeFilter,
            IMediaService mediaService,
            IStorageProvider storageProvider,
            ILogger<MediaController> logger)
            : base(context, scopeFilter, logger)
        {
            _mediaService = mediaService;
            _storageProvider = storageProvider;
        }

        // GET: Media/Index - Document Library
        public async Task<IActionResult> Index(string? collection, string? search, string? fileType)
        {
            try
            {
                // Start with base query
                var query = _context.MediaFiles.AsQueryable();

                // ⚠️ CRITICAL: Apply scope FIRST (security takes precedence)
                query = _scopeFilter.ApplyScope(query, CurrentScope);

                // Apply includes
                query = query
                    .Include(mf => mf.Collection)
                    .Include(mf => mf.UploadedBy)
                    .Where(mf => mf.IsActive); // Only show active files

                // Apply collection filter
                if (!string.IsNullOrWhiteSpace(collection))
                {
                    query = query.Where(mf => mf.Collection.CollectionName == collection);
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(mf =>
                        mf.OriginalFilename.ToLower().Contains(search) ||
                        (mf.Title != null && mf.Title.ToLower().Contains(search)) ||
                        (mf.Description != null && mf.Description.ToLower().Contains(search))
                    );
                }

                // Apply file type filter
                if (!string.IsNullOrWhiteSpace(fileType))
                {
                    query = query.Where(mf => mf.FileExtension == fileType);
                }

                // Order by creation date (newest first)
                query = query.OrderByDescending(mf => mf.CreatedAt);

                // Execute query
                var mediaFiles = await query.ToListAsync();

                // Map to view models
                var viewModels = mediaFiles.Select(mf => new MediaFileViewModel
                {
                    MediaId = mf.MediaId,
                    OriginalFilename = mf.OriginalFilename,
                    Title = mf.Title,
                    Description = mf.Description,
                    MimeType = mf.MimeType,
                    FileSizeBytes = mf.FileSizeBytes,
                    FileExtension = mf.FileExtension,
                    FilePath = mf.FilePath ?? string.Empty,
                    CollectionName = mf.Collection.CollectionName,
                    CollectionDisplayName = mf.Collection.CollectionDisplayName,
                    ModuleName = mf.Collection.ModuleName,
                    UploadedByPayroll = mf.UploadedByPayroll,
                    UploaderName = $"{mf.UploadedBy.FirstName} {mf.UploadedBy.LastName}",
                    CreatedAt = mf.CreatedAt,
                    UpdatedAt = mf.UpdatedAt,
                    VersionNumber = mf.VersionNumber,
                    IsLatestVersion = mf.IsLatestVersion,
                    IsActive = mf.IsActive,
                    UploadStatus = mf.UploadStatus
                }).ToList();

                // Get collections summary for sidebar
                var collectionsQuery = _context.MediaCollections
                    .Where(mc => mc.IsActive)
                    .AsQueryable();

                var collections = await collectionsQuery
                    .Select(mc => new MediaCollectionSummary
                    {
                        CollectionId = mc.CollectionId,
                        CollectionName = mc.CollectionName,
                        CollectionDisplayName = mc.CollectionDisplayName,
                        ModuleName = mc.ModuleName,
                        FileCount = mc.MediaFiles.Count(mf => mf.IsActive),
                        TotalSizeBytes = mc.MediaFiles.Where(mf => mf.IsActive).Sum(mf => mf.FileSizeBytes)
                    })
                    .OrderBy(mc => mc.ModuleName)
                    .ThenBy(mc => mc.CollectionDisplayName)
                    .ToListAsync();

                // Get storage stats
                var stats = await _storageProvider.GetStatsAsync();
                var storageStats = new MediaStorageStats
                {
                    TotalFilesCount = stats.TotalFilesCount,
                    ActiveFilesCount = stats.ActiveFilesCount,
                    TotalSizeBytes = stats.TotalSizeBytes,
                    ActiveSizeBytes = stats.ActiveSizeBytes
                };

                // Build library view model
                var libraryViewModel = new MediaLibraryViewModel
                {
                    Files = viewModels,
                    Collections = collections,
                    StorageStats = storageStats,
                    CurrentCollection = collection,
                    SearchTerm = search
                };

                return View(libraryViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading media library");
                TempData["Error"] = "An error occurred while loading the media library.";
                return View(new MediaLibraryViewModel());
            }
        }

        // GET: Media/Download/{id}
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                // Get file with scope filtering
                var query = _context.MediaFiles
                    .Include(mf => mf.Collection)
                    .Where(mf => mf.MediaId == id && mf.IsActive);

                // ⚠️ CRITICAL: Apply scope filtering
                query = _scopeFilter.ApplyScope(query, CurrentScope);

                var mediaFile = await query.FirstOrDefaultAsync();

                if (mediaFile == null)
                {
                    _logger.LogWarning($"File not found or access denied. MediaId: {id}, User: {CurrentUserId}");
                    return NotFound("File not found or you don't have permission to access it.");
                }

                // Get file content from storage
                var fileStream = await _storageProvider.RetrieveAsync(mediaFile.FilePath!);

                if (fileStream == null)
                {
                    _logger.LogError($"File content not found on disk. MediaId: {id}, Path: {mediaFile.FilePath}");
                    return NotFound("File content not found.");
                }

                // Convert stream to bytes for download
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                // Return file for download
                return File(fileBytes, mediaFile.MimeType ?? "application/octet-stream", mediaFile.OriginalFilename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading file. MediaId: {id}");
                return StatusCode(500, "An error occurred while downloading the file.");
            }
        }

        // GET: Media/View/{id}
        public async Task<IActionResult> View(int id)
        {
            try
            {
                // Get file with scope filtering
                var query = _context.MediaFiles
                    .Include(mf => mf.Collection)
                    .Where(mf => mf.MediaId == id && mf.IsActive);

                // ⚠️ CRITICAL: Apply scope filtering
                query = _scopeFilter.ApplyScope(query, CurrentScope);

                var mediaFile = await query.FirstOrDefaultAsync();

                if (mediaFile == null)
                {
                    _logger.LogWarning($"File not found or access denied. MediaId: {id}, User: {CurrentUserId}");
                    return NotFound("File not found or you don't have permission to access it.");
                }

                // Get file content from storage
                var fileStream = await _storageProvider.RetrieveAsync(mediaFile.FilePath!);

                if (fileStream == null)
                {
                    _logger.LogError($"File content not found on disk. MediaId: {id}, Path: {mediaFile.FilePath}");
                    return NotFound("File content not found.");
                }

                // Convert stream to bytes for viewing
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                // Return file for inline viewing (browser will decide how to display)
                return File(fileBytes, mediaFile.MimeType ?? "application/octet-stream");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error viewing file. MediaId: {id}");
                return StatusCode(500, "An error occurred while viewing the file.");
            }
        }

        // GET: Media/GetFile/{id} - Serves file content for displaying in page
        public async Task<IActionResult> GetFile(int id)
        {
            try
            {
                // Get file with scope filtering
                var query = _context.MediaFiles
                    .Where(mf => mf.MediaId == id && mf.IsActive);

                // ⚠️ CRITICAL: Apply scope filtering
                query = _scopeFilter.ApplyScope(query, CurrentScope);

                var mediaFile = await query.FirstOrDefaultAsync();

                if (mediaFile == null)
                {
                    return NotFound();
                }

                // Get file content from storage
                var fileStream = await _storageProvider.RetrieveAsync(mediaFile.FilePath!);

                if (fileStream == null)
                {
                    return NotFound();
                }

                // Determine content type for inline display
                var contentType = mediaFile.MimeType ?? "application/octet-stream";
                
                // Add content disposition for inline viewing
                Response.Headers.Add("Content-Disposition", $"inline; filename=\"{mediaFile.OriginalFilename}\"");

                return File(fileStream, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error serving file. MediaId: {id}");
                return StatusCode(500);
            }
        }

        // GET: Media/Upload
        public async Task<IActionResult> Upload()
        {
            // Placeholder - will implement in next step
            return View();
        }

        // GET: Media/Categories
        public async Task<IActionResult> Categories()
        {
            // Placeholder - will implement in next step
            return View();
        }

        // GET: Media/Access
        public async Task<IActionResult> Access()
        {
            // Placeholder - will implement in next step
            return View();
        }
    }
}
