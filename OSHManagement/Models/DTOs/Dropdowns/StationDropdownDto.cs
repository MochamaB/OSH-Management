namespace OSHManagement.Models.DTOs.Dropdowns
{
    /// <summary>
    /// DTO for Station dropdown data
    /// Includes OrgCategoryId for cascading dropdown support (Category → Stations)
    /// Scope-aware: Users only see stations within their scope
    /// </summary>
    public class StationDropdownDto
    {
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public int OrgCategoryId { get; set; } // For cascading Category → Station
    }
}
