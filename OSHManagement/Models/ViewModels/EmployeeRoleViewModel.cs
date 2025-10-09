using System.ComponentModel.DataAnnotations;

namespace OSHManagement.Models.ViewModels
{
    /// <summary>
    /// ViewModel for displaying employee role assignments
    /// </summary>
    public class EmployeeRoleAssignmentViewModel
    {
        public int EmployeeRoleId { get; set; }
        public int EmployeeId { get; set; }
        public int RoleId { get; set; }

        [Display(Name = "Employee")]
        public string EmployeeName { get; set; } = string.Empty;

        public string PayrollNo { get; set; } = string.Empty;

        [Display(Name = "Role")]
        public string RoleName { get; set; } = string.Empty;

        [Display(Name = "Scope Level")]
        public string ScopeLevel { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Assigned At")]
        public DateTime AssignedAt { get; set; }

        [Display(Name = "Assigned By")]
        public string? AssignedBy { get; set; }

        public string? StationName { get; set; }
        public string? DepartmentName { get; set; }
    }

    /// <summary>
    /// ViewModel for managing an employee's roles
    /// </summary>
    public class ManageEmployeeRolesViewModel
    {
        public int EmployeeId { get; set; }

        [Display(Name = "Employee Name")]
        public string EmployeeName { get; set; } = string.Empty;

        public string PayrollNo { get; set; } = string.Empty;
        public string? Designation { get; set; }
        public string? StationName { get; set; }
        public string? DepartmentName { get; set; }

        [Display(Name = "Current Roles")]
        public List<EmployeeCurrentRoleViewModel> CurrentRoles { get; set; } = new List<EmployeeCurrentRoleViewModel>();

        [Display(Name = "Available Roles")]
        public List<AvailableRoleViewModel> AvailableRoles { get; set; } = new List<AvailableRoleViewModel>();

        [Display(Name = "Assign New Roles")]
        public List<int> SelectedRoleIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// Current role assigned to employee
    /// </summary>
    public class EmployeeCurrentRoleViewModel
    {
        public int EmployeeRoleId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ScopeLevelName { get; set; } = string.Empty;
        public int PermissionsCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime AssignedAt { get; set; }
        public string? AssignedBy { get; set; }
    }

    /// <summary>
    /// Available role for assignment
    /// </summary>
    public class AvailableRoleViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ScopeLevelName { get; set; } = string.Empty;
        public int PermissionsCount { get; set; }
    }

    /// <summary>
    /// ViewModel for assigning roles to an employee
    /// </summary>
    public class AssignRolesToEmployeeViewModel
    {
        public int EmployeeId { get; set; }

        [Display(Name = "Employee")]
        public string EmployeeName { get; set; } = string.Empty;

        public string PayrollNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select at least one role")]
        [Display(Name = "Roles to Assign")]
        public List<int> SelectedRoleIds { get; set; } = new List<int>();

        [Display(Name = "Assigned By")]
        public string AssignedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel for bulk employee role operations
    /// </summary>
    public class BulkEmployeeRoleOperationViewModel
    {
        [Required(ErrorMessage = "Please select at least one employee")]
        public List<int> SelectedEmployeeIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "Operation is required")]
        [Display(Name = "Operation")]
        public BulkEmployeeRoleOperation Operation { get; set; }

        // For bulk role assignment
        [Display(Name = "Roles to Assign")]
        public List<int>? RoleIds { get; set; }

        // For bulk role removal
        [Display(Name = "Roles to Remove")]
        public List<int>? RolesToRemove { get; set; }

        [Display(Name = "Assigned By")]
        public string? AssignedBy { get; set; }
    }

    public enum BulkEmployeeRoleOperation
    {
        AssignRoles,
        RemoveRoles,
        ReplaceRoles,
        ActivateRoles,
        DeactivateRoles
    }

    /// <summary>
    /// ViewModel for employee role history/audit
    /// </summary>
    public class EmployeeRoleHistoryViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string PayrollNo { get; set; } = string.Empty;

        public List<RoleHistoryItemViewModel> History { get; set; } = new List<RoleHistoryItemViewModel>();
    }

    /// <summary>
    /// Single role history entry
    /// </summary>
    public class RoleHistoryItemViewModel
    {
        public int EmployeeRoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // "Assigned", "Deactivated", "Reactivated"
        public DateTime ActionDate { get; set; }
        public string? ActionBy { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// ViewModel for role assignment report/summary
    /// </summary>
    public class RoleAssignmentSummaryViewModel
    {
        public int TotalEmployees { get; set; }
        public int EmployeesWithRoles { get; set; }
        public int EmployeesWithoutRoles { get; set; }
        public int TotalRoleAssignments { get; set; }

        public List<RoleSummaryItemViewModel> RoleSummaries { get; set; } = new List<RoleSummaryItemViewModel>();
    }

    /// <summary>
    /// Summary for each role
    /// </summary>
    public class RoleSummaryItemViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string ScopeLevelName { get; set; } = string.Empty;
        public int AssignedCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
    }
}
