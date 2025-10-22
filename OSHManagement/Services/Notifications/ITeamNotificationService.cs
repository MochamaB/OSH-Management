using OSHManagement.Models;

namespace OSHManagement.Services.Notifications
{
    /// <summary>
    /// Specialized notification service for Team-related events
    /// Centralizes all team notification logic
    /// </summary>
    public interface ITeamNotificationService
    {
        /// <summary>
        /// Notify when a new team is created
        /// Notifies: OSH Managers, Station Manager, Station users
        /// </summary>
        Task NotifyTeamCreatedAsync(Team team, Station station, string createdBy);

        /// <summary>
        /// Notify when a member is added to a team
        /// Notifies: New member, All existing team members, Team Lead, Station Manager
        /// </summary>
        Task NotifyMemberAddedAsync(Team team, Employee member, string roleName, string addedBy);

        /// <summary>
        /// Notify when a member is removed from a team
        /// Notifies: Removed member, All team members, Team Lead, Station Manager
        /// </summary>
        Task NotifyMemberRemovedAsync(Team team, Employee member, string reason, string removedBy);

        /// <summary>
        /// Notify when a team member's role changes
        /// Notifies: Member, All team members, Team Lead, Station Manager
        /// </summary>
        Task NotifyRoleChangedAsync(Team team, Employee member, string oldRole, string newRole, string changedBy);

        /// <summary>
        /// Notify when a team is activated
        /// Notifies: All team members, Station Manager, OSH Managers
        /// </summary>
        Task NotifyTeamActivatedAsync(Team team, Station station, string activatedBy);

        /// <summary>
        /// Notify when a team is deactivated/disbanded
        /// Notifies: All team members, Station Manager, OSH Managers
        /// </summary>
        Task NotifyTeamDeactivatedAsync(Team team, Station station, string reason, string deactivatedBy);

        /// <summary>
        /// Notify when team details are updated
        /// Notifies: All team members
        /// </summary>
        Task NotifyTeamUpdatedAsync(Team team, List<string> changedFields, string updatedBy);
    }
}
