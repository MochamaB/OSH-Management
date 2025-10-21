# Notification System Architecture
## OSH Management System

---

## Table of Contents
1. [Overview](#overview)
2. [Core Components](#core-components)
3. [Service Architecture](#service-architecture)
4. [Event-Driven Architecture](#event-driven-architecture)
5. [Template System](#template-system)
6. [Background Processing](#background-processing)
7. [Real-Time Delivery (SignalR)](#real-time-delivery-signalr)
8. [UI Components](#ui-components)
9. [Admin Configuration UI](#admin-configuration-ui)
10. [Notification Triggers](#notification-triggers)
11. [Implementation Plan](#implementation-plan)

---

## Overview

A comprehensive, multi-channel notification system that:
- Sends notifications through multiple channels (In-App, Email, SMS, WhatsApp)
- Uses event-driven architecture for decoupling
- Supports customizable templates
- Provides real-time updates via SignalR
- Allows user-configurable preferences
- Tracks delivery status across all channels

---

## Core Components

### 1. Database Schema

#### **Notification Table**
Stores all in-app notifications.

```sql
CREATE TABLE Notifications (
    NotificationId INT PRIMARY KEY IDENTITY(1,1),
    
    -- Recipient Information (Type-Discriminator Pattern)
    RecipientType NVARCHAR(20) NOT NULL, -- 'Employee', 'Role', 'Station', 'Department', 'Team'
    RecipientId INT NOT NULL, -- Single ID column that works for all recipient types
    
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(1000) NOT NULL,
    NotificationType NVARCHAR(20) NOT NULL, -- 'Info', 'Success', 'Warning', 'Error', 'ActionRequired'
    Priority NVARCHAR(20) NOT NULL DEFAULT 'Normal', -- 'Low', 'Normal', 'High', 'Urgent'
    Category NVARCHAR(50), -- 'Employee', 'Team', 'Incident', 'Training', 'Safety', etc.
    ActionUrl NVARCHAR(500), -- Optional link to related entity
    IsRead BIT NOT NULL DEFAULT 0,
    ReadAt DATETIME2,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2, -- Optional auto-delete date
    CreatedBy NVARCHAR(50),

    INDEX IX_Notifications_Recipient (RecipientType, RecipientId, IsRead),
    INDEX IX_Notifications_Created (CreatedAt DESC)
);
```

**Recipient Pattern Examples:**
- Employee notification: `RecipientType = 'Employee', RecipientId = 123` (Employee #123)
- Role-based: `RecipientType = 'Role', RecipientId = 5` (All users with Role #5)
- Station-based: `RecipientType = 'Station', RecipientId = 10` (All users in Station #10)
- Department-based: `RecipientType = 'Department', RecipientId = 7` (All users in Department #7)
- Team-based: `RecipientType = 'Team', RecipientId = 15` (All members of Team #15)

#### **NotificationDelivery Table**
Tracks multi-channel delivery status.

```sql
CREATE TABLE NotificationDeliveries (
    NotificationDeliveryId INT PRIMARY KEY IDENTITY(1,1),
    NotificationId INT NOT NULL,
    Channel NVARCHAR(20) NOT NULL, -- 'InApp', 'Email', 'SMS', 'WhatsApp'
    RecipientAddress NVARCHAR(255), -- Email address, phone number, WhatsApp number
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Sent', 'Failed', 'Delivered', 'Read'
    SentAt DATETIME2,
    DeliveredAt DATETIME2,
    ReadAt DATETIME2,
    ErrorMessage NVARCHAR(500),
    RetryCount INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (NotificationId) REFERENCES Notifications(NotificationId) ON DELETE CASCADE,
    INDEX IX_NotificationDeliveries_Status (Status, CreatedAt),
    INDEX IX_NotificationDeliveries_Notification (NotificationId)
);
```

#### **NotificationTemplate Table**
Templates for different notification events.

```sql
CREATE TABLE NotificationTemplates (
    TemplateId INT PRIMARY KEY IDENTITY(1,1),
    TemplateName NVARCHAR(100) NOT NULL UNIQUE, -- 'EmployeeCreated', 'TeamMemberAdded', etc.
    Category NVARCHAR(50) NOT NULL, -- 'Employee', 'Team', 'Incident', etc.
    Channel NVARCHAR(20) NOT NULL, -- 'InApp', 'Email', 'SMS', 'WhatsApp'
    SubjectTemplate NVARCHAR(200), -- For email/WhatsApp
    BodyTemplate NVARCHAR(MAX) NOT NULL, -- Supports placeholders: {EmployeeName}, {StationName}, etc.
    IsActive BIT NOT NULL DEFAULT 1,
    Description NVARCHAR(500), -- Admin notes about template usage
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(50),

    UNIQUE INDEX IX_NotificationTemplates_Unique (TemplateName, Channel)
);
```

#### **NotificationPreference Table**
User-specific notification preferences.

```sql
CREATE TABLE NotificationPreferences (
    PreferenceId INT PRIMARY KEY IDENTITY(1,1),
    EmployeeId INT NOT NULL,
    Category NVARCHAR(50) NOT NULL, -- 'Employee', 'Team', 'Incident', 'All', etc.
    InAppEnabled BIT NOT NULL DEFAULT 1,
    EmailEnabled BIT NOT NULL DEFAULT 1,
    SmsEnabled BIT NOT NULL DEFAULT 0,
    WhatsAppEnabled BIT NOT NULL DEFAULT 0,
    MinPriority NVARCHAR(20) NOT NULL DEFAULT 'Normal', -- Only notify if priority >= this
    QuietHoursStart TIME, -- e.g., 22:00
    QuietHoursEnd TIME, -- e.g., 07:00
    DigestFrequency NVARCHAR(20), -- 'Instant', 'Hourly', 'Daily', 'Weekly'
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,

    FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId) ON DELETE CASCADE,
    UNIQUE INDEX IX_NotificationPreferences_Unique (EmployeeId, Category)
);
```

---

## Service Architecture

### 1. Core Services

#### **INotificationService** (Main Orchestrator)
Central service for managing notifications.

```csharp
public interface INotificationService
{
    // Create and send notification
    Task CreateNotificationAsync(NotificationDto notification);

    // Retrieve notifications
    Task<List<NotificationViewModel>> GetUserNotificationsAsync(string employeeId, bool unreadOnly = false, int limit = 50);
    Task<NotificationViewModel?> GetNotificationByIdAsync(int notificationId);

    // Manage notifications
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(string employeeId);
    Task DeleteNotificationAsync(int notificationId);
    Task DeleteAllReadAsync(string employeeId);

    // Statistics
    Task<int> GetUnreadCountAsync(string employeeId);
    Task<Dictionary<string, int>> GetUnreadCountByCategoryAsync(string employeeId);

    // Cleanup
    Task CleanupExpiredNotificationsAsync();
}
```

**Implementation Responsibilities:**
- Create notifications in database
- Resolve recipients (Employee, Role, Station, Department)
- Check user preferences before sending
- Queue delivery to appropriate channels
- Trigger SignalR real-time updates
- Handle notification lifecycle (read, delete, expire)

---

#### **INotificationTemplateService** (Template Management)
Manages notification templates and rendering.

```csharp
public interface INotificationTemplateService
{
    // Retrieve templates
    Task<NotificationTemplate?> GetTemplateAsync(string templateName, NotificationChannel channel);
    Task<List<NotificationTemplate>> GetAllTemplatesAsync();
    Task<List<NotificationTemplate>> GetTemplatesByCategoryAsync(string category);

    // Render templates with data
    string RenderTemplate(NotificationTemplate template, Dictionary<string, string> data);

    // CRUD operations
    Task CreateTemplateAsync(NotificationTemplate template);
    Task UpdateTemplateAsync(NotificationTemplate template);
    Task DeleteTemplateAsync(int templateId);

    // Testing
    Task<string> PreviewTemplateAsync(int templateId, Dictionary<string, string> sampleData);
}
```

**Template Placeholder Syntax:**
- `{PropertyName}` - Simple replacement
- `{PropertyName:Format}` - Formatted replacement (e.g., `{Date:dd/MM/yyyy}`)
- `{If:Condition}...{EndIf}` - Conditional blocks (future enhancement)

---

#### **INotificationChannelService** (Abstract Delivery Interface)
Abstract interface for all notification channels.

```csharp
public interface INotificationChannelService
{
    // Send notification through specific channel
    Task<DeliveryResult> SendAsync(NotificationDelivery delivery);

    // Channel support
    bool SupportsChannel(NotificationChannel channel);
    NotificationChannel Channel { get; }

    // Validation
    bool ValidateRecipientAddress(string address);
}

public class DeliveryResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
```

---

### 2. Channel-Specific Services

All channel services implement `INotificationChannelService`.

#### **InAppNotificationService**
```csharp
public class InAppNotificationService : INotificationChannelService
{
    // Responsibilities:
    // - Store notification in database
    // - Trigger SignalR real-time update to connected users
    // - No external API needed
    // - Mark as delivered immediately
}
```

#### **EmailNotificationService**
```csharp
public class EmailNotificationService : INotificationChannelService
{
    // Dependencies:
    // - SMTP client or SendGrid/Mailgun SDK
    // - Razor view engine for HTML rendering

    // Responsibilities:
    // - Render HTML email from template
    // - Send via SMTP/API
    // - Track delivery status
    // - Support attachments (future)
    // - Handle bounces and failures
}
```

**Configuration (appsettings.json):**
```json
"EmailSettings": {
  "Provider": "SMTP", // or "SendGrid", "Mailgun"
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "SmtpUsername": "noreply@oshmanagement.com",
  "SmtpPassword": "encrypted_password",
  "FromEmail": "noreply@oshmanagement.com",
  "FromName": "OSH Management System"
}
```

#### **SmsNotificationService**
```csharp
public class SmsNotificationService : INotificationChannelService
{
    // Dependencies:
    // - Twilio SDK or Africa's Talking or local SMS gateway

    // Responsibilities:
    // - Format message for SMS (160 char limit)
    // - Send via SMS gateway API
    // - Track delivery status
    // - Handle character encoding (unicode for special chars)
    // - Cost tracking per SMS
}
```

**Configuration:**
```json
"SmsSettings": {
  "Provider": "Twilio", // or "AfricasTalking", "LocalGateway"
  "AccountSid": "your_account_sid",
  "AuthToken": "your_auth_token",
  "FromNumber": "+1234567890",
  "MaxLength": 160
}
```

#### **WhatsAppNotificationService**
```csharp
public class WhatsAppNotificationService : INotificationChannelService
{
    // Dependencies:
    // - WhatsApp Business API or Twilio WhatsApp SDK

    // Responsibilities:
    // - Send WhatsApp messages via approved templates
    // - Support rich media (images, documents, buttons)
    // - Track delivery and read status
    // - Handle template approval workflow
}
```

**Important:** WhatsApp requires pre-approved message templates due to their policy.

---

## Event-Driven Architecture

### 1. Notification Events

#### **NotificationEvent Class**
```csharp
public class NotificationEvent
{
    public string EventType { get; set; } // "EmployeeCreated", "TeamMemberAdded"
    public Dictionary<string, string> Data { get; set; } // {EmployeeName: "John", StationName: "Boito"}
    
    // Recipient targeting (supports multiple types)
    public List<int> RecipientEmployeeIds { get; set; } = new(); // Direct employee IDs
    public List<int> RecipientRoleIds { get; set; } = new(); // Notify all users with these roles
    public List<int> RecipientStationIds { get; set; } = new(); // Notify all users in these stations
    public List<int> RecipientDepartmentIds { get; set; } = new(); // Notify all users in these departments
    public List<int> RecipientTeamIds { get; set; } = new(); // Notify all members of these teams
    
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public string? ActionUrl { get; set; }
    public string Category { get; set; }
    public List<NotificationChannel> Channels { get; set; } = new(); // Leave empty to use user preferences
}
```

#### **NotificationPriority Enum**
```csharp
public enum NotificationPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}
```

#### **NotificationChannel Enum**
```csharp
public enum NotificationChannel
{
    InApp = 1,
    Email = 2,
    SMS = 3,
    WhatsApp = 4
}
```

---

### 2. Event Publisher Pattern

#### **INotificationEventPublisher**
```csharp
public interface INotificationEventPublisher
{
    Task PublishAsync(NotificationEvent notificationEvent);
}
```

#### **Implementation Example**
```csharp
public class NotificationEventPublisher : INotificationEventPublisher
{
    private readonly OshDbContext _context;
    private readonly INotificationTemplateService _templateService;
    
    public async Task PublishAsync(NotificationEvent notificationEvent)
    {
        var notifications = new List<Notification>();
        
        // Get template for this event
        var template = await _templateService.GetTemplateAsync(
            notificationEvent.EventType, 
            NotificationChannel.InApp);
        
        if (template == null) return; // No template configured
        
        // Render message from template
        var title = template.SubjectTemplate ?? notificationEvent.EventType;
        var message = _templateService.RenderTemplate(template, notificationEvent.Data);
        
        // Create notifications for direct employee recipients
        foreach (var employeeId in notificationEvent.RecipientEmployeeIds)
        {
            notifications.Add(new Notification
            {
                RecipientType = "Employee",
                RecipientId = employeeId,
                Title = title,
                Message = message,
                Category = notificationEvent.Category,
                Priority = notificationEvent.Priority.ToString(),
                ActionUrl = notificationEvent.ActionUrl,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        // Create notifications for role-based recipients
        foreach (var roleId in notificationEvent.RecipientRoleIds)
        {
            notifications.Add(new Notification
            {
                RecipientType = "Role",
                RecipientId = roleId,
                Title = title,
                Message = message,
                Category = notificationEvent.Category,
                Priority = notificationEvent.Priority.ToString(),
                ActionUrl = notificationEvent.ActionUrl,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        // Create notifications for station-based recipients
        foreach (var stationId in notificationEvent.RecipientStationIds)
        {
            notifications.Add(new Notification
            {
                RecipientType = "Station",
                RecipientId = stationId,
                Title = title,
                Message = message,
                Category = notificationEvent.Category,
                Priority = notificationEvent.Priority.ToString(),
                ActionUrl = notificationEvent.ActionUrl,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        // Save all notifications
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();
        
        // TODO: Trigger SignalR push for real-time updates (Phase 3)
        // TODO: Queue email/SMS delivery (Phase 2)
    }
}
```

#### **Usage in Controllers**

**Example 1: Employee Created - Notify by Role and Station**
```csharp
// In EmployeeController.Create()
await _notificationEventPublisher.PublishAsync(new NotificationEvent
{
    EventType = "EmployeeCreated",
    Category = "Employee",
    Data = new Dictionary<string, string>
    {
        { "EmployeeName", $"{employee.FirstName} {employee.LastName}" },
        { "PayrollNo", employee.PayrollNo },
        { "StationName", stationName },
        { "CreatedDate", DateTime.Now.ToString("dd/MM/yyyy") }
    },
    RecipientRoleIds = new List<int> { 2 }, // Role ID 2 = HR Manager
    RecipientStationIds = new List<int> { employee.StationId }, // All users in the station
    Priority = NotificationPriority.Normal,
    ActionUrl = $"/Employee/Details/{employee.EmployeeId}"
});
```

**Example 2: Team Member Added - Notify Specific Employee and Team**
```csharp
// In TeamController (when adding member to team)
await _notificationEventPublisher.PublishAsync(new NotificationEvent
{
    EventType = "TeamMemberAdded",
    Category = "Team",
    Data = new Dictionary<string, string>
    {
        { "EmployeeName", $"{employee.FirstName} {employee.LastName}" },
        { "TeamName", team.TeamName },
        { "MemberRole", memberRole },
        { "AppointmentDate", DateTime.Now.ToString("dd/MM/yyyy") }
    },
    RecipientEmployeeIds = new List<int> { employee.EmployeeId }, // Notify the new member
    RecipientTeamIds = new List<int> { team.TeamId }, // Notify all existing team members
    Priority = NotificationPriority.Normal,
    ActionUrl = $"/Team/Details/{team.TeamId}"
});
```

**Example 3: Urgent Incident - Notify Multiple Groups**
```csharp
// In IncidentController.Create()
await _notificationEventPublisher.PublishAsync(new NotificationEvent
{
    EventType = "IncidentReported",
    Category = "Incident",
    Data = new Dictionary<string, string>
    {
        { "IncidentType", incident.IncidentType },
        { "Location", incident.Location },
        { "ReportedBy", reporterName },
        { "Severity", incident.Severity }
    },
    RecipientRoleIds = new List<int> { 3, 5 }, // Safety Manager + OSH Manager
    RecipientStationIds = new List<int> { incident.StationId },
    RecipientDepartmentIds = new List<int> { incident.DepartmentId ?? 0 },
    Priority = NotificationPriority.Urgent,
    ActionUrl = $"/Incident/Details/{incident.IncidentId}",
    Channels = new List<NotificationChannel> 
    { 
        NotificationChannel.InApp, 
        NotificationChannel.Email 
    } // Force both channels for urgent incidents
});
```

---

### 3. Querying Notifications (How Users See Their Notifications)

Since notifications use the type-discriminator pattern, retrieving a user's notifications requires checking multiple recipient types:

```csharp
public async Task<List<Notification>> GetUserNotificationsAsync(int employeeId)
{
    var employee = await _context.Employees
        .Include(e => e.EmployeeRoles)
        .FirstAsync(e => e.EmployeeId == employeeId);
    
    // Get user's role IDs
    var roleIds = employee.EmployeeRoles.Select(er => er.RoleId).ToList();
    
    // Get user's team IDs (via TeamMembers using PayrollNo)
    var teamIds = await _context.TeamMembers
        .Where(tm => tm.EmployeePayroll == employee.PayrollNo && tm.IsActive)
        .Select(tm => tm.TeamId)
        .ToListAsync();
    
    // Query notifications for this user across all recipient types
    var notifications = await _context.Notifications
        .Where(n => 
            // Direct employee notifications
            (n.RecipientType == "Employee" && n.RecipientId == employeeId) ||
            
            // Role-based notifications
            (n.RecipientType == "Role" && roleIds.Contains(n.RecipientId)) ||
            
            // Station-based notifications
            (n.RecipientType == "Station" && n.RecipientId == employee.StationId) ||
            
            // Department-based notifications
            (n.RecipientType == "Department" && employee.DepartmentId.HasValue && n.RecipientId == employee.DepartmentId.Value) ||
            
            // Team-based notifications
            (n.RecipientType == "Team" && teamIds.Contains(n.RecipientId))
        )
        .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow) // Exclude expired
        .OrderByDescending(n => n.CreatedAt)
        .Take(50)
        .ToListAsync();
    
    return notifications;
}
```

**Optimized Query with Index:**
```sql
-- The index on (RecipientType, RecipientId, IsRead) makes this fast
SELECT * FROM Notifications
WHERE (RecipientType = 'Employee' AND RecipientId = 123)
   OR (RecipientType = 'Role' AND RecipientId IN (2, 5, 7))
   OR (RecipientType = 'Station' AND RecipientId = 10)
   OR (RecipientType = 'Department' AND RecipientId = 8)
   OR (RecipientType = 'Team' AND RecipientId IN (15, 22))
ORDER BY CreatedAt DESC;
```

---

### 4. Event Processing Flow

```
Controller Action
    ↓
Publish NotificationEvent
    ↓
NotificationEventPublisher
    ↓
Resolve Recipients (Create notifications for each recipient type)
    ↓
For Each Recipient:
    - Check NotificationPreference (channels, quiet hours, digest)
    - Get Template (EventType + Channel)
    - Render Template with Data
    - Create Notification record
    - Queue NotificationDelivery for each enabled channel
    ↓
Background Job processes delivery queue
    ↓
Send via Channel Services (Email, SMS, WhatsApp)
    ↓
Update Delivery Status
    ↓
SignalR pushes InApp notification to user
```

---

## Template System

### 1. Template Structure

#### **Template Placeholders**
Templates support dynamic data replacement using `{PlaceholderName}` syntax.

**Example Templates:**

**InApp Template:**
```
Title: New Employee Added
Message: {EmployeeName} (Payroll: {PayrollNo}) has been added to {StationName}.
```

**Email Template:**
```
Subject: New Employee Added - {EmployeeName}

Body:
Hello {RecipientName},

A new employee has been added to the OSH Management System:

Employee Details:
- Name: {EmployeeName}
- Payroll Number: {PayrollNo}
- Station: {StationName}
- Date Added: {CreatedDate}

You can view the full employee profile here:
{ActionUrl}

Best regards,
OSH Management System
```

**SMS Template (160 chars max):**
```
New employee {EmployeeName} added to {StationName}. Payroll: {PayrollNo}. View: {ActionUrl}
```

**WhatsApp Template:**
```
🆕 *New Employee Added*

*Name:* {EmployeeName}
*Payroll:* {PayrollNo}
*Station:* {StationName}
*Date:* {CreatedDate}

[View Details]({ActionUrl})
```

---

### 2. Template Categories by Event

#### **Employee Management**
- `EmployeeCreated` - New employee added
- `EmployeeUpdated` - Employee details changed
- `EmployeeDeactivated` - Employee status changed to inactive
- `EmployeePromoted` - Job title/role changed
- `EmployeeTransferred` - Station/department changed

#### **Team Management**
- `TeamCreated` - New team formed
- `TeamMemberAdded` - Employee added to team
- `TeamMemberRemoved` - Employee removed from team
- `TeamRoleChanged` - Member role updated (e.g., promoted to Team Lead)
- `TeamDisbanded` - Team deactivated

#### **Incident Management**
- `IncidentReported` - New incident logged
- `IncidentAssigned` - Assigned to investigator
- `InvestigationStarted` - Investigation begun
- `InvestigationCompleted` - Investigation finished
- `ActionRequired` - Corrective action needed

#### **Training Management**
- `TrainingScheduled` - New training session scheduled
- `TrainingReminder` - Reminder 24h before training
- `TrainingCompleted` - Training session completed
- `CertificateIssued` - Certificate generated
- `CertificateExpiring` - Certificate expires in 30 days

#### **Safety & Compliance**
- `InspectionDue` - Scheduled inspection reminder
- `InspectionCompleted` - Inspection results available
- `HazardReported` - New hazard identified
- `PpeRequested` - PPE request submitted
- `PpeIssued` - PPE issued to employee

#### **System Notifications**
- `PasswordChanged` - Password updated
- `RoleAssigned` - New system role granted
- `AccessGranted` - Access to new module granted
- `SystemMaintenance` - Scheduled downtime notice

---

### 3. Multi-Channel Template Strategy

Each event can have different templates per channel:

| Event | InApp | Email | SMS | WhatsApp |
|-------|-------|-------|-----|----------|
| EmployeeCreated | Short summary | Full details HTML | Brief text | Rich formatted |
| IncidentReported | Alert with link | Detailed report | Urgent notice | Location + details |
| TrainingReminder | Reminder card | Calendar invite | Time + location | Interactive buttons |

---

### 4. Template Variables (Common)

Standard variables available in all templates:
- `{RecipientName}` - Name of notification recipient
- `{RecipientEmail}` - Email of recipient
- `{SystemName}` - "OSH Management System"
- `{CurrentDate}` - Today's date
- `{CurrentTime}` - Current time
- `{ActionUrl}` - Link to related entity
- `{SenderName}` - Who triggered the event

---

## Background Processing

### 1. Queue-Based Delivery

Use **Hangfire** for background job processing.

#### **Job Queue Types**
- **Immediate Queue**: High/Urgent priority notifications (processed instantly)
- **Standard Queue**: Normal priority (processed within 1 minute)
- **Bulk Queue**: Low priority or digest emails (batched)

#### **Implementation**
```csharp
public class NotificationBackgroundService
{
    public void QueueNotificationDelivery(NotificationDelivery delivery)
    {
        switch (delivery.Priority)
        {
            case NotificationPriority.Urgent:
            case NotificationPriority.High:
                BackgroundJob.Enqueue<INotificationChannelService>(
                    service => service.SendAsync(delivery));
                break;

            case NotificationPriority.Normal:
                BackgroundJob.Schedule<INotificationChannelService>(
                    service => service.SendAsync(delivery),
                    TimeSpan.FromSeconds(30));
                break;

            case NotificationPriority.Low:
                BackgroundJob.Schedule<INotificationChannelService>(
                    service => service.SendAsync(delivery),
                    TimeSpan.FromMinutes(5));
                break;
        }
    }
}
```

---

### 2. Retry Logic

#### **Retry Strategy**
- **Max Retries**: 3 attempts
- **Backoff**: Exponential (1min, 5min, 15min)
- **Failure Handling**: Mark as permanently failed after 3 failures

```csharp
public async Task<DeliveryResult> SendWithRetryAsync(NotificationDelivery delivery)
{
    int maxRetries = 3;
    int[] retryDelaysInMinutes = { 1, 5, 15 };

    for (int attempt = 0; attempt <= maxRetries; attempt++)
    {
        var result = await _channelService.SendAsync(delivery);

        if (result.Success)
        {
            delivery.Status = "Delivered";
            return result;
        }

        if (attempt < maxRetries)
        {
            delivery.RetryCount = attempt + 1;
            await Task.Delay(TimeSpan.FromMinutes(retryDelaysInMinutes[attempt]));
        }
    }

    delivery.Status = "Failed";
    delivery.ErrorMessage = "Max retries exceeded";
    return new DeliveryResult { Success = false };
}
```

---

### 3. Batch Processing & Digest

#### **Digest Notifications**
For users who prefer daily/weekly summaries instead of instant notifications.

```csharp
public class NotificationDigestService
{
    // Send daily digest at 8 AM
    [RecurringJob("0 8 * * *")]
    public async Task SendDailyDigestAsync()
    {
        var usersWithDailyDigest = await GetUsersWithDigestPreference("Daily");

        foreach (var user in usersWithDailyDigest)
        {
            var unreadNotifications = await GetUnreadNotificationsAsync(user.EmployeeId);

            if (unreadNotifications.Any())
            {
                var digestEmail = BuildDigestEmail(user, unreadNotifications);
                await _emailService.SendAsync(digestEmail);
            }
        }
    }
}
```

---

### 4. Scheduled Jobs (Hangfire)

```csharp
// Cleanup expired notifications (daily at 2 AM)
RecurringJob.AddOrUpdate<INotificationService>(
    "cleanup-expired-notifications",
    service => service.CleanupExpiredNotificationsAsync(),
    "0 2 * * *");

// Send training reminders (daily at 9 AM)
RecurringJob.AddOrUpdate<ITrainingNotificationService>(
    "training-reminders",
    service => service.SendTrainingRemindersAsync(),
    "0 9 * * *");

// Certificate expiry warnings (weekly on Monday at 8 AM)
RecurringJob.AddOrUpdate<ICertificateService>(
    "certificate-expiry-warnings",
    service => service.SendExpiryWarningsAsync(),
    "0 8 * * 1");
```

---

## Real-Time Delivery (SignalR)

### 1. NotificationHub

```csharp
using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub
{
    private readonly INotificationService _notificationService;

    public NotificationHub(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // Send notification to specific user
    public async Task SendNotificationToUser(string employeeId, NotificationViewModel notification)
    {
        await Clients.User(employeeId).SendAsync("ReceiveNotification", notification);
    }

    // Send notification to all users with specific role
    public async Task SendNotificationToRole(int roleId, NotificationViewModel notification)
    {
        await Clients.Group($"Role_{roleId}").SendAsync("ReceiveNotification", notification);
    }

    // User joins their role groups on connect
    public override async Task OnConnectedAsync()
    {
        var employeeId = Context.User?.Identity?.Name;
        if (employeeId != null)
        {
            var userRoles = await GetUserRolesAsync(employeeId);
            foreach (var roleId in userRoles)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{roleId}");
            }
        }

        await base.OnConnectedAsync();
    }
}
```

---

### 2. Client-Side Integration

#### **_Layout.cshtml** (SignalR Connection)
```html
<!-- SignalR Script -->
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@latest/dist/browser/signalr.min.js"></script>

<script>
    // Initialize SignalR connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .withAutomaticReconnect()
        .build();

    // Handle incoming notifications
    connection.on("ReceiveNotification", function (notification) {
        console.log("New notification:", notification);

        // Update badge count
        updateNotificationBadge();

        // Show toast notification
        showToast(notification);

        // Play sound for urgent notifications
        if (notification.priority === "Urgent") {
            playNotificationSound();
        }

        // Add to notification dropdown
        prependNotificationToDropdown(notification);
    });

    // Start connection
    connection.start()
        .then(() => console.log("SignalR connected"))
        .catch(err => console.error("SignalR error:", err));

    // Reconnection handling
    connection.onreconnecting(error => {
        console.log("SignalR reconnecting...");
    });

    connection.onreconnected(connectionId => {
        console.log("SignalR reconnected");
        updateNotificationBadge(); // Refresh badge
    });
</script>
```

---

### 3. Real-Time Notification Flow

```
Event Triggered (e.g., Employee Created)
    ↓
NotificationService.CreateNotificationAsync()
    ↓
Save to Database
    ↓
Queue External Channels (Email, SMS)
    ↓
[REAL-TIME] NotificationHub.SendNotificationToUser()
    ↓
SignalR pushes to connected client
    ↓
Client receives notification
    ↓
Update UI (badge, dropdown, toast)
```

---

## UI Components

### 1. Notification Bell/Icon (Header)

#### **Location**: `_Layout.cshtml` (Header Navigation)

```html
<div class="notification-dropdown dropdown">
    <a href="javascript:void(0);" class="nav-link icon" data-bs-toggle="dropdown" aria-expanded="false">
        <i class="ri-notification-3-line header-icon"></i>
        <span class="notification-badge badge bg-danger rounded-pill" id="notificationBadge">0</span>
    </a>

    <div class="dropdown-menu dropdown-menu-end notification-dropdown-menu">
        <!-- Header -->
        <div class="notification-header d-flex align-items-center justify-content-between p-3 border-bottom">
            <h6 class="mb-0">Notifications</h6>
            <a href="javascript:void(0);" onclick="markAllAsRead()" class="text-primary fs-12">Mark all as read</a>
        </div>

        <!-- Notification List -->
        <div class="notification-list" id="notificationList" style="max-height: 400px; overflow-y: auto;">
            <!-- Notifications loaded via AJAX -->
        </div>

        <!-- Footer -->
        <div class="notification-footer text-center p-2 border-top">
            <a href="/Notifications" class="text-primary">View All Notifications</a>
        </div>
    </div>
</div>
```

#### **JavaScript Functions**
```javascript
// Load recent notifications
function loadNotifications() {
    $.get('/Notifications/GetRecent?limit=10', function(data) {
        renderNotifications(data);
        updateBadgeCount(data.unreadCount);
    });
}

// Mark all as read
function markAllAsRead() {
    $.post('/Notifications/MarkAllAsRead', function() {
        $('#notificationBadge').text('0').hide();
        $('.notification-item').removeClass('unread');
    });
}

// Update badge count
function updateBadgeCount(count) {
    const badge = $('#notificationBadge');
    if (count > 0) {
        badge.text(count > 99 ? '99+' : count).show();
    } else {
        badge.hide();
    }
}
```

---

### 2. Notification Center Page

#### **Route**: `/Notifications`

**Features:**
- DataTable with all notifications
- Filters: Category, Priority, Read/Unread, Date Range
- Bulk actions: Mark as read, Delete
- Notification details modal
- Pagination

**Layout:**
```
+--------------------------------------------------+
| [Filters]  Category: [All ▼]  Status: [All ▼]   |
|           Priority: [All ▼]   Date: [____]       |
+--------------------------------------------------+
| [ ] | Icon | Title             | Time  | Actions |
|-----|------|-------------------|-------|---------|
| [x] | 📋   | New Employee...   | 2h ago| [View]  |
| [ ] | ⚠️   | Incident Report...| 5h ago| [View]  |
| [ ] | 📅   | Training Reminder.| 1d ago| [View]  |
+--------------------------------------------------+
| [Mark Selected as Read]  [Delete Selected]       |
+--------------------------------------------------+
```

---

### 3. Notification Card Component

#### **Partial View**: `_NotificationCard.cshtml`

```html
@model NotificationViewModel

<div class="notification-item @(Model.IsRead ? "" : "unread")" data-notification-id="@Model.NotificationId">
    <div class="d-flex align-items-start gap-3 p-3 border-bottom">
        <!-- Icon -->
        <div class="flex-shrink-0">
            <span class="avatar avatar-md avatar-rounded bg-@Model.TypeColorClass">
                <i class="@Model.Icon"></i>
            </span>
        </div>

        <!-- Content -->
        <div class="flex-fill">
            <div class="d-flex justify-content-between align-items-start">
                <h6 class="mb-1">@Model.Title</h6>
                <span class="fs-11 text-muted">@Model.TimeAgo</span>
            </div>
            <p class="mb-2 fs-13 text-muted">@Model.Message</p>

            @if (!string.IsNullOrEmpty(Model.ActionUrl))
            {
                <a href="@Model.ActionUrl" class="btn btn-sm btn-primary">
                    View Details
                </a>
            }
        </div>

        <!-- Actions -->
        <div class="dropdown">
            <button class="btn btn-icon btn-sm" data-bs-toggle="dropdown">
                <i class="ri-more-2-fill"></i>
            </button>
            <ul class="dropdown-menu">
                <li><a class="dropdown-item" onclick="markAsRead(@Model.NotificationId)">Mark as Read</a></li>
                <li><a class="dropdown-item" onclick="deleteNotification(@Model.NotificationId)">Delete</a></li>
            </ul>
        </div>
    </div>
</div>
```

---

### 4. Toast Notification (Real-Time)

```javascript
function showToast(notification) {
    const toast = `
        <div class="toast align-items-center text-white bg-${getPriorityColor(notification.priority)} border-0" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${notification.title}</strong>
                    <p class="mb-0">${notification.message}</p>
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>`;

    $('#toastContainer').append(toast);
    $('.toast').last().toast('show');
}
```

---

## Admin Configuration UI

### 1. Template Management

#### **Route**: `/Admin/NotificationTemplates`

**Features:**
- List all templates grouped by category
- Create/Edit template with live preview
- Test send with sample data
- Activate/Deactivate templates
- Template versioning (future)

**Template Editor:**
```
+------------------------------------------------------------+
| Template Name: [EmployeeCreated_Email                   ▼] |
| Category:      [Employee                                ▼] |
| Channel:       [Email                                   ▼] |
+------------------------------------------------------------+
| Subject: [New Employee Added - {EmployeeName}            ] |
+------------------------------------------------------------+
| Body Template:                                             |
| +--------------------------------------------------------+ |
| | Hello {RecipientName},                                 | |
| |                                                        | |
| | A new employee has been added:                        | |
| | - Name: {EmployeeName}                                | |
| | - Payroll: {PayrollNo}                                | |
| |                                                        | |
| +--------------------------------------------------------+ |
+------------------------------------------------------------+
| Available Placeholders:                                    |
| {RecipientName}, {EmployeeName}, {PayrollNo},             |
| {StationName}, {CreatedDate}, {ActionUrl}                 |
+------------------------------------------------------------+
| [Preview with Sample Data] [Test Send] [Save Template]    |
+------------------------------------------------------------+
```

---

### 2. Channel Configuration

#### **Route**: `/Admin/NotificationSettings`

**Email Settings:**
```
+------------------------------------------------------------+
| Email Provider:     [SMTP                                ▼] |
| SMTP Host:         [smtp.gmail.com                        ] |
| SMTP Port:         [587                                   ] |
| Username:          [noreply@oshmanagement.com             ] |
| Password:          [********************                  ] |
| From Email:        [noreply@oshmanagement.com             ] |
| From Name:         [OSH Management System                 ] |
+------------------------------------------------------------+
| [Test Email Connection] [Save Settings]                    |
+------------------------------------------------------------+
```

**SMS Settings:**
```
+------------------------------------------------------------+
| SMS Provider:      [Twilio                               ▼] |
| Account SID:       [AC************************************] |
| Auth Token:        [********************                  ] |
| From Number:       [+1234567890                           ] |
| Max Message Length:[160                                   ] |
+------------------------------------------------------------+
| [Test SMS] [Save Settings]                                 |
+------------------------------------------------------------+
```

**WhatsApp Settings:**
```
+------------------------------------------------------------+
| Provider:          [Twilio WhatsApp API                  ▼] |
| API Key:           [********************                  ] |
| WhatsApp Number:   [+1234567890                           ] |
| Template Status:   [3 Approved, 2 Pending                 ] |
+------------------------------------------------------------+
| [View Templates] [Save Settings]                           |
+------------------------------------------------------------+
```

---

### 3. User Notification Preferences

#### **Route**: `/Profile/NotificationPreferences`

**User can customize:**
```
+------------------------------------------------------------+
| Category        | In-App | Email | SMS | WhatsApp         |
|-----------------|--------|-------|-----|------------------|
| Employee        | [x]    | [x]   | [ ] | [ ]              |
| Team            | [x]    | [x]   | [ ] | [x]              |
| Incident        | [x]    | [x]   | [x] | [x]   (Urgent)   |
| Training        | [x]    | [x]   | [ ] | [ ]              |
| Safety          | [x]    | [ ]   | [ ] | [ ]              |
+------------------------------------------------------------+
| Minimum Priority: [Normal                                ▼] |
| Quiet Hours:      [22:00] to [07:00]                       |
| Digest Frequency: [Instant                               ▼] |
+------------------------------------------------------------+
| [Save Preferences]                                         |
+------------------------------------------------------------+
```

---

## Notification Triggers

### Event-to-Recipient Matrix

| Event                   | Trigger Point                       | Recipients                                      | Channels         | Priority |
|-------------------------|-------------------------------------|-------------------------------------------------|------------------|----------|
| **Employee Created**    | `EmployeeController.Create()`       | HR Managers, Station Manager                    | InApp, Email     | Normal   |
| **Employee Updated**    | `EmployeeController.Edit()`         | HR Managers, Employee                           | InApp, Email     | Low      |
| **Employee Deactivated**| `EmployeeController.Deactivate()`   | HR Managers, Station Manager, Employee          | InApp, Email, SMS| Normal   |
| **Team Created**        | `TeamController.Create()`           | Team Members, Station Manager, Safety Officer   | InApp, Email     | Normal   |
| **Team Member Added**   | `TeamController.AddMember()`        | Team Lead, New Member, HOD                      | InApp, Email     | Normal   |
| **Team Member Removed** | `TeamController.RemoveMember()`     | Removed Member, Team Lead, HOD                  | InApp, Email     | Normal   |
| **Incident Reported**   | `IncidentController.Create()`       | Safety Officer, Station Manager, Investigation Team | InApp, Email, SMS | Urgent   |
| **Incident Assigned**   | `IncidentController.Assign()`       | Assigned Investigator, Reporter                 | InApp, Email, SMS| High     |
| **Investigation Started**| `IncidentController.StartInvestigation()` | Reporter, Station Manager, Safety Officer | InApp, Email     | Normal   |
| **Investigation Completed** | `IncidentController.CompleteInvestigation()` | Reporter, Station Manager, Management | InApp, Email | Normal |
| **Training Scheduled**  | `TrainingController.Create()`       | Enrolled Employees, Training Coordinator        | InApp, Email     | Normal   |
| **Training Reminder**   | Background Job (24h before)         | Enrolled Employees                              | InApp, Email, SMS| High     |
| **Training Completed**  | `TrainingController.MarkComplete()` | Employee, Supervisor, Training Coordinator      | InApp, Email     | Low      |
| **Certificate Issued**  | `CertificateController.Issue()`     | Employee, Supervisor                            | InApp, Email     | Normal   |
| **Certificate Expiring**| Background Job (30 days before)     | Employee, Supervisor, HR                        | InApp, Email, SMS| High     |
| **Inspection Due**      | Background Job (7 days before)      | Station Manager, Safety Officer, OSH Committee  | InApp, Email     | High     |
| **Inspection Completed**| `InspectionController.Submit()`     | Station Manager, Safety Officer, Management     | InApp, Email     | Normal   |
| **Hazard Reported**     | `HazardController.Create()`         | Safety Officer, Station Manager, OSH Committee  | InApp, Email, SMS| Urgent   |
| **PPE Requested**       | `PpeController.Request()`           | PPE Manager, Station Manager                    | InApp, Email     | Normal   |
| **PPE Issued**          | `PpeController.Issue()`             | Employee, Supervisor                            | InApp            | Low      |
| **Password Changed**    | `AccountController.ChangePassword()`| Employee (self)                                 | InApp, Email     | Normal   |
| **Role Assigned**       | `EmployeeController.AssignRole()`   | Employee, HR Manager                            | InApp, Email     | Normal   |
| **System Maintenance**  | Admin Scheduled                     | All Active Users                                | InApp, Email     | High     |

---

### Implementation Example

```csharp
// In EmployeeController.Create() - After saving employee
await _notificationEventPublisher.PublishAsync(new NotificationEvent
{
    EventType = "EmployeeCreated",
    Category = "Employee",
    Data = new Dictionary<string, string>
    {
        { "EmployeeName", $"{employee.FirstName} {employee.LastName}" },
        { "PayrollNo", employee.PayrollNo },
        { "StationName", station.StationName },
        { "CreatedDate", DateTime.Now.ToString("dd MMMM yyyy") },
        { "CreatedBy", User.Identity.Name }
    },
    RecipientRoleIds = new List<int>
    {
        RoleConstants.HR_MANAGER,
        RoleConstants.STATION_MANAGER
    },
    RecipientStationIds = new List<int> { employee.StationId },
    Priority = NotificationPriority.Normal,
    ActionUrl = $"/Employee/Details/{employee.EmployeeId}",
    Channels = new List<NotificationChannel>
    {
        NotificationChannel.InApp,
        NotificationChannel.Email
    }
});
```

---

## Implementation Plan

### **Phase 1: MVP (Foundation) - Week 1-2**

**Objective:** Core notification infrastructure with in-app notifications only.

#### **Tasks:**
1. **Database Setup**
   - [ ] Create migration for `Notifications` table
   - [ ] Create migration for `NotificationDeliveries` table
   - [ ] Create migration for `NotificationTemplates` table
   - [ ] Create migration for `NotificationPreferences` table
   - [ ] Seed initial templates for Employee/Team events
   - [ ] Apply migrations

2. **Core Models & DTOs**
   - [ ] Create `Notification` entity model
   - [ ] Create `NotificationDelivery` entity model
   - [ ] Create `NotificationTemplate` entity model
   - [ ] Create `NotificationPreference` entity model
   - [ ] Create `NotificationDto` for creating notifications
   - [ ] Create `NotificationViewModel` for UI display
   - [ ] Create `NotificationEvent` class
   - [ ] Create enums: `NotificationPriority`, `NotificationChannel`, `NotificationType`

3. **Core Services**
   - [ ] Create `INotificationService` interface
   - [ ] Implement `NotificationService` with core methods:
     - CreateNotificationAsync()
     - GetUserNotificationsAsync()
     - MarkAsReadAsync()
     - MarkAllAsReadAsync()
     - GetUnreadCountAsync()
   - [ ] Create `INotificationTemplateService` interface
   - [ ] Implement `NotificationTemplateService`:
     - GetTemplateAsync()
     - RenderTemplate() (string replacement)
   - [ ] Create `INotificationEventPublisher` interface
   - [ ] Implement `NotificationEventPublisher`:
     - PublishAsync()
     - Resolve recipients from roles/stations
   - [ ] Register services in `Program.cs`

4. **In-App Channel**
   - [ ] Create `INotificationChannelService` interface
   - [ ] Implement `InAppNotificationService`
   - [ ] Store notifications in database
   - [ ] Mark as delivered immediately

5. **UI Components**
   - [ ] Create notification bell icon in `_Layout.cshtml`
   - [ ] Implement notification dropdown with AJAX loading
   - [ ] Create `NotificationsController` with actions:
     - GetRecent() - Returns JSON of last 10 notifications
     - MarkAsRead(id)
     - MarkAllAsRead()
     - Delete(id)
   - [ ] Add notification badge with unread count
   - [ ] Create `_NotificationCard.cshtml` partial view
   - [ ] Add JavaScript functions for notification management

6. **Integration**
   - [ ] Add notification trigger to `EmployeeController.Create()`
   - [ ] Add notification trigger to `TeamController.Create()`
   - [ ] Test notification flow end-to-end

**Deliverables:**
- ✅ Database schema with all notification tables
- ✅ Core notification service with CRUD operations
- ✅ Template service with basic string replacement
- ✅ In-app notifications working (bell icon + dropdown)
- ✅ 2-3 events triggering notifications (Employee Created, Team Created)

---

### **Phase 2: Email Notifications - Week 3**

**Objective:** Add email delivery channel with SMTP support.

#### **Tasks:**
1. **Email Service**
   - [ ] Create `EmailNotificationService` implementing `INotificationChannelService`
   - [ ] Configure SMTP settings in `appsettings.json`
   - [ ] Implement SendAsync() method using `SmtpClient` or FluentEmail
   - [ ] Create HTML email templates (basic styling)
   - [ ] Handle delivery status tracking

2. **Email Templates**
   - [ ] Create email templates for key events:
     - EmployeeCreated_Email
     - TeamMemberAdded_Email
     - IncidentReported_Email
   - [ ] Add template rendering with HTML support
   - [ ] Test email rendering with sample data

3. **User Preferences**
   - [ ] Create default notification preferences for new users
   - [ ] Implement preference checking before sending emails
   - [ ] Add "Unsubscribe" link in emails (optional)

4. **Testing**
   - [ ] Test SMTP connection
   - [ ] Send test emails to verify formatting
   - [ ] Test email delivery tracking

**Deliverables:**
- ✅ Email notifications working via SMTP
- ✅ HTML email templates for 3+ events
- ✅ User preference checking (email on/off per category)
- ✅ Delivery status tracking for emails

---

### **Phase 3: Real-Time Updates (SignalR) - Week 4**

**Objective:** Add real-time push notifications to connected users.

#### **Tasks:**
1. **SignalR Setup**
   - [ ] Install `Microsoft.AspNetCore.SignalR` NuGet package
   - [ ] Create `NotificationHub.cs`
   - [ ] Configure SignalR in `Program.cs`
   - [ ] Map hub endpoint: `/notificationHub`

2. **Hub Methods**
   - [ ] Implement `SendNotificationToUser(employeeId, notification)`
   - [ ] Implement `SendNotificationToRole(roleId, notification)`
   - [ ] Implement `OnConnectedAsync()` - Join user to role groups
   - [ ] Implement `OnDisconnectedAsync()` - Cleanup

3. **Client Integration**
   - [ ] Add SignalR client library to `_Layout.cshtml`
   - [ ] Initialize SignalR connection on page load
   - [ ] Listen for `ReceiveNotification` event
   - [ ] Update notification bell badge in real-time
   - [ ] Show toast notification for new alerts
   - [ ] Add notification to dropdown without refresh

4. **Service Integration**
   - [ ] Inject `IHubContext<NotificationHub>` into `NotificationService`
   - [ ] Trigger SignalR push after creating in-app notification
   - [ ] Handle connection failures gracefully

5. **Toast Notifications**
   - [ ] Implement toast notification UI component
   - [ ] Add sound for urgent notifications (optional)
   - [ ] Add browser notification API support (optional)

**Deliverables:**
- ✅ Real-time notifications via SignalR
- ✅ Toast notifications for new alerts
- ✅ Badge updates without page refresh
- ✅ Notifications appear in dropdown instantly

---

### **Phase 4: Notification Center Page - Week 5**

**Objective:** Full notification management page with filters and bulk actions.

#### **Tasks:**
1. **Notification Center Page**
   - [ ] Create `/Notifications/Index` view
   - [ ] Implement DataTable with server-side processing
   - [ ] Add filters:
     - Category dropdown
     - Priority dropdown
     - Read/Unread toggle
     - Date range picker
   - [ ] Add pagination
   - [ ] Add search functionality

2. **Bulk Actions**
   - [ ] Add "Select All" checkbox
   - [ ] Implement "Mark Selected as Read" button
   - [ ] Implement "Delete Selected" button
   - [ ] Add confirmation dialogs

3. **Notification Details Modal**
   - [ ] Create modal for full notification details
   - [ ] Show delivery status across channels
   - [ ] Show timestamps (created, read, delivered)
   - [ ] Add actions (Mark Read, Delete, Archive)

4. **Controller Actions**
   - [ ] `Index(filters)` - Main page with filtered results
   - [ ] `GetPaginated(filters)` - JSON for DataTable
   - [ ] `Details(id)` - Notification details
   - [ ] `BulkMarkAsRead(ids[])`
   - [ ] `BulkDelete(ids[])`

**Deliverables:**
- ✅ Full notification center page
- ✅ Advanced filtering and search
- ✅ Bulk operations (mark read, delete)
- ✅ Notification details modal
- ✅ Responsive design for mobile

---

### **Phase 5: Background Processing (Hangfire) - Week 6**

**Objective:** Queue-based notification delivery with retry logic.

#### **Tasks:**
1. **Hangfire Setup**
   - [ ] Install `Hangfire.Core` and `Hangfire.SqlServer` NuGet packages
   - [ ] Configure Hangfire in `Program.cs`
   - [ ] Add Hangfire dashboard: `/hangfire`
   - [ ] Secure dashboard with authorization

2. **Queue Implementation**
   - [ ] Create `NotificationBackgroundService`
   - [ ] Implement job queuing:
     - Immediate queue (Urgent/High)
     - Standard queue (Normal - 30s delay)
     - Bulk queue (Low - 5min delay)
   - [ ] Queue email/SMS deliveries as background jobs

3. **Retry Logic**
   - [ ] Implement retry with exponential backoff
   - [ ] Track `RetryCount` in `NotificationDeliveries`
   - [ ] Mark as permanently failed after 3 retries
   - [ ] Log failures for monitoring

4. **Recurring Jobs**
   - [ ] Daily notification digest (8 AM)
   - [ ] Cleanup expired notifications (2 AM)
   - [ ] Training reminders (9 AM)
   - [ ] Certificate expiry warnings (Monday 8 AM)

5. **Monitoring**
   - [ ] Add dashboard widget showing:
     - Jobs queued
     - Jobs failed
     - Average delivery time
   - [ ] Set up email alerts for job failures

**Deliverables:**
- ✅ Hangfire integrated with job queue
- ✅ Background processing for email/SMS
- ✅ Retry logic with exponential backoff
- ✅ Recurring jobs for scheduled notifications
- ✅ Hangfire dashboard for monitoring

---

### **Phase 6: SMS & WhatsApp Channels - Week 7-8**

**Objective:** Add SMS and WhatsApp delivery channels.

#### **Tasks (SMS):**
1. **SMS Service**
   - [ ] Choose SMS provider (Twilio, Africa's Talking, etc.)
   - [ ] Create `SmsNotificationService` implementing `INotificationChannelService`
   - [ ] Configure SMS provider credentials in `appsettings.json`
   - [ ] Implement SendAsync() using provider SDK
   - [ ] Handle character limits (160 chars)
   - [ ] Track delivery status via webhooks

2. **SMS Templates**
   - [ ] Create SMS templates for urgent events:
     - IncidentReported_SMS
     - TrainingReminder_SMS
     - CertificateExpiring_SMS
   - [ ] Ensure messages fit in 160 characters
   - [ ] Test SMS delivery

3. **Cost Tracking**
   - [ ] Add `Cost` field to `NotificationDeliveries`
   - [ ] Track SMS cost per delivery
   - [ ] Create report for SMS usage/cost

#### **Tasks (WhatsApp):**
1. **WhatsApp Service**
   - [ ] Choose provider (Twilio WhatsApp API or WhatsApp Business API)
   - [ ] Create `WhatsAppNotificationService`
   - [ ] Configure WhatsApp API credentials
   - [ ] Submit templates for WhatsApp approval
   - [ ] Implement SendAsync() using approved templates

2. **WhatsApp Templates**
   - [ ] Submit templates to WhatsApp for approval:
     - IncidentReported_WhatsApp
     - TrainingScheduled_WhatsApp
     - CertificateExpiring_WhatsApp
   - [ ] Wait for template approval (1-2 days)
   - [ ] Test WhatsApp message delivery

3. **Rich Features**
   - [ ] Add support for WhatsApp buttons (optional)
   - [ ] Add support for images/documents (optional)
   - [ ] Track read receipts

**Deliverables:**
- ✅ SMS notifications working via Twilio/Africa's Talking
- ✅ WhatsApp notifications with approved templates
- ✅ SMS/WhatsApp templates for 3+ urgent events
- ✅ Cost tracking for SMS usage
- ✅ Delivery status webhooks configured

---

### **Phase 7: Admin Configuration UI - Week 9**

**Objective:** Allow admins to manage templates and channel settings.

#### **Tasks:**
1. **Template Management UI**
   - [ ] Create `/Admin/NotificationTemplates` page
   - [ ] List all templates grouped by category
   - [ ] Create template editor form:
     - Template name
     - Category dropdown
     - Channel dropdown
     - Subject field (for email/WhatsApp)
     - Body textarea with placeholder hints
   - [ ] Implement CRUD operations
   - [ ] Add template preview with sample data
   - [ ] Add "Test Send" button to send sample notification

2. **Channel Configuration UI**
   - [ ] Create `/Admin/NotificationSettings` page
   - [ ] Email settings form (SMTP/SendGrid)
   - [ ] SMS settings form (Twilio/Africa's Talking)
   - [ ] WhatsApp settings form
   - [ ] Add "Test Connection" buttons
   - [ ] Secure settings with encryption

3. **Template Versioning (Optional)**
   - [ ] Add `TemplateVersion` field
   - [ ] Track template changes history
   - [ ] Allow rollback to previous version

4. **Authorization**
   - [ ] Restrict admin pages to Admin/HR Manager roles
   - [ ] Add authorization checks in controllers

**Deliverables:**
- ✅ Admin UI for managing notification templates
- ✅ Admin UI for configuring email/SMS/WhatsApp settings
- ✅ Template preview and test send functionality
- ✅ Role-based access control for admin pages

---

### **Phase 8: User Preferences & Advanced Features - Week 10**

**Objective:** User customization and advanced notification features.

#### **Tasks:**
1. **User Preferences UI**
   - [ ] Create `/Profile/NotificationPreferences` page
   - [ ] Table showing all categories with channel toggles
   - [ ] Add "Minimum Priority" dropdown
   - [ ] Add "Quiet Hours" time pickers
   - [ ] Add "Digest Frequency" dropdown (Instant, Daily, Weekly)
   - [ ] Save preferences to `NotificationPreferences` table

2. **Digest Notifications**
   - [ ] Implement daily digest job (sends at 8 AM)
   - [ ] Implement weekly digest job (sends Monday 8 AM)
   - [ ] Group notifications by category in digest
   - [ ] Create digest email template
   - [ ] Allow users to unsubscribe from digests

3. **Notification Scheduling**
   - [ ] Add `ScheduledFor` field to `Notifications`
   - [ ] Implement scheduled notification job
   - [ ] Allow admins to schedule system notifications
   - [ ] Support recurring notifications (e.g., monthly reports)

4. **Analytics Dashboard**
   - [ ] Create `/Admin/NotificationAnalytics` page
   - [ ] Show metrics:
     - Notifications sent (by channel)
     - Delivery success rate
     - Average delivery time
     - Failed notifications
     - Cost (SMS/WhatsApp)
   - [ ] Add date range filters
   - [ ] Add charts (Chart.js or similar)

5. **Rate Limiting**
   - [ ] Implement per-user rate limits (e.g., max 10 emails/hour)
   - [ ] Prevent notification spam
   - [ ] Queue excess notifications for later delivery

**Deliverables:**
- ✅ User notification preferences page
- ✅ Digest notifications (daily/weekly)
- ✅ Scheduled and recurring notifications
- ✅ Analytics dashboard for admins
- ✅ Rate limiting to prevent spam

---

### **Phase 9: Testing & Optimization - Week 11**

**Objective:** Comprehensive testing and performance optimization.

#### **Tasks:**
1. **Unit Testing**
   - [ ] Write unit tests for `NotificationService`
   - [ ] Write unit tests for `NotificationTemplateService`
   - [ ] Write unit tests for channel services (Email, SMS)
   - [ ] Mock external dependencies (SMTP, SMS APIs)
   - [ ] Achieve 80%+ code coverage

2. **Integration Testing**
   - [ ] Test end-to-end notification flow
   - [ ] Test SignalR real-time updates
   - [ ] Test email delivery with real SMTP
   - [ ] Test SMS delivery with sandbox numbers
   - [ ] Test Hangfire background jobs

3. **Performance Testing**
   - [ ] Load test with 1000 notifications/min
   - [ ] Test database query performance
   - [ ] Add indexes if needed
   - [ ] Optimize SignalR connection handling
   - [ ] Test memory usage under load

4. **Bug Fixes & Refinements**
   - [ ] Fix any bugs found during testing
   - [ ] Improve error handling
   - [ ] Add logging for all critical operations
   - [ ] Improve UI responsiveness

5. **Documentation**
   - [ ] Write developer documentation
   - [ ] Write admin user guide
   - [ ] Write end-user guide
   - [ ] Create video tutorials (optional)

**Deliverables:**
- ✅ Comprehensive unit and integration tests
- ✅ Performance optimizations applied
- ✅ All bugs fixed
- ✅ Complete documentation

---

### **Phase 10: Deployment & Monitoring - Week 12**

**Objective:** Deploy to production and set up monitoring.

#### **Tasks:**
1. **Production Configuration**
   - [ ] Update `appsettings.Production.json` with real credentials
   - [ ] Configure production SMTP server
   - [ ] Configure production SMS gateway
   - [ ] Set up WhatsApp Business API for production
   - [ ] Enable HTTPS for SignalR

2. **Database Migration**
   - [ ] Apply all migrations to production database
   - [ ] Seed initial templates
   - [ ] Create default notification preferences for existing users

3. **Monitoring Setup**
   - [ ] Set up Application Insights or similar
   - [ ] Track notification delivery success rate
   - [ ] Set up alerts for:
     - High failure rate (>10%)
     - Long delivery times (>5min)
     - Background job failures
   - [ ] Create dashboard for monitoring

4. **User Training**
   - [ ] Train HR/Admin staff on template management
   - [ ] Train users on notification preferences
   - [ ] Provide user guide and FAQ

5. **Go-Live**
   - [ ] Deploy to production
   - [ ] Monitor closely for first 48 hours
   - [ ] Gather user feedback
   - [ ] Make quick fixes if needed

**Deliverables:**
- ✅ Production deployment successful
- ✅ Monitoring and alerting configured
- ✅ User training completed
- ✅ System stable and operational

---

## Success Metrics

Track these KPIs to measure notification system success:

1. **Delivery Metrics**
   - In-App delivery rate: >99%
   - Email delivery rate: >95%
   - SMS delivery rate: >98%
   - WhatsApp delivery rate: >97%

2. **Performance Metrics**
   - Average in-app delivery time: <2 seconds
   - Average email delivery time: <1 minute
   - Average SMS delivery time: <30 seconds

3. **User Engagement**
   - Notification read rate: >70%
   - User preference configuration rate: >50% of users
   - Unsubscribe rate: <5%

4. **System Health**
   - Background job failure rate: <1%
   - SignalR connection success rate: >95%
   - API uptime: >99.9%

---

## Future Enhancements (Post-Launch)

1. **Push Notifications** (Browser/Mobile)
   - Web Push API for browser notifications
   - Mobile app push notifications (Firebase/APNs)

2. **Integration with External Tools**
   - Slack notifications
   - Microsoft Teams notifications
   - Telegram bot

3. **AI-Powered Features**
   - Smart notification grouping
   - Predict optimal delivery time per user
   - Auto-suggest notification preferences

4. **Advanced Templates**
   - Drag-and-drop email designer
   - Dynamic content based on user role
   - Localization (multi-language templates)

5. **Analytics & Insights**
   - User engagement heatmap
   - Peak notification times
   - Channel preference trends
   - ROI analysis for SMS costs

---

## Conclusion

This notification system provides a robust, scalable, and user-friendly solution for the OSH Management System. The phased implementation plan ensures steady progress with working features at each milestone.

**Key Strengths:**
- **Decoupled Architecture**: Event-driven design keeps code maintainable
- **Multi-Channel Support**: Reach users through their preferred channel
- **Real-Time Updates**: SignalR provides instant in-app notifications
- **Scalable**: Queue-based processing handles high volumes
- **Customizable**: Users control their notification experience
- **Admin-Friendly**: Template and channel management via UI

The system is designed to grow with the application, starting with core in-app notifications (Phase 1) and expanding to full multi-channel delivery with advanced features (Phases 2-10).

---

**Document Version:** 1.0
**Last Updated:** {{ CurrentDate }}
**Author:** OSH Management Development Team
**Status:** Architecture Approved - Ready for Implementation
