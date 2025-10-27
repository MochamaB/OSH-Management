# Media Management System - Implementation Guide

## Document Overview
**Purpose**: Implementation guide for Media/File Management system in OSHManagement
**Created**: 2025-10-23
**Updated**: 2025-10-23
**Status**: ✅ Database Migrations Complete - Models Updated
**Target**: Production-ready file management with scope-based security

---

## Table of Contents
1. [Executive Summary](#executive-summary)
2. [Implementation Status](#implementation-status)
3. [Updated Models Reference](#updated-models-reference)
4. [Service Layer Architecture](#service-layer-architecture)
5. [Scope-Based Access Control](#scope-based-access-control)
6. [File Storage Strategy](#file-storage-strategy)
7. [File Manager Module Design](#file-manager-module-design)
8. [Next Implementation Steps](#next-implementation-steps)
9. [Testing Strategy](#testing-strategy)

---

## Executive Summary

### Problem Statement
The OSHManagement system needs a centralized, secure, and scalable file management system that:
- Handles document uploads for ALL modules (Teams, Policies, Incidents, Hazards, etc.)
- Supports polymorphic relationships (attach files to ANY entity)
- Implements scope-based security (Station/Department level restrictions)
- Allows "upload first, associate later" workflow
- Provides a dedicated File Manager UI for browsing and managing files

### Solution Overview
**Polymorphic Media Association Pattern**
```
MediaFile (core file metadata)
    ↓
MediaAssociation (polymorphic pivot table)
    ↓
ANY Entity (Team, Policy, Incident, etc.)
```

### Key Features
✅ **Polymorphic Relationships** - One system serves all modules
✅ **Scope-Based Security** - Station/Department/Team filtering
✅ **Orphaned File Support** - Upload without immediate association
✅ **Version Control** - Document revision history
✅ **Multiple Storage Providers** - Local/Cloud abstraction
✅ **Audit Trail** - Complete access logging
✅ **File Manager UI** - Standalone module for file management

---

## Implementation Status

### ✅ Completed Items

#### **Database Migrations** (All Run Successfully)
1. ✅ Script 1: Changed `MediaAssociation.AssociatedRecordId` from `int` to `string` (polymorphic support)
2. ✅ Script 2: Added missing columns (soft delete, audit fields, metadata)
3. ✅ Script 3: Created performance indexes (coming next)
4. ✅ Script 4: Seeded 10 default collections

#### **Model Updates** (Synced with Database)
- ✅ `MediaFile` - Added AltText, CustomProperties, IsActive, ProcessingStatus, DeletedAt, DeletedByPayroll
- ✅ `MediaCollection` - Added CreatedByPayroll, IsActive, DeletedAt, DeletedByPayroll
- ✅ `MediaAssociation` - Changed AssociatedRecordId to string, added audit/soft delete fields

#### **Default Collections Created**
1. `team_icons` - Team Icons & Logos
2. `team_documents` - Team Documents
3. `osh_policy_documents` - OSH Policy Documents
4. `incident_evidence` - Incident Evidence
5. `incident_reports` - Incident Reports
6. `risk_assessment_reports` - Risk Assessment Reports
7. `committee_training_certificates` - Training Certificates
8. `committee_meeting_minutes` - Meeting Minutes
9. `contractor_safety_charters` - Contractor Safety Charters
10. `general_uploads` - General Uploads (orphaned files)

### 🚧 Remaining Tasks

1. ⏳ Add DbContext configuration for Media tables
2. ⏳ Implement scope filtering for Media entities
3. ⏳ Create service layer (IMediaService, MediaService)
4. ⏳ Implement storage provider (LocalStorageProvider)
5. ⏳ Create MediaController
6. ⏳ Build File Manager UI
7. ⏳ Integrate file uploads into existing forms

---

## Updated Models Reference

### MediaAssociation.cs (✅ Updated)

**Key Changes**:
- ✅ `AssociatedRecordId`: `int` → `string` (supports any PK type)
- ✅ Added audit fields: `CreatedByPayroll`, `CreatedBy` navigation
- ✅ Added soft delete: `IsActive`, `DeletedAt`, `DeletedByPayroll`, `DeletedBy` navigation

```csharp
using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class MediaAssociation
    {
        [Key]
        public int AssociationId { get; set; }

        public int MediaId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AssociatedTable { get; set; } = string.Empty;

        // Changed from int to string for polymorphic support
        [Required]
        [MaxLength(100)]
        public string AssociatedRecordId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string AssociationType { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? AssociationLabel { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsRequired { get; set; }
        public int? MaxFilesAllowed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Audit fields
        [MaxLength(20)]
        public string? CreatedByPayroll { get; set; }

        // Soft delete support
        public bool IsActive { get; set; } = true;
        public DateTime? DeletedAt { get; set; }

        [MaxLength(20)]
        public string? DeletedByPayroll { get; set; }

        // Navigation properties
        public MediaFile Media { get; set; } = null!;
        public Employee? CreatedBy { get; set; }
        public Employee? DeletedBy { get; set; }
    }
}
```

### MediaCollection.cs (✅ Updated)

**Key Changes**:
- ✅ Added audit fields: `CreatedByPayroll`, `CreatedBy` navigation
- ✅ Added soft delete: `IsActive`, `DeletedAt`, `DeletedByPayroll`, `DeletedBy` navigation

```csharp
using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class MediaCollection
    {
        [Key]
        public int CollectionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CollectionName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string CollectionDisplayName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ModuleName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int MaxFileSizeMb { get; set; }
        public string? AllowedFileTypes { get; set; }  // JSON array
        public int? RetentionPolicyDays { get; set; }

        public bool IsPublic { get; set; }
        public bool RequiresAuthentication { get; set; }
        public string? AllowedRoles { get; set; }  // JSON array

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Audit fields
        [MaxLength(20)]
        public string? CreatedByPayroll { get; set; }

        // Soft delete support
        public bool IsActive { get; set; } = true;
        public DateTime? DeletedAt { get; set; }

        [MaxLength(20)]
        public string? DeletedByPayroll { get; set; }

        // Navigation properties
        public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();
        public Employee? CreatedBy { get; set; }
        public Employee? DeletedBy { get; set; }
    }
}
```

### MediaFile.cs (✅ Updated)

**Key Changes**:
- ✅ Added accessibility: `AltText`
- ✅ Added extensibility: `CustomProperties` (JSON)
- ✅ Added processing: `ProcessingStatus`
- ✅ Added soft delete: `IsActive`, `DeletedAt`, `DeletedByPayroll`, `DeletedBy` navigation

```csharp
using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class MediaFile
    {
        [Key]
        public int MediaId { get; set; }

        public int CollectionId { get; set; }

        [Required]
        [MaxLength(255)]
        public string OriginalFilename { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string SystemFilename { get; set; } = string.Empty;

        [MaxLength(64)]
        public string? FileHash { get; set; }

        [MaxLength(100)]
        public string? MimeType { get; set; }

        public long FileSizeBytes { get; set; }

        [MaxLength(10)]
        public string? FileExtension { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [Required]
        [MaxLength(20)]
        public string StorageProvider { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        // Accessibility support
        [MaxLength(500)]
        public string? AltText { get; set; }

        // Custom metadata (JSON)
        public string? CustomProperties { get; set; }

        public int VersionNumber { get; set; } = 1;
        public int? ParentMediaId { get; set; }
        public bool IsLatestVersion { get; set; } = true;

        [Required]
        [MaxLength(20)]
        public string UploadStatus { get; set; } = "Complete";

        // Background processing tracking
        [MaxLength(50)]
        public string? ProcessingStatus { get; set; }

        [Required]
        [MaxLength(20)]
        public string UploadedByPayroll { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Soft delete support
        public bool IsActive { get; set; } = true;
        public DateTime? DeletedAt { get; set; }

        [MaxLength(20)]
        public string? DeletedByPayroll { get; set; }

        // Navigation properties
        public MediaCollection Collection { get; set; } = null!;
        public MediaFile? ParentMedia { get; set; }
        public ICollection<MediaFile> ChildVersions { get; set; } = new List<MediaFile>();
        public ICollection<MediaAssociation> Associations { get; set; } = new List<MediaAssociation>();
        public ICollection<MediaAccessLog> AccessLogs { get; set; } = new List<MediaAccessLog>();
        public Employee UploadedBy { get; set; } = null!;
        public Employee? DeletedBy { get; set; }
    }
}
```

---

## Service Layer Architecture

> **Note**: The database migration scripts have been executed. Completed scripts are available in:
> - `Database/Migrations/01_Alter_MediaAssociations_Table_Fixed.sql` (✅ Completed)
> - `Database/Migrations/02_Add_Missing_Media_Columns_With_SoftDelete.sql` (✅ Completed)
> - `Database/Migrations/03_Add_Performance_Indexes.sql` (⏳ Pending)
> - `Database/Migrations/04_Seed_Default_Collections_Fixed.sql` (✅ Completed)

### Service Interfaces

#### IMediaService.cs

```csharp
using Microsoft.AspNetCore.Http;
using OSHManagement.Models;

namespace OSHManagement.Services.Media
{
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
        Task<bool> SoftDeleteAsync(int mediaId);

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
```

---

#### IStorageProvider.cs

```csharp
namespace OSHManagement.Services.Media
{
    public interface IStorageProvider
    {
        /// <summary>
        /// Provider name (Local, Azure, AWS, etc.)
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Store file and return storage path
        /// </summary>
        Task<string> StoreAsync(
            Stream fileStream,
            string fileName,
            string contentType);

        /// <summary>
        /// Retrieve file as stream
        /// </summary>
        Task<Stream> RetrieveAsync(string filePath);

        /// <summary>
        /// Delete file from storage
        /// </summary>
        Task<bool> DeleteAsync(string filePath);

        /// <summary>
        /// Check if file exists
        /// </summary>
        Task<bool> ExistsAsync(string filePath);

        /// <summary>
        /// Get public URL (with optional expiry for signed URLs)
        /// </summary>
        Task<string> GetPublicUrlAsync(
            string filePath,
            TimeSpan? expiry = null);

        /// <summary>
        /// Get storage statistics
        /// </summary>
        Task<StorageStats> GetStatsAsync();
    }
}
```

---

### DTOs and Options Classes

```csharp
namespace OSHManagement.Services.Media
{
    // Upload options
    public class MediaUploadOptions
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? AltText { get; set; }
        public Dictionary<string, object>? CustomProperties { get; set; }
        public bool AllowDuplicates { get; set; } = false;
        public string? UploadedByPayroll { get; set; }
    }

    // Association options
    public class MediaAssociationOptions
    {
        public string? AssociationLabel { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsPrimary { get; set; } = false;
        public bool IsRequired { get; set; } = false;
        public int? MaxFilesAllowed { get; set; }
    }

    // Metadata update
    public class MediaMetadataUpdate
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? AltText { get; set; }
        public Dictionary<string, object>? CustomProperties { get; set; }
    }

    // File filtering
    public class MediaFileFilter
    {
        public bool? IsActive { get; set; } = true;
        public DateTime? UploadedAfter { get; set; }
        public DateTime? UploadedBefore { get; set; }
        public string? UploadedByPayroll { get; set; }
        public List<string>? FileExtensions { get; set; }
        public long? MaxFileSizeBytes { get; set; }
        public int? Limit { get; set; }
        public int? Offset { get; set; }
    }

    // Search criteria
    public class MediaSearchCriteria
    {
        public string? SearchTerm { get; set; }
        public string? CollectionName { get; set; }
        public string? FileExtension { get; set; }
        public DateTime? UploadedAfter { get; set; }
        public DateTime? UploadedBefore { get; set; }
        public string? UploadedByPayroll { get; set; }
        public bool IncludeInactive { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Storage statistics
    public class StorageStats
    {
        public long TotalFilesCount { get; set; }
        public long TotalSizeBytes { get; set; }
        public long ActiveFilesCount { get; set; }
        public long ActiveSizeBytes { get; set; }
        public Dictionary<string, long> SizeByCollection { get; set; } = new();
    }
}
```

---

## Scope-Based Access Control

### Media Scope Filtering Logic

The following scope filtering logic needs to be added to `ScopeFilterService.cs` to ensure users only see media files within their access level.

**Add to ApplyScopeByType method:**

```csharp
// MediaFile filtering
if (entityType == typeof(MediaFile))
{
    return (IQueryable<T>)ApplyMediaFileScope(query.Cast<MediaFile>(), scope);
}

// MediaCollection filtering  
if (entityType == typeof(MediaCollection))
{
    return (IQueryable<T>)ApplyMediaCollectionScope(query.Cast<MediaCollection>(), scope);
}

// MediaAssociation filtering
if (entityType == typeof(MediaAssociation))
{
    return (IQueryable<T>)ApplyMediaAssociationScope(query.Cast<MediaAssociation>(), scope);
}
```

**Add these scope methods:**

```csharp
/// <summary>
/// Apply scope filtering to MediaFile queries
/// </summary>
private IQueryable<MediaFile> ApplyMediaFileScope(IQueryable<MediaFile> query, UserScope scope)
{
    return scope.Level switch
    {
        // Organization: See all files
        ScopeLevel.Organization => query,

        // Station: See files uploaded by station members
        ScopeLevel.Station => query.Where(mf =>
            mf.UploadedBy.StationId == scope.StationId
        ),

        // Department: See files uploaded by department members
        ScopeLevel.Department => query.Where(mf =>
            mf.UploadedBy.DepartmentId == scope.DepartmentId
        ),

        // Team/Self: See only own uploaded files
        ScopeLevel.Team => query.Where(mf =>
            mf.UploadedByPayroll == scope.PayrollNo
        ),

        ScopeLevel.Self => query.Where(mf =>
            mf.UploadedByPayroll == scope.PayrollNo
        ),

        _ => query.Where(_ => false)
    };
}

/// <summary>
/// Apply scope filtering to MediaCollection queries
/// </summary>
private IQueryable<MediaCollection> ApplyMediaCollectionScope(IQueryable<MediaCollection> query, UserScope scope)
{
    // Organization scope sees all collections
    if (scope.IsOrganizationScope)
        return query;

    // Filter by AllowedRoles (stored as JSON array)
    return query.Where(mc =>
        // Public collections visible to all
        mc.IsPublic ||
        // Collections with no role restrictions
        mc.AllowedRoles == null ||
        // Collections where user has allowed role
        scope.Roles.Any(userRole =>
            mc.AllowedRoles.Contains(userRole)
        )
    );
}

/// <summary>
/// Apply scope filtering to MediaAssociation queries
/// </summary>
private IQueryable<MediaAssociation> ApplyMediaAssociationScope(IQueryable<MediaAssociation> query, UserScope scope)
{
    return scope.Level switch
    {
        // Organization: See all associations
        ScopeLevel.Organization => query,

        // Station: See associations for station files
        ScopeLevel.Station => query.Where(ma =>
            ma.Media.UploadedBy.StationId == scope.StationId
        ),

        // Department: See associations for department files
        ScopeLevel.Department => query.Where(ma =>
            ma.Media.UploadedBy.DepartmentId == scope.DepartmentId
        ),

        // Team/Self: See associations for own files
        ScopeLevel.Team => query.Where(ma =>
            ma.Media.UploadedByPayroll == scope.PayrollNo
        ),

        ScopeLevel.Self => query.Where(ma =>
            ma.Media.UploadedByPayroll == scope.PayrollNo
        ),

        _ => query.Where(_ => false)
    };
}
```

---

## File Storage Strategy

### Storage Path Structure (Recommended)

```
wwwroot/uploads/
├── teams/
│   ├── icons/{StationId}/{YYYY-MM}/{guid}_filename.ext
│   └── documents/{StationId}/{YYYY-MM}/{guid}_filename.ext
├── policies/{StationId}/{YYYY}/{guid}_filename.ext
├── incidents/
│   ├── evidence/{StationId}/{IncidentId}/{guid}_filename.ext
│   └── reports/{StationId}/{YYYY-MM}/{guid}_filename.ext
├── hazards/{StationId}/{YYYY-MM}/{guid}_filename.ext
└── general/{StationId}/{YYYY-MM}/{guid}_filename.ext
```

**Benefits**:
- ✅ Clear module separation
- ✅ Station-level scoping built-in
- ✅ Date-based archiving
- ✅ Easy backups per module

---

## Next Implementation Steps

### Phase 1: Database Configuration (Priority: High)
1. Add DbContext configuration for Media tables
2. Add indexes, foreign keys, unique constraints
3. Test with EF Core migrations

### Phase 2: Service Layer (Priority: High)
1. Create `Services/Media/` folder
2. Implement `IMediaService` interface
3. Implement `LocalStorageProvider`
4. Add scope filtering to `ScopeFilterService`

### Phase 3: Controller & API (Priority: Medium)
1. Create `MediaController` inheriting from `ScopedController`
2. Implement upload/download/delete actions
3. Add API endpoints for file associations
4. Add file manager routes

### Phase 4: UI Implementation (Priority: Medium)
1. Create file upload component (drag & drop)
2. Build file manager view
3. Add file pickers to existing forms (Team, Incident, Policy)
4. Implement gallery/preview components

### Phase 5: Testing (Priority: High)
1. Unit tests for MediaService
2. Integration tests for file upload/download
3. Scope filtering tests
4. Performance tests with large files

---

## Summary

### ✅ Completed
- Database migrations executed
- Models updated and synced with database
- 10 default collections created
- Polymorphic association support
- Comprehensive soft delete functionality
- Audit trail fields added

### 🚧 Next Steps
1. Add DbContext configuration
2. Implement service layer
3. Create controller
4. Build UI components
5. Add integration tests

**The foundation is complete - ready to build the service layer and UI!**

---

**Document End**


## Service Layer Architecture

### Service Interfaces

#### IMediaService.cs

```csharp
using Microsoft.AspNetCore.Http;
using OSHManagement.Models;

namespace OSHManagement.Services.Media
{
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
        Task<bool> SoftDeleteAsync(int mediaId);

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
```

---

#### IStorageProvider.cs

```csharp
namespace OSHManagement.Services.Media
{
    public interface IStorageProvider
    {
        /// <summary>
        /// Provider name (Local, Azure, AWS, etc.)
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Store file and return storage path
        /// </summary>
        Task<string> StoreAsync(
            Stream fileStream,
            string fileName,
            string contentType);

        /// <summary>
        /// Retrieve file as stream
        /// </summary>
        Task<Stream> RetrieveAsync(string filePath);

        /// <summary>
        /// Delete file from storage
        /// </summary>
        Task<bool> DeleteAsync(string filePath);

        /// <summary>
        /// Check if file exists
        /// </summary>
        Task<bool> ExistsAsync(string filePath);

        /// <summary>
        /// Get public URL (with optional expiry for signed URLs)
        /// </summary>
        Task<string> GetPublicUrlAsync(
            string filePath,
            TimeSpan? expiry = null);

        /// <summary>
        /// Get storage statistics
        /// </summary>
        Task<StorageStats> GetStatsAsync();
    }
}
```

---

### DTOs and Options Classes

```csharp
namespace OSHManagement.Services.Media
{
    // Upload options
    public class MediaUploadOptions
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? AltText { get; set; }
        public Dictionary<string, object>? CustomProperties { get; set; }
        public bool AllowDuplicates { get; set; } = false;
        public string? UploadedByPayroll { get; set; }
    }

    // Association options
    public class MediaAssociationOptions
    {
        public string? AssociationLabel { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsPrimary { get; set; } = false;
        public bool IsRequired { get; set; } = false;
        public int? MaxFilesAllowed { get; set; }
    }

    // Metadata update
    public class MediaMetadataUpdate
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? AltText { get; set; }
        public Dictionary<string, object>? CustomProperties { get; set; }
    }

    // File filtering
    public class MediaFileFilter
    {
        public bool? IsActive { get; set; } = true;
        public DateTime? UploadedAfter { get; set; }
        public DateTime? UploadedBefore { get; set; }
        public string? UploadedByPayroll { get; set; }
        public List<string>? FileExtensions { get; set; }
        public long? MaxFileSizeBytes { get; set; }
        public int? Limit { get; set; }
        public int? Offset { get; set; }
    }

    // Search criteria
    public class MediaSearchCriteria
    {
        public string? SearchTerm { get; set; }
        public string? CollectionName { get; set; }
        public string? FileExtension { get; set; }
        public DateTime? UploadedAfter { get; set; }
        public DateTime? UploadedBefore { get; set; }
        public string? UploadedByPayroll { get; set; }
        public bool IncludeInactive { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Storage statistics
    public class StorageStats
    {
        public long TotalFilesCount { get; set; }
        public long TotalSizeBytes { get; set; }
        public long ActiveFilesCount { get; set; }
        public long ActiveSizeBytes { get; set; }
        public Dictionary<string, long> SizeByCollection { get; set; } = new();
    }
}
```

---

## Scope-Based Access Control

### Media Scope Filtering Logic

#### Add to ScopeFilterService.cs

```csharp
// Add to ScopeFilterService.ApplyScopeByType method

// MediaFile filtering (Transactional with Station scope)
if (entityType == typeof(MediaFile))
{
    return (IQueryable<T>)ApplyMediaFileScope(query.Cast<MediaFile>(), scope);
}

// MediaCollection filtering (Reference with visibility rules)
if (entityType == typeof(MediaCollection))
{
    return (IQueryable<T>)ApplyMediaCollectionScope(query.Cast<MediaCollection>(), scope);
}

// MediaAssociation filtering (Transactional, follows associated entity)
if (entityType == typeof(MediaAssociation))
{
    return (IQueryable<T>)ApplyMediaAssociationScope(query.Cast<MediaAssociation>(), scope);
}
```

#### New Scope Methods

```csharp
/// <summary>
/// Apply scope filtering to MediaFile queries
/// Files are scoped based on:
/// 1. Who uploaded them (UploadedByPayroll)
/// 2. What entities they're associated with (via MediaAssociations)
/// 3. Collection permissions (AllowedRoles)
/// </summary>
private IQueryable<MediaFile> ApplyMediaFileScope(IQueryable<MediaFile> query, UserScope scope)
{
    return scope.Level switch
    {
        // Station scope: See files uploaded by station members OR associated with station entities
        ScopeLevel.Station => query.Where(mf =>
            // Files uploaded by users in this station
            mf.UploadedBy.StationId == scope.StationId ||
            // Files associated with station entities
            mf.Associations.Any(a =>
                (a.AssociatedTable == "teams" &&
                 a.Media.Collection.MediaFiles.Any(m => m.UploadedBy.StationId == scope.StationId)) ||
                (a.AssociatedTable == "osh_policies" &&
                 a.Media.Collection.MediaFiles.Any(m => m.UploadedBy.StationId == scope.StationId)) ||
                (a.AssociatedTable == "incidents" &&
                 a.Media.Collection.MediaFiles.Any(m => m.UploadedBy.StationId == scope.StationId))
            )
        ),

        // Department scope: See files uploaded by department members
        ScopeLevel.Department => query.Where(mf =>
            mf.UploadedBy.DepartmentId == scope.DepartmentId
        ),

        // Team/Self scope: See only files uploaded by self OR explicitly shared
        ScopeLevel.Team => query.Where(mf =>
            mf.UploadedByPayroll == scope.PayrollNo ||
            // Files associated with teams user is member of
            mf.Associations.Any(a =>
                a.AssociatedTable == "teams" &&
                a.Media.Collection.MediaFiles.Any(m =>
                    m.UploadedBy.PayrollNo == scope.PayrollNo
                )
            )
        ),

        ScopeLevel.Self => query.Where(mf =>
            mf.UploadedByPayroll == scope.PayrollNo
        ),

        _ => query
    };
}

/// <summary>
/// Apply scope filtering to MediaCollection queries
/// Collections are reference data but respect role-based access
/// </summary>
private IQueryable<MediaCollection> ApplyMediaCollectionScope(IQueryable<MediaCollection> query, UserScope scope)
{
    // Organization scope sees all collections
    if (scope.IsOrganizationScope)
        return query;

    // Filter by AllowedRoles (stored as JSON array)
    return query.Where(mc =>
        // Public collections visible to all
        mc.IsPublic ||
        // Collections with no role restrictions
        mc.AllowedRoles == null ||
        // Collections where user has allowed role
        scope.Roles.Any(userRole =>
            mc.AllowedRoles.Contains(userRole)
        )
    );
}

/// <summary>
/// Apply scope filtering to MediaAssociation queries
/// Associations inherit scope from the associated entity
/// </summary>
private IQueryable<MediaAssociation> ApplyMediaAssociationScope(IQueryable<MediaAssociation> query, UserScope scope)
{
    return scope.Level switch
    {
        // Station scope: See associations where media file is scoped to station
        ScopeLevel.Station => query.Where(ma =>
            ma.Media.UploadedBy.StationId == scope.StationId
        ),

        // Department scope: See associations where media file is scoped to department
        ScopeLevel.Department => query.Where(ma =>
            ma.Media.UploadedBy.DepartmentId == scope.DepartmentId
        ),

        // Team/Self scope: See associations for own uploads
        ScopeLevel.Team => query.Where(ma =>
            ma.Media.UploadedByPayroll == scope.PayrollNo
        ),

        ScopeLevel.Self => query.Where(ma =>
            ma.Media.UploadedByPayroll == scope.PayrollNo
        ),

        _ => query
    };
}
```

---

### Authorization Rules Summary

| Scope Level | Can Upload | Can View | Can Associate | Can Delete |
|------------|-----------|----------|---------------|-----------|
| **Organization** | ✅ All collections | ✅ All files | ✅ Any entity | ✅ Any file |
| **Station** | ✅ Station collections | ✅ Station files | ✅ Station entities | ✅ Station files |
| **Department** | ✅ Dept collections | ✅ Dept files | ✅ Dept entities | ✅ Own files |
| **Team** | ✅ Limited collections | ✅ Own + team files | ✅ Team entities | ✅ Own files |
| **Self** | ✅ Basic collections | ✅ Own files only | ❌ No | ✅ Own files |

---

## File Storage Strategy

### Storage Path Structure

#### Option 1: Module-Based Organization (RECOMMENDED)

```
wwwroot/
└── uploads/
    ├── teams/
    │   ├── icons/
    │   │   └── {StationId}/
    │   │       └── {YYYY-MM}/
    │   │           └── {guid}_teamicon.png
    │   └── documents/
    │       └── {StationId}/
    │           └── {YYYY-MM}/
    │               └── {guid}_charter.pdf
    │
    ├── policies/
    │   └── {StationId}/
    │       └── {YYYY}/
    │           └── {guid}_policy.pdf
    │
    ├── incidents/
    │   ├── evidence/
    │   │   └── {StationId}/
    │   │       └── {IncidentId}/
    │   │           ├── {guid}_photo1.jpg
    │   │           └── {guid}_photo2.jpg
    │   └── reports/
    │       └── {StationId}/
    │           └── {YYYY-MM}/
    │               └── {guid}_report.pdf
    │
    ├── hazards/
    │   └── {StationId}/
    │       └── {YYYY-MM}/
    │           └── {guid}_assessment.pdf
    │
    └── general/
        └── {StationId}/
            └── {YYYY-MM}/
                └── {guid}_file.ext
```

**Benefits**:
- ✅ Clear module separation
- ✅ Easy to backup module-specific data
- ✅ Station-level scoping built-in
- ✅ Date-based archiving simple

---

#### Option 2: Organization Structure-Based

```
wwwroot/
└── uploads/
    └── stations/
        └── {StationId}/
            ├── departments/
            │   └── {DepartmentId}/
            │       └── {YYYY-MM}/
            │           └── {guid}_file.ext
            │
            ├── teams/
            │   └── {TeamId}/
            │       └── {YYYY-MM}/
            │           └── {guid}_file.ext
            │
            └── general/
                └── {YYYY-MM}/
                    └── {guid}_file.ext
```

**Benefits**:
- ✅ Matches organizational hierarchy
- ✅ Easy station-specific cleanup
- ❌ Cross-module files harder to find

---

### Recommended: Hybrid Approach

```
wwwroot/
└── uploads/
    └── {ModuleName}/          # teams, policies, incidents, etc.
        └── {StationId}/        # Station-level isolation
            └── {YYYY-MM}/      # Date-based partitioning
                └── {collection}/   # Collection within module
                    └── {guid}_{original_name}.{ext}
```

**Example Paths**:
```
/uploads/teams/5/2025-10/icons/a1b2c3d4_logo.png
/uploads/policies/5/2025-10/documents/e5f6g7h8_safety_policy.pdf
/uploads/incidents/5/2025-10/evidence/i9j0k1l2_photo.jpg
```

**Filename Format**: `{guid}_{sanitized_original_name}.{ext}`
- GUID ensures uniqueness
- Original name preserved for user recognition
- Sanitized to remove special characters

---

### Storage Configuration

```csharp
// appsettings.json
{
  "MediaStorage": {
    "Provider": "Local",  // Local, Azure, AWS
    "RootPath": "wwwroot/uploads",
    "MaxFileSizeMB": 50,
    "PathStructure": "Module-Station-Date",
    "PreserveOriginalFilename": true,

    "Azure": {
      "ConnectionString": "",
      "ContainerName": "osh-media"
    },

    "Cleanup": {
      "OrphanedFilesRetentionDays": 90,
      "SoftDeleteRetentionDays": 365,
      "EnableAutoCleanup": true
    }
  }
}
```

---

## File Manager Module Design

### UI Components (file-manager.html Integration)

#### Page Structure

```
File Manager
├── Left Sidebar
│   ├── Collections Browser
│   │   ├── By Module (Teams, Policies, Incidents)
│   │   └── Quick Access (Recent, Starred, My Uploads)
│   └── Storage Stats
│       └── Total Size, File Count
│
├── Main Content Area
│   ├── Toolbar
│   │   ├── Upload Button
│   │   ├── Search Box
│   │   ├── View Toggle (Grid/List)
│   │   └── Filter Dropdown (Type, Date, Size)
│   │
│   ├── File Grid/List
│   │   └── File Cards
│   │       ├── Thumbnail/Icon
│   │       ├── Filename
│   │       ├── Size, Date, Uploader
│   │       └── Actions (View, Download, Associate, Delete)
│   │
│   └── Pagination
│
└── Right Sidebar (File Details)
    ├── Preview
    ├── Metadata
    │   ├── Title, Description
    │   ├── Collection, Module
    │   └── Upload Info
    ├── Associations
    │   └── List of linked entities
    ├── Version History
    └── Access Log (for admins)
```

---

### File Manager Actions

#### 1. Upload Actions

```typescript
// Upload single file
POST /api/media/upload
{
  collection: "team_icons",
  file: FormData,
  title: "Team Alpha Logo",
  description: "Official logo for Team Alpha",
  autoAssociate: false  // Upload without association
}

// Upload multiple files
POST /api/media/upload-multiple
{
  collection: "incident_evidence",
  files: FormData[],
  associateWith: {
    table: "incidents",
    recordId: "123",
    type: "evidence_photo"
  }
}
```

---

#### 2. Browse Actions

```typescript
// Get files by collection (scope-filtered)
GET /api/media/collection/{collectionName}
  ?page=1
  &pageSize=20
  &sortBy=createdAt
  &sortOrder=desc
  &fileType=image

// Search files (scope-filtered)
GET /api/media/search
  ?q=safety
  &collection=osh_policy_documents
  &uploadedAfter=2025-01-01
  &station={stationId}

// Get orphaned files (no associations)
GET /api/media/orphaned
  ?daysOld=30
  &collection=general_uploads
```

---

#### 3. View/Preview Actions

```typescript
// Get file metadata
GET /api/media/{mediaId}
Response: {
  mediaId: 123,
  originalFilename: "safety_policy.pdf",
  title: "OSH Safety Policy 2025",
  collection: "osh_policy_documents",
  uploadedBy: { payrollNo: "12345", name: "John Doe" },
  associations: [
    { table: "osh_policies", recordId: "5", type: "policy_document" }
  ],
  canEdit: true,
  canDelete: false
}

// Download file
GET /api/media/{mediaId}/download
Response: FileStream

// Preview/Thumbnail (for images)
GET /api/media/{mediaId}/preview
Response: Resized image stream
```

---

#### 4. Associate/Link Actions

```typescript
// Associate file with entity
POST /api/media/{mediaId}/associate
{
  table: "teams",
  recordId: "45",
  type: "team_icon",
  options: {
    isPrimary: true,
    label: "Team Logo"
  }
}

// Remove association
DELETE /api/media/{mediaId}/associate
{
  table: "teams",
  recordId: "45",
  type: "team_icon"
}

// Get all associations for file
GET /api/media/{mediaId}/associations
Response: [
  { table: "teams", recordId: "45", type: "team_icon", label: "Team Logo" }
]
```

---

#### 5. Edit/Update Actions

```typescript
// Update metadata
PATCH /api/media/{mediaId}
{
  title: "Updated Title",
  description: "Updated description",
  altText: "Logo for Team Alpha"
}

// Create new version
POST /api/media/{mediaId}/version
{
  file: FormData,
  versionNote: "Updated logo with new branding"
}

// Revert to version
POST /api/media/{mediaId}/revert/{versionId}
```

---

#### 6. Delete Actions

```typescript
// Soft delete (sets IsActive = false)
DELETE /api/media/{mediaId}?soft=true

// Hard delete (permanent removal)
DELETE /api/media/{mediaId}?soft=false
  &confirm=true

// Restore soft-deleted file
POST /api/media/{mediaId}/restore

// Bulk delete orphaned files
DELETE /api/media/orphaned/cleanup
{
  daysOld: 90,
  collection: "general_uploads"
}
```

---

#### 7. Admin Actions

```typescript
// Get storage statistics
GET /api/media/stats
Response: {
  totalFiles: 1234,
  totalSizeGB: 5.67,
  byModule: {
    "teams": { count: 200, sizeGB: 0.5 },
    "incidents": { count: 500, sizeGB: 3.2 }
  },
  byStation: {
    "5": { count: 600, sizeGB: 2.1 }
  }
}

// Get access logs
GET /api/media/{mediaId}/access-log
  ?limit=50

// Cleanup orphaned files (background job)
POST /api/media/maintenance/cleanup-orphaned
{
  daysOld: 90,
  dryRun: false
}
```

---

### Controller Structure

```csharp
[ApiController]
[Route("api/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly IUserScopeService _scopeService;
    private readonly ILogger<MediaController> _logger;

    // Upload endpoints
    [HttpPost("upload")]
    public async Task<ActionResult<MediaFileDto>> Upload(...) { }

    [HttpPost("upload-multiple")]
    public async Task<ActionResult<List<MediaFileDto>>> UploadMultiple(...) { }

    // Browse endpoints
    [HttpGet("collection/{collectionName}")]
    public async Task<ActionResult<PagedResult<MediaFileDto>>> GetByCollection(...) { }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<MediaFileDto>>> Search(...) { }

    [HttpGet("orphaned")]
    [Authorize(Roles = "Admin,Station_Manager")]
    public async Task<ActionResult<List<MediaFileDto>>> GetOrphaned(...) { }

    // View endpoints
    [HttpGet("{mediaId}")]
    public async Task<ActionResult<MediaFileDetailsDto>> Get(int mediaId) { }

    [HttpGet("{mediaId}/download")]
    public async Task<IActionResult> Download(int mediaId) { }

    [HttpGet("{mediaId}/preview")]
    public async Task<IActionResult> Preview(int mediaId) { }

    // Associate endpoints
    [HttpPost("{mediaId}/associate")]
    public async Task<ActionResult<MediaAssociationDto>> Associate(...) { }

    [HttpDelete("{mediaId}/associate")]
    public async Task<IActionResult> Disassociate(...) { }

    [HttpGet("{mediaId}/associations")]
    public async Task<ActionResult<List<MediaAssociationDto>>> GetAssociations(...) { }

    // Edit endpoints
    [HttpPatch("{mediaId}")]
    public async Task<ActionResult<MediaFileDto>> Update(...) { }

    [HttpPost("{mediaId}/version")]
    public async Task<ActionResult<MediaFileDto>> CreateVersion(...) { }

    // Delete endpoints
    [HttpDelete("{mediaId}")]
    public async Task<IActionResult> Delete(int mediaId, bool soft = true) { }

    [HttpPost("{mediaId}/restore")]
    public async Task<IActionResult> Restore(int mediaId) { }

    // Admin endpoints
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Station_Manager")]
    public async Task<ActionResult<StorageStatsDto>> GetStats() { }

    [HttpGet("{mediaId}/access-log")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<MediaAccessLogDto>>> GetAccessLog(...) { }
}
```

---

## Implementation Steps

### Phase 1: Database Foundation (Day 1)
1. ✅ Run Script 1: Alter MediaAssociations table
2. ✅ Run Script 2: Add missing columns to MediaFiles and MediaCollections
3. ✅ Run Script 3: Add performance indexes
4. ✅ Run Script 4: Seed default collections
5. ✅ Verify all changes in SSMS
6. ✅ Update C# models to match database

### Phase 2: Service Layer (Days 2-3)
1. ✅ Create `Services/Media` folder
2. ✅ Implement `IStorageProvider` interface
3. ✅ Implement `LocalStorageProvider` class
4. ✅ Create all DTOs and options classes
5. ✅ Implement `IMediaService` interface
6. ✅ Implement `MediaService` class
7. ✅ Add scope filtering to `ScopeFilterService`
8. ✅ Register services in `Program.cs`

### Phase 3: API Layer (Day 4)
1. ✅ Create `Controllers/MediaController.cs`
2. ✅ Implement all endpoints
3. ✅ Add authorization attributes
4. ✅ Test with Postman/Swagger

### Phase 4: UI Integration (Days 5-6)
1. ✅ Create `Views/Media/FileManager.cshtml`
2. ✅ Integrate file-manager.html template
3. ✅ Create JavaScript for file operations
4. ✅ Add drag-drop upload
5. ✅ Add file preview modal
6. ✅ Add association dialogs

### Phase 5: Module Integration (Days 7-8)
1. ✅ Add media upload to Team creation/edit
2. ✅ Add media upload to Policy module
3. ✅ Add media upload to Incident module
4. ✅ Add media display components
5. ✅ Test all integrations

### Phase 6: Testing & Optimization (Days 9-10)
1. ✅ Unit tests for services
2. ✅ Integration tests for API
3. ✅ Performance testing with large files
4. ✅ Security testing (scope violations)
5. ✅ Load testing
6. ✅ Documentation

---

## Testing Strategy

### Unit Tests

```csharp
// MediaServiceTests.cs
[Fact]
public async Task UploadAsync_ValidFile_ReturnsMediaFile()
{
    // Arrange
    var mockFile = CreateMockFormFile("test.pdf", 1024);
    var service = CreateMediaService();

    // Act
    var result = await service.UploadAsync(mockFile, "general_uploads");

    // Assert
    Assert.NotNull(result);
    Assert.Equal("test.pdf", result.OriginalFilename);
    Assert.True(result.IsActive);
}

[Fact]
public async Task AssociateAsync_ValidParams_CreatesAssociation()
{
    // Test association creation
}

[Fact]
public async Task GetByAssociationAsync_WithScopeFilter_ReturnsFilteredResults()
{
    // Test scope filtering
}
```

---

### Integration Tests

```csharp
// MediaControllerIntegrationTests.cs
[Fact]
public async Task Upload_AsStationUser_OnlySeesStationFiles()
{
    // Arrange
    var client = CreateClientWithStationScope(stationId: 5);

    // Act
    var response = await client.GetAsync("/api/media/collection/team_icons");
    var files = await response.Content.ReadAsAsync<List<MediaFileDto>>();

    // Assert
    Assert.All(files, f => Assert.Equal(5, f.UploadedBy.StationId));
}

[Fact]
public async Task Associate_WithDifferentStation_ReturnsForbidden()
{
    // Test cross-station association prevention
}
```

---

### Security Tests

```csharp
// MediaSecurityTests.cs
[Fact]
public async Task GetFileStream_FromDifferentStation_ReturnsForbidden()
{
    // Ensure station isolation
}

[Fact]
public async Task Delete_OtherUserFile_ReturnsForbidden()
{
    // Ensure users can't delete others' files
}
```

---

## Rollback Plan

If issues occur during implementation:

### Rollback Script

```sql
USE [OSHManagement];
GO

BEGIN TRANSACTION;

-- Revert AssociatedRecordId to int (if no string IDs exist)
ALTER TABLE MediaAssociations
ALTER COLUMN AssociatedRecordId INT NOT NULL;

-- Remove added columns
ALTER TABLE MediaAssociations DROP CONSTRAINT FK_MediaAssociations_CreatedBy;
ALTER TABLE MediaAssociations DROP COLUMN CreatedByPayroll;

ALTER TABLE MediaFiles DROP COLUMN AltText;
ALTER TABLE MediaFiles DROP COLUMN CustomProperties;
ALTER TABLE MediaFiles DROP COLUMN IsActive;
ALTER TABLE MediaFiles DROP COLUMN ProcessingStatus;

ALTER TABLE MediaCollections DROP CONSTRAINT FK_MediaCollections_CreatedBy;
ALTER TABLE MediaCollections DROP COLUMN CreatedByPayroll;

-- Remove indexes
-- (List specific indexes to drop)

COMMIT TRANSACTION;
GO
```

---

## Appendix

### Common Association Patterns

```csharp
// Pattern 1: Team Icon
await _mediaService.AssociateAsync(
    mediaId: fileId,
    tableName: "teams",
    recordId: teamId.ToString(),
    associationType: "team_icon"
);

// Pattern 2: Policy Document
await _mediaService.AssociateAsync(
    mediaId: fileId,
    tableName: "osh_policies",
    recordId: policyId.ToString(),
    associationType: "policy_document"
);

// Pattern 3: Multiple Incident Photos
foreach (var photo in photos)
{
    await _mediaService.AssociateAsync(
        mediaId: photo.MediaId,
        tableName: "incidents",
        recordId: incidentId.ToString(),
        associationType: "evidence_photo"
    );
}
```

---

### Performance Considerations

1. **File Upload**: Use streaming for large files (>10MB)
2. **Thumbnails**: Generate asynchronously using background jobs
3. **Caching**: Cache collection configurations
4. **Pagination**: Always paginate file lists
5. **Indexes**: Ensure all indexes from Script 3 are applied

---

### Security Checklist

- [x] All endpoints require authentication
- [x] Scope-based filtering applied to all queries
- [x] File size limits enforced
- [x] MIME type validation
- [x] Malware scanning (optional, recommended)
- [x] Secure file paths (no directory traversal)
- [x] Audit logging enabled
- [x] Access control on downloads

---

## Next Steps

1. **Review this document** with the team
2. **Execute database scripts** in SSMS (test environment first)
3. **Begin Phase 2** (Service Layer implementation)
4. **Setup testing environment** for file uploads
5. **Document any deviations** from this plan

---

**Document End**
