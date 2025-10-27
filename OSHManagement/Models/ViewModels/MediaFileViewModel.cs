namespace OSHManagement.Models.ViewModels
{
    public class MediaFileViewModel
    {
        public int MediaId { get; set; }
        public string OriginalFilename { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? MimeType { get; set; }
        public long FileSizeBytes { get; set; }
        public string? FileExtension { get; set; }
        public string FilePath { get; set; } = string.Empty;
        
        // Collection info
        public string CollectionName { get; set; } = string.Empty;
        public string CollectionDisplayName { get; set; } = string.Empty;
        public string? ModuleName { get; set; }
        
        // Uploader info
        public string UploadedByPayroll { get; set; } = string.Empty;
        public string UploaderName { get; set; } = string.Empty;
        
        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Version info
        public int VersionNumber { get; set; }
        public bool IsLatestVersion { get; set; }
        
        // Status
        public bool IsActive { get; set; }
        public string UploadStatus { get; set; } = string.Empty;
        
        // Computed properties
        public string FileSizeFormatted => FormatFileSize(FileSizeBytes);
        public string FileTypeCategory => GetFileTypeCategory();
        public string IconClass => GetIconClass();
        public bool IsImage => MimeType?.StartsWith("image/") ?? false;
        public bool IsPdf => MimeType == "application/pdf";
        public bool IsDocument => FileTypeCategory == "Document";
        
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
        
        private string GetFileTypeCategory()
        {
            if (IsImage) return "Image";
            if (IsPdf) return "PDF";
            
            return FileExtension?.ToLower() switch
            {
                ".doc" or ".docx" => "Document",
                ".xls" or ".xlsx" => "Spreadsheet",
                ".ppt" or ".pptx" => "Presentation",
                ".zip" or ".rar" or ".7z" => "Archive",
                ".mp4" or ".avi" or ".mov" => "Video",
                ".mp3" or ".wav" => "Audio",
                _ => "File"
            };
        }
        
        private string GetIconClass()
        {
            return GetFileTypeCategory() switch
            {
                "Image" => "ri-image-line text-info",
                "PDF" => "ri-file-pdf-line text-danger",
                "Document" => "ri-file-word-line text-primary",
                "Spreadsheet" => "ri-file-excel-line text-success",
                "Presentation" => "ri-file-ppt-line text-warning",
                "Archive" => "ri-file-zip-line text-secondary",
                "Video" => "ri-video-line text-purple",
                "Audio" => "ri-music-line text-teal",
                _ => "ri-file-line text-muted"
            };
        }
    }
    
    public class MediaLibraryViewModel
    {
        public List<MediaFileViewModel> Files { get; set; } = new();
        public List<MediaCollectionSummary> Collections { get; set; } = new();
        public MediaStorageStats? StorageStats { get; set; }
        public string? CurrentCollection { get; set; }
        public string? SearchTerm { get; set; }
    }
    
    public class MediaCollectionSummary
    {
        public int CollectionId { get; set; }
        public string CollectionName { get; set; } = string.Empty;
        public string CollectionDisplayName { get; set; } = string.Empty;
        public string? ModuleName { get; set; }
        public int FileCount { get; set; }
        public long TotalSizeBytes { get; set; }
        public string TotalSizeFormatted => FormatFileSize(TotalSizeBytes);
        
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
    }
    
    public class MediaStorageStats
    {
        public long TotalFilesCount { get; set; }
        public long ActiveFilesCount { get; set; }
        public long TotalSizeBytes { get; set; }
        public long ActiveSizeBytes { get; set; }
        
        public string TotalSizeFormatted => FormatFileSize(TotalSizeBytes);
        public string ActiveSizeFormatted => FormatFileSize(ActiveSizeBytes);
        
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
    }
}
