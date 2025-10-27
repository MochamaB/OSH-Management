using System.Text.Json;

namespace OSHManagement.Models.ViewModels
{
    /// <summary>
    /// View model for displaying media collection information
    /// </summary>
    public class MediaCollectionViewModel
    {
        // Basic Info
        public int CollectionId { get; set; }
        public string CollectionName { get; set; } = string.Empty;
        public string CollectionDisplayName { get; set; } = string.Empty;
        public string? ModuleName { get; set; }
        public string? Description { get; set; }

        // File Restrictions
        public int MaxFileSizeMb { get; set; }
        public List<string> AllowedFileTypes { get; set; } = new();
        public string? AllowedFileTypesJson { get; set; }
        public int? RetentionPolicyDays { get; set; }

        // Access Control
        public bool IsPublic { get; set; }
        public bool RequiresAuthentication { get; set; }
        public List<string> AllowedRoles { get; set; } = new();
        public string? AllowedRolesJson { get; set; }

        // Statistics
        public int FileCount { get; set; }
        public long TotalSizeBytes { get; set; }
        public string TotalSizeFormatted { get; set; } = "0 B";
        public DateTime? LastUploadDate { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedByPayroll { get; set; }
        public string? CreatedByName { get; set; }
        public bool IsActive { get; set; } = true;

        // Display Properties
        public string StatusText => IsActive ? "Active" : "Inactive";
        public string StatusClass => IsActive ? "success" : "danger";
        public string FormattedCreatedDate => CreatedAt.ToString("MMM dd, yyyy");
        public string FormattedUpdatedDate => UpdatedAt?.ToString("MMM dd, yyyy") ?? "—";
        public string FormattedLastUpload => LastUploadDate?.ToString("MMM dd, yyyy") ?? "No uploads";

        // File Type Display
        public List<FileTypeIcon> FileTypeIcons { get; set; } = new();

        // Module Styling
        public string ModuleIcon => GetModuleIcon(ModuleName);
        public string ModuleBadgeColor => GetModuleBadgeColor(ModuleName);

        // Helper for allowed types display
        public string AllowedFileTypesDisplay => AllowedFileTypes.Any()
            ? string.Join(", ", AllowedFileTypes.Select(ft => ft.ToUpper()))
            : "All file types";

        // Retention display
        public string RetentionPolicyDisplay => RetentionPolicyDays.HasValue
            ? $"{RetentionPolicyDays} days"
            : "No retention policy";

        // Icon/Color helpers
        private static string GetModuleIcon(string? moduleName) => moduleName switch
        {
            "Teams" => "ri-team-line",
            "Policies" => "ri-file-shield-line",
            "Incidents" => "ri-alarm-warning-line",
            "Hazards" => "ri-error-warning-line",
            "Committee" => "ri-government-line",
            "Contractors" => "ri-user-settings-line",
            "General" => "ri-folder-line",
            _ => "ri-folder-line"
        };

        private static string GetModuleBadgeColor(string? moduleName) => moduleName switch
        {
            "Teams" => "primary",
            "Policies" => "info",
            "Incidents" => "danger",
            "Hazards" => "warning",
            "Committee" => "success",
            "Contractors" => "secondary",
            "General" => "light",
            _ => "light"
        };
    }

    /// <summary>
    /// Helper class for file type icon display
    /// </summary>
    public class FileTypeIcon
    {
        public string Extension { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string ColorClass { get; set; } = string.Empty;

        public static string GetIconForExtension(string ext) => ext.ToLower() switch
        {
            "pdf" => "ri-file-pdf-line",
            "doc" or "docx" => "ri-file-word-line",
            "xls" or "xlsx" => "ri-file-excel-line",
            "ppt" or "pptx" => "ri-file-ppt-line",
            "jpg" or "jpeg" or "png" or "gif" or "svg" or "webp" => "ri-image-line",
            "mp4" or "mov" or "avi" or "wmv" => "ri-video-line",
            "mp3" or "wav" or "ogg" => "ri-music-line",
            "zip" or "rar" or "7z" or "tar" or "gz" => "ri-file-zip-line",
            "txt" => "ri-file-text-line",
            "csv" => "ri-file-list-line",
            "html" or "htm" => "ri-html5-line",
            "css" => "ri-css3-line",
            "js" => "ri-javascript-line",
            _ => "ri-file-line"
        };

        public static string GetColorForExtension(string ext) => ext.ToLower() switch
        {
            "pdf" => "text-danger",
            "doc" or "docx" => "text-primary",
            "xls" or "xlsx" => "text-success",
            "ppt" or "pptx" => "text-warning",
            "jpg" or "jpeg" or "png" or "gif" or "svg" or "webp" => "text-info",
            "mp4" or "mov" or "avi" or "wmv" => "text-purple",
            "mp3" or "wav" or "ogg" => "text-pink",
            "zip" or "rar" or "7z" or "tar" or "gz" => "text-secondary",
            "txt" or "csv" => "text-muted",
            "html" or "htm" or "css" or "js" => "text-orange",
            _ => "text-muted"
        };
    }
}
