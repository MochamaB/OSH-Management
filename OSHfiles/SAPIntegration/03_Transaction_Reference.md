# SAP Transaction Code Reference - Quick Guide

## Table of Contents
1. [Development Transactions](#development-transactions)
2. [Data Viewing Transactions](#data-viewing-transactions)
3. [HR Module Transactions](#hr-module-transactions)
4. [Transport & System Transactions](#transport--system-transactions)
5. [Security & Authorization](#security--authorization)
6. [Monitoring & Troubleshooting](#monitoring--troubleshooting)

---

## Development Transactions

### SE11 - ABAP Dictionary
**Purpose**: Create/view database table structures, data types, domains

**When to Use**:
- Creating Z-tables
- Viewing table structure (fields, keys, indexes)
- Checking if table exists
- Activating table changes

**How to Use**:
```
1. /nSE11
2. Select "Database table"
3. Enter table name: ZOSH_EMPLOYEE_DATA
4. Click "Display" or "Create"
```

**Common Tasks**:
- Create table: Enter name → Create → Define fields → Technical Settings → Activate
- View structure: Enter name → Display → See fields/indexes
- Check dependencies: Utilities → Where-used list

---

### SE16N - General Table Display
**Purpose**: View/search data in any SAP table

**When to Use**:
- Checking if data exists
- Verifying sync results
- Testing queries before coding
- Debugging data issues

**How to Use**:
```
1. /nSE16N
2. Table: PA0001
3. Set filters:
   - PERNR = 00012345
   - ENDDA = 99991231
4. Execute (F8)
```

**Power Features**:
```
Settings (Ctrl+Shift+F9):
□ Allow all functions (enables editing - use carefully!)
□ Number of entries (shows row count)
□ Display table name (shows technical name)

Right-click on table → Settings → User Parameters:
- Set default output format (e.g., Excel)
- Set maximum rows
```

---

### SE38 - ABAP Editor
**Purpose**: Create/edit ABAP programs (reports, batch jobs)

**When to Use**:
- Creating sync programs
- Creating test data generators
- Debugging code

**How to Use**:
```
1. /nSE38
2. Program: ZOSH_EMPLOYEE_SYNC
3. Create → Type: Executable program
4. Enter code
5. Save (Ctrl+S)
6. Check syntax (Ctrl+F2)
7. Activate (Ctrl+F3)
8. Execute (F8)
```

**Useful Menu Options**:
```
Program → Check → Extended Check (finds hidden errors)
Program → Generate (recompiles)
Utilities → Runtime Analysis (performance testing)
Goto → Text Elements → Title (program description)
```

---

### SE37 - Function Builder
**Purpose**: Create/view/test RFC function modules

**When to Use**:
- Creating custom RFCs for integration
- Testing RFC execution
- Checking RFC parameters

**How to Use**:
```
Create:
1. /nSE37
2. Function Module: Z_OSH_GET_EMPLOYEES
3. Create
4. Attributes → Processing Type: Remote-enabled
5. Import/Export/Tables tabs → Define parameters
6. Source Code → Write code
7. Activate

Test:
1. /nSE37
2. Function Module: Z_OSH_GET_EMPLOYEES
3. Test/Execute (F8)
4. Fill import parameters
5. Execute (F8)
6. View tables tab for results
```

---

### SE80 - Object Navigator
**Purpose**: Comprehensive development workbench (all-in-one tool)

**When to Use**:
- Managing packages
- Viewing all related objects
- Repository browser
- Advanced development

**How to Use**:
```
1. /nSE80
2. Dropdown: Select "Repository Browser"
3. Navigate: Package → Programs → Tables
4. Double-click to edit
```

**Advantages**:
- See all objects in one place
- Easy navigation between related objects
- Integrated testing
- Transport management

---

### SE93 - Transaction Code Maintenance
**Purpose**: Create custom transaction codes for your programs

**When to Use**:
- Creating shortcuts to your Z-programs
- User-friendly access (instead of SE38 → Execute)

**How to Use**:
```
1. /nSE93
2. Transaction Code: ZOSH_SYNC
3. Create
4. Type: Program and selection screen
5. Program: ZOSH_EMPLOYEE_SYNC
6. Save to package
7. Transport

Users can now run: /nZOSH_SYNC
```

---

## Data Viewing Transactions

### SE16 - Data Browser (Old Version)
**Purpose**: Same as SE16N but older interface

**When to Use**: If SE16N is not available in your system

**Prefer SE16N** when available (more features, better UI)

---

### SE16N - General Table Display (Detailed)
**Purpose**: Best tool for viewing table data

**Advanced Features**:
```
Column Operations:
- Right-click column header → Sort, Filter, Sum
- Drag columns to reorder
- Hide/show columns

Export Options:
- Spreadsheet (Excel)
- Local file
- Clipboard

Selection Criteria:
- Use * for wildcards (e.g., VORNA = 'John*')
- Use ranges: BEGDA = 20240101...20241231
- Complex conditions: ( STAT2 = '3' OR STAT2 = '1' )
```

---

### SQVI - QuickViewer
**Purpose**: Create custom queries without ABAP coding

**When to Use**:
- Need to join multiple tables
- Create reusable queries
- Business users need reports

**How to Use**:
```
1. /nSQVI
2. Create query: ZOSH_EMPLOYEE_LIST
3. Add tables: PA0001, PA0002
4. Join condition: PA0001-PERNR = PA0002-PERNR
5. Select fields to display
6. Execute
```

---

## HR Module Transactions

### PA20 - Display HR Master Data
**Purpose**: View employee infotypes (read-only)

**When to Use**:
- Looking up employee details
- Verifying org assignment
- Checking active records

**How to Use**:
```
1. /nPA20
2. Personnel No: 00012345
3. Infotype: 0001 (Org Assignment)
4. Period: Today's date
5. Execute (F8)
```

**Common Infotypes**:
```
0001 - Organizational Assignment (dept, station, position)
0002 - Personal Data (name, DOB)
0006 - Address
0105 - Communication (email, phone)
0007 - Planned Working Time
0008 - Basic Pay
0009 - Bank Details
```

---

### PA30 - Maintain HR Master Data
**Purpose**: Create/edit employee records

**When to Use**:
- Creating test employees in DEV
- Updating employee info

**How to Use**:
```
1. /nPA30
2. Personnel No: 00090001 (for new, use range 90000-99999)
3. Infotype: 0001
4. Create (if new employee)
5. Fill fields
6. Save
```

**⚠️ Warning**: Only use in DEV! Production changes need approval.

---

### PA40 - Personnel Actions
**Purpose**: Guided process for HR actions (hire, transfer, terminate)

**When to Use**:
- Hiring new test employees (easier than PA30)
- Employee transfers
- Terminations

**How to Use**:
```
1. /nPA40
2. Action Type: Hiring
3. Fill personnel details
4. System guides through infotypes
5. Save
```

---

### PPOME - Organizational Management
**Purpose**: View/edit organizational structure

**When to Use**:
- Understanding org hierarchy
- Creating test organizational units
- Viewing reporting structure

**How to Use**:
```
1. /nPPOME
2. Select organizational plan
3. View structure tree
4. Create/edit units
```

---

## Transport & System Transactions

### SE09 / SE10 - Transport Organizer
**Purpose**: Create and manage transport requests

**When to Use**:
- Creating new transport
- Releasing transport
- Checking transport contents
- Adding objects to transport

**How to Use**:
```
Create Transport:
1. /nSE09
2. Click "Create" icon
3. Type: Workbench Request
4. Description: "OSH Integration - Z-Tables and Programs"
5. Save → Note transport number

Release Transport:
1. /nSE09
2. Find your transport (show your requests)
3. Right-click task → Release
4. Right-click request → Release
5. Transport ready for QAS import
```

**Transport States**:
- 🟡 Modifiable (can add objects)
- 🔵 Released (locked, ready for import)
- 🟢 Imported (in target system)

---

### STMS - Transport Management System
**Purpose**: Import transports to QAS/PROD (Basis team uses this)

**When to Use**:
- Checking import status
- Monitoring transport queue
- (Usually Basis team responsibility)

**How to Use**:
```
1. /nSTMS
2. Import Overview
3. Select target system (QAS or PROD)
4. View import queue
5. Double-click to see log
```

---

### SCC9 - Client Copy - Remote
**Purpose**: Copy data from one client to another

**When to Use**:
- Copying org structure from QAS to DEV
- Setting up test environment

**How to Use**:
```
⚠️ Basis team task - request from them
Provide:
- Source: QAS client 300
- Target: DEV client 100
- Profile: SAP_USER (or custom)
- Tables: T001P, PA0001, PA0002 (specific tables)
```

---

## Security & Authorization

### SU01 - User Maintenance
**Purpose**: View/create user accounts

**When to Use**:
- Creating service account for integration
- Checking your own authorization
- Viewing assigned roles

**How to Use**:
```
1. /nSU01
2. User: OSH_INTEGRATION
3. Display → Roles tab
4. See assigned profiles
```

---

### SU53 - Display Authorization Check
**Purpose**: Shows WHY you got "No authorization" error

**When to Use**:
- Immediately after authorization error
- Finding which authorization object failed

**How to Use**:
```
1. Try to access table/transaction
2. Get "No authorization" error
3. Immediately run: /nSU53
4. See failed authorization object
5. Send screenshot to security team with request
```

**Example Output**:
```
Authorization Object: S_TABU_NAM
Field: ACTVT (Activity) → Value 03 required (Display)
Field: TABLE → Value ZOSH_EMPLOYEE_DATA
Status: ❌ No authorization
```

---

### SU24 - Authorization Default Values
**Purpose**: View which authorization objects are checked by transaction

**When to Use**:
- Understanding security requirements
- Designing authorization roles

**How to Use**:
```
1. /nSU24
2. Transaction: SE16N
3. Display
4. See all auth objects checked
```

---

### PFCG - Role Maintenance
**Purpose**: Create/maintain authorization roles

**When to Use**:
- Creating custom role for integration user
- Assigning specific table/transaction access

**How to Use**:
```
1. /nPFCG
2. Role: ZOSH_INTEGRATION_ROLE
3. Create
4. Menu → Add transactions
5. Authorizations → Generate profile
6. User → Assign to users
```

---

## Monitoring & Troubleshooting

### SM36 - Schedule Background Job
**Purpose**: Schedule programs to run automatically

**When to Use**:
- Scheduling daily employee sync
- Setting up recurring jobs

**How to Use**:
```
1. /nSM36
2. Job Name: ZOSH_DAILY_SYNC
3. Job Class: C (normal priority)
4. Start Condition:
   - Immediate (for testing)
   - Date/Time (e.g., daily at 2 AM)
   - After Job (chain jobs)
5. Steps:
   - Program: ZOSH_EMPLOYEE_SYNC
6. Save
```

---

### SM37 - Job Monitoring
**Purpose**: View status and logs of background jobs

**When to Use**:
- Checking if job ran successfully
- Viewing job logs
- Debugging job failures
- Canceling running jobs

**How to Use**:
```
1. /nSM37
2. Job Name: ZOSH* (wildcard search)
3. Execute (F8)
4. Job list appears with status:
   - ✅ Finished (successful)
   - ❌ Cancelled (failed)
   - 🔄 Active (running now)
   - ⏸️  Scheduled (waiting)
5. Double-click job → View spool/log
```

---

### ST22 - ABAP Runtime Errors
**Purpose**: View error dumps when program crashes

**When to Use**:
- Program terminated with error
- Debugging code issues
- Finding root cause

**How to Use**:
```
1. /nST22
2. See list of recent dumps
3. Double-click to see details:
   - Error message
   - Source code line
   - Variable values
   - Call stack
4. Fix code, re-test
```

**Common Errors**:
```
TSV_TNEW_PAGE_ALLOC_FAILED = Out of memory
TIME_OUT = Program ran too long
DBIF_RSQL_SQL_ERROR = Database error
SYSTEM_FAILURE = General system error
```

---

### SM50 - Work Process Overview
**Purpose**: Monitor active processes on server

**When to Use**:
- Checking if long-running program is still active
- System performance issues
- Basis team troubleshooting

**How to Use**:
```
1. /nSM50
2. See all active work processes
3. Find your program
4. Can cancel if stuck
```

---

### SM21 - System Log
**Purpose**: View system-level error messages

**When to Use**:
- Authorization issues
- Database connection errors
- System problems

**How to Use**:
```
1. /nSM21
2. Filter by:
   - User
   - Transaction
   - Date/time
3. Read → View log entries
```

---

### ST05 - SQL Trace
**Purpose**: Performance analysis of database queries

**When to Use**:
- Program is slow
- Optimizing queries
- Understanding what program does

**How to Use**:
```
1. /nST05
2. Activate Trace (with SQL statements)
3. Run your program
4. Deactivate Trace
5. Display Trace
6. Analyze:
   - Which tables queried
   - How many records
   - Execution time
```

---

## Quick Reference by Task

### "I need to..."

#### View table data
```
→ SE16N
```

#### Create table
```
→ SE11 → Database table → Create
```

#### Write program
```
→ SE38 → Create → Write code → Activate → Execute
```

#### Create RFC
```
→ SE37 → Create → Remote-enabled → Activate → Test
```

#### Create transport
```
→ SE09 → Create request
```

#### Schedule job
```
→ SM36 → Schedule
```

#### Check job status
```
→ SM37 → Enter job name → Execute
```

#### Check authorization error
```
→ SU53 (immediately after error)
```

#### View error logs
```
→ ST22 (program crash)
→ SM21 (system log)
→ SM37 → Job log (background job)
```

#### View org structure
```
→ PPOME
```

#### View employee
```
→ PA20 → Personnel number → Infotype
```

#### Create test employee
```
→ PA30 → New personnel number → Create
```

---

## Transaction Code Patterns

### Prefix Meanings
```
SE** = System Engineering (development tools)
SM** = System Management (monitoring, admin)
SU** = Security/Users
PA** = Personnel Administration
ST** = System Trace (performance, errors)
SQ** = Query tools
PP** = Personnel Planning (org mgmt)
```

### Navigation Shortcuts
```
/n[TCODE]    = Close current, open new transaction
/o[TCODE]    = Open in new session (new window)
/nEX         = End transaction
/nEXIT       = Log off
/i           = Delete session
/o           = Create new session
```

---

## Frequently Used Combinations

### Creating Z-Object Workflow
```
1. SE11 → Create table
2. SE38 → Create program
3. SE37 → Create RFC (optional)
4. SE09 → Create transport
5. Add objects to transport
6. SE38 → Test program (F8)
7. SE09 → Release transport
```

### Debugging Workflow
```
1. SM37 → Check job status
2. If failed → View job log
3. ST22 → Check for dumps
4. SE38 → Program → /h debug mode
5. ST05 → SQL trace (if slow)
6. Fix code → Re-test
```

### Daily Monitoring Workflow
```
1. SM37 → Check ZOSH* jobs
2. SE16N → ZOSH_EMPLOYEE_DATA → Count records
3. ST22 → Check for new errors
4. SU53 → Check auth issues (if reported)
```

---

## Transaction Cheat Sheet (Print This!)

```
┌────────────────────────────────────────────────────────────┐
│             ESSENTIAL TRANSACTIONS                          │
├──────────┬──────────────────────────────────────────────────┤
│ SE11     │ Table structures                                │
│ SE16N    │ View table data ⭐ MOST USED                    │
│ SE38     │ ABAP programs                                   │
│ SE37     │ Function modules (RFCs)                         │
│ SE09     │ Transport organizer                             │
│ SM36     │ Schedule background job                         │
│ SM37     │ Monitor jobs ⭐ MOST USED                       │
│ ST22     │ Error dumps ⭐ DEBUGGING                        │
│ SU53     │ Auth check ⭐ WHEN DENIED                       │
│ PA20     │ Display employee                                │
│ PA30     │ Maintain employee                               │
└──────────┴──────────────────────────────────────────────────┘
```

---

## Tips for Transaction Mastery

1. **Use wildcards**: `/nSE*` (shows dropdown of all SE transactions)
2. **Favorites**: Right-click transaction → Add to Favorites
3. **History**: Click dropdown in command field → See recent
4. **F4 Help**: Press F4 in any field for dropdown options
5. **F1 Help**: Press F1 on any field for documentation
6. **Ctrl+Y**: Opens Personal Settings (adjust defaults)

---

**Next**: Proceed to **04_Step_by_Step_Implementation.md** to start building!
