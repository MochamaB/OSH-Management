using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models.ViewModels
{
    public class EmployeeViewModel
    {
        public int EmployeeId { get; set; }

        [Display(Name = "Payroll Number")]
        public string PayrollNo { get; set; } = string.Empty;

        [Display(Name = "Roll Number")]
        public string? RollNo { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Email Address")]
        public string? EmailAddress { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNo { get; set; }

        [Required(ErrorMessage = "Station is required")]
        [Display(Name = "Station")]
        public int StationId { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        [Display(Name = "Username")]
        public string? Username { get; set; }

        [StringLength(20, ErrorMessage = "Employment status cannot exceed 20 characters")]
        [Display(Name = "Employment Status")]
        public string? EmploymentStatus { get; set; }

        [StringLength(20, ErrorMessage = "Employee type cannot exceed 20 characters")]
        [Display(Name = "Employee Type")]
        public string? EmployeeType { get; set; }

        [StringLength(50, ErrorMessage = "Designation cannot exceed 50 characters")]
        [Display(Name = "Designation")]
        public string? Designation { get; set; }

        [Display(Name = "Hire Date")]
        public DateTime? HireDate { get; set; }

        [Display(Name = "Service Years")]
        public int? ServiceYears { get; set; }

        [Display(Name = "Contract End Date")]
        public DateTime? ContractEndDate { get; set; }

        [StringLength(20, ErrorMessage = "HOD payroll cannot exceed 20 characters")]
        [Display(Name = "HOD Payroll")]
        public string? HodPayroll { get; set; }

        [StringLength(20, ErrorMessage = "Supervisor payroll cannot exceed 20 characters")]
        [Display(Name = "Supervisor Payroll")]
        public string? SupervisorPayroll { get; set; }

        // Password Management (Edit only)
        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Display(Name = "Confirm New Password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        // Flag to indicate if user wants to update password
        public bool UpdatePassword { get; set; }

        // Read-only properties for display
        public string StationName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? HodFullName { get; set; }
        public string? SupervisorFullName { get; set; }
        public string? AvatarUrl { get; set; }
        public List<string> RoleNames { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Helper properties for UI
        public string FullName => $"{FirstName} {LastName}";
        public string Initials
        {
            get
            {
                var first = !string.IsNullOrEmpty(FirstName) ? FirstName[0].ToString().ToUpper() : "";
                var last = !string.IsNullOrEmpty(LastName) ? LastName[0].ToString().ToUpper() : "";
                return first + last;
            }
        }
        public string FormattedHireDate => HireDate?.ToString("MMM dd, yyyy") ?? "—";
        public string EmployeeTypeDisplay => EmployeeType ?? "—";
    }
}
