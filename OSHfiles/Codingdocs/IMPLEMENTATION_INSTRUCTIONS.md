# Dynamic Roles & Scope System - Implementation Instructions

## ✅ Completed Steps

1. ✅ **Added OSHfiles to .gitignore**
2. ✅ **Fixed SQL script error** - 3_ModernRolesAndPermissions.sql (subquery issue)
3. ✅ **Removed legacy role sync** - LegacyDataMigrationService.cs

## 📝 What You Need to Do Now

### Step 1: Re-run the Fixed SQL Script (SSMS)

Since you already ran migrations 1 and 2, now run the **fixed** version of script 3:

**File**: `OSHManagement\Database\Migrations\3_ModernRolesAndPermissions.sql`

**Run in SSMS** - This will now work without errors!

**Expected Output**:
```
✅ Created 24 permissions
✅ Created 12 roles
✅ Created 120+ role-permission assignments
✅ Assigned Admin role to ADMIN001
✅ ScopeLevel is now required (NOT NULL)

Summary:
  Roles created:               12
  Permissions created:         24
  Role-Permission assignments: 120+

Roles by Scope Level:
  Organization (1): 3 roles
  Station (2):      2 roles
  Department (3):   2 roles
  Team (4):         2 roles
  Self (5):         2 roles
```

---

### Step 2: Update Role Model (Package Manager Console)

**No EF migration needed** - we already altered the database directly with SQL.

Just update the C# model to match:

**File to edit**: `Models\Role.cs`

**Replace the entire file with**:

```csharp
using System.ComponentModel.DataAnnotations;
using OSHManagement.Models.Authorization;

namespace OSHManagement.Models
{
    /// <summary>
    /// Represents a role with dynamic scope and permission assignment
    /// </summary>
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        /// <summary>
        /// Data access scope level for this role
        /// Determines what organizational data the role can access
        /// </summary>
        [Required]
        public ScopeLevel ScopeLevel { get; set; }

        /// <summary>
        /// Prevents deletion of critical system roles (Admin, Employee, etc.)
        /// </summary>
        public bool IsSystemRole { get; set; } = false;

        /// <summary>
        /// Allows role to access data across departments (within station)
        /// </summary>
        public bool AllowCrossDepartmentAccess { get; set; } = false;

        /// <summary>
        /// Allows role to access data across stations (advanced feature)
        /// </summary>
        public bool AllowCrossStationAccess { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
```

**Changes Made**:
- ❌ Removed `LegacyRoleMapping` property
- ✅ Added `ScopeLevel` property (required)
- ✅ Added `IsSystemRole` property
- ✅ Added `AllowCrossDepartmentAccess` property
- ✅ Added `AllowCrossStationAccess` property

---

### Step 3: Update UserScopeService (Dynamic Scope)

**File to edit**: `Services\UserScopeService.cs`

**Replace the `DetermineScopeLevelAsync` method** (around line 94-142):

**OLD (Hardcoded)**:
```csharp
public async Task<ScopeLevel> DetermineScopeLevelAsync(int userId)
{
    // ... lots of hardcoded role name checks
    if (roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                      r.Equals("OSH Manager", StringComparison.OrdinalIgnoreCase)))
        return ScopeLevel.Organization;
    // ... 40+ more lines
}
```

**NEW (Dynamic)**:
```csharp
public async Task<ScopeLevel> DetermineScopeLevelAsync(int userId)
{
    var employee = await _context.Employees
        .Include(e => e.EmployeeRoles.Where(er => er.IsActive))
            .ThenInclude(er => er.Role)
        .FirstOrDefaultAsync(e => e.EmployeeId == userId);

    if (employee == null)
    {
        _logger.LogWarning($"Employee {userId} not found during scope determination");
        return ScopeLevel.Self;
    }

    // Get all active roles
    var activeRoles = employee.EmployeeRoles
        .Where(er => er.IsActive && er.Role.IsActive)
        .Select(er => er.Role)
        .ToList();

    if (!activeRoles.Any())
    {
        _logger.LogInformation($"Employee {userId} ({employee.PayrollNo}) has no active roles, checking position-based scope");

        // Fallback: Check position-based scope
        if (await IsHODAsync(employee.PayrollNo))
        {
            _logger.LogInformation($"Employee {userId} is HOD (position-based), granting Department scope");
            return ScopeLevel.Department;
        }

        if (await IsSupervisorAsync(employee.PayrollNo))
        {
            _logger.LogInformation($"Employee {userId} is Supervisor (position-based), granting Team scope");
            return ScopeLevel.Team;
        }

        _logger.LogInformation($"Employee {userId} has no roles or position, defaulting to Self scope");
        return ScopeLevel.Self;
    }

    // Get the broadest scope (lowest enum value = broadest access)
    // Organization = 1, Station = 2, Department = 3, Team = 4, Self = 5
    var broadestScope = activeRoles.Min(r => r.ScopeLevel);

    _logger.LogDebug($"Employee {userId} ({employee.PayrollNo}) has {activeRoles.Count} active role(s), broadest scope: {broadestScope}");

    return broadestScope;
}
```

