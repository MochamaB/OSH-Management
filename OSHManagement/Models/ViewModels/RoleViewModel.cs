using System.ComponentModel.DataAnnotations;
using OSHManagement.Models.Authorization;

namespace OSHManagement.Models.ViewModels
{
    /// <summary>
    /// ViewModel for displaying role information in lists
    /// </summary>
    public class RoleViewModel
    {
        public int RoleId { get; set; }

        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Scope Level")]
        public ScopeLevel ScopeLevel { get; set; }

        [Display(Name = "Scope Level Name")]
        public string ScopeLevelName { get; set; } = string.Empty;

        [Display(Name = "System Role")]
        public bool IsSystemRole { get; set; }

        [Display(Name = "Cross-Department")]
        public bool AllowCrossDepartmentAccess { get; set; }

        [Display(Name = "Cross-Station")]
        public bool AllowCrossStationAccess { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Permissions")]
        public int PermissionsCount { get; set; }

        [Display(Name = "Employees")]
        public int EmployeesCount { get; set; }

        public List<string> PermissionNames { get; set; } = new List<string>();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// ViewModel for creating a new role
    /// </summary>
    public class CreateRoleViewModel
    {
        [Required(ErrorMessage = "Role name is required")]
        [StringLength(50, ErrorMessage = "Role name cannot exceed 50 characters")]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Scope level is required")]
        [Display(Name = "Scope Level")]
        public ScopeLevel ScopeLevel { get; set; }

        [Display(Name = "Mark as System Role (prevents deletion)")]
        public bool IsSystemRole { get; set; }

        [Display(Name = "Allow Cross-Department Access")]
        public bool AllowCrossDepartmentAccess { get; set; }

        [Display(Name = "Allow Cross-Station Access")]
        public bool AllowCrossStationAccess { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Assign Permissions")]
        public List<int> SelectedPermissionIds { get; set; } = new List<int>();

        [Display(Name = "Assign Employees")]
        public List<int> SelectedEmployeeIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// ViewModel for editing an existing role
    /// </summary>
    public class EditRoleViewModel
    {
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Role name is required")]
        [StringLength(50, ErrorMessage = "Role name cannot exceed 50 characters")]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Scope level is required")]
        [Display(Name = "Scope Level")]
        public ScopeLevel ScopeLevel { get; set; }

        [Display(Name = "System Role")]
        public bool IsSystemRole { get; set; }

        [Display(Name = "Allow Cross-Department Access")]
        public bool AllowCrossDepartmentAccess { get; set; }

        [Display(Name = "Allow Cross-Station Access")]
        public bool AllowCrossStationAccess { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Assign Permissions")]
        public List<int> SelectedPermissionIds { get; set; } = new List<int>();

        [Display(Name = "Assign Employees")]
        public List<int> SelectedEmployeeIds { get; set; } = new List<int>();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// ViewModel for managing role permissions
    /// </summary>
    public class RolePermissionsViewModel
    {
        public int RoleId { get; set; }

        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        [Display(Name = "Scope Level")]
        public ScopeLevel ScopeLevel { get; set; }

        public List<PermissionGroupViewModel> PermissionGroups { get; set; } = new List<PermissionGroupViewModel>();

        public List<int> SelectedPermissionIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// Grouped permissions by module for easier selection
    /// </summary>
    public class PermissionGroupViewModel
    {
        public string Module { get; set; } = string.Empty;
        public List<PermissionSelectionViewModel> Permissions { get; set; } = new List<PermissionSelectionViewModel>();
    }

    /// <summary>
    /// Permission checkbox selection
    /// </summary>
    public class PermissionSelectionViewModel
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Action { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// ViewModel for managing employees assigned to a role
    /// </summary>
    public class RoleEmployeesViewModel
    {
        public int RoleId { get; set; }

        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        [Display(Name = "Scope Level")]
        public ScopeLevel ScopeLevel { get; set; }

        public List<RoleEmployeeViewModel> Employees { get; set; } = new List<RoleEmployeeViewModel>();
    }

    /// <summary>
    /// Employee assigned to a role
    /// </summary>
    public class RoleEmployeeViewModel
    {
        public int EmployeeId { get; set; }
        public string PayrollNo { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Designation { get; set; }
        public string? StationName { get; set; }
        public string? DepartmentName { get; set; }
        public bool IsActive { get; set; }
        public DateTime AssignedAt { get; set; }
        public string? AssignedBy { get; set; }
    }

    /// <summary>
    /// ViewModel for assigning employees to a role
    /// </summary>
    public class AssignEmployeesToRoleViewModel
    {
        public int RoleId { get; set; }

        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select at least one employee")]
        [Display(Name = "Select Employees")]
        public List<int> SelectedEmployeeIds { get; set; } = new List<int>();

        [Display(Name = "Assigned By")]
        public string AssignedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel for bulk role operations
    /// </summary>
    public class BulkRoleOperationViewModel
    {
        [Required(ErrorMessage = "Please select at least one role")]
        public List<int> SelectedRoleIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "Operation is required")]
        [Display(Name = "Operation")]
        public BulkRoleOperation Operation { get; set; }

        // For bulk activation/deactivation
        public bool? SetActive { get; set; }

        // For bulk permission assignment
        public List<int>? PermissionIds { get; set; }
    }

    public enum BulkRoleOperation
    {
        Activate,
        Deactivate,
        AddPermissions,
        RemovePermissions,
        Delete
    }
}
