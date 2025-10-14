namespace OSHManagement.Models.DTOs.Dropdowns
{
    /// <summary>
    /// DTO for Department dropdown data
    /// Includes StationId for cascading dropdown support (Station → Departments)
    /// Scope-aware: Department users see ONLY their department (Principle of Least Privilege)
    /// </summary>
    public class DepartmentDropdownDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int StationId { get; set; } // For cascading Station → Department
    }
}
