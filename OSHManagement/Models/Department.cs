using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        [MaxLength(20)]
        public string DepartmentCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        public int StationId { get; set; }
        public int? ParentDepartmentId { get; set; }

        [MaxLength(20)]
        public string? DepartmentHeadPayroll { get; set; }

        [MaxLength(50)]
        public string? LegacyDepartmentMapping { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Station Station { get; set; } = null!;
        public Department? ParentDepartment { get; set; }
        public ICollection<Department> ChildDepartments { get; set; } = new List<Department>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
