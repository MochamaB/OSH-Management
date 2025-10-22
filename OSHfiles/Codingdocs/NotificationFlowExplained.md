# Notification System Flow - Complete Guide
## How Notifications Work in OSH Management System

---

## 📋 Table of Contents
1. [The Big Picture](#the-big-picture)
2. [Step-by-Step Flow](#step-by-step-flow)
3. [Background Services](#background-services)
4. [SignalR Real-Time](#signalr-real-time)
5. [Implementation Examples](#implementation-examples)

---

## The Big Picture

```
USER ACTION → EVENT PUBLISHER → RECIPIENTS RESOLUTION → DATABASE → CHANNELS → USER RECEIVES
   (Controller)    (Process)      (Who gets it?)        (Save)    (How?)     (Notification)
```

---

## Step-by-Step Flow

### **1. Trigger Event in Controller**

When an action happens, publish a notification event:

```csharp
// After creating employee in EmployeeController
await _notificationEventPublisher.PublishAsync(new NotificationEvent
{
    EventType = "EmployeeCreated",
    Category = "Employee",
    Priority = NotificationPriority.Normal,
    
    Data = new Dictionary<string, string>
    {
        { "EmployeeName", $"{employee.FirstName} {employee.LastName}" },
        { "PayrollNo", employee.PayrollNo },
        { "StationName", station.StationName }
    },
    
    RecipientRoleIds = new List<int> { HR_MANAGER_ROLE_ID },
    RecipientStationIds = new List<int> { employee.StationId },
    ActionUrl = $"/Employee/Details/{employee.EmployeeId}"
});
```

### **2. Event Publisher Processes**

```csharp
public async Task PublishAsync(NotificationEvent notificationEvent)
{
    // Get template
    var template = await _templateService.GetTemplateAsync(
        notificationEvent.EventType, NotificationChannel.InApp);
    
    // Render message
    var message = _templateService.RenderTemplate(template, notificationEvent.Data);
    
    // Create notification records (one per recipient type)
    foreach (var roleId in notificationEvent.RecipientRoleIds)
    {
        var notification = new Notification
        {
            RecipientType = "Role",
            RecipientId = roleId,
            Title = template.SubjectTemplate,
            Message = message,
            // ... other properties
        };
        
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        
        // Process delivery for this notification
        await ProcessDelivery(notification, notificationEvent);
    }
}
```

### **3. Process Delivery**

```csharp
private async Task ProcessDelivery(Notification notification, NotificationEvent evt)
{
    // Resolve actual employees
    var employees = await ResolveRecipients(notification);
    
    foreach (var employee in employees)
    {
        // Check preferences
        var prefs = await GetUserPreferences(employee.EmployeeId, notification.Category);
        
        // For each enabled channel
        if (prefs.InAppEnabled)
        {
            // INSTANT - via SignalR
            await DeliverInApp(employee, notification);
        }
        
        if (prefs.EmailEnabled)
        {
            // QUEUED - via Hangfire
            var delivery = new NotificationDelivery
            {
                NotificationId = notification.NotificationId,
                Channel = "Email",
                RecipientAddress = employee.Email,
                Status = "Pending"
            };
            _context.NotificationDeliveries.Add(delivery);
            await _context.SaveChangesAsync();
            
            // Queue to background job
            BackgroundJob.Enqueue<EmailService>(s => s.SendAsync(delivery));
        }
    }
}
```

### **4. Recipient Resolution**

```csharp
private async Task<List<Employee>> ResolveRecipients(Notification notification)
{
    return notification.RecipientType switch
    {
        "Employee" => await _context.Employees
            .Where(e => e.EmployeeId == notification.RecipientId).ToListAsync(),
            
        "Role" => await _context.EmployeeRoles
            .Where(er => er.RoleId == notification.RecipientId)
            .Include(er => er.Employee)
            .Select(er => er.Employee).ToListAsync(),
            
        "Station" => await _context.Employees
            .Where(e => e.StationId == notification.RecipientId).ToListAsync(),
            
        "Team" => await (from tm in _context.TeamMembers
                         join e in _context.Employees on tm.EmployeePayroll equals e.PayrollNo
                         where tm.TeamId == notification.RecipientId && tm.IsActive
                         select e).ToListAsync(),
                         
        _ => new List<Employee>()
    };
}
```

---

## Background Services (Hangfire)

### **Setup**

```csharp
// Program.cs
builder.Services.AddHangfire(config => config
    .UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();

app.UseHangfireDashboard("/hangfire");

// Schedule recurring jobs
RecurringJob.AddOrUpdate<INotificationService>(
    "cleanup-expired",
    s => s.CleanupExpiredNotificationsAsync(),
    Cron.Daily(2)); // 2 AM daily
```

### **Priority-Based Queuing**

```csharp
void QueueDelivery(NotificationDelivery delivery, string priority)
{
    switch (priority)
    {
        case "Urgent":
            BackgroundJob.Enqueue<EmailService>(s => s.SendAsync(delivery));
            break;
        case "Normal":
            BackgroundJob.Schedule<EmailService>(
                s => s.SendAsync(delivery), TimeSpan.FromSeconds(30));
            break;
        case "Low":
            BackgroundJob.Schedule<EmailService>(
                s => s.SendAsync(delivery), TimeSpan.FromMinutes(5));
            break;
    }
}
```

### **Retry Logic**

```csharp
public async Task<DeliveryResult> SendAsync(NotificationDelivery delivery)
{
    try
    {
        // Attempt send
        var result = await _emailService.SendEmailAsync(delivery);
        
        if (result.Success)
        {
            delivery.Status = "Sent";
            delivery.SentAt = DateTime.UtcNow;
        }
        else
        {
            delivery.Status = "Failed";
            delivery.RetryCount++;
            
            // Schedule retry (max 3 attempts)
            if (delivery.RetryCount < 3)
            {
                var delay = GetRetryDelay(delivery.RetryCount); // 1min, 5min, 15min
                BackgroundJob.Schedule<EmailService>(
                    s => s.SendAsync(delivery), delay);
            }
        }
        
        _context.Update(delivery);
        await _context.SaveChangesAsync();
        return result;
    }
    catch (Exception ex)
    {
        // Handle error
    }
}
```

---

## SignalR Real-Time

### **Hub Setup**

```csharp
[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var employeeId = Context.User?.FindFirst("EmployeeId")?.Value;
        var employee = await _context.Employees
            .Include(e => e.EmployeeRoles)
            .FirstOrDefaultAsync(e => e.EmployeeId == int.Parse(employeeId));
        
        // Join groups
        foreach (var role in employee.EmployeeRoles)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{role.RoleId}");
        
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Station_{employee.StationId}");
        
        await base.OnConnectedAsync();
    }
}
```

### **Send to User**

```csharp
// In NotificationService
await _hubContext.Clients
    .User(employeeId.ToString())
    .SendAsync("ReceiveNotification", new
    {
        notificationId = notification.NotificationId,
        title = notification.Title,
        message = notification.Message,
        priority = notification.Priority,
        actionUrl = notification.ActionUrl
    });
```

### **Client-Side (_Layout.cshtml)**

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveNotification", function (notification) {
    updateBadgeCount();
    addToDropdown(notification);
    showToast(notification);
    if (notification.priority === "Urgent") playSound();
});

connection.start().then(() => console.log("SignalR connected"));
```

---

## Implementation Examples

### **Example 1: Employee Created**

```csharp
// EmployeeController.Create()
await _notificationEventPublisher.PublishAsync(new NotificationEvent
{
    EventType = "EmployeeCreated",
    Category = "Employee",
    Data = new Dictionary<string, string>
    {
        { "EmployeeName", $"{employee.FirstName} {employee.LastName}" },
        { "PayrollNo", employee.PayrollNo },
        { "StationName", station.StationName }
    },
    RecipientRoleIds = new List<int> { HR_MANAGER_ROLE },
    RecipientStationIds = new List<int> { employee.StationId },
    Priority = NotificationPriority.Normal,
    ActionUrl = $"/Employee/Details/{employee.EmployeeId}"
});
```

### **Example 2: Team Member Added**

```csharp
// TeamController.AddMember()
await _notificationEventPublisher.PublishAsync(new NotificationEvent
{
    EventType = "TeamMemberAdded",
    Category = "Team",
    Data = new Dictionary<string, string>
    {
        { "EmployeeName", $"{member.FirstName} {member.LastName}" },
        { "TeamName", team.TeamName },
        { "MemberRole", roleName }
    },
    RecipientEmployeeIds = new List<int> { member.EmployeeId }, // Notify new member
    RecipientTeamIds = new List<int> { team.TeamId }, // Notify all team members
    Priority = NotificationPriority.Normal,
    ActionUrl = $"/Team/Details/{team.TeamId}"
});
```

### **Example 3: Urgent Incident**

```csharp
// IncidentController.Create()
await _notificationEventPublisher.PublishAsync(new NotificationEvent
{
    EventType = "IncidentReported",
    Category = "Incident",
    Data = new Dictionary<string, string>
    {
        { "IncidentType", incident.IncidentType },
        { "Location", incident.Location },
        { "Severity", incident.Severity }
    },
    RecipientRoleIds = new List<int> { SAFETY_OFFICER, OSH_MANAGER },
    RecipientStationIds = new List<int> { incident.StationId },
    Priority = NotificationPriority.Urgent,
    ActionUrl = $"/Incident/Details/{incident.IncidentId}",
    Channels = new List<NotificationChannel> 
    { 
        NotificationChannel.InApp,
        NotificationChannel.Email,
        NotificationChannel.SMS 
    }
});
```

---

## Summary: Complete Flow

1. **Controller Action** → Publish NotificationEvent
2. **Event Publisher** → Get template, render message, create Notification records
3. **Resolve Recipients** → Employee, Role, Station, Department, Team
4. **Check Preferences** → Channels enabled, quiet hours, digest
5. **Create Deliveries** → One per channel per recipient
6. **In-App Channel** → Immediate via SignalR push
7. **External Channels** → Queue to Hangfire (Email/SMS/WhatsApp)
8. **Background Processing** → Send with retry logic
9. **Update Status** → Track sent/delivered/failed
10. **User Receives** → Bell icon, dropdown, toast, email inbox, SMS

---

## Key Components

- `INotificationEventPublisher` - Entry point for triggering notifications
- `INotificationService` - Core notification management
- `INotificationTemplateService` - Template rendering
- `INotificationChannelService` - Channel-specific delivery
- `NotificationHub` - SignalR real-time push
- `Hangfire` - Background job processing with retry logic

---

**Best Practice:** Always call `_notificationEventPublisher.PublishAsync()` after successful database operations in controllers to trigger notifications automatically.