**Changes Made**:
- ❌ Removed ALL hardcoded role name checks
- ✅ Now reads `Role.ScopeLevel` directly from database
- ✅ Takes minimum scope (1=broadest, 5=narrowest)
- ✅ Falls back to position-based (HOD/Supervisor) if no roles
- ✅ Better logging for debugging

---

### Step 4: Build the Solution

**In Package Manager Console**:
```powershell
# Build the solution to check for errors
dotnet build
```

**Expected**: Build should succeed with no errors.

---

### Step 5: Test the Dynamic Scope System

#### Test 1: Verify Roles Were Created

**In SSMS**, run:
```sql
SELECT RoleId, RoleName, ScopeLevel, IsSystemRole
FROM Roles
ORDER BY ScopeLevel, RoleName;
```

**Expected**: 12 roles with scope levels 1-5

#### Test 2: Verify Permissions Were Created

**In SSMS**, run:
```sql
SELECT Module, COUNT(*) as PermissionCount
FROM Permissions
GROUP BY Module
ORDER BY Module;
```

**Expected**:
- Employee: 5 permissions
- Incident: 7 permissions
- Hazard: 6 permissions
- Organization: 2 permissions
- Administration: 4 permissions

#### Test 3: Verify Admin User Has Role

**In SSMS**, run:
```sql
SELECT e.PayrollNo, e.FirstName, e.LastName, r.RoleName, r.ScopeLevel
FROM Employees e
INNER JOIN EmployeeRoles er ON e.EmployeeId = er.EmployeeId
INNER JOIN Roles r ON er.RoleId = r.RoleId
WHERE e.PayrollNo = 'ADMIN001' AND er.IsActive = 1;
```

**Expected**: ADMIN001 has Admin role with ScopeLevel = 1 (Organization)

#### Test 4: Login and Verify Scope

1. **Run the application**
2. **Login as**: ADMIN001 / Admin@123
3. **Check claims** (in debug or via middleware):
   - Should have claim: `Scope = Organization`
   - Should have claim: `StationId = 1` (or whatever admin's station is)
   - Should have 24 permission claims

4. **Test access**:
   - Go to Employee Index
   - Should see ALL employees (Organization scope)
   - No filtering by station/department

---

## 🎯 What Changed?

### Before (Hardcoded)
```csharp
// Creating new role required code changes
if (role == "Regional Manager")  // ❌ Hardcoded
    return ScopeLevel.Station;
```

### After (Dynamic)
```csharp
// Creating new role is just database insert
INSERT INTO Roles (RoleName, Description, ScopeLevel, IsActive)
VALUES ('Regional Manager', 'Manages region', 2, 1);
-- ✅ Works immediately, no code changes!
```

---

## 📋 Summary of Files Changed

### ✅ Modified
1. `.gitignore` - Added OSHfiles/
2. `Services/LegacyDataMigrationService.cs` - Removed role sync
3. `Database/Migrations/3_ModernRolesAndPermissions.sql` - Fixed subquery error

### 📝 To Modify (Manual)
4. `Models/Role.cs` - Remove LegacyRoleMapping, add ScopeLevel
5. `Services/UserScopeService.cs` - Replace hardcoded logic with dynamic

---

## 🚨 Troubleshooting

### Error: "Column ScopeLevel does not exist"
**Fix**: Run migration 2 (`2_AddScopeLevelToRoles.sql`) in SSMS

### Error: "Cannot insert NULL into ScopeLevel"
**Fix**: Run migration 3 (`3_ModernRolesAndPermissions.sql`) to seed roles

### Error: "Admin user can't see all data"
**Check**:
1. Admin role has ScopeLevel = 1
2. User has Admin role assigned
3. UserScopeService is using new logic

---

## ✅ Next Steps (After This Works)

Once dynamic scope works, you can:

1. **Build Role Management UI** - Create/Edit roles via UI
2. **Add Permission Policies** - Use `[Authorize(Policy = "Incident.Create")]`
3. **Build Employee Role Assignment UI** - Assign roles to employees
4. **Auto-Generate Permissions** - Scan controllers for permissions

See `OSHfiles/Codingdocs/8._DynamicRolesPermissionsAndScopeSystem.md` for full details.

---

## 📞 Need Help?

If you encounter errors:
1. Check the SQL script output in SSMS
2. Check build errors in Visual Studio
3. Check runtime errors in browser console
4. Check application logs

All scripts are in: `OSHManagement\Database\Migrations\`
