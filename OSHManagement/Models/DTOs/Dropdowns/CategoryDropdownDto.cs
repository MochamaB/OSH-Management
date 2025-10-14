namespace OSHManagement.Models.DTOs.Dropdowns
{
    /// <summary>
    /// DTO for Organization Category dropdown data
    /// Used in filters and form dropdowns
    /// No scope filtering - reference data visible to all users
    /// </summary>
    public class CategoryDropdownDto
    {
        public int OrgCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
