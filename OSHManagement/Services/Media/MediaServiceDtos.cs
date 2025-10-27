namespace OSHManagement.Services.Media
{
    /// <summary>
    /// Options for uploading media files
    /// </summary>
    public class MediaUploadOptions
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? AltText { get; set; }
        public Dictionary<string, object>? CustomProperties { get; set; }
        public bool AllowDuplicates { get; set; } = false;
        public string? UploadedByPayroll { get; set; }
        public int? StationId { get; set; }  // Required for file path structure
    }

    /// <summary>
    /// Options for creating media associations
    /// </summary>
    public class MediaAssociationOptions
    {
        public string? AssociationLabel { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsPrimary { get; set; } = false;
        public bool IsRequired { get; set; } = false;
        public int? MaxFilesAllowed { get; set; }
    }

    /// <summary>
    /// DTO for updating file metadata
    /// </summary>
    public class MediaMetadataUpdate
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? AltText { get; set; }
        public Dictionary<string, object>? CustomProperties { get; set; }
    }

    /// <summary>
    /// Filter options for querying media files
    /// </summary>
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

    /// <summary>
    /// Search criteria for media files
    /// </summary>
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

    /// <summary>
    /// Storage statistics summary
    /// </summary>
    public class StorageStats
    {
        public long TotalFilesCount { get; set; }
        public long TotalSizeBytes { get; set; }
        public long ActiveFilesCount { get; set; }
        public long ActiveSizeBytes { get; set; }
        public Dictionary<string, long> SizeByCollection { get; set; } = new();
    }
}
