using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class EmployeeRole
    {
        [Key]
        public int EmployeeRoleId { get; set; }

        public int EmployeeId { get; set; }
        public int RoleId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }

        [MaxLength(20)]
        public string? AssignedBy { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Employee Employee { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}
