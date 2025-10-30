namespace OSHManagement.Models.DTOs.Dropdowns
{
    /// <summary>
    /// DTO for Employee dropdown data
    /// Used for HOD, Supervisor, and general employee selections
    /// ALWAYS scope-aware: Security-critical data
    /// </summary>
    public class EmployeeDropdownDto
    {
        public int EmployeeId { get; set; }
        public string PayrollNo { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty; // FirstName + LastName
        public int? StationId { get; set; }
        public int? DepartmentId { get; set; }
        public string? Designation { get; set; }
        public string? RoleNames { get; set; } // Comma-separated role names
        public string? TeamRole { get; set; } // Role in a specific team (if applicable)
    }
}
