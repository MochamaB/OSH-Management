using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [MaxLength(20)]
        public string PayrollNo { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? RollNo { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(100)]
        [EmailAddress]
        public string? EmailAddress { get; set; }

        [MaxLength(20)]
        public string? PhoneNo { get; set; }

        public int StationId { get; set; }
        public int? DepartmentId { get; set; }

        [MaxLength(50)]
        public string? Username { get; set; }

        [MaxLength(255)]
        public string? PasswordHash { get; set; }

        [MaxLength(100)]
        public byte[]? LegacyPassword { get; set; }

        [MaxLength(20)]
        public string? EmploymentStatus { get; set; }

        [MaxLength(20)]
        public string? EmployeeType { get; set; }

        [MaxLength(50)]
        public string? Designation { get; set; }

        public DateTime? HireDate { get; set; }

        public int? ServiceYears { get; set; }

        public DateTime? ContractEndDate { get; set; }

        [MaxLength(20)]
        public string? HodPayroll { get; set; }

        [MaxLength(20)]
        public string? SupervisorPayroll { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Station? Station { get; set; }
        public Department? Department { get; set; }
        public ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
    }
}
