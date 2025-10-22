# 📧 Notification System Database Scripts

## Overview

These SQL scripts set up the **Phase 1** notification system infrastructure without using Entity Framework migrations. They create all necessary tables, seed initial data, and configure notification channels.

---

## 📋 Files Overview

| File | Purpose | Run Order |
|------|---------|-----------|
| `01_CreateNotificationTables.sql` | Creates all 5 notification tables | 1️⃣ First |
| `02_SeedNotificationTemplates.sql` | Seeds initial templates for Employee/Team events | 2️⃣ Second |
| `03_SeedNotificationChannelConfigs.sql` | Configures Email/SMS/WhatsApp settings | 3️⃣ Third |
| `04_ViewNotificationConfigs.sql` | View & verify system configuration | ✅ Anytime |
| `99_RollbackNotificationTables.sql` | ⚠️ Removes all tables (use carefully!) | 🔙 Rollback only |

---

## 🚀 Installation Steps

### **Step 1: Create Tables**

```sql
-- Run in SSMS
:r "01_CreateNotificationTables.sql"
```

**Creates:**
- ✅ `Notifications` - Stores all in-app notifications
- ✅ `NotificationTemplates` - Event templates (InApp, Email, SMS, WhatsApp)
- ✅ `NotificationDeliveries` - Multi-channel delivery tracking
- ✅ `NotificationPreferences` - User notification preferences
- ✅ `NotificationChannelConfigs` - **NEW!** Dynamic channel configuration

---

### **Step 2: Seed Templates**

```sql
-- Run in SSMS
:r "02_SeedNotificationTemplates.sql"
```

**Seeds 11 initial templates:**
- ✅ EmployeeCreated (InApp + Email)
- ✅ EmployeeUpdated (InApp)
- ✅ TeamCreated (InApp + Email)
- ✅ TeamMemberAdded (InApp + Email)
- ✅ TeamMemberRemoved (InApp)
- ✅ IncidentReported (InApp + Email)
- ✅ WelcomeMessage (InApp)

---

### **Step 3: Configure Channels**

```sql
-- Run in SSMS
:r "03_SeedNotificationChannelConfigs.sql"
```

**Configures:**
- ✅ **Email** - ENABLED (SMTP ready)
- ⏸️ **SMS** - Disabled (Phase 2)
- ⏸️ **WhatsApp** - Disabled (Phase 2)

---

### **Step 4: Verify Installation**

```sql
-- Run in SSMS
:r "04_ViewNotificationConfigs.sql"
```

**Shows:**
- Channel status (Enabled/Disabled)
- All configurations (with masked passwords)
- Template counts by category
- Validation checks

---

## ⚙️ Configuration Management

### **Key Feature: Database-Driven Configuration**

Instead of hardcoding settings in `appsettings.json`, all channel configurations are stored in the `NotificationChannelConfigs` table and can be **edited via Admin UI**.

### **Email Configuration Table:**

| ConfigKey | ConfigValue | Description |
|-----------|-------------|-------------|
| `Provider` | `SMTP` | Email provider type |
| `SmtpHost` | `smtp.gmail.com` | SMTP server |
| `SmtpPort` | `587` | SMTP port |
| `EnableSsl` | `true` | Use TLS/SSL |
| `SmtpUsername` | `noreply@oshmanagement.com` | SMTP username |
| `SmtpPassword` | `********` | SMTP password (encrypted) |
| `FromEmail` | `noreply@oshmanagement.com` | From address |
| `FromName` | `OSH Management System` | Display name |
| `Enabled` | `true` | Enable/disable email |

---

## 🔐 Security: Password Encryption

The `IsEncrypted` column indicates which values should be encrypted:

```sql
-- Encrypted configs (passwords, API keys)
SELECT * FROM NotificationChannelConfigs 
WHERE IsEncrypted = 1;
```

**⚠️ IMPORTANT:** 
- Implement encryption in your C# service when reading/writing passwords
- Use `System.Security.Cryptography` or Azure Key Vault
- Never store passwords in plain text!

---

## 📊 Common Queries

### **1. Update Email Password**

```sql
-- IMPORTANT: Encrypt this value in your C# code!
UPDATE NotificationChannelConfigs 
SET ConfigValue = 'your_encrypted_password', 
    UpdatedAt = GETUTCDATE(), 
    UpdatedBy = 'Admin'
WHERE Channel = 'Email' AND ConfigKey = 'SmtpPassword';
```

### **2. Enable/Disable Email Channel**

```sql
-- Disable Email
UPDATE NotificationChannelConfigs 
SET ConfigValue = 'false' 
WHERE Channel = 'Email' AND ConfigKey = 'Enabled';

-- Enable Email
UPDATE NotificationChannelConfigs 
SET ConfigValue = 'true' 
WHERE Channel = 'Email' AND ConfigKey = 'Enabled';
```

### **3. Change SMTP Server**

