using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models.ViewModels
{
    /// <summary>
    /// ViewModel for displaying permission information in lists
    /// </summary>
    public class PermissionViewModel
    {
        public int PermissionId { get; set; }

        [Display(Name = "Permission Name")]
        public string PermissionName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Module")]
        public string Module { get; set; } = string.Empty;

        [Display(Name = "Action")]
        public string Action { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Assigned Roles")]
        public int AssignedRolesCount { get; set; }

        public List<string> RoleNames { get; set; } = new List<string>();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// ViewModel for creating a new permission
    /// </summary>
    public class CreatePermissionViewModel
    {
        [Required(ErrorMessage = "Permission name is required")]
        [StringLength(100, ErrorMessage = "Permission name cannot exceed 100 characters")]
        [Display(Name = "Permission Name")]
        [RegularExpression(@"^[A-Za-z]+\.[A-Za-z]+$", ErrorMessage = "Permission name must be in format: Module.Action (e.g., Employee.Read)")]
        public string PermissionName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Module is required")]
        [StringLength(50, ErrorMessage = "Module name cannot exceed 50 characters")]
        [Display(Name = "Module")]
        public string Module { get; set; } = string.Empty;

        [Required(ErrorMessage = "Action is required")]
        [StringLength(50, ErrorMessage = "Action cannot exceed 50 characters")]
        [Display(Name = "Action")]
        public string Action { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Assign to Roles")]
        public List<int> SelectedRoleIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// ViewModel for editing an existing permission
    /// </summary>
    public class EditPermissionViewModel
    {
        public int PermissionId { get; set; }

        [Required(ErrorMessage = "Permission name is required")]
        [StringLength(100, ErrorMessage = "Permission name cannot exceed 100 characters")]
        [Display(Name = "Permission Name")]
        public string PermissionName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Module is required")]
        [StringLength(50, ErrorMessage = "Module name cannot exceed 50 characters")]
        [Display(Name = "Module")]
        public string Module { get; set; } = string.Empty;

        [Required(ErrorMessage = "Action is required")]
        [StringLength(50, ErrorMessage = "Action cannot exceed 50 characters")]
        [Display(Name = "Action")]
        public string Action { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Assign to Roles")]
        public List<int> SelectedRoleIds { get; set; } = new List<int>();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// ViewModel for managing permission assignments to roles
    /// </summary>
    public class PermissionRolesViewModel
    {
        public int PermissionId { get; set; }

        [Display(Name = "Permission Name")]
        public string PermissionName { get; set; } = string.Empty;

        [Display(Name = "Module")]
        public string Module { get; set; } = string.Empty;

        [Display(Name = "Action")]
        public string Action { get; set; } = string.Empty;

        public List<PermissionRoleViewModel> Roles { get; set; } = new List<PermissionRoleViewModel>();
    }

    /// <summary>
    /// Role assigned to a permission
    /// </summary>
    public class PermissionRoleViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ScopeLevelName { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
        public DateTime? AssignedAt { get; set; }
    }

    /// <summary>
    /// ViewModel for assigning permissions to roles
    /// </summary>
    public class AssignPermissionsToRolesViewModel
    {
        public int PermissionId { get; set; }

        [Display(Name = "Permission Name")]
        public string PermissionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select at least one role")]
        [Display(Name = "Select Roles")]
        public List<int> SelectedRoleIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// ViewModel for bulk permission operations
    /// </summary>
    public class BulkPermissionOperationViewModel
    {
        [Required(ErrorMessage = "Please select at least one permission")]
        public List<int> SelectedPermissionIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "Operation is required")]
        [Display(Name = "Operation")]
        public BulkPermissionOperation Operation { get; set; }

        // For bulk activation/deactivation
        public bool? SetActive { get; set; }

        // For bulk role assignment
        public List<int>? RoleIds { get; set; }
    }

    public enum BulkPermissionOperation
    {
        Activate,
        Deactivate,
        AssignToRoles,
        RemoveFromRoles,
        Delete
    }

    /// <summary>
    /// ViewModel for permission module grouping
    /// </summary>
    public class PermissionModuleViewModel
    {
        public string Module { get; set; } = string.Empty;
        public int PermissionsCount { get; set; }
        public List<PermissionViewModel> Permissions { get; set; } = new List<PermissionViewModel>();
    }
}
