using OSHManagement.Models;
using OSHManagement.Models.Enums;
using OSHManagement.Services.Notifications.DTOs;

namespace OSHManagement.Services.Notifications
{
    /// <summary>
    /// Specialized notification service for Team-related events
    /// Centralizes all team notification logic to avoid repetition in controllers
    /// </summary>
    public class TeamNotificationService : ITeamNotificationService
    {
        private readonly INotificationEventPublisher _eventPublisher;
        private readonly ILogger<TeamNotificationService> _logger;

        // Role constants - should match your database Role IDs
        private const int OSH_MANAGER_ROLE_ID = 4;
        private const int STATION_MANAGER_ROLE_ID = 3;
        private const int SAFETY_OFFICER_ROLE_ID = 5;

        public TeamNotificationService(
            INotificationEventPublisher eventPublisher,
            ILogger<TeamNotificationService> logger)
        {
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task NotifyTeamCreatedAsync(Team team, Station station, string createdBy)
        {
            try
            {
                await _eventPublisher.PublishAsync(new NotificationEvent
                {
                    EventType = "TeamCreated",
                    Category = "Team",
                    Priority = NotificationPriority.Normal,
                    ActionUrl = $"/Team/Details/{team.TeamId}",
                    Data = new Dictionary<string, string>
                    {
                        { "TeamName", team.TeamName },
                        { "TeamType", team.TeamType },
                        { "StationName", station.StationName },
                        { "CreatedDate", DateTime.Now.ToString("dd MMMM yyyy") },
                        { "CreatedBy", createdBy }
                    },
                    RecipientRoleIds = new List<int>
                    {
                        OSH_MANAGER_ROLE_ID,
                        STATION_MANAGER_ROLE_ID,
                        SAFETY_OFFICER_ROLE_ID
                    },
                    RecipientStationIds = new List<int> { team.StationId }
                });

                _logger.LogInformation("Team created notification sent for team {TeamId}", team.TeamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending team created notification for team {TeamId}", team.TeamId);
            }
        }

        public async Task NotifyMemberAddedAsync(Team team, Employee member, string roleName, string addedBy)
        {
            try
            {
                await _eventPublisher.PublishAsync(new NotificationEvent
                {
                    EventType = "TeamMemberAdded",
                    Category = "Team",
                    Priority = NotificationPriority.Normal,
                    ActionUrl = $"/Team/Details/{team.TeamId}",
                    Data = new Dictionary<string, string>
                    {
                        { "EmployeeName", $"{member.FirstName} {member.LastName}" },
                        { "PayrollNo", member.PayrollNo },
                        { "TeamName", team.TeamName },
                        { "MemberRole", roleName },
                        { "AppointmentDate", DateTime.Now.ToString("dd MMMM yyyy") },
                        { "AddedBy", addedBy }
                    },
                    RecipientEmployeeIds = new List<int> { member.EmployeeId }, // Notify new member
                    RecipientTeamIds = new List<int> { team.TeamId }, // Notify existing members
                    RecipientRoleIds = new List<int> { STATION_MANAGER_ROLE_ID }
                });

                _logger.LogInformation("Member added notification sent for employee {PayrollNo} to team {TeamId}",
                    member.PayrollNo, team.TeamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending member added notification for employee {PayrollNo}",
                    member.PayrollNo);
            }
        }

        public async Task NotifyMemberRemovedAsync(Team team, Employee member, string reason, string removedBy)
        {
            try
            {
                await _eventPublisher.PublishAsync(new NotificationEvent
                {
                    EventType = "TeamMemberRemoved",
                    Category = "Team",
                    Priority = NotificationPriority.Normal,
                    ActionUrl = $"/Team/Details/{team.TeamId}",
                    Data = new Dictionary<string, string>
                    {
                        { "EmployeeName", $"{member.FirstName} {member.LastName}" },
                        { "PayrollNo", member.PayrollNo },
                        { "TeamName", team.TeamName },
                        { "Reason", reason ?? "Not specified" },
                        { "RemovalDate", DateTime.Now.ToString("dd MMMM yyyy") },
                        { "RemovedBy", removedBy }
                    },
                    RecipientEmployeeIds = new List<int> { member.EmployeeId }, // Notify removed member
                    RecipientTeamIds = new List<int> { team.TeamId }, // Notify remaining members
                    RecipientRoleIds = new List<int> { STATION_MANAGER_ROLE_ID }
                });

                _logger.LogInformation("Member removed notification sent for employee {PayrollNo} from team {TeamId}",
                    member.PayrollNo, team.TeamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending member removed notification for employee {PayrollNo}",
                    member.PayrollNo);
            }
        }

        public async Task NotifyRoleChangedAsync(Team team, Employee member, string oldRole, string newRole, string changedBy)
        {
            try
            {
                await _eventPublisher.PublishAsync(new NotificationEvent
                {
                    EventType = "TeamRoleChanged",
                    Category = "Team",
                    Priority = NotificationPriority.Normal,
                    ActionUrl = $"/Team/Details/{team.TeamId}",
                    Data = new Dictionary<string, string>
                    {
                        { "EmployeeName", $"{member.FirstName} {member.LastName}" },
                        { "PayrollNo", member.PayrollNo },
                        { "TeamName", team.TeamName },
                        { "OldRole", oldRole },
                        { "NewRole", newRole },
                        { "ChangeDate", DateTime.Now.ToString("dd MMMM yyyy") },
                        { "ChangedBy", changedBy }
                    },
                    RecipientEmployeeIds = new List<int> { member.EmployeeId },
                    RecipientTeamIds = new List<int> { team.TeamId },
                    RecipientRoleIds = new List<int> { STATION_MANAGER_ROLE_ID }
                });

                _logger.LogInformation("Role changed notification sent for employee {PayrollNo} in team {TeamId}",
                    member.PayrollNo, team.TeamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending role changed notification for employee {PayrollNo}",
                    member.PayrollNo);
            }
        }

        public async Task NotifyTeamActivatedAsync(Team team, Station station, string activatedBy)
        {
            try
            {
                await _eventPublisher.PublishAsync(new NotificationEvent
                {
                    EventType = "TeamActivated",
                    Category = "Team",
                    Priority = NotificationPriority.Normal,
                    ActionUrl = $"/Team/Details/{team.TeamId}",
                    Data = new Dictionary<string, string>
                    {
                        { "TeamName", team.TeamName },
                        { "TeamType", team.TeamType },
                        { "StationName", station.StationName },
                        { "ActivatedDate", DateTime.Now.ToString("dd MMMM yyyy") },
                        { "ActivatedBy", activatedBy }
                    },
                    RecipientTeamIds = new List<int> { team.TeamId },
                    RecipientRoleIds = new List<int> { OSH_MANAGER_ROLE_ID, STATION_MANAGER_ROLE_ID }
                });

                _logger.LogInformation("Team activated notification sent for team {TeamId}", team.TeamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending team activated notification for team {TeamId}", team.TeamId);
            }
        }

        public async Task NotifyTeamDeactivatedAsync(Team team, Station station, string reason, string deactivatedBy)
        {
            try
            {
                await _eventPublisher.PublishAsync(new NotificationEvent
                {
                    EventType = "TeamDeactivated",
                    Category = "Team",
                    Priority = NotificationPriority.Normal,
                    ActionUrl = $"/Team/Details/{team.TeamId}",
                    Data = new Dictionary<string, string>
                    {
                        { "TeamName", team.TeamName },
                        { "TeamType", team.TeamType },
                        { "StationName", station.StationName },
                        { "Reason", reason ?? "Not specified" },
                        { "DeactivatedDate", DateTime.Now.ToString("dd MMMM yyyy") },
                        { "DeactivatedBy", deactivatedBy }
                    },
                    RecipientTeamIds = new List<int> { team.TeamId },
                    RecipientRoleIds = new List<int> { OSH_MANAGER_ROLE_ID, STATION_MANAGER_ROLE_ID },
                    Channels = new List<NotificationChannel>
                    {
                        NotificationChannel.InApp,
                        NotificationChannel.Email
                    }
                });

                _logger.LogInformation("Team deactivated notification sent for team {TeamId}", team.TeamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending team deactivated notification for team {TeamId}", team.TeamId);
            }
        }

        public async Task NotifyTeamUpdatedAsync(Team team, List<string> changedFields, string updatedBy)
        {
            try
            {
                var changedFieldsText = changedFields.Any()
                    ? string.Join(", ", changedFields)
                    : "Multiple fields";

                await _eventPublisher.PublishAsync(new NotificationEvent
                {
                    EventType = "TeamUpdated",
                    Category = "Team",
                    Priority = NotificationPriority.Low,
                    ActionUrl = $"/Team/Details/{team.TeamId}",
                    Data = new Dictionary<string, string>
                    {
                        { "TeamName", team.TeamName },
                        { "ChangedFields", changedFieldsText },
                        { "UpdatedDate", DateTime.Now.ToString("dd MMMM yyyy") },
                        { "UpdatedBy", updatedBy }
                    },
                    RecipientTeamIds = new List<int> { team.TeamId }
                });

                _logger.LogInformation("Team updated notification sent for team {TeamId}", team.TeamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending team updated notification for team {TeamId}", team.TeamId);
            }
        }
    }
}
