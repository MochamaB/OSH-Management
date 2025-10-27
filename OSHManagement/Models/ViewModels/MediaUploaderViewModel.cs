namespace OSHManagement.Models.ViewModels
{
    /// <summary>
    /// View model for configuring the media uploader component
    /// </summary>
    public class MediaUploaderViewModel
    {
        /// <summary>
        /// Unique identifier for this uploader instance (for multiple uploaders on same page)
        /// </summary>
        public string UploaderId { get; set; } = $"uploader_{Guid.NewGuid():N}";

        /// <summary>
        /// Collection to upload files to
        /// </summary>
        public string CollectionName { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the collection
        /// </summary>
        public string CollectionDisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Maximum file size in MB
        /// </summary>
        public int MaxFileSizeMb { get; set; } = 10;

        /// <summary>
        /// Allowed file types (e.g., ".pdf,.docx,.jpg")
        /// </summary>
        public string? AcceptedFileTypes { get; set; }

        /// <summary>
        /// Maximum number of files allowed in one upload
        /// </summary>
        public int? MaxFiles { get; set; }

        /// <summary>
        /// Whether to show the upload zone inline or prepare for modal
        /// </summary>
        public bool ShowInline { get; set; } = true;

        /// <summary>
        /// Height of the dropzone (e.g., "300px")
        /// </summary>
        public string DropzoneHeight { get; set; } = "300px";

        /// <summary>
        /// Custom message to display in dropzone
        /// </summary>
        public string? CustomMessage { get; set; }

        /// <summary>
        /// URL to redirect after successful upload (optional)
        /// </summary>
        public string? RedirectUrl { get; set; }

        /// <summary>
        /// Whether to show file metadata fields (title, description)
        /// </summary>
        public bool ShowMetadataFields { get; set; } = true;

        /// <summary>
        /// Table name for automatic association (optional)
        /// </summary>
        public string? AssociateToTable { get; set; }

        /// <summary>
        /// Record ID for automatic association (optional)
        /// </summary>
        public string? AssociateToRecordId { get; set; }

        /// <summary>
        /// Association type (optional)
        /// </summary>
        public string? AssociationType { get; set; }

        /// <summary>
        /// JavaScript callback function name to call on success
        /// </summary>
        public string? OnSuccessCallback { get; set; }

        /// <summary>
        /// JavaScript callback function name to call on error
        /// </summary>
        public string? OnErrorCallback { get; set; }

        /// <summary>
        /// Whether to auto-process queue (true) or wait for manual trigger (false)
        /// </summary>
        public bool AutoProcessQueue { get; set; } = true;

        /// <summary>
        /// CSS class for the dropzone container
        /// </summary>
        public string ContainerClass { get; set; } = "card custom-card";

        /// <summary>
        /// Whether to show the collection selector dropdown
        /// </summary>
        public bool ShowCollectionSelector { get; set; } = false;

        /// <summary>
        /// Available collections for selector (if ShowCollectionSelector = true)
        /// </summary>
        public List<MediaCollection>? AvailableCollections { get; set; }

        /// <summary>
        /// Helper: Get accepted file types as comma-separated string for display
        /// </summary>
        public string AcceptedFileTypesDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(AcceptedFileTypes))
                    return "All file types";

                return AcceptedFileTypes.Replace(".", "").Replace(",", ", ").ToUpper();
            }
        }
    }
}
