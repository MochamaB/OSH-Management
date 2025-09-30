using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class RolePermission
    {
        [Key]
        public int RolePermissionId { get; set; }

        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Role Role { get; set; } = null!;
        public Permission Permission { get; set; } = null!;
    }
}
