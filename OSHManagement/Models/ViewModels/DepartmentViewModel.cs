using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models.ViewModels
{
    public class DepartmentViewModel
    {
        public int DepartmentId { get; set; }

        [StringLength(20, ErrorMessage = "Department code cannot exceed 20 characters")]
        [Display(Name = "Department Code")]
        public string DepartmentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters")]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Station is required")]
        [Display(Name = "Station")]
        public int StationId { get; set; }

        [Display(Name = "Parent Department")]
        public int? ParentDepartmentId { get; set; }

        [StringLength(20, ErrorMessage = "Department head payroll cannot exceed 20 characters")]
        [Display(Name = "Department Head Payroll")]
        public string? DepartmentHeadPayroll { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Read-only properties for display
        public string StationName { get; set; } = string.Empty;
        public string? ParentDepartmentName { get; set; }
        public string? DepartmentHeadUsername { get; set; }
        public string? DepartmentHeadFullName { get; set; }
        public int EmployeeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Helper properties for UI
        public string StatusClass => IsActive ? "success" : "danger";
        public string StatusText => IsActive ? "Active" : "Inactive";
        public string FormattedCreatedDate => CreatedAt.ToString("MMM dd, yyyy");
        public string FormattedUpdatedDate => UpdatedAt?.ToString("MMM dd, yyyy") ?? "Never";
    }
}
