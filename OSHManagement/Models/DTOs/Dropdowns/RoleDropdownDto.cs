using OSHManagement.Models.Authorization;

namespace OSHManagement.Models.DTOs.Dropdowns
{
    /// <summary>
    /// DTO for Role dropdown data
    /// No scope filtering - reference data managed by admins only
    /// </summary>
    public class RoleDropdownDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ScopeLevel ScopeLevel { get; set; }
    }
}
