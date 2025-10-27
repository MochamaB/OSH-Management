namespace OSHManagement.Services.Media
{
    /// <summary>
    /// Context information for building storage paths
    /// Follows hybrid structure: {Module}/{Station}/{Date}/{Collection}
    /// </summary>
    public class StorageContext
    {
        /// <summary>
        /// Module name (teams, policies, incidents, etc.)
        /// </summary>
        public string ModuleName { get; set; } = "general";

        /// <summary>
        /// Station ID for station-level isolation
        /// </summary>
        public int StationId { get; set; }

        /// <summary>
        /// Collection name for organization within module
        /// </summary>
        public string CollectionName { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp for date-based partitioning
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
