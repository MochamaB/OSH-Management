namespace OSHManagement.Models.DTOs
{
    /// <summary>
    /// DTO for employee selection in role assignment
    /// Includes station and department names for display in checkboxes
    /// Always scope-aware: Security-critical data
    /// </summary>
    public class EmployeeSelectionDto
    {
        public int EmployeeId { get; set; }
        public string PayrollNo { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Designation { get; set; }
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }
}
