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

        // POST: Media/UploadFile - API endpoint for Dropzone
        [HttpPost]
        [RequestSizeLimit(104857600)] // 100MB max
        public async Task<IActionResult> UploadFile(
            IFormFile file,
            string collectionName,
            string? title = null,
            string? description = null,
            string? associateToTable = null,
            string? associateToRecordId = null,
            string? associationType = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, error = "No file uploaded" });
                }

                // Get collection to validate
                var collection = await _context.MediaCollections
                    .FirstOrDefaultAsync(mc => mc.CollectionName == collectionName && mc.IsActive);

                if (collection == null)
                {
                    return Json(new { success = false, error = $"Collection '{collectionName}' not found or inactive" });
                }

                // Validate file size
                var fileSizeMb = file.Length / (1024.0 * 1024.0);
                if (fileSizeMb > collection.MaxFileSizeMb)
                {
                    return Json(new { 
                        success = false, 
                        error = $"File size ({fileSizeMb:0.2}MB) exceeds maximum allowed ({collection.MaxFileSizeMb}MB)" 
                    });
                }

                // Validate file type (if restrictions exist)
                var allowedTypes = ParseJsonStringArray(collection.AllowedFileTypes);
                if (allowedTypes.Any())
                {
                    var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
                    if (!allowedTypes.Contains(extension))
                    {
                        return Json(new { 
                            success = false, 
                            error = $"File type '.{extension}' is not allowed. Allowed types: {string.Join(", ", allowedTypes)}" 
                        });
                    }
                }

                // Prepare upload options
                var uploadOptions = new MediaUploadOptions
                {
                    Title = !string.IsNullOrEmpty(title) ? title : Path.GetFileNameWithoutExtension(file.FileName),
                    Description = description
                };

                // Upload file using service
                var mediaFile = await _mediaService.UploadAsync(file, collectionName, uploadOptions);

                // Create association if specified
                if (!string.IsNullOrEmpty(associateToTable) && !string.IsNullOrEmpty(associateToRecordId))
                {
                    await _mediaService.AssociateAsync(
                        mediaFile.MediaId,
                        associateToTable,
                        associateToRecordId,
                        associationType ?? "attachment"
                    );
                }

                _logger.LogInformation($"File uploaded successfully. MediaId: {mediaFile.MediaId}, Collection: {collectionName}, User: {CurrentPayrollNo}");

                return Json(new
                {
                    success = true,
                    mediaId = mediaFile.MediaId,
                    fileName = mediaFile.OriginalFilename,
                    fileSize = FormatFileSize(mediaFile.FileSizeBytes),
                    message = "File uploaded successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file. Collection: {collectionName}, User: {CurrentPayrollNo}");
                return Json(new { success = false, error = "An error occurred while uploading the file" });
            }
        }

        // GET: Media/Upload
        public async Task<IActionResult> Upload()
        {
            // Get active collections for selector
            var collections = await _context.MediaCollections
                .Where(mc => mc.IsActive)
                .OrderBy(mc => mc.ModuleName)
                .ThenBy(mc => mc.CollectionDisplayName)
                .ToListAsync();

            // Configure uploader - MANUAL MODE for form submission
            var uploaderConfig = new MediaUploaderViewModel
            {
                AvailableCollections = collections, // For page use, not component
                MaxFileSizeMb = 10,
                ShowInline = true,
                DropzoneHeight = "400px",
                AutoProcessQueue = false, // MANUAL: User clicks Upload button
                ShowCollectionSelector = false, // Collection selector is in page, not component
                ShowMetadataFields = false // Metadata fields are in page, not component
            };

            return View(uploaderConfig);
        }

        // GET: Media/CreateCollection
        public IActionResult CreateCollection()
        {
            // Initialize empty view model with defaults
            var viewModel = new MediaCollectionViewModel
            {
                MaxFileSizeMb = 10, // Default 10MB
                IsPublic = false,
                RequiresAuthentication = true,
                IsActive = true,
                AllowedFileTypes = new List<string>(),
                AllowedRoles = new List<string>()
            };

            // Populate module dropdown options
            ViewBag.AvailableModules = new List<string>
            {
                "Teams",
                "Policies",
                "Incidents",
                "Hazards",
                "Committee",
                "Contractors",
                "General"
            };

            return View(viewModel);
        }

        // POST: Media/CreateCollection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCollection(MediaCollectionViewModel model)
        {
            try
            {
                // Generate collection name from display name (lowercase, underscores)
                var collectionName = model.CollectionDisplayName
                    .ToLower()
                    .Replace(" ", "_")
                    .Replace("-", "_")
                    .Trim();

                // Check if collection name already exists
                var existingCollection = await _context.MediaCollections
                    .FirstOrDefaultAsync(mc => mc.CollectionName == collectionName);

                if (existingCollection != null)
                {
                    TempData["Error"] = $"A collection with the name '{collectionName}' already exists. Please use a different display name.";
                    
                    // Repopulate modules for view
                    ViewBag.AvailableModules = new List<string>
                    {
                        "Teams", "Policies", "Incidents", "Hazards", "Committee", "Contractors", "General"
                    };
                    return View(model);
                }

                // Create new collection
                var newCollection = new MediaCollection
                {
                    CollectionName = collectionName,
                    CollectionDisplayName = model.CollectionDisplayName,
                    ModuleName = model.ModuleName,
                    Description = model.Description,
                    MaxFileSizeMb = model.MaxFileSizeMb,
                    AllowedFileTypes = model.AllowedFileTypes.Any() 
                        ? System.Text.Json.JsonSerializer.Serialize(model.AllowedFileTypes) 
                        : null,
                    RetentionPolicyDays = model.RetentionPolicyDays,
                    IsPublic = model.IsPublic,
                    RequiresAuthentication = model.RequiresAuthentication,
                    AllowedRoles = model.AllowedRoles.Any() 
                        ? System.Text.Json.JsonSerializer.Serialize(model.AllowedRoles) 
                        : null,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByPayroll = CurrentPayrollNo
                };

                _context.MediaCollections.Add(newCollection);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"New collection created. CollectionId: {newCollection.CollectionId}, Name: {collectionName}, User: {CurrentPayrollNo}");
                TempData["Success"] = $"Collection '{model.CollectionDisplayName}' created successfully.";
                
                return RedirectToAction(nameof(CollectionDetails), new { id = newCollection.CollectionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating collection. DisplayName: {model.CollectionDisplayName}");
                TempData["Error"] = "An error occurred while creating the collection. Please try again.";
                
                // Repopulate modules for view
                ViewBag.AvailableModules = new List<string>
                {
                    "Teams", "Policies", "Incidents", "Hazards", "Committee", "Contractors", "General"
                };
                return View(model);
            }
        }

        // GET: Media/Categories
        public async Task<IActionResult> Categories(string? search, string? status, string? module)
        {
            try
            {
                // 1. Base query - ALL collections (no scope filtering - collections are global)
                var query = _context.MediaCollections
                    .Include(mc => mc.MediaFiles)
                    .Include(mc => mc.CreatedBy)
                    .AsQueryable();

                // 2. Apply filters
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(mc =>
                        mc.CollectionDisplayName.Contains(search) ||
                        mc.CollectionName.Contains(search) ||
                        (mc.Description != null && mc.Description.Contains(search)));
                }

                if (!string.IsNullOrEmpty(status))
                {
                    bool isActive = status.ToLower() == "active";
                    query = query.Where(mc => mc.IsActive == isActive);
                }
                else
                {
                    // Default: show only active collections
                    query = query.Where(mc => mc.IsActive);
                }

                if (!string.IsNullOrEmpty(module) && module != "all")
                {
                    query = query.Where(mc => mc.ModuleName == module);
                }

                // 3. Get collections ordered by module, then name
                var collections = await query
                    .OrderBy(mc => mc.ModuleName)
                    .ThenBy(mc => mc.CollectionDisplayName)
                    .ToListAsync();

                // 4. Map to view models
                var viewModels = collections.Select(mc => new MediaCollectionViewModel
                {
                    CollectionId = mc.CollectionId,
                    CollectionName = mc.CollectionName,
                    CollectionDisplayName = mc.CollectionDisplayName,
                    ModuleName = !string.IsNullOrEmpty(mc.ModuleName) ? mc.ModuleName : InferModuleFromCollectionName(mc.CollectionName),
                    Description = mc.Description,
                    MaxFileSizeMb = mc.MaxFileSizeMb,
                    AllowedFileTypes = ParseJsonStringArray(mc.AllowedFileTypes),
                    AllowedFileTypesJson = mc.AllowedFileTypes,
                    RetentionPolicyDays = mc.RetentionPolicyDays,
                    IsPublic = mc.IsPublic,
                    RequiresAuthentication = mc.RequiresAuthentication,
                    AllowedRoles = ParseJsonStringArray(mc.AllowedRoles),
                    AllowedRolesJson = mc.AllowedRoles,
                    FileCount = mc.MediaFiles.Count(f => f.IsActive),
                    TotalSizeBytes = mc.MediaFiles.Where(f => f.IsActive).Sum(f => f.FileSizeBytes),
                    TotalSizeFormatted = FormatFileSize(mc.MediaFiles.Where(f => f.IsActive).Sum(f => f.FileSizeBytes)),
                    LastUploadDate = mc.MediaFiles.Any() ? mc.MediaFiles.Max(f => f.CreatedAt) : (DateTime?)null,
                    CreatedAt = mc.CreatedAt,
                    UpdatedAt = mc.UpdatedAt,
                    CreatedByPayroll = mc.CreatedByPayroll,
                    CreatedByName = mc.CreatedBy != null ? $"{mc.CreatedBy.FirstName} {mc.CreatedBy.LastName}" : null,
                    IsActive = mc.IsActive,
                    FileTypeIcons = GetFileTypeIcons(ParseJsonStringArray(mc.AllowedFileTypes))
                }).ToList();

                // 5. Calculate statistics for stat cards
                ViewBag.TotalCollections = collections.Count;
                ViewBag.ActiveCollections = collections.Count(c => c.IsActive);
                ViewBag.InactiveCollections = collections.Count(c => !c.IsActive);
                ViewBag.TotalFiles = collections.Sum(c => c.MediaFiles.Count(f => f.IsActive));

                // 6. Get distinct modules for filter dropdown
                var allCollections = await _context.MediaCollections.ToListAsync();
                ViewBag.AvailableModules = allCollections
                    .Where(c => !string.IsNullOrEmpty(c.ModuleName))
                    .Select(c => c.ModuleName)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                // 7. Pass filter values back to view
                ViewBag.CurrentSearch = search;
                ViewBag.CurrentStatus = status;
                ViewBag.CurrentModule = module;

                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading media categories");
                TempData["Error"] = "An error occurred while loading media categories.";
                return View(new List<MediaCollectionViewModel>());
            }
        }

        // GET: Media/CollectionDetails/{id}
        public async Task<IActionResult> CollectionDetails(int id)
        {
            try
            {
                var collection = await _context.MediaCollections
                    .Include(mc => mc.MediaFiles.Where(f => f.IsActive))
                    .Include(mc => mc.CreatedBy)
                    .FirstOrDefaultAsync(mc => mc.CollectionId == id);

                if (collection == null)
                {
                    TempData["Error"] = "Collection not found.";
                    return RedirectToAction(nameof(Categories));
                }

                // Map to view model
                var viewModel = new MediaCollectionViewModel
                {
                    CollectionId = collection.CollectionId,
                    CollectionName = collection.CollectionName,
                    CollectionDisplayName = collection.CollectionDisplayName,
                    ModuleName = !string.IsNullOrEmpty(collection.ModuleName) ? collection.ModuleName : InferModuleFromCollectionName(collection.CollectionName),
                    Description = collection.Description,
                    MaxFileSizeMb = collection.MaxFileSizeMb,
                    AllowedFileTypes = ParseJsonStringArray(collection.AllowedFileTypes),
                    AllowedFileTypesJson = collection.AllowedFileTypes,
                    RetentionPolicyDays = collection.RetentionPolicyDays,
                    IsPublic = collection.IsPublic,
                    RequiresAuthentication = collection.RequiresAuthentication,
                    AllowedRoles = ParseJsonStringArray(collection.AllowedRoles),
                    AllowedRolesJson = collection.AllowedRoles,
                    FileCount = collection.MediaFiles.Count,
                    TotalSizeBytes = collection.MediaFiles.Sum(f => f.FileSizeBytes),
                    TotalSizeFormatted = FormatFileSize(collection.MediaFiles.Sum(f => f.FileSizeBytes)),
                    LastUploadDate = collection.MediaFiles.Any() ? collection.MediaFiles.Max(f => f.CreatedAt) : (DateTime?)null,
                    CreatedAt = collection.CreatedAt,
                    UpdatedAt = collection.UpdatedAt,
                    CreatedByPayroll = collection.CreatedByPayroll,
                    CreatedByName = collection.CreatedBy != null ? $"{collection.CreatedBy.FirstName} {collection.CreatedBy.LastName}" : null,
                    IsActive = collection.IsActive,
                    FileTypeIcons = GetFileTypeIcons(ParseJsonStringArray(collection.AllowedFileTypes))
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading collection details. CollectionId: {id}");
                TempData["Error"] = "An error occurred while loading collection details.";
                return RedirectToAction(nameof(Categories));
            }
        }

        // GET: Media/EditCollection/{id}
        public async Task<IActionResult> EditCollection(int id)
        {
            try
            {
                var collection = await _context.MediaCollections
                    .FirstOrDefaultAsync(mc => mc.CollectionId == id);

                if (collection == null)
                {
                    TempData["Error"] = "Collection not found.";
                    return RedirectToAction(nameof(Categories));
                }

                var viewModel = new MediaCollectionViewModel
                {
                    CollectionId = collection.CollectionId,
                    CollectionName = collection.CollectionName,
                    CollectionDisplayName = collection.CollectionDisplayName,
                    ModuleName = collection.ModuleName,
                    Description = collection.Description,
                    MaxFileSizeMb = collection.MaxFileSizeMb,
                    AllowedFileTypes = ParseJsonStringArray(collection.AllowedFileTypes),
                    AllowedFileTypesJson = collection.AllowedFileTypes,
                    RetentionPolicyDays = collection.RetentionPolicyDays,
                    IsPublic = collection.IsPublic,
                    RequiresAuthentication = collection.RequiresAuthentication,
                    AllowedRoles = ParseJsonStringArray(collection.AllowedRoles),
                    AllowedRolesJson = collection.AllowedRoles,
                    IsActive = collection.IsActive
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading collection for edit. CollectionId: {id}");
                TempData["Error"] = "An error occurred while loading the collection.";
                return RedirectToAction(nameof(Categories));
            }
        }

        // POST: Media/EditCollection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCollection(MediaCollectionViewModel model)
        {
            try
            {
                var collection = await _context.MediaCollections
                    .FirstOrDefaultAsync(mc => mc.CollectionId == model.CollectionId);

                if (collection == null)
                {
                    TempData["Error"] = "Collection not found.";
                    return RedirectToAction(nameof(Categories));
                }

                // Update properties
                collection.CollectionDisplayName = model.CollectionDisplayName;
                collection.Description = model.Description;
                collection.MaxFileSizeMb = model.MaxFileSizeMb;
                collection.RetentionPolicyDays = model.RetentionPolicyDays;
                collection.IsPublic = model.IsPublic;
                collection.RequiresAuthentication = model.RequiresAuthentication;
                collection.UpdatedAt = DateTime.UtcNow;

                // Update JSON arrays if changed
                if (model.AllowedFileTypes.Any())
                {
                    collection.AllowedFileTypes = System.Text.Json.JsonSerializer.Serialize(model.AllowedFileTypes);
                }

                if (model.AllowedRoles.Any())
                {
                    collection.AllowedRoles = System.Text.Json.JsonSerializer.Serialize(model.AllowedRoles);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Collection updated. CollectionId: {collection.CollectionId}, User: {CurrentUserId}");
                TempData["Success"] = "Collection updated successfully.";
                return RedirectToAction(nameof(CollectionDetails), new { id = collection.CollectionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating collection. CollectionId: {model.CollectionId}");
                TempData["Error"] = "An error occurred while updating the collection.";
                return View(model);
            }
        }

        // POST: Media/DeleteCollection/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCollection(int id)
        {
            try
            {
                var collection = await _context.MediaCollections
                    .Include(mc => mc.MediaFiles)
                    .FirstOrDefaultAsync(mc => mc.CollectionId == id);

                if (collection == null)
                {
                    TempData["Error"] = "Collection not found.";
                    return RedirectToAction(nameof(Categories));
                }

                // Check if collection has files
                var activeFileCount = collection.MediaFiles.Count(f => f.IsActive);
                if (activeFileCount > 0)
                {
                    TempData["Error"] = $"Cannot delete collection with {activeFileCount} active files. Please remove or reassign files first.";
                    return RedirectToAction(nameof(Categories));
                }

                // Soft delete
                collection.IsActive = false;
                collection.DeletedAt = DateTime.UtcNow;
                collection.DeletedByPayroll = CurrentPayrollNo;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Collection soft deleted. CollectionId: {id}, User: {CurrentPayrollNo}");
                TempData["Success"] = "Collection deleted successfully.";
                return RedirectToAction(nameof(Categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting collection. CollectionId: {id}");
                TempData["Error"] = "An error occurred while deleting the collection.";
                return RedirectToAction(nameof(Categories));
            }
        }

        // Helper: Parse JSON string array
        private List<string> ParseJsonStringArray(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new List<string>();
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        // Helper: Get file type icons
        private List<FileTypeIcon> GetFileTypeIcons(List<string> extensions)
        {
            return extensions.Select(ext => new FileTypeIcon
            {
                Extension = ext,
                IconClass = FileTypeIcon.GetIconForExtension(ext),
                ColorClass = FileTypeIcon.GetColorForExtension(ext)
            }).ToList();
        }

        // Helper: Format file size
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        // Helper: Infer module name from collection name
        private string InferModuleFromCollectionName(string collectionName)
        {
            var name = collectionName.ToLower();
            
            if (name.Contains("incident")) return "Incidents";
            if (name.Contains("team")) return "Teams";
            if (name.Contains("policy") || name.Contains("osh_policy")) return "Policies";
            if (name.Contains("hazard") || name.Contains("risk")) return "Hazards";
            if (name.Contains("committee") || name.Contains("meeting") || name.Contains("training")) return "Committee";
            if (name.Contains("contractor")) return "Contractors";
            if (name.Contains("general")) return "General";
            
            return "General"; // Default
        }

        // GET: Media/Access
        public async Task<IActionResult> Access()
        {
            // Placeholder - will implement in next step
            return View();
        }
    }
}
