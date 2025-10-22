using OSHManagement.Models;

namespace OSHManagement.Services.Notifications
{
    /// <summary>
    /// Specialized notification service for Employee-related events
    /// Centralizes all employee notification logic
    /// </summary>
    public interface IEmployeeNotificationService
    {
        /// <summary>
        /// Notify when a new employee is created
        /// Notifies: HR Managers, Station Managers, Station users
        /// </summary>
        Task NotifyEmployeeCreatedAsync(Employee employee, Station station, string createdBy);

        /// <summary>
        /// Notify when employee details are updated
        /// Notifies: Employee (self), HR Managers
        /// </summary>
        Task NotifyEmployeeUpdatedAsync(Employee employee, string updatedBy, List<string> changedFields);

        /// <summary>
        /// Notify when employee is deactivated
        /// Notifies: Employee (self), HR Managers, Station Manager
        /// </summary>
        Task NotifyEmployeeDeactivatedAsync(Employee employee, Station station, string reason, string deactivatedBy);

        /// <summary>
        /// Notify when employee is transferred to new station
        /// Notifies: Employee (self), Old Station users, New Station users, HR Managers
        /// </summary>
        Task NotifyEmployeeTransferredAsync(Employee employee, Station oldStation, Station newStation, string transferredBy);

        /// <summary>
        /// Notify when employee role is assigned
        /// Notifies: Employee (self), HR Managers
        /// </summary>
        Task NotifyRoleAssignedAsync(Employee employee, string roleName, string assignedBy);

        /// <summary>
        /// Notify when employee is promoted
        /// Notifies: Employee (self), HR Managers, Station Manager
        /// </summary>
        Task NotifyEmployeePromotedAsync(Employee employee, string oldDesignation, string newDesignation, string promotedBy);
    }
}
