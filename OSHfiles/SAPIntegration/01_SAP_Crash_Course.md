# SAP Crash Course - Absolute Beginner's Guide

## Table of Contents
1. [SAP System Basics](#sap-system-basics)
2. [SAP GUI Navigation](#sap-gui-navigation)
3. [Understanding Transaction Codes](#understanding-transaction-codes)
4. [SAP Tables and Data](#sap-tables-and-data)
5. [SAP Development Objects](#sap-development-objects)
6. [Transport System](#transport-system)
7. [Authorization Basics](#authorization-basics)

---

## SAP System Basics

### What is SAP?
SAP (Systems, Applications, and Products) is an enterprise resource planning (ERP) system that manages business processes across an organization.

### SAP Architecture
```
┌─────────────────────────────────────────────┐
│          SAP Production (PROD/PRD)          │
│        Real business data - READ ONLY       │
│            Live transactions                │
└──────────────────┬──────────────────────────┘
                   │
                   │ (Transport)
                   │
┌──────────────────▼──────────────────────────┐
│      SAP Quality Assurance (QAS/QA)         │
│      Copy of production structure           │
│      Testing ground for changes             │
└──────────────────┬──────────────────────────┘
                   │
                   │ (Transport)
                   │
┌──────────────────▼──────────────────────────┐
│        SAP Development (DEV)                │
│      Where you create custom objects        │
│      May have limited/test data             │
└─────────────────────────────────────────────┘
```

### SAP Client Concept
- Each SAP system has multiple "clients" (like separate databases)
- Client 000 = Master client (don't use)
- Client 100 = Typically development client
- Client 300 = Typically quality/test client
- Client 400 = Typically production client
- **You MUST specify client when logging in**

### SAP Modules
Your focus is **SAP HCM (Human Capital Management)**
- Module code: **PA** (Personnel Administration)
- Module code: **OM** (Organizational Management)

---

## SAP GUI Navigation

### Logging In
```
1. Open SAP Logon Pad
2. Double-click your system (e.g., "DEV - Development System")
3. Enter credentials:
   - Client: [e.g., 100]
   - User: [your username]
   - Password: [your password]
   - Language: EN
4. Click "Log On" button
```

### Main Screen Elements
```
┌─────────────────────────────────────────────┐
│ [Menu Bar]  System  Help                    │ ← Top menu
├─────────────────────────────────────────────┤
│ Transaction: [SE16__] 🔍 📋 📂 ⭐         │ ← Command field (most important!)
├─────────────────────────────────────────────┤
│                                             │
│         [Main Work Area]                    │
│                                             │
│                                             │
│                                             │
└─────────────────────────────────────────────┘
│ Session 1/6 | SAPSID | Client: 100 | User  │ ← Status bar
└─────────────────────────────────────────────┘
```

### Essential Keyboard Shortcuts
```
ENTER          = Execute/Confirm
F3             = Back (one step)
F8             = Execute (run report/program)
F12            = Cancel
Ctrl+S         = Save
Ctrl+F         = Find on screen
/n             = End current transaction
/nEXIT         = Log off
/nSE16         = Jump to transaction SE16
/o             = Open new session (new window)
```

### The Command Field (Most Important!)
At the top of every screen is a command field. Type transaction codes here:

```
Transaction: [SE16____] ← Type here and press ENTER

Examples:
/nSE16         - Go to table viewer
/nSE11         - Go to ABAP Dictionary
/nSE38         - Go to ABAP Editor
/nPA20         - Display HR Master Data
```

---

## Understanding Transaction Codes

### What are Transaction Codes (T-Codes)?
Short codes that open specific SAP screens/functions. Like shortcuts.

### Common Transaction Code Patterns
```
SE** = Development tools
  SE11 = ABAP Dictionary (view table structures)
  SE16 = Data Browser (view table data)
  SE16N = General Table Display (better than SE16)
  SE38 = ABAP Editor (write programs)
  SE80 = Object Navigator (development workbench)
  
PA** = Personnel Administration
  PA20 = Display HR Master Data
  PA30 = Maintain HR Master Data
  
SM** = System Management
  SM36 = Background Job Scheduling
  SM37 = Background Job Monitoring
  SU01 = User Maintenance
  SU53 = Display Authorization Check
  
ST** = Performance/Technical
  ST22 = ABAP Runtime Errors (debugging)
```

### How to Find Transaction Codes
1. Menu path → Note the T-Code in status bar
2. Press F1 on any field → Technical Information → Transaction Code
3. Google: "SAP transaction for [what you want]"

---

## SAP Tables and Data

### What are SAP Tables?
Database tables that store all SAP data. Like SQL Server tables but with SAP conventions.

### SAP Table Naming Convention
```
First 2-3 letters = Module/Area
  PA**** = Personnel Administration
  T**** = Customizing/Configuration tables
  Z**** = Custom tables (your own)
  
Examples:
  PA0001 = HR Organizational Assignment
  PA0002 = HR Personal Data
  T001P = Personnel Areas
  ZOSH_EMPLOYEE_DATA = Your custom table
```

### Viewing Table Data

#### Method 1: SE16N (Easiest - Use This!)
```
1. Type: /nSE16N in command field
2. Enter table name: PA0001
3. Press ENTER or click "Execute" button
4. Set filters (optional):
   - ENDDA = 99991231 (active records)
   - PERNR = 00012345 (specific employee)
5. Click "Execute" (F8)
6. View data in table format
```

#### Method 2: SE16 (Older version)
```
Same as SE16N but older interface
Prefer SE16N when available
```

### Understanding SAP Table Structure

#### Viewing Table Definition
```
Transaction: SE11 (ABAP Dictionary)
1. Enter table name: PA0001
2. Click "Display" button
3. You'll see:
   - Fields tab: Column names and types
   - Technical Settings: Table category, size class
   - Indexes: Performance indexes
```

#### Common Field Types
```
CLNT = Client (always 3 characters)
CHAR = Character string
NUMC = Numeric character (numbers stored as text)
DATS = Date (YYYYMMDD format)
TIMS = Time (HHMMSS format)
CURR = Currency amount
INT4 = Integer (4 bytes)
```

### Important HR Tables for Your Project
```
┌──────────┬─────────────────────────────────────┬─────────────┐
│ Table    │ Description                         │ Key Fields  │
├──────────┼─────────────────────────────────────┼─────────────┤
│ PA0001   │ Organizational Assignment           │ PERNR       │
│          │ (Department, Station, Position)     │ BEGDA/ENDDA │
├──────────┼─────────────────────────────────────┼─────────────┤
│ PA0002   │ Personal Data (Name, DOB, etc.)     │ PERNR       │
│          │ ⚠️  Contains sensitive data!         │ BEGDA/ENDDA │
├──────────┼─────────────────────────────────────┼─────────────┤
│ PA0105   │ Communication (Email, Phone)        │ PERNR       │
│          │                                     │ SUBTY       │
├──────────┼─────────────────────────────────────┼─────────────┤
│ T001P    │ Personnel Areas (like Stations)     │ WERKS, BTRTL│
├──────────┼─────────────────────────────────────┼─────────────┤
│ T527X    │ Organizational Units                │ ORGEH       │
└──────────┴─────────────────────────────────────┴─────────────┘
```

### Understanding Date Fields (BEGDA/ENDDA)
SAP uses "validity periods" for HR data:
```
BEGDA = Begin Date (Start of validity)
ENDDA = End Date (End of validity)

Active record: ENDDA = 99991231 (December 31, 9999)
Historical record: ENDDA = past date

When querying, ALWAYS filter:
WHERE ENDDA = '99991231'  -- Only active records
```

---

## SAP Development Objects

### Custom Objects (Z*/Y*)
SAP reserves Z* and Y* prefixes for custom development:
- **Z-Tables**: ZOSH_EMPLOYEE_DATA
- **Z-Programs**: ZOSH_EMPLOYEE_SYNC
- **Z-Function Modules**: Z_OSH_GET_EMPLOYEES
- **Z-Views**: ZOSH_EMPLOYEE_V

**Rule**: Never modify standard SAP objects, always create Z* copies!

### Creating a Z-Table

#### Step 1: Open SE11
```
Transaction: /nSE11
Select: "Database table" radio button
Enter name: ZOSH_EMPLOYEE_DATA
Click: "Create" button
```

#### Step 2: Define Table Properties
```
Short Description: Employee Master Data for OSH Integration
Delivery Class: A (Application table)
Tab Strip: "Fields" (already selected)
```

#### Step 3: Add Fields
```
Field Name          Key    Type        Length   Description
────────────────────────────────────────────────────────────
MANDT               X      CLNT        3        Client
PERNR               X      CHAR        8        Personnel Number
VORNA                      CHAR        40       First Name
NACHN                      CHAR        40       Last Name
STELL                      CHAR        40       Position
WERKS                      CHAR        4        Plant/Station
ORGEH                      CHAR        8        Org Unit
EMAIL                      CHAR        100      Email
STAT2                      CHAR        1        Status
BEGDA                      DATS        8        Start Date
ENDDA                      DATS        8        End Date
ZSYNC_DATE                 DATS        8        Last Sync Date
ZSYNC_TIME                 TIMS        6        Last Sync Time
```

#### Step 4: Technical Settings
```
Tab: "Technical Settings"
Data Class: APPL0 (Master data)
Size Category: 2 (1000-10000 records) or higher
Buffering: Not allowed (data changes frequently)
```

#### Step 5: Activate
```
Click: "Activate" button (Ctrl+F3)
Or: Menu → Table → Activate
Icon: Traffic light turns green
```

### Creating ABAP Programs

#### Transaction: SE38
```
1. Transaction: /nSE38
2. Program name: ZOSH_EMPLOYEE_SYNC
3. Click "Create"
4. Title: Employee Sync for OSH Integration
5. Type: Executable program
6. Click "Save"
7. Package: $TMP (for local testing) or create custom package
8. Enter code (see ABAP templates document)
9. Save (Ctrl+S)
10. Activate (Ctrl+F3)
11. Execute (F8)
```

---

## Transport System

### What is a Transport?
A package that moves your custom objects from DEV → QAS → PROD

### Transport Request Structure
```
Transport Request: DEVK900123
  └── Task: DEVK900124 (your changes)
  └── Task: DEVK900125 (colleague's changes)
```

### Creating a Transport Request

#### Transaction: SE09 or SE10
```
1. Transaction: /nSE09
2. Click "Create" button
3. Type: Customizing Request (for config) 
        or Workbench Request (for code)
4. Description: "OSH Integration - Z-Tables and Programs"
5. Click "Save"
6. Note the transport number: DEVK900123
```

### Adding Objects to Transport
When you create/modify objects, SAP prompts:
```
"Object ZOSH_EMPLOYEE_DATA is new. Create object directory entry?"
→ Click "Yes"

"Prompt for workbench request?"
→ Click "Own Requests" button
→ Select your transport: DEVK900123
→ Click "Continue"
```

### Releasing Transport

#### Step 1: Release Your Task
```
Transaction: SE09
1. Display your request: DEVK900123
2. Expand to see tasks
3. Right-click your task (DEVK900124)
4. Select "Release"
5. Enter description of changes
6. Click "Save"
```

#### Step 2: Release the Request
```
1. Right-click main request (DEVK900123)
2. Select "Release"
3. Confirm
4. Transport is now ready for import to QAS
```

### Importing to QAS/PROD
This is done by SAP Basis team:
```
Transaction: STMS (Transport Management System)
1. Basis team imports your transport
2. You test in QAS
3. After approval, Basis imports to PROD
```

---

## Authorization Basics

### What are Authorization Objects?
Permission controls. Like Windows file permissions but for SAP transactions/tables.

### Common Authorization Objects
```
S_TABU_NAM = Table Authorization
  - Controls which tables you can view/edit
  
S_DEVELOP = Development Authorization
  - Controls if you can create programs/tables
  
S_RFC = Remote Function Call Authorization
  - Controls which RFCs can be executed
  
P_ORGIN = HR Master Data Authorization
  - Controls which HR data you can see
```

### Checking Your Authorization

#### Transaction: SU53
```
1. Try to do something (e.g., view table)
2. If denied, immediately run: /nSU53
3. Shows which authorization object failed
4. Send this info to security team to request access
```

### Viewing Authorization Profile

#### Transaction: SU01
```
1. Transaction: /nSU01
2. Enter your username
3. Click "Display"
4. Tab: "Profiles"
5. See assigned profiles (e.g., SAP_ALL = god mode)
```

---

## Essential Tips for Beginners

### 1. Always Know Your Environment
```
Check status bar:
- Bottom right: System ID (DEV, QAS, PRD)
- Don't accidentally work in production!
```

### 2. Use Multiple Sessions
```
Press /o to open new session
Keep SE16N open in one, SE38 in another
```

### 3. Never Modify Standard Objects
```
❌ Don't edit PA0001
✅ Create ZOSH_EMPLOYEE_DATA and copy data
```

### 4. Save Your Work in $TMP Package First
```
$TMP = Local objects (not transported)
Good for testing before creating transport
```

### 5. Document Everything
```
Add comments in ABAP code:
* Purpose: Sync employees to OSH system
* Author: Your Name
* Date: 2025-10-16
```

### 6. Test in DEV First!
```
DEV → Test → Fix → Test
Only then create transport to QAS
```

### 7. Use SAP Help
```
Press F1 on any field = Context help
Press F4 on any field = List of possible values (dropdown)
```

---

## Quick Reference Card

### Most Used Transaction Codes
```
SE11  = Dictionary (table structures)
SE16N = View table data
SE38  = ABAP Editor (programs)
SE09  = Transport Organizer
SM36  = Schedule jobs
SM37  = Monitor jobs
SU01  = User admin
SU53  = Authorization check
ST22  = Error logs
PA20  = Display employee
```

### Most Used Keyboard Shortcuts
```
F1    = Help
F4    = Search help (dropdown)
F8    = Execute
F3    = Back
Ctrl+S = Save
Ctrl+F3 = Activate
/n    = New transaction
/o    = New session
```

### Most Used Tables for HR
```
PA0001 = Org Assignment (department/station)
PA0002 = Personal Data (name)
PA0105 = Communication (email)
T001P  = Personnel areas
```

---

## Next Steps

After mastering these basics, proceed to:
1. **02_Environment_Strategy.md** - How to set up DEV/QAS/PROD
2. **04_Step_by_Step_Implementation.md** - Build your integration
3. **05_ABAP_Code_Templates.md** - Copy-paste working code

---

## Getting Help

### Internal Resources
1. SAP Basis team (system admin)
2. ABAP developers (coding help)
3. Security team (authorization issues)
4. Functional consultants (business process)

### External Resources
1. SAP Help Portal: https://help.sap.com
2. SAP Community: https://community.sap.com
3. YouTube: Search "SAP SE16N tutorial", "SAP ABAP basics"
4. Google: "SAP [transaction code] tutorial"

### When Stuck
1. Check ST22 for error details (Transaction: /nST22)
2. Run SU53 if authorization error
3. Ask your SAP team - they're there to help!
4. Document the error message exactly (screenshot)

---

**Remember**: SAP is complex, but you only need to learn a small subset for this integration. Focus on tables, programs, and transports - everything else can wait!
