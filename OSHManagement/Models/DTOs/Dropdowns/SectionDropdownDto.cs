namespace OSHManagement.Models.DTOs.Dropdowns
{
    /// <summary>
    /// DTO for Section dropdown data
    /// Includes StationId for cascading dropdown support
    /// Scope-aware: Users see sections within their station
    /// </summary>
    public class SectionDropdownDto
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int StationId { get; set; } // For cascading
    }
}
