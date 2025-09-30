using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models
{
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PermissionName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Module { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Action { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
