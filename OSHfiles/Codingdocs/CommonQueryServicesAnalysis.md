# Common Query Services Analysis and Design

## Overview

This document analyzes the repetitive queries across controllers and proposes a service-based architecture to eliminate code duplication, ensure consistent scope application, and improve maintainability.

---

## Table of Contents

1. [Current State Analysis](#current-state-analysis)
2. [Identified Patterns](#identified-patterns)
3. [Scope Requirements](#scope-requirements)
4. [Proposed Service Architecture](#proposed-service-architecture)
5. [Service Specifications](#service-specifications)
6. [Scope Application Strategy](#scope-application-strategy)
7. [Implementation Guidelines](#implementation-guidelines)
8. [Benefits](#benefits)

---

## Current State Analysis

### Repetitive Query Patterns Found

After analyzing all controllers, the following repetitive query patterns were identified:

#### 1. **Organization Categories** (Used in 4 controllers)
```csharp
var categories = await _context.OrgCategories
    .Where(c => c.IsActive)
    .OrderBy(c => c.CategoryName)
    .Select(c => new { c.OrgCategoryId, c.CategoryName })
    .ToListAsync();
```

**Controllers**: `EmployeeController`, `StationController`, `SectionController`, `OrganizationController`

**Usage Context**:
- Filter dropdowns in Index pages
- Form dropdowns in Create/Edit pages

---

#### 2. **Stations** (Used in 6 controllers)
```csharp
// Pattern 1: For dropdowns (simple)
var stations = await _context.Stations
    .Where(s => s.IsActive)
    .OrderBy(s => s.StationName)
    .Select(s => new { s.StationId, s.StationName })
    .ToListAsync();

// Pattern 2: For cascading dropdowns (with category relationship)
var stations = await _context.Stations
    .Where(s => s.IsActive)
    .OrderBy(s => s.StationName)
    .Select(s => new { s.StationId, s.StationName, s.OrgCategoryId })
    .ToListAsync();

// Pattern 3: For validation/existence checks
var stationExists = await _context.Stations
    .AnyAsync(s => s.StationId == stationId && s.IsActive);
```

**Controllers**: `EmployeeController`, `DepartmentController`, `SectionController`, `StationController`, `OrganizationController`

**Usage Context**:
- Filter dropdowns in Index pages
- Form dropdowns in Create/Edit pages
- Cascading dropdown logic (Category → Stations)
- Validation checks

---

#### 3. **Departments** (Used in 4 controllers)
```csharp
// Pattern 1: For dropdowns (simple)
var departments = await _context.Departments
    .Where(d => d.IsActive)
    .OrderBy(d => d.DepartmentName)
    .Select(d => new { d.DepartmentId, d.DepartmentName })
    .ToListAsync();

// Pattern 2: For cascading dropdowns (with station relationship)
var departments = await _context.Departments
    .Where(d => d.IsActive && d.StationId == stationId)
    .OrderBy(d => d.DepartmentName)
    .Select(d => new { d.DepartmentId, d.DepartmentName })
    .ToListAsync();

// Pattern 3: Parent departments (hierarchical)
var parentDepartments = await _context.Departments
    .Where(d => d.IsActive)
    .OrderBy(d => d.DepartmentName)
    .Select(d => new { d.DepartmentId, d.DepartmentName })
    .ToListAsync();
```

**Controllers**: `EmployeeController`, `DepartmentController`, `SectionController`

**Usage Context**:
- Filter dropdowns in Index pages
- Form dropdowns in Create/Edit pages
- Cascading dropdown logic (Station → Departments)
- Hierarchical selection (parent departments)

---

#### 4. **Employees** (Used in 5 controllers)
```csharp
// Pattern 1: For HOD dropdown
var hodNames = await _context.Employees
    .Where(e => e.EmploymentStatus == "Active")
    .OrderBy(e => e.FirstName)
    .ThenBy(e => e.LastName)
    .Select(e => new { e.PayrollNo, FullName = e.FirstName + " " + e.LastName })
    .ToListAsync();

// Pattern 2: For Supervisor dropdown
var supervisorNames = await _context.Employees
    .Where(e => e.EmploymentStatus == "Active")
    .OrderBy(e => e.FirstName)
    .ThenBy(e => e.LastName)
    .Select(e => new { e.PayrollNo, FullName = e.FirstName + " " + e.LastName })
    .ToListAsync();

// Pattern 3: For general employee dropdown
var employees = await _context.Employees
    .Where(e => e.EmploymentStatus == "Active")
    .OrderBy(e => e.FirstName)
    .Select(e => new { e.EmployeeId, FullName = e.FirstName + " " + e.LastName })
    .ToListAsync();

// Pattern 4: For validation/existence checks
var exists = await _context.Employees
    .AnyAsync(e => e.PayrollNo == payrollNo);
```

**Controllers**: `EmployeeController`, `SectionController`, `AccountController`, `ScopedController`

**Usage Context**:
- HOD selection dropdowns
- Supervisor selection dropdowns
- Employee assignment dropdowns
- Unique validation (PayrollNo, RollNo, Username, Email)

---

#### 5. **Roles** (Used in 2 controllers)
```csharp
var roles = await _context.Roles
    .Where(r => r.IsActive)
    .OrderBy(r => r.RoleName)
    .Select(r => new { r.RoleId, r.RoleName })
    .ToListAsync();
```

**Controllers**: `EmployeeController`, `RoleController`

**Usage Context**:
- Role assignment dropdowns in Employee Create/Edit
- Role management pages

---

## Identified Patterns

### Three Main Query Categories:

1. **Dropdown Queries**
   - Purpose: Populate `<select>` elements
   - Characteristics:
     - Filter by `IsActive = true`
     - Order alphabetically
     - Project to minimal DTO (Id + Name)
     - No includes needed

2. **Cascading Dropdown Queries**
   - Purpose: Populate dependent dropdowns (e.g., Category → Stations → Departments)
   - Characteristics:
     - Filter by `IsActive = true` AND parent ID
     - Include parent relationship field
     - Order alphabetically
     - Project to DTO with parent reference

3. **Validation Queries**
   - Purpose: Check existence or uniqueness
   - Characteristics:
     - Return `bool` via `AnyAsync()`
     - Specific field matching
     - No projections needed

---

## Scope Requirements

### Scope Levels and Data Access Rules

Based on `7._DataAccessFilter.md` and `ScopeFilterService.cs`:

| Scope Level | Value | Data Access Rule |
|-------------|-------|------------------|
| **Organization** | 1 | See ALL data (no filtering) |
| **Station** | 2 | See only data within their station |
| **Department** | 3 | See only data within their department |
| **Team** | 4 | See only direct reports (supervisor relationship) |
| **Self** | 5 | See only own data |

### Entities Currently Supporting Scope

From `ScopeFilterService.cs` (Current Implementation):
- ✅ **Employee** - Full scope support (implemented)
- ✅ **Incident** - Full scope support (implemented)
- ✅ **Hazard** - Full scope support (implemented)

### Entities REQUIRING Scope (To Be Implemented):
- 🔶 **Station** - CONDITIONAL scope (organizational hierarchy)
- 🔶 **Department** - CONDITIONAL scope (organizational hierarchy)
- 🔶 **Section** - CONDITIONAL scope (organizational hierarchy)

### Entities NOT Requiring Scope:
- ❌ **OrgCategory** - NO scope (pure reference data)
- ❌ **Role** - NO scope (admin-only reference data)
- ❌ **Permission** - NO scope (admin-only reference data)

### Which Entities Need Scope in Dropdown Services?

#### **Reference Data (NO Scope Needed)**
- **OrgCategories**: Organization structure - all users see all categories
- **Roles**: Security reference - managed by admins only
- **Permissions**: Security reference - managed by admins only

#### **Organizational Hierarchy (CONDITIONAL Scope) ⚠️**

**Critical: These ARE NOT pure reference data - they represent organizational structure that MUST respect scope!**

**Stations**:
- **Scope Logic**:
  - Organization scope → See ALL stations
  - Station scope → See ONLY their station
  - Department/Team/Self → See ONLY their station (inherited)
- **For filters**: Apply scope (user can only filter by stations they have access to)
- **For forms**: Apply scope (user can only assign employees to stations they manage)
- **Security Reason**: A Station Manager should NOT see other stations' data

**Departments**:
- **Scope Logic**:
  - Organization scope → See ALL departments
  - Station scope → See departments in their station ONLY
  - Department scope → See ONLY their department
  - Team/Self → See ONLY their department (inherited)
- **For filters**: Apply scope (user can only filter by departments in their station)
- **For forms**: Apply scope (user can only assign to departments they manage)
- **Security Reason**: A Department Head should NOT see other departments' data

**Sections**:
- **Scope Logic**: Similar to Departments
- Follows station hierarchy for scope determination

#### **Transactional Data (ALWAYS Apply Scope) 🔒**

**Employees**:
- **CRITICAL**: Employee data is highly sensitive - scope MUST always be applied
- **For filters**: MUST apply scope
- **For HOD dropdown**: Apply scope (can only select HODs within scope)
- **For Supervisor dropdown**: Apply scope (can only select supervisors within scope)
- **For forms**: Apply scope (can only assign employees within scope)
- **No scope provided**: Return empty list (fail-safe)

**Incidents/Hazards**:
- Similar to Employees - security-critical transactional data

---

## Proposed Service Architecture

### Service Layer Structure

```
Services/
├── IOrganizationService.cs                    // Categories ONLY (no scope)
├── OrganizationService.cs
│
├── IOrganizationalHierarchyService.cs         // Stations, Departments, Sections (conditional scope)
├── OrganizationalHierarchyService.cs
│
├── IEmployeeService.cs                        // Employees (always scope)
├── EmployeeService.cs
│
├── IRoleService.cs                            // Roles & Permissions (no scope)
├── RoleService.cs
│
├── ValidationServices/
│   ├── IEmployeeValidationService.cs
│   └── EmployeeValidationService.cs
│
└── [Existing Services - TO BE ENHANCED]
    ├── ScopeFilterService.cs ⚠️ NEEDS ENHANCEMENT
    └── UserScopeService.cs
```

### Service Naming Rationale

**Why Split Services?**

1. **OrganizationService**: Pure reference data (Categories)
   - No scope logic needed
   - Rarely changes
   - Can be cached aggressively

2. **OrganizationalHierarchyService**: Structural entities (Stations, Departments, Sections)
   - CONDITIONAL scope logic
   - Represents organizational structure
   - Cannot be cached per-user due to scope

3. **EmployeeService**: Transactional data
   - ALWAYS apply scope
   - Security-critical
   - Never cache

4. **RoleService**: Security reference data
   - Admin-only management
   - No scope needed
   - Can be cached

### Service Responsibilities

#### 1. **OrganizationService** (Categories Only)
**Purpose**: Provide organization category dropdown data

**Responsibilities**:
- Get active organization categories
- Get category by ID
- Category existence validation

**Scope Handling**: NONE - Reference data visible to all users

---

#### 2. **OrganizationalHierarchyService** (Stations, Departments, Sections)
**Purpose**: Provide dropdown data for organizational structure with scope awareness

**Responsibilities**:
- Get stations (with conditional scope)
- Get departments (with conditional scope)
- Get sections (with conditional scope)
- Support cascading queries (Category → Stations, Station → Departments)
- Handle parent-child relationships (Parent Station, Parent Department)

**Scope Handling**: CONDITIONAL (based on user scope level)
- Organization scope → All data
- Station scope → Only user's station and below
- Department scope → Only user's department and below
- Team/Self scope → Inherited from station/department

---

#### 3. **EmployeeService**
**Purpose**: Provide employee dropdown data with strict scope enforcement

**Responsibilities**:
- Get active employees (with scope)
- Get HOD candidates (with scope)
- Get supervisor candidates (with scope)
- Get employees by station (with scope)
- Get employees by department (with scope)
- Get employee name lookups (with scope)

**Scope Handling**: ALWAYS applied (security-critical)
- No scope provided → Return empty list (fail-safe)
- All queries MUST go through ScopeFilterService

---

#### 4. **RoleService**
**Purpose**: Provide role/permission dropdown data

**Responsibilities**:
- Get active roles (no scope - admin only)
- Get permissions (no scope - admin only)
- Get roles by scope level
- Get permissions by module

**Scope Handling**: NONE (reference data, admin-managed)

---

#### 5. **EmployeeValidationService**
**Purpose**: Employee field validation

**Responsibilities**:
- Check PayrollNo uniqueness
- Check RollNo uniqueness
- Check Username uniqueness
- Check Email uniqueness
- Check Phone uniqueness

**Scope Handling**: NONE (validation should check globally for uniqueness)

---

## Service Specifications

### 1. OrganizationService Interface (Categories Only)

```csharp
public interface IOrganizationService
{
    // Categories (NO scope - reference data)
    Task<List<CategoryDropdownDto>> GetActiveCategoriesAsync();
    Task<CategoryDropdownDto?> GetCategoryByIdAsync(int categoryId);
    Task<bool> CategoryExistsAsync(int categoryId);
}
```

---

### 2. OrganizationalHierarchyService Interface

```csharp
public interface IOrganizationalHierarchyService
{
    // Stations (WITH conditional scope)
    Task<List<StationDropdownDto>> GetActiveStationsAsync(UserScope? scope = null);
    Task<List<StationDropdownDto>> GetStationsByCategoryAsync(int categoryId, UserScope? scope = null);
    Task<StationDropdownDto?> GetStationByIdAsync(int stationId, UserScope? scope = null);
    Task<bool> StationExistsAsync(int stationId, UserScope? scope = null);
    
    // Get current user's station (for auto-selection)
    Task<StationDropdownDto?> GetCurrentUserStationAsync(UserScope scope);
    Task<int?> GetCurrentUserStationCategoryAsync(UserScope scope);

    // Departments (WITH conditional scope)
    Task<List<DepartmentDropdownDto>> GetActiveDepartmentsAsync(UserScope? scope = null);
    Task<List<DepartmentDropdownDto>> GetDepartmentsByStationAsync(int stationId, UserScope? scope = null);
    Task<DepartmentDropdownDto?> GetDepartmentByIdAsync(int departmentId, UserScope? scope = null);
    Task<bool> DepartmentExistsAsync(int departmentId, UserScope? scope = null);
    
    // Get current user's department (for auto-selection)
    Task<DepartmentDropdownDto?> GetCurrentUserDepartmentAsync(UserScope scope);

    // Sections (WITH conditional scope)
    Task<List<SectionDropdownDto>> GetActiveSectionsAsync(UserScope? scope = null);
    Task<List<SectionDropdownDto>> GetSectionsByStationAsync(int stationId, UserScope? scope = null);
    Task<List<SectionDropdownDto>> GetSectionsByDepartmentAsync(int? departmentId, UserScope? scope = null);
}
```

**DTOs**:
```csharp
public class CategoryDropdownDto
{
    public int OrgCategoryId { get; set; }
    public string CategoryName { get; set; }
}

public class StationDropdownDto
{
    public int StationId { get; set; }
    public string StationName { get; set; }
    public int OrgCategoryId { get; set; } // For cascading
}

public class DepartmentDropdownDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public int StationId { get; set; } // For cascading
}

public class SectionDropdownDto
{
    public int SectionId { get; set; }
    public string SectionName { get; set; }
    public int DepartmentId { get; set; } // For cascading
}
```

---

### 3. EmployeeService Interface

```csharp
public interface IEmployeeService
{
    // General employee dropdowns (WITH scope)
    Task<List<EmployeeDropdownDto>> GetActiveEmployeesAsync(UserScope? scope = null);
    Task<List<EmployeeDropdownDto>> GetEmployeesByStationAsync(int stationId, UserScope? scope = null);
    Task<List<EmployeeDropdownDto>> GetEmployeesByDepartmentAsync(int departmentId, UserScope? scope = null);

    // HOD candidates (WITH scope)
    Task<List<EmployeeDropdownDto>> GetHodCandidatesAsync(UserScope? scope = null);

    // Supervisor candidates (WITH scope)
    Task<List<EmployeeDropdownDto>> GetSupervisorCandidatesAsync(UserScope? scope = null);
}
```

**DTO**:
```csharp
public class EmployeeDropdownDto
{
    public int EmployeeId { get; set; }
    public string PayrollNo { get; set; }
    public string FullName { get; set; } // FirstName + LastName
    public int? StationId { get; set; }
    public int? DepartmentId { get; set; }
}
```

---

### 4. RoleService Interface

```csharp
public interface IRoleService
{
    // Roles (NO scope - admin only)
    Task<List<RoleDropdownDto>> GetActiveRolesAsync();
    Task<List<RoleDropdownDto>> GetRolesByScopeLevelAsync(ScopeLevel scopeLevel);

    // Permissions (NO scope - admin only)
    Task<List<PermissionDropdownDto>> GetActivePermissionsAsync();
    Task<List<PermissionDropdownDto>> GetPermissionsByModuleAsync(string module);
}
```

**DTOs**:
```csharp
public class RoleDropdownDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; }
    public string Description { get; set; }
    public ScopeLevel ScopeLevel { get; set; }
}

public class PermissionDropdownDto
{
    public int PermissionId { get; set; }
    public string PermissionName { get; set; }
    public string Module { get; set; }
    public string Action { get; set; }
}
```

---

### 5. EmployeeValidationService Interface

```csharp
public interface IEmployeeValidationService
{
    Task<bool> IsPayrollNoUniqueAsync(string payrollNo, int? excludeEmployeeId = null);
    Task<bool> IsRollNoUniqueAsync(string? rollNo, int? excludeEmployeeId = null);
    Task<bool> IsUsernameUniqueAsync(string? username, int? excludeEmployeeId = null);
    Task<bool> IsEmailUniqueAsync(string? email, int? excludeEmployeeId = null);
    Task<bool> IsPhoneUniqueAsync(string? phone, int? excludeEmployeeId = null);
}
```

---

## Scope Application Strategy

### Station/Department Scope: Read vs Write Context ⚠️ CRITICAL

**Important Distinction:**

Stations and Departments serve dual purposes in the application:
1. **Read Context** (for filtering/viewing data): User can see their organizational context
2. **Write Context** (for assigning employees): User can ONLY assign within their strict scope

**Example: Department Head (John, HR Department in Nairobi Factory)**

**In Read Context** (filters, viewing lists):
- ✅ Can see "Nairobi Factory" station (for organizational context/filtering)
- ✅ Can see "HR Department" (their department)
- ✅ Can VIEW employees in their department
- 📊 Scope shows organizational boundaries for reports/dashboards

**In Write Context** (creating/editing employees, assigning):
- ❌ CANNOT assign employees to other departments (even in same station)
- ✅ Can ONLY assign employees to "HR Department"
- ❌ CANNOT create records in other departments
- 🔒 Scope enforces data modification boundaries

**Security Principle**: **Principle of Least Privilege**
- Users see organizational structure for context
- Users can ONLY modify data within their scope
- Department scope = Can only assign to THEIR department (not all departments in station)

**Exception Handling**:
- Users with `Role.AllowCrossDepartmentAccess = true` can cross department boundaries
- Users with `Role.AllowCrossStationAccess = true` can cross station boundaries
- These flags override default scope restrictions (future enhancement)

---

### Decision Tree for Scope Application

```
┌─────────────────────────────────────────────┐
│  Is the query for reference data?          │
│  (Categories, Roles, Permissions)           │
└──────────────┬──────────────────────────────┘
               │
       YES ────┴──── NO
        │              │
        ▼              ▼
   NO SCOPE       ┌───────────────────────────────┐
   APPLIED        │ Is it organizational hierarchy?│
                  │ (Stations, Departments)        │
                  └──────────────┬─────────────────┘
                                 │
                         YES ────┴──── NO
                          │              │
                          ▼              ▼
                  ┌──────────────┐  ┌──────────────┐
                  │ User Scope?  │  │ ALWAYS APPLY │
                  └──────┬───────┘  │ SCOPE        │
                         │          │ (Employees,  │
                 ┌───────┼───────┐  │  Incidents)  │
                 │       │       │  └──────────────┘
            Organization │ Other
                 │       │       │
                 ▼       ▼       ▼
            ALL DATA  FILTERED  FILTERED
                      BY SCOPE  BY SCOPE
```

### Implementation Logic

#### CRITICAL: Enhance ScopeFilterService First!

**Before implementing services, add Station/Department/Section scope support to `ScopeFilterService.cs`:**

```csharp
private IQueryable<Station> ApplyStationScope(IQueryable<Station> query, UserScope scope)
{
    return scope.Level switch
    {
        ScopeLevel.Station => query.Where(s => s.StationId == scope.StationId),
        ScopeLevel.Department => query.Where(s => s.StationId == scope.StationId),
        ScopeLevel.Team => query.Where(s => s.StationId == scope.StationId),
        ScopeLevel.Self => query.Where(s => s.StationId == scope.StationId),
        _ => query // Organization scope sees all
    };
}

private IQueryable<Department> ApplyDepartmentScope(IQueryable<Department> query, UserScope scope)
{
    return scope.Level switch
    {
        ScopeLevel.Station => query.Where(d => d.StationId == scope.StationId),

        // ✅ CORRECT: Department scope users can ONLY see their own department
        // This enforces Principle of Least Privilege - they cannot assign to other departments
        ScopeLevel.Department => query.Where(d => d.DepartmentId == scope.DepartmentId),

        ScopeLevel.Team => query.Where(d => d.DepartmentId == scope.DepartmentId),
        ScopeLevel.Self => query.Where(d => d.DepartmentId == scope.DepartmentId),
        _ => query
    };
}
```

#### For OrganizationalHierarchyService (CORRECT)

**✅ USE ScopeFilterService - Don't reinvent the wheel!**

```csharp
public async Task<List<StationDropdownDto>> GetActiveStationsAsync(UserScope? scope = null)
{
    var query = _context.Stations.Where(s => s.IsActive);

    // Use existing ScopeFilterService (centralized scope logic)
    if (scope != null)
    {
        query = _scopeFilterService.ApplyScope(query, scope);
    }

    return await query
        .OrderBy(s => s.StationName)
        .Select(s => new StationDropdownDto
        {
            StationId = s.StationId,
            StationName = s.StationName,
            OrgCategoryId = s.OrgCategoryId
        })
        .ToListAsync();
}

// Cascading: SCOPE FIRST, then filter by category
public async Task<List<StationDropdownDto>> GetStationsByCategoryAsync(
    int categoryId, 
    UserScope? scope = null)
{
    var query = _context.Stations
        .Where(s => s.IsActive && s.OrgCategoryId == categoryId);

    // CRITICAL: Apply scope BEFORE returning (security first!)
    if (scope != null)
    {
        query = _scopeFilterService.ApplyScope(query, scope);
    }

    return await query
        .OrderBy(s => s.StationName)
        .Select(s => new StationDropdownDto { ... })
        .ToListAsync();
}
```

#### For EmployeeDropdownService

```csharp
public async Task<List<EmployeeDropdownDto>> GetActiveEmployeesAsync(UserScope? scope = null)
{
    var query = _context.Employees.Where(e => e.EmploymentStatus == "Active");

    // ALWAYS apply scope for employees (security-critical)
    if (scope != null)
    {
        // Use existing ScopeFilterService
        query = _scopeFilterService.ApplyScope(query, scope);
    }
    else
    {
        // No scope provided = no access
        return new List<EmployeeDropdownDto>();
    }

    return await query
        .OrderBy(e => e.FirstName)
        .ThenBy(e => e.LastName)
        .Select(e => new EmployeeDropdownDto
        {
            EmployeeId = e.EmployeeId,
            PayrollNo = e.PayrollNo,
            FullName = e.FirstName + " " + e.LastName,
            StationId = e.StationId,
            DepartmentId = e.DepartmentId
        })
        .ToListAsync();
}
```

---

## Implementation Guidelines

### Step 1: Create DTOs

Create a new folder: `Models/DTOs/Dropdowns/`

Add all DTO classes defined above.

### Step 2: Create Service Interfaces

Create interfaces in `Services/` folder or subfolder `Services/DropdownServices/`

### Step 3: Implement Services

Implement each service with:
- Constructor injecting `OshDbContext` and `IScopeFilterService`
- Methods following specifications above
- Proper async/await patterns
- Logging for errors

### Step 4: Register Services in DI

In `Program.cs`:
```csharp
// Common Query Services
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IOrganizationalHierarchyService, OrganizationalHierarchyService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IRoleService, RoleService>();

// Validation Services
builder.Services.AddScoped<IEmployeeValidationService, EmployeeValidationService>();

// Memory Cache (for reference data)
builder.Services.AddMemoryCache();
```

### Step 5: Update Controllers

**Before**:
```csharp
public async Task<IActionResult> Index()
{
    // ... main query ...

    var stations = await _context.Stations
        .Where(s => s.IsActive)
        .OrderBy(s => s.StationName)
        .Select(s => new { s.StationId, s.StationName })
        .ToListAsync();

    ViewBag.Stations = stations;
    return View(employees);
}
```

**After**:
```csharp
private readonly IOrganizationalHierarchyService _orgHierarchyService;
private readonly IOrganizationService _organizationService;

public MyController(
    OshDbContext context,
    IScopeFilterService scopeFilter,
    IOrganizationalHierarchyService orgHierarchyService,
    IOrganizationService organizationService,
    ILogger<MyController> logger)
    : base(context, scopeFilter, logger)
{
    _orgHierarchyService = orgHierarchyService;
    _organizationService = organizationService;
}

public async Task<IActionResult> Index()
{
    // ... main query ...

    // Categories (no scope)
    ViewBag.Categories = await _organizationService.GetActiveCategoriesAsync();
    
    // Stations (with scope)
    ViewBag.Stations = await _orgHierarchyService.GetActiveStationsAsync(CurrentScope);
    
    return View(employees);
}
```

### Step 6: Update Validation Logic

**Before**:
```csharp
[HttpGet]
public async Task<IActionResult> CheckPayrollNo(string payrollNo, int? employeeId)
{
    var exists = await _context.Employees
        .AnyAsync(e => e.PayrollNo == payrollNo &&
                      (employeeId == null || e.EmployeeId != employeeId));
    return Json(!exists);
}
```

**After**:
```csharp
private readonly IEmployeeValidationService _validationService;

[HttpGet]
public async Task<IActionResult> CheckPayrollNo(string payrollNo, int? employeeId)
{
    var isUnique = await _validationService.IsPayrollNoUniqueAsync(payrollNo, employeeId);
    return Json(isUnique);
}
```

---

## Benefits

### 1. **Code Reusability**
- ✅ Eliminate duplicate queries across 6+ controllers
- ✅ Single source of truth for dropdown data
- ✅ Easier to maintain and update

### 2. **Consistent Scope Application**
- ✅ Scope logic centralized in services
- ✅ Impossible to forget scope filtering
- ✅ Security by default

### 3. **Performance Optimization**
- ✅ Services can implement caching for reference data
- ✅ Consistent query patterns (easier for EF query optimization)
- ✅ Reduced database round trips

**Caching Strategy:**

```csharp
// For reference data (Categories, Roles) - cache aggressively
public async Task<List<CategoryDropdownDto>> GetActiveCategoriesAsync()
{
    return await _cache.GetOrCreateAsync(
        "ActiveCategories",
        async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(30);
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2);
            
            return await _context.OrgCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .Select(c => new CategoryDropdownDto { ... })
                .ToListAsync();
        });
}
```

**When to Cache:**
- ✅ **Categories**: Rarely change, cache for 30 minutes
- ✅ **Roles**: Rarely change, cache for 30 minutes
- ⚠️ **Stations**: Can change, consider short cache (5 minutes) or no cache
- ❌ **Employees**: Change often, DON'T cache (scope-dependent too)

### 4. **Testability**
- ✅ Services can be unit tested independently
- ✅ Controllers become thinner (easier to test)
- ✅ Mock services for integration tests

### 5. **Maintainability**
- ✅ Changes to dropdown logic in ONE place
- ✅ Easy to add new scope rules
- ✅ Clear separation of concerns

### 6. **Type Safety**
- ✅ DTOs provide compile-time safety
- ✅ No more anonymous types with `new { }`
- ✅ IntelliSense support

---

## Migration Strategy

### Phase 0: Enhance ScopeFilterService (Day 1-2) ⚠️ CRITICAL FIRST!
1. **Add Station scope support** to `ScopeFilterService.cs`
2. **Add Department scope support** to `ScopeFilterService.cs`
3. **Add Section scope support** to `ScopeFilterService.cs`
4. **Test scope filtering thoroughly** (see Testing Requirements below)
5. **This MUST be done before implementing services!**

**Critical Testing Requirements for Phase 0:**

Test with 5 different user accounts (one per scope level):

1. **Organization Scope User** (e.g., Admin):
   - ✅ Should see ALL stations
   - ✅ Should see ALL departments
   - ✅ Should see ALL employees

2. **Station Scope User** (e.g., Station Manager):
   - ✅ Should see ONLY their station
   - ✅ Should see ALL departments in their station
   - ✅ Should see ALL employees in their station
   - ❌ Should NOT see other stations' data

3. **Department Scope User** (e.g., Department Head):
   - ✅ Should see their station (for context)
   - ✅ Should see ONLY their department (NOT other departments)
   - ✅ Should see ONLY employees in their department
   - ❌ Should NOT be able to assign employees to other departments
   - ❌ Should NOT see other departments in dropdown

4. **Team Scope User** (e.g., Supervisor):
   - ✅ Should see their station/department (for context)
   - ✅ Should see ONLY direct reports
   - ❌ Should NOT see other teams' employees

5. **Self Scope User** (e.g., Regular Employee):
   - ✅ Should see ONLY their own record
   - ❌ Should NOT see other employees

### Phase 1: Create Core Services (Week 1)
1. Create DTO classes in `Models/DTOs/Dropdowns/`
2. Create `OrganizationService` (Categories only)
3. Create `OrganizationalHierarchyService` (Stations, Departments, Sections)
4. Create `EmployeeService` (with strict scope enforcement)
5. Register in DI
6. Add unit tests for each service

### Phase 2: Update High-Traffic Controllers (Week 2)
1. Update `EmployeeController` (uses all services)
2. Update `SectionController` (uses hierarchy + employee services)
3. Update `DepartmentController` (uses hierarchy services)
4. Test thoroughly with different user scopes
5. Verify scope filtering works correctly

### Phase 3: Complete Migration (Week 3)
1. Update `StationController`
2. Update remaining controllers
3. Create `EmployeeValidationService`
4. Create `RoleService`
5. Remove old `PopulateDropdowns()` helper methods
6. Integration testing

### Phase 4: Optimization & Cleanup (Week 4)
1. Add caching for reference data (Categories, Roles)
2. Add performance logging
3. Optimize queries based on metrics
4. Complete documentation
5. Code review and refactoring

---

## Summary

### Key Decisions:

| Entity | Scope Required? | Reason |
|--------|----------------|--------|
| **OrgCategories** | ❌ NO | Reference data - all users see all |
| **Stations** | ✅ CONDITIONAL | Organizational hierarchy - MUST respect scope |
| **Departments** | ✅ CONDITIONAL | Organizational hierarchy - MUST respect scope |
| **Sections** | ✅ CONDITIONAL | Organizational hierarchy - MUST respect scope |
| **Employees** | ✅ ALWAYS | Security-critical - MUST ALWAYS apply scope |
| **Incidents** | ✅ ALWAYS | Security-critical - MUST ALWAYS apply scope |
| **Hazards** | ✅ ALWAYS | Security-critical - MUST ALWAYS apply scope |
| **Roles** | ❌ NO | Reference data - admin-managed only |
| **Permissions** | ❌ NO | Reference data - admin-managed only |

### Scope Application Rules:

1. **Organization Scope (Level 1)**: See ALL data (no filtering)
2. **Station Scope (Level 2)**: See only their station's data
3. **Department Scope (Level 3)**: See only their station's data (departments within station)
4. **Team Scope (Level 4)**: See only their station's data (limited to team)
5. **Self Scope (Level 5)**: See only their own data

### Service Dependencies:

```
Controllers (ScopedController)
    ↓
    ├── CurrentScope (from UserScopeService)
    ↓
┌───────────────────────────────────┐
│ Common Query Services             │
│ - OrganizationService             │
│ - OrganizationalHierarchyService  │
│ - EmployeeService                 │
│ - RoleService                     │
└───────┬───────────────────────────┘
        ↓
┌───────────────────────────────────┐
│ ScopeFilterService ⚠️ ENHANCED    │
│ - Station scope support (NEW)     │
│ - Department scope support (NEW)  │
│ - Section scope support (NEW)     │
│ - Employee scope (existing)       │
│ - Incident scope (existing)       │
│ - Hazard scope (existing)         │
└───────┬───────────────────────────┘
        ↓
┌───────────────────────────────────┐
│ UserScopeService                  │
│ (reads claims, builds UserScope)  │
└───────────────────────────────────┘
```

### Critical Implementation Notes:

1. **ALWAYS enhance ScopeFilterService FIRST** before creating services
2. **NEVER manually implement scope logic** - use ScopeFilterService
3. **SCOPE TAKES PRECEDENCE** over user-selected filters (security first)
4. **Employee queries MUST fail-safe** - return empty if no scope provided
5. **Cascading filters apply scope first**, then cascading filter

### Security Implications - Department Scope Bug Example:

**❌ WRONG Implementation** (Security Vulnerability):
```csharp
// If Department scope filtered by StationId instead of DepartmentId
ScopeLevel.Department => query.Where(d => d.StationId == scope.StationId)
// Result: Department Head sees ALL departments in station
// Can assign employees to OTHER departments (unauthorized!)
```

**✅ CORRECT Implementation** (Secure):
```csharp
// Department scope MUST filter by DepartmentId
ScopeLevel.Department => query.Where(d => d.DepartmentId == scope.DepartmentId)
// Result: Department Head sees ONLY their department
// Can ONLY assign employees to their own department
```

**Real-World Impact of Bug:**
- 🔓 Department Head could modify other departments' employees
- 🔓 Could assign employees across department boundaries
- 🔓 Data integrity violation (employees in wrong departments)
- 🔓 Security audit failure
- 🔓 Compliance violations (unauthorized access to HR data)

---

## Next Steps

1. ✅ Review this analysis (COMPLETE)
2. ⚠️ **Phase 0: Enhance ScopeFilterService** (CRITICAL - DO THIS FIRST!)
   - Add Station/Department/Section scope methods
   - Update `ApplyScopeByType` method
   - Test thoroughly
3. Begin implementation with Phase 1 (Core Services)
4. Test with different user scopes before proceeding to Phase 2

**Estimated Total Effort**: 4-5 weeks for complete migration
- Phase 0: 2 days (ScopeFilterService enhancement)
- Phase 1: 1 week (Core services)
- Phase 2: 1 week (High-traffic controllers)
- Phase 3: 1 week (Remaining controllers)
- Phase 4: 1 week (Optimization & cleanup)

**Immediate Priority**: 
1. **ScopeFilterService enhancement** (MUST be first!)
2. `OrganizationalHierarchyService` (used most frequently)
3. `EmployeeService` (security-critical)

---

## Document Revision History

**Version 2.1** - Security critical fixes:
- ✅ Added "Read vs Write Context" section explaining scope distinction
- ✅ Added critical testing requirements for Phase 0
- ✅ Added security implications section with bug example
- ✅ Emphasized Department scope bug fix (StationId vs DepartmentId)
- ✅ Added real-world impact analysis of scope bugs
- ✅ Clarified Principle of Least Privilege implementation

**Version 2.0** - Critical fixes applied:
- ✅ Fixed scope contradiction for Stations/Departments
- ✅ Split services: Organization vs OrganizationalHierarchy
- ✅ Added ScopeFilterService enhancement requirement
- ✅ Emphasized using ScopeFilterService (not manual logic)
- ✅ Added caching strategy
- ✅ Clarified cascading with scope precedence
- ✅ Updated all interfaces and examples

**Version 1.0** - Initial analysis