```sql
-- Switch to Office 365
UPDATE NotificationChannelConfigs 
SET ConfigValue = 'smtp.office365.com' 
WHERE Channel = 'Email' AND ConfigKey = 'SmtpHost';

UPDATE NotificationChannelConfigs 
SET ConfigValue = '587' 
WHERE Channel = 'Email' AND ConfigKey = 'SmtpPort';
```

### **4. Add New Template**

```sql
INSERT INTO NotificationTemplates 
    (TemplateName, Category, Channel, SubjectTemplate, BodyTemplate, IsActive)
VALUES 
    ('TrainingScheduled', 'Training', 'InApp', 
     'Training Session Scheduled',
     'Training: {TrainingName} scheduled on {TrainingDate} at {Location}',
     1);
```

### **5. View All Unread Notifications**

```sql
SELECT 
    N.NotificationId,
    N.Title,
    N.Message,
    N.Priority,
    N.CreatedAt,
    E.FirstName + ' ' + E.LastName AS RecipientName
FROM Notifications N
LEFT JOIN Employees E ON N.RecipientType = 'Employee' AND N.RecipientId = E.EmployeeId
WHERE N.IsRead = 0
ORDER BY N.Priority DESC, N.CreatedAt DESC;
```

---

## 🧪 Testing Queries

### **Test Email Configuration**

```sql
-- Check if all required Email configs are set
SELECT 
    ConfigKey,
    CASE WHEN ConfigValue = '' THEN '❌ NOT SET' ELSE '✓ SET' END AS Status,
    IsRequired
FROM NotificationChannelConfigs
WHERE Channel = 'Email' AND IsRequired = 1
ORDER BY DisplayOrder;
```

### **Simulate Notification**

```sql
-- Create test notification for Employee #1
INSERT INTO Notifications 
    (RecipientType, RecipientId, Title, Message, Priority, Category, CreatedAt)
VALUES 
    ('Employee', 1, 'Test Notification', 'This is a test message', 'Normal', 'System', GETUTCDATE());

-- Verify it was created
SELECT * FROM Notifications ORDER BY CreatedAt DESC;
```

---

## 🛠️ Admin UI Integration

Your Admin UI should provide screens to:

### **1. Notification Templates Management**
- ✅ List all templates by category
- ✅ Create/Edit/Delete templates
- ✅ Live preview with sample data
- ✅ Activate/Deactivate templates
- ✅ Test send to yourself

### **2. Channel Configuration**
- ✅ Edit Email SMTP settings
- ✅ Test Email connection
- ✅ Enable/Disable channels globally
- ✅ Configure SMS (Phase 2)
- ✅ Configure WhatsApp (Phase 2)

### **3. User Preferences**
- ✅ Allow users to configure per-category preferences
- ✅ Set quiet hours
- ✅ Choose digest frequency
- ✅ Enable/disable channels per category

---

## 📈 Monitoring Queries

### **Notification Statistics**

```sql
-- Daily notification counts
SELECT 
    CAST(CreatedAt AS DATE) AS Date,
    Category,
    COUNT(*) AS NotificationCount,
    SUM(CASE WHEN IsRead = 1 THEN 1 ELSE 0 END) AS ReadCount
FROM Notifications
WHERE CreatedAt >= DATEADD(DAY, -7, GETDATE())
GROUP BY CAST(CreatedAt AS DATE), Category
ORDER BY Date DESC, Category;
```

### **Delivery Success Rate**

```sql
-- Email delivery success rate
SELECT 
    Status,
    COUNT(*) AS Count,
    CAST(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER() AS DECIMAL(5,2)) AS Percentage
FROM NotificationDeliveries
WHERE Channel = 'Email'
GROUP BY Status;
```

---

## ⚠️ Rollback / Cleanup

**To remove all notification tables:**

1. **BACKUP YOUR DATABASE FIRST!**
2. Open `99_RollbackNotificationTables.sql` in SSMS
3. Uncomment the safety blocks
4. Run the script

```sql
-- WARNING: This deletes EVERYTHING!
:r "99_RollbackNotificationTables.sql"
```

---

## 🎯 Next Steps After Installation

1. ✅ **Update Email Password** in `NotificationChannelConfigs`
2. ✅ **Test Email Connection** by sending a test notification
3. ✅ **Create Admin UI** for managing templates and configurations
4. ✅ **Implement NotificationService** in C# (see Architecture doc)
5. ✅ **Add triggers** in Employee/Team controllers
6. ✅ **Test end-to-end** notification flow

---

## 📚 Related Documentation

- `OSHfiles/Codingdocs/NotificationSystemArchitecture.md` - Complete system architecture
- Phase 1 focuses on: InApp + Email notifications
- Phase 2 will add: SMS + WhatsApp
- Phase 3 will add: SignalR real-time updates

---

## 🤝 Support

For issues or questions:
1. Review the architecture document
2. Check `04_ViewNotificationConfigs.sql` for system status
3. Verify all required configs are set
4. Test with simple Employee/Team notifications first

---

**✨ Happy Notifying! ✨**
