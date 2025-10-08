namespace OSHManagement.Models.Authorization
{
    /// <summary>
    /// Defines the data access scope levels for users
    /// Lower number = broader access
    /// </summary>
    public enum ScopeLevel
    {
        /// <summary>
        /// Organization-wide access - sees all data across all stations
        /// Typically: Admin, OSH Manager, HR Manager
        /// </summary>
        Organization = 1,

        /// <summary>
        /// Station-level access - sees all data within assigned station
        /// Typically: Station Manager
        /// </summary>
        Station = 2,

        /// <summary>
        /// Department-level access - sees all data within assigned department
        /// Typically: Department Head, HOD
        /// </summary>
        Department = 3,

        /// <summary>
        /// Team-level access - sees only direct reports
        /// Typically: Supervisor
        /// </summary>
        Team = 4,

        /// <summary>
        /// Self-only access - sees only own data
        /// Typically: Regular Employee
        /// </summary>
        Self = 5
    }
}
