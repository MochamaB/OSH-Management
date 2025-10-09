using System.ComponentModel.DataAnnotations;
using OSHManagement.Models.Authorization;

namespace OSHManagement.Models
{
    /// <summary>
    /// Represents a role with dynamic scope and permission assignment
    /// </summary>
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        /// <summary>
        /// Data access scope level for this role
        /// Determines what organizational data the role can access
        /// </summary>
        [Required]
        public ScopeLevel ScopeLevel { get; set; }

        /// <summary>
        /// Prevents deletion of critical system roles (Admin, Employee, etc.)
        /// </summary>
        public bool IsSystemRole { get; set; } = false;

        /// <summary>
        /// Allows role to access data across departments (within station)
        /// </summary>
        public bool AllowCrossDepartmentAccess { get; set; } = false;

        /// <summary>
        /// Allows role to access data across stations (advanced feature)
        /// </summary>
        public bool AllowCrossStationAccess { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
