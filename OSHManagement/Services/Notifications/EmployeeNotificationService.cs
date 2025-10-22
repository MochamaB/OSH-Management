using OSHManagement.Models;
using OSHManagement.Models.Enums;
using OSHManagement.Services.Notifications.DTOs;

namespace OSHManagement.Services.Notifications
{
    /// <summary>
    /// Specialized notification service for Employee-related events
    /// Centralizes all employee notification logic to avoid repetition in controllers
    /// </summary>
    public class EmployeeNotificationService : IEmployeeNotificationService
    {
        private readonly INotificationEventPublisher _eventPublisher;
        private readonly ILogger<EmployeeNotificationService> _logger;

        // Role constants - should match your database Role IDs
        private const int HR_MANAGER_ROLE_ID = 2;
        private const int STATION_MANAGER_ROLE_ID = 3;

        public EmployeeNotificationService(
            INotificationEventPublisher eventPublisher,
            ILogger<EmployeeNotificationService> logger)
        {
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task NotifyEmployeeCreatedAsync(Employee employee, Station station, string createdBy)
        {
            try
            {
                await _eventPublisher.PublishAsync(new NotificationEvent
                {
                    EventType = "EmployeeCreated",
                    Category = "Employee",
                    Priority = NotificationPriority.Normal,
                    ActionUrl = $"/Employee/Details/{employee.EmployeeId}",
                    Data = new Dictionary<string, string>
                    {
                        { "EmployeeName", $"{employee.FirstName} {employee.LastName}" },
                        { "PayrollNo", employee.PayrollNo },
                        { "StationName", station.StationName },
                        { "Designation", employee.Designation ?? "Not specified" },
                        { "CreatedDate", DateTime.Now.ToString("dd MMMM yyyy") },
                        { "CreatedBy", createdBy }
                    },
                    RecipientRoleIds = new List<int>
                    {
                        HR_MANAGER_ROLE_ID,
                        STATION_MANAGER_ROLE_ID
                    },
                    RecipientStationIds = new List<int> { employee.StationId }
                });

                _logger.LogInformation("Employee created notification sent for {PayrollNo}", employee.PayrollNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending employee created notification for {PayrollNo}", employee.PayrollNo);
            }
        }

        public async Task NotifyEmployeeUpdatedAsync(Employee employee, string updatedBy, List<string> changedFields)
        {
            try
            {
                var changedFieldsText = changedFields.Any()
                    ? string.Join(", ", changedFields)
                    : "Multiple fields";

                await _eventPublisher.PublishAsync(new NotificationEvent
                {
                    EventType = "EmployeeUpdated",
                    Category = "Employee",
                    Priority = NotificationPriority.Low,
                    ActionUrl = $"/Employee/Details/{employee.EmployeeId}",
                    Data = new Dictionary<string, string>
                    {
                        { "EmployeeName", $"{employee.FirstName} {employee.LastName}" },
                        { "PayrollNo", employee.PayrollNo },
                        { "ChangedFields", changedFieldsText },
                        { "UpdatedDate", DateTime.Now.ToString("dd MMMM yyyy") },
                        { "UpdatedBy", updatedBy }
                    },
                    RecipientEmployeeIds = new List<int> { employee.EmployeeId }, // Notify the employee
                    RecipientRoleIds = new List<int> { HR_MANAGER_ROLE_ID }
                });

                _logger.LogInformation("Employee updated notification sent for {PayrollNo}", employee.PayrollNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending employee updated notification for {PayrollNo}", employee.PayrollNo);
            }
        }

        public async Task NotifyEmployeeDeactivatedAsync(Employee employee, Station station, string reason, string deactivatedBy)
        {
            try
            {
                await _eventPublisher.PublishAsync(new NotificationEvent
                {
                    EventType = "EmployeeDeactivated",
                    Category = "Employee",
                    Priority = NotificationPriority.Normal,
                    ActionUrl = $"/Employee/Details/{employee.EmployeeId}",
                    Data = new Dictionary<string, string>
                    {
                        { "EmployeeName", $"{employee.FirstName} {employee.LastName}" },
                        { "PayrollNo", employee.PayrollNo },
                        { "StationName", station.StationName },
                        { "Reason", reason ?? "Not specified" },
                        { "DeactivatedDate", DateTime.Now.ToString("dd MMMM yyyy") },
                        { "DeactivatedBy", deactivatedBy }
                    },
                    RecipientEmployeeIds = new List<int> { employee.EmployeeId },
                    RecipientRoleIds = new List<int> { HR_MANAGER_ROLE_ID, STATION_MANAGER_ROLE_ID },
                    Channels = new List<NotificationChannel>
                    {
                        NotificationChannel.InApp,
                        NotificationChannel.Email
                    }
                });

                _logger.LogInformation("Employee deactivated notification sent for {PayrollNo}", employee.PayrollNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending employee deactivated notification for {PayrollNo}", employee.PayrollNo);
            }
        }

        public async Task NotifyEmployeeTransferredAsync(Employee employee, Station oldStation, Station newStation, string transferredBy)
        {
            try
            {
                await _eventPublisher.PublishAsync(new NotificationEvent
                {
                    EventType = "EmployeeTransferred",
                    Category = "Employee",
                    Priority = NotificationPriority.Normal,
                    ActionUrl = $"/Employee/Details/{employee.EmployeeId}",
                    Data = new Dictionary<string, string>
                    {
                        { "EmployeeName", $"{employee.FirstName} {employee.LastName}" },
                        { "PayrollNo", employee.PayrollNo },
                        { "OldStation", oldStation.StationName },
                        { "NewStation", newStation.StationName },
                        { "TransferDate", DateTime.Now.ToString("dd MMMM yyyy") },
                        { "TransferredBy", transferredBy }
                    },
                    RecipientEmployeeIds = new List<int> { employee.EmployeeId },
                    RecipientStationIds = new List<int> { oldStation.StationId, newStation.StationId },
                    RecipientRoleIds = new List<int> { HR_MANAGER_ROLE_ID }
                });

                _logger.LogInformation("Employee transferred notification sent for {PayrollNo}", employee.PayrollNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending employee transferred notification for {PayrollNo}", employee.PayrollNo);
            }
        }

        public async Task NotifyRoleAssignedAsync(Employee employee, string roleName, string assignedBy)
        {
            try
            {
                await _eventPublisher.PublishAsync(new NotificationEvent
                {
                    EventType = "RoleAssigned",
                    Category = "Employee",
                    Priority = NotificationPriority.Normal,
                    ActionUrl = $"/Employee/Details/{employee.EmployeeId}",
                    Data = new Dictionary<string, string>
                    {
                        { "EmployeeName", $"{employee.FirstName} {employee.LastName}" },
                        { "PayrollNo", employee.PayrollNo },
                        { "RoleName", roleName },
                        { "AssignedDate", DateTime.Now.ToString("dd MMMM yyyy") },
                        { "AssignedBy", assignedBy }
                    },
                    RecipientEmployeeIds = new List<int> { employee.EmployeeId },
                    RecipientRoleIds = new List<int> { HR_MANAGER_ROLE_ID },
                    Channels = new List<NotificationChannel>
                    {
                        NotificationChannel.InApp,
                        NotificationChannel.Email
                    }
                });

                _logger.LogInformation("Role assigned notification sent for {PayrollNo}", employee.PayrollNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending role assigned notification for {PayrollNo}", employee.PayrollNo);
            }
        }

        public async Task NotifyEmployeePromotedAsync(Employee employee, string oldDesignation, string newDesignation, string promotedBy)
        {
            try
            {
                await _eventPublisher.PublishAsync(new NotificationEvent
                {
                    EventType = "EmployeePromoted",
                    Category = "Employee",
                    Priority = NotificationPriority.Normal,
                    ActionUrl = $"/Employee/Details/{employee.EmployeeId}",
                    Data = new Dictionary<string, string>
                    {
                        { "EmployeeName", $"{employee.FirstName} {employee.LastName}" },
                        { "PayrollNo", employee.PayrollNo },
                        { "OldDesignation", oldDesignation },
                        { "NewDesignation", newDesignation },
                        { "PromotionDate", DateTime.Now.ToString("dd MMMM yyyy") },
                        { "PromotedBy", promotedBy }
                    },
                    RecipientEmployeeIds = new List<int> { employee.EmployeeId },
                    RecipientRoleIds = new List<int> { HR_MANAGER_ROLE_ID, STATION_MANAGER_ROLE_ID },
                    Channels = new List<NotificationChannel>
                    {
                        NotificationChannel.InApp,
                        NotificationChannel.Email
                    }
                });

                _logger.LogInformation("Employee promoted notification sent for {PayrollNo}", employee.PayrollNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending employee promoted notification for {PayrollNo}", employee.PayrollNo);
            }
        }
    }
}
