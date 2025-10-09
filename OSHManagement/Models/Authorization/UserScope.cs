namespace OSHManagement.Models.Authorization
{
    /// <summary>
    /// Represents the current user's data access scope
    /// Used for filtering queries based on organizational hierarchy
    /// </summary>
    public class UserScope
    {
        /// <summary>
        /// Employee ID of the current user
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Payroll number of the current user
        /// </summary>
        public string PayrollNo { get; set; } = string.Empty;

        /// <summary>
        /// Scope level determining data access breadth
        /// </summary>
        public ScopeLevel Level { get; set; }

        /// <summary>
        /// Station ID the user belongs to (null for organization-wide users)
        /// </summary>
        public int? StationId { get; set; }

        /// <summary>
        /// Department ID the user belongs to (null if not assigned to department)
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// List of roles assigned to the user
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();

        // Helper properties for cleaner code
        public bool IsOrganizationScope => Level == ScopeLevel.Organization;
        public bool IsStationScope => Level == ScopeLevel.Station;
        public bool IsDepartmentScope => Level == ScopeLevel.Department;
        public bool IsTeamScope => Level == ScopeLevel.Team;
        public bool IsSelfScope => Level == ScopeLevel.Self;

        /// <summary>
        /// Checks if user has any of the specified roles
        /// </summary>
        public bool HasRole(params string[] roleNames)
        {
            return Roles.Any(r => roleNames.Contains(r, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Checks if user can see organization-wide data
        /// </summary>
        public bool CanSeeAllData => Level == ScopeLevel.Organization;

        /// <summary>
        /// Checks if user can only see their own data
        /// </summary>
        public bool CanOnlySeeSelf => Level == ScopeLevel.Self;
    }
}
