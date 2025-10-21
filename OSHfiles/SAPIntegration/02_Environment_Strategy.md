# SAP Environment Strategy - DEV/QAS/PROD Setup

## Table of Contents
1. [Environment Overview](#environment-overview)
2. [The DEV Data Problem](#the-dev-data-problem)
3. [Test Data Strategy](#test-data-strategy)
4. [Environment-Specific Configuration](#environment-specific-configuration)
5. [Promotion Path](#promotion-path)
6. [Rollback Strategy](#rollback-strategy)

---

## Environment Overview

### Your Three Environments

```
┌─────────────────────────────────────────────────────────┐
│                    DEVELOPMENT (DEV)                    │
│  Client: 100 (typical)                                  │
│  Purpose: Build and test custom objects                │
│  Data: Synthetic/minimal test data                     │
│  Users: Developers only                                 │
│  Changes: Frequent, unrestricted                        │
│  Risk: Low (can break without impact)                  │
└────────────────────┬────────────────────────────────────┘
                     │
                     │ Transport: DEVK900123
                     │
┌────────────────────▼────────────────────────────────────┐
│              QUALITY ASSURANCE (QAS)                    │
│  Client: 300 (typical)                                  │
│  Purpose: Integration testing with real data           │
│  Data: Copy of production structure + sample data      │
│  Users: Testers, key users, developers                 │
│  Changes: Controlled via transports only               │
│  Risk: Medium (testing ground)                          │
└────────────────────┬────────────────────────────────────┘
                     │
                     │ Transport: DEVK900123 (same)
                     │ + Business sign-off
                     │
┌────────────────────▼────────────────────────────────────┐
│                  PRODUCTION (PROD/PRD)                  │
│  Client: 400 (typical)                                  │
│  Purpose: Live business operations                      │
│  Data: Real employee/organizational data                │
│  Users: All end users                                   │
│  Changes: Strictly controlled, scheduled                │
│  Risk: HIGH (any issue affects business)                │
└─────────────────────────────────────────────────────────┘
```

### Key Principles
1. **Always start in DEV** - Never create objects in QAS or PROD
2. **Test thoroughly in DEV** before transporting
3. **QAS is your safety net** - Catch issues before production
4. **PROD is sacred** - Only import tested, approved transports

---

## The DEV Data Problem

### Current Situation
Your DEV environment lacks organizational structure (stations, departments, employees).

### Why This Happens
- DEV is often refreshed from empty client
- Real data is in QAS/PROD only
- Privacy concerns prevent copying real employee data to DEV

### Impact on Development
```
Without Data:
❌ Can't test table queries
❌ Can't validate sync program logic
❌ Can't test OData/RFC responses
❌ Can't test data mapping
❌ Integration development is blind
```

---

## Test Data Strategy

### Option 1: Create Synthetic Test Data (RECOMMENDED)

#### Why This is Best
✅ **Privacy-compliant** - No real employee data in DEV  
✅ **Lightweight** - Only what you need  
✅ **Controlled** - You design the test scenarios  
✅ **Fast** - No waiting for Basis team  
✅ **Repeatable** - Can reset anytime  

#### What to Create
```
Minimal Realistic Test Set:
- 3 Organization Categories (Factories, Estates, Head Office)
- 5 Stations/Plants
- 15 Departments
- 100 Employees (covering all scenarios)
```

#### How to Create

##### Method 1: Manual Entry via PA30 (Slow but Simple)
```
Transaction: /nPA30
1. Create employee record
2. Fill infotypes:
   - 0001: Organizational Assignment
   - 0002: Personal Data
   - 0105: Communication
3. Save
4. Repeat for 100 employees (tedious!)
```

##### Method 2: ABAP Program (Fast - RECOMMENDED)
```abap
REPORT ZOSH_CREATE_TEST_DATA.
* Creates synthetic test data for OSH integration

* Create test employees
DATA: lt_pa0001 TYPE TABLE OF pa0001,
      lt_pa0002 TYPE TABLE OF pa0002.

* Employee 1: Factory worker
APPEND VALUE #(
  pernr = '00090001'
  begda = sy-datum
  endda = '99991231'
  bukrs = '1000'     " Company code
  werks = 'FAC1'     " Factory 1
  orgeh = 'PROD01'   " Production dept
  stell = 'WORKER'   " Position
  stat2 = '3'        " Active
) TO lt_pa0001.

APPEND VALUE #(
  pernr = '00090001'
  begda = sy-datum
  endda = '99991231'
  vorna = 'John'
  nachn = 'Doe'
  gbdat = '19850615'
) TO lt_pa0002.

* Insert test data
INSERT pa0001 FROM TABLE lt_pa0001.
INSERT pa0002 FROM TABLE lt_pa0002.

COMMIT WORK.
WRITE: / 'Test data created successfully'.
```

##### Method 3: Use LSMW Tool (Medium Complexity)
```
Transaction: /nLSMW
1. Create project: ZOSH_TEST_DATA
2. Import from Excel file with test employees
3. Map to infotypes 0001, 0002
4. Execute load
```

#### Recommended Test Data Structure
```
┌────────────────────────────────────────────────────────┐
│ ORGANIZATION CATEGORIES                                │
├──────────┬─────────────────────────────────────────────┤
│ Code     │ Name                                        │
├──────────┼─────────────────────────────────────────────┤
│ FAC      │ Factories                                   │
│ EST      │ Estates                                     │
│ HO       │ Head Office                                 │
└──────────┴─────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────┐
│ STATIONS/PLANTS                                        │
├──────────┬─────────────────────┬───────────────────────┤
│ Code     │ Name                │ Category              │
├──────────┼─────────────────────┼───────────────────────┤
│ FAC1     │ Nairobi Factory     │ FAC                   │
│ FAC2     │ Mombasa Factory     │ FAC                   │
│ EST1     │ Kiambu Estate       │ EST                   │
│ EST2     │ Nyeri Estate        │ EST                   │
│ HO01     │ Head Office         │ HO                    │
└──────────┴─────────────────────┴───────────────────────┘

┌────────────────────────────────────────────────────────┐
│ DEPARTMENTS                                            │
├──────────┬─────────────────────┬───────────────────────┤
│ Code     │ Name                │ Station               │
├──────────┼─────────────────────┼───────────────────────┤
│ PROD01   │ Production          │ FAC1                  │
│ PROD02   │ Production          │ FAC2                  │
│ HR01     │ Human Resources     │ FAC1                  │
│ HR02     │ Human Resources     │ HO01                  │
│ SAFE01   │ Safety & OSH        │ FAC1                  │
│ SAFE02   │ Safety & OSH        │ FAC2                  │
│ MAINT01  │ Maintenance         │ FAC1                  │
│ FIELD01  │ Field Operations    │ EST1                  │
│ FIELD02  │ Field Operations    │ EST2                  │
└──────────┴─────────────────────┴───────────────────────┘

┌────────────────────────────────────────────────────────┐
│ EMPLOYEES (Sample Structure)                           │
├──────────┬─────────────┬───────────┬───────────────────┤
│ PayrollNo│ Name        │ Station   │ Department        │
├──────────┼─────────────┼───────────┼───────────────────┤
│ 00090001 │ John Doe    │ FAC1      │ PROD01            │
│ 00090002 │ Jane Smith  │ FAC1      │ HR01              │
│ 00090003 │ Mike Johnson│ FAC1      │ SAFE01            │
│ 00090004 │ Alice Brown │ FAC2      │ PROD02            │
│ ...      │ ...         │ ...       │ ...               │
│ 00090100 │ Test User100│ EST2      │ FIELD02           │
└──────────┴─────────────┴───────────┴───────────────────┘

Employee Types:
- 20 Factory Workers (PROD01, PROD02)
- 20 Estate Workers (FIELD01, FIELD02)
- 10 HR Staff (HR01, HR02)
- 10 Safety Officers (SAFE01, SAFE02)
- 10 Maintenance (MAINT01)
- 10 Supervisors (various)
- 10 Managers (various)
- 10 Admin/Support (HO01)
```

---

### Option 2: Copy Subset from QAS

#### Pros
✅ Real organizational structure  
✅ Realistic data relationships  
✅ No data creation effort  

#### Cons
❌ Requires Basis team help  
❌ Privacy concerns (real employee names)  
❌ Larger data volume  
❌ Takes time to coordinate  

#### How to Do It
```
Request to SAP Basis Team:
"Please perform client copy from QAS to DEV with following:
- Organizational units (T001P, T527X)
- Sample employees from Plant FAC1 only (50 records)
- Anonymize personal data (names, emails)
- Date range: Active employees only
- Transaction: SCC9 or SCC3"

Alternative: Use table extracts
1. Export from QAS: Transaction SE16N → Download to Excel
2. Upload to DEV: LSMW or custom program
```

---

### Option 3: Hybrid Approach (RECOMMENDED)

#### Strategy
1. **Create minimal synthetic org structure** in DEV (manual)
2. **Test all code logic** with synthetic data
3. **Do final integration testing** in QAS with real data

#### Benefits
✅ Fast to set up  
✅ Privacy-compliant  
✅ Real data validation in QAS  
✅ Best of both worlds  

#### Implementation
```
Week 1: DEV Setup
- Create 5 stations manually (via customizing)
- Create 15 departments manually
- Generate 100 test employees via ABAP program
- Build Z-table, sync program, RFC
- Test thoroughly in DEV

Week 2: QAS Testing
- Transport to QAS
- Test with real organizational data
- Validate data mappings
- Fix any issues in DEV, re-transport

Week 3: Production
- After QAS sign-off, transport to PROD
```

---

## Environment-Specific Configuration

### Configuration Tables

#### Create: ZOSH_CONFIG (Custom Config Table)
```
Transaction: SE11
Table: ZOSH_CONFIG

Fields:
┌──────────────┬──────────┬────────────────────────────┐
│ Field        │ Type     │ Description                │
├──────────────┼──────────┼────────────────────────────┤
│ MANDT        │ CLNT(3)  │ Client                     │
│ CONFIG_KEY   │ CHAR(30) │ Configuration Key          │
│ CONFIG_VALUE │ CHAR(255)│ Configuration Value        │
│ ENVIRONMENT  │ CHAR(10) │ DEV/QAS/PROD               │
│ DESCRIPTION  │ CHAR(100)│ Description                │
└──────────────┴──────────┴────────────────────────────┘

Example Data:
CONFIG_KEY          CONFIG_VALUE                 ENVIRONMENT
─────────────────────────────────────────────────────────────
OSH_SYNC_ENABLED    'TRUE'                       DEV
OSH_SYNC_ENABLED    'TRUE'                       QAS
OSH_SYNC_ENABLED    'FALSE'                      PROD
OSH_TARGET_URL      'https://dev.osh.local'      DEV
OSH_TARGET_URL      'https://qas.osh.local'      QAS
OSH_TARGET_URL      'https://prod.osh.local'     PROD
OSH_SYNC_SCOPE      'SAMPLE_DATA'                DEV
OSH_SYNC_SCOPE      'ALL_ACTIVE'                 QAS
OSH_SYNC_SCOPE      'ALL_ACTIVE'                 PROD
```

### Using Configuration in Code
```abap
REPORT ZOSH_EMPLOYEE_SYNC.

DATA: lv_environment TYPE char10,
      lv_sync_enabled TYPE char5,
      lv_sync_scope TYPE char20.

* Determine current environment
CALL FUNCTION 'OWN_LOGICAL_SYSTEM_GET'
  IMPORTING
    own_logical_system = lv_environment.

* Get configuration
SELECT SINGLE config_value FROM zosh_config
  INTO lv_sync_enabled
  WHERE config_key = 'OSH_SYNC_ENABLED'
    AND environment = lv_environment.

IF lv_sync_enabled <> 'TRUE'.
  WRITE: / 'Sync disabled in this environment'.
  EXIT.
ENDIF.

* Continue with sync...
```

---

## Promotion Path

### Standard Flow: DEV → QAS → PROD

#### Phase 1: Development (DEV)
```
Week 1-2: Build Phase
├── Day 1-2: Create Z-tables
│   └── ZOSH_EMPLOYEE_DATA
│   └── ZOSH_ORG_MAPPING
│   └── ZOSH_CONFIG
├── Day 3-5: Create sync program
│   └── ZOSH_EMPLOYEE_SYNC
│   └── Test with synthetic data
├── Day 6-7: Create RFC/OData service
│   └── Z_OSH_GET_EMPLOYEES
│   └── Z_OSH_GET_ORG_MAPPING
├── Day 8-10: Integration testing
│   └── Test C# app → DEV SAP connection
│   └── Validate data mapping
│   └── Fix bugs, iterate
└── Transport Creation
    └── Create transport request
    └── Add all objects to transport
    └── Release transport
```

#### Phase 2: Quality Assurance (QAS)
```
Week 3: QAS Testing
├── Day 1: Import transport
│   └── Basis team imports DEVK900123
│   └── Verify all objects activated
├── Day 2-3: Data validation
│   └── Run sync program manually
│   └── Check Z-table populated correctly
│   └── Verify org mapping accuracy
├── Day 4-5: Integration testing
│   └── Point C# app to QAS
│   └── Test full end-to-end sync
│   └── User acceptance testing
└── Sign-off
    └── Get approval from:
        - OSH System Owner
        - HR Department
        - IT Security
        - SAP Basis Team
```

#### Phase 3: Production (PROD)
```
Week 4: Production Deployment
├── Pre-deployment
│   └── Schedule downtime window (if needed)
│   └── Notify users
│   └── Prepare rollback plan
├── Deployment (e.g., Friday 10 PM)
│   └── Basis imports transport
│   └── Validate objects activated
│   └── Run smoke tests
├── Post-deployment
│   └── Schedule background job
│   └── Monitor first sync execution
│   └── Validate data in OSH system
└── Go-live support
    └── Week 1: Daily monitoring
    └── Week 2-4: Weekly check-ins
```

---

## Transport Checklist

### Pre-Transport (In DEV)
```
□ All objects tested individually
□ Integration test successful (C# → DEV SAP)
□ No syntax errors (SE38 → Check → Extended Check)
□ No authorization issues (tested with restricted user)
□ Documentation updated
□ Transport description is clear
□ All related objects in same transport:
  □ Z-tables
  □ Z-programs
  □ Z-function modules
  □ Z-structures/types
  □ Configuration entries
□ Task released
□ Transport request released
```

### Post-Transport to QAS
```
□ Import successful (no errors in STMS)
□ All objects activated (green lights in SE80)
□ Table structures match (SE11)
□ Programs execute without error (SE38 → F8)
□ RFC callable (SE37 → Test/Execute)
□ Data sync produces expected results
□ C# integration test successful
□ Performance acceptable
□ No new authorization issues
□ User acceptance testing passed
□ Sign-off received
```

### Post-Transport to PROD
```
□ Import successful
□ All objects activated
□ Background job scheduled correctly
□ First sync execution successful
□ Data visible in OSH system
□ No errors in ST22 (runtime errors)
□ No errors in SM37 (job log)
□ Performance within SLA
□ Monitoring alerts configured
□ Documentation updated for production
□ Runbook created for support team
```

---

## Rollback Strategy

### When to Rollback
- Critical bug discovered in PROD
- Data corruption detected
- Performance unacceptable
- Integration causes SAP system issues

### Rollback Methods

#### Method 1: Deactivate Objects (Fastest)
```
Transaction: SE80
1. Navigate to object (e.g., ZOSH_EMPLOYEE_SYNC)
2. Right-click → Inactive Version → Restore
3. Deactivates current version
4. Stops program from running
```

#### Method 2: Delete Transport (Nuclear Option)
```
Contact Basis Team:
"Please reverse transport DEVK900123 from PROD"

They will use STMS to import a deletion transport
This removes all objects added by the transport
```

#### Method 3: Disable via Configuration (Safest)
```
Transaction: SE16N
Table: ZOSH_CONFIG
1. Find record: CONFIG_KEY = 'OSH_SYNC_ENABLED'
2. Change CONFIG_VALUE to 'FALSE'
3. Sync program checks this and exits immediately
4. No code changes needed
5. Can re-enable when fixed
```

#### Method 4: Deschedule Background Job
```
Transaction: SM37
1. Find job: ZOSH_EMPLOYEE_SYNC
2. Right-click → Cancel
3. Job stops running
4. Fix issue in DEV
5. Re-transport when ready
```

### Rollback Decision Tree
```
Is PROD broken?
├── YES → Immediate action required
│   ├── Is sync running now?
│   │   ├── YES → SM37: Cancel job
│   │   └── NO → Continue
│   ├── Is data corrupted?
│   │   ├── YES → Contact DBA for restore
│   │   └── NO → Continue
│   └── Disable sync:
│       └── ZOSH_CONFIG: Set ENABLED = FALSE
├── NO → Non-critical issue
    └── Can wait for next release cycle
        └── Fix in DEV → Transport to QAS → Test → PROD
```

---

## Environment Comparison Matrix

```
┌──────────────────┬─────────────┬─────────────┬─────────────┐
│ Aspect           │ DEV         │ QAS         │ PROD        │
├──────────────────┼─────────────┼─────────────┼─────────────┤
│ Data Volume      │ 100 records │ 1,000       │ 10,000+     │
│ Data Realism     │ Synthetic   │ Real copy   │ Live        │
│ Change Control   │ None        │ Transports  │ Strict      │
│ Testing          │ Unit tests  │ UAT         │ Smoke only  │
│ Downtime OK?     │ Always      │ Yes         │ Scheduled   │
│ Backup Needed?   │ No          │ Optional    │ Always      │
│ Monitoring       │ Optional    │ Recommended │ Required    │
│ Access           │ Developers  │ Testers     │ End users   │
│ Sync Frequency   │ On-demand   │ Hourly      │ Every 6hrs  │
└──────────────────┴─────────────┴─────────────┴─────────────┘
```

---

## Next Steps

1. **Complete DEV setup** (see 04_Step_by_Step_Implementation.md)
2. **Create test data** (use templates in 05_ABAP_Code_Templates.md)
3. **Build integration** (see 08_CSharp_Integration_Code.md)
4. **Test thoroughly** (use 07_Testing_Checklist.md)
5. **Transport to QAS**
6. **After approval, deploy to PROD**

---

## Key Takeaways

✅ **Use synthetic test data in DEV** - fastest, privacy-compliant  
✅ **Test thoroughly in DEV** - catch issues early  
✅ **QAS validates with real data** - final safety check  
✅ **Always have rollback plan** - for production  
✅ **Configuration-driven** - enable/disable without code changes  
✅ **Document everything** - for future maintenance  

**Remember**: The journey from DEV to PROD should be deliberate and well-tested. Rushing leads to production issues!
