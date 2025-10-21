# SAP Integration Troubleshooting Guide

## Quick Diagnosis Table

| Symptom | Likely Cause | Check | Fix |
|---------|--------------|-------|-----|
| Cannot login to SAP | Wrong credentials | Try with different user | Reset password |
| Transport import fails | Object conflicts | STMS import log | Delete conflicting object |
| Sync returns 0 records | No active employees | SE16N > PA0001 | Create test data |
| Authorization denied | Missing auth object | SU53 | Request authorization |
| RFC connection fails | Network/firewall | Ping SAP server | Check firewall rules |
| OData 401 error | Bad credentials | Test in browser | Verify username/password |
| Sync is slow | Too many records | ST05 SQL trace | Add batch processing |
| Job doesn't run | Job not scheduled | SM37 | Schedule job |
| Data not mapping | Missing mapping | ZOSH_ORG_MAPPING | Add mappings |
| C# connection timeout | Long-running query | Check SAP logs | Increase timeout |

---

## SAP-Side Issues

### Issue: Cannot Log In to SAP

**Symptoms:**
- Login screen rejects credentials
- "User locked" message
- "Password expired" message

**Diagnosis:**
1. Check if username is correct (case-sensitive)
2. Check if client is correct (usually 100, 300, or 400)
3. Check if password has expired
4. Check if account is locked

**Solutions:**
```
Transaction: SU01 (ask admin to check)
1. Display user
2. Check Lock Status
3. If locked → Click "Unlock"
4. If expired → Click "Reset Password"

Or contact: sap-security@company.com
```

---

### Issue: Authorization Denied

**Symptoms:**
- "No authorization for transaction SE16N"
- "No authorization for table PA0001"
- Any error message mentioning authorization

**Diagnosis:**
```
Transaction: /nSU53 (run immediately after error)
Shows:
- Which authorization object failed
- Which field values are required
- What you currently have
```

**Example Output:**
```
Authorization check failed:
Object: S_TABU_NAM
Field: TABLE = PA0001
Field: ACTVT = 03 (Display)
Missing authorization ❌
```

**Solution:**
```
1. Screenshot SU53 output
2. Email to: sap-security@company.com
3. Request: "Need read access to table PA0001"
4. Justification: "For OSH integration employee sync"
5. Security team adds authorization
6. Log out and back in
7. Try again
```

---

### Issue: Table Shows No Data

**Symptoms:**
- SE16N > PA0001 > Execute > 0 entries found
- Sync program outputs "No employees found"

**Diagnosis:**
```
Check which environment you're in:
- Status bar → System ID (DEV/QAS/PROD)
- DEV often has no/limited data
- QAS/PROD should have data
```

**Solutions:**

**In DEV:**
```
Create test data:
Transaction: SE38
Program: ZOSH_CREATE_TEST_DATA
Parameters: P_COUNT = 100
Execute (F8)
```

**In QAS/PROD:**
```
Check filters:
SE16N > PA0001
Remove all filters temporarily
Execute
If 0 records → Contact HR/SAP team (data issue)
```

---

### Issue: Transport Import Failed

**Symptoms:**
- STMS shows red status
- Import log has errors
- Objects not activated

**Diagnosis:**
```
Transaction: /nSTMS
1. Import Overview
2. Find your transport
3. Double-click → View log
4. Look for error messages
```

**Common Errors:**

**Error: "Object already exists"**
```
Cause: Object exists in target without transport
Fix: Ask Basis to import with overwrite flag
Or: Delete object manually in target first
```

**Error: "Activation error for object"**
```
Cause: Missing dependency (another table/program)
Fix:
1. Identify missing object in log
2. Add to transport in DEV
3. Re-release transport
4. Re-import to target
```

**Error: "Not enough authorization"**
```
Cause: Import user lacks authorization
Fix: Contact Basis team (their user needs auth)
```

---

### Issue: Program Syntax Error

**Symptoms:**
- Cannot activate program
- Red traffic light in SE38
- Error messages when checking syntax

**Diagnosis:**
```
Transaction: SE38
Program: ZOSH_EMPLOYEE_SYNC
Check (Ctrl+F2)
Read error messages
```

**Common Errors:**

**Error: "Field XXXXX is unknown"**
```
Cause: Typo in field name or table doesn't exist
Fix:
1. SE11 > Check table name
2. Verify field name exact (case-sensitive)
3. Correct spelling
4. Activate (Ctrl+F3)
```

**Error: "ENDSELECT without SELECT"**
```
Cause: Missing SELECT or extra ENDSELECT
Fix: Match each SELECT with ENDSELECT
```

**Error: "Type mismatch"**
```
Cause: Assigning wrong data type
Fix: Check variable types match
```

---

### Issue: Background Job Failed

**Symptoms:**
- SM37 shows job with red status "Cancelled"
- Job log has errors
- Sync didn't run

**Diagnosis:**
```
Transaction: /nSM37
Job name: ZOSH*
Execute
Double-click failed job
Click "Job Log" button
Read error message
```

**Common Causes:**

**Cause: Program dump (ST22)**
```
Fix:
1. Transaction: ST22
2. Find dump matching job time
3. Read error details
4. Fix program bug
5. Re-run job: SM37 > Job > Copy > Execute
```

**Cause: Authorization error**
```
Fix:
1. Job log shows "No authorization"
2. Note which table/object
3. Request auth for background user (not your user!)
4. Re-run job
```

**Cause: Database lock**
```
Fix:
1. Transaction: SM12
2. Check for locks on ZOSH_EMPLOYEE_DATA
3. Delete lock if stale
4. Re-run job
```

---

### Issue: Sync Is Very Slow

**Symptoms:**
- Sync program runs for 30+ minutes
- Job times out
- User reports slowness

**Diagnosis:**
```
Transaction: ST05 (SQL Trace)
1. Activate Trace
2. Run sync program
3. Deactivate Trace
4. Display Trace
5. Look for:
   - Table scans (FETCH >10000 rows)
   - Repeated identical queries
   - Missing indexes
```

**Solutions:**

**Add index to ZOSH_EMPLOYEE_DATA:**
```
Transaction: SE11
Table: ZOSH_EMPLOYEE_DATA
Menu > Indexes
Create index on: WERKS, ORGEH, ZSYNC_DATE
Activate
```

**Use batch processing:**
```
Modify program to process in chunks:
DATA: lv_batch_size TYPE i VALUE 1000.
SELECT ... UP TO lv_batch_size ROWS.
COMMIT WORK.
Loop and continue...
```

**Use incremental sync:**
```
Instead of full sync, only sync changed records
Use ZOSH_EMPLOYEE_SYNC_INCREMENTAL program
Check PA0001-AEDTM (last changed date)
```

---

## C# Application Issues

### Issue: Cannot Connect to SAP

**Symptoms:**
- Timeout exception
- Connection refused error
- 401 Unauthorized

**Diagnosis:**

**Test 1: Ping SAP server**
```powershell
ping sap.company.com
```
Expected: Replies (not timeout)

**Test 2: Test port**
```powershell
Test-NetConnection -ComputerName sap.company.com -Port 8000
```
Expected: TcpTestSucceeded : True

**Test 3: Test in browser**
```
Open: https://sap.company.com:8000/sap/opu/odata/sap/Z_OSH_EMPLOYEE_SRV/$metadata
Expected: XML metadata (not error page)
```

**Solutions:**

**If ping fails:**
```
- Check VPN connected
- Check network connectivity
- Contact network team
```

**If port blocked:**
```
- Check firewall rules
- Request port 8000 opened
- Contact network security team
```

**If 401 Unauthorized:**
```
- Verify username correct
- Verify password correct
- Test credentials in SAP GUI first
- Check if account locked (SU01)
```

---

### Issue: OData Returns Empty Result

**Symptoms:**
- HTTP 200 OK but 0 records
- JSON response has empty results array

**Diagnosis:**

**Check SAP data:**
```
Transaction: SE16N
Table: ZOSH_EMPLOYEE_DATA
Execute
If 0 records → Run sync program first (SE38)
```

**Check OData service:**
```
Browser test:
https://sap.company.com:8000/sap/opu/odata/sap/Z_OSH_EMPLOYEE_SRV/EmployeeSet?$format=json

Should return JSON with employees
If empty → Check SAP table has data
```

**Solutions:**

**If SAP table empty:**
```
1. Run ZOSH_EMPLOYEE_SYNC in SAP (SE38)
2. Wait for completion
3. Retry from C#
```

**If OData filter wrong:**
```
Check filter syntax:
Correct: $filter=Status eq '3'
Wrong: $filter=Status = 3
```

---

### Issue: RFC Call Fails

**Symptoms:**
- RfcException thrown
- "Function module not found"
- Connection timeout

**Diagnosis:**

**Test RFC in SAP:**
```
Transaction: SE37
Function: Z_OSH_GET_EMPLOYEES
Test/Execute (F8)
If works in SAP → C# configuration issue
If fails in SAP → RFC not created or not activated
```

**Solutions:**

**Function not found:**
```
1. SE37 > Z_OSH_GET_EMPLOYEES > Display
2. Attributes tab
3. Check: Processing Type = "Remote-Enabled Module"
4. If not → Change and activate
5. Retry from C#
```

**Connection parameters wrong:**
```csharp
// Verify in appsettings.json:
"Rfc": {
  "Host": "sap.company.com",        // ✓ Hostname not IP
  "SystemNumber": "00",              // ✓ Two digits
  "Client": "400",                   // ✓ Three digits
  "Username": "OSH_INTEGRATION",     // ✓ Uppercase
  "Password": "***",                 // ✓ Correct password
  "Language": "EN"                   // ✓ Two letters
}
```

---

### Issue: Data Mapping Errors

**Symptoms:**
- Exception: "No station mapping for plant code: FAC1"
- Employees imported but stations/departments NULL
- Foreign key constraint errors

**Diagnosis:**

**Check mappings in SAP:**
```
Transaction: SE16N
Table: ZOSH_ORG_MAPPING

Check:
- All active WERKS codes from PA0001 have STATION mapping
- All active ORGEH codes from PA0001 have DEPT mapping
```

**Find unmapped codes:**
```
Transaction: SE16N
Table: PA0001
WHERE ENDDA = 99991231

Note unique WERKS values (e.g., FAC1, FAC2, EST1)
Note unique ORGEH values (e.g., PROD01, HR01)

Compare to ZOSH_ORG_MAPPING
Add any missing mappings
```

**Solutions:**

**Add missing mapping:**
```
Transaction: SE16N
Table: ZOSH_ORG_MAPPING
Enable "Allow all functions"
Create Entries:

ZMAP_TYPE: STATION
ZSAP_CODE: FAC1
ZOSH_ID: 101
ZOSH_NAME: Nairobi Factory
ZACTIVE: X

Save
Retry sync
```

---

### Issue: Special Characters Corrupted

**Symptoms:**
- Names with ü, ñ, é show as ???
- Encoding errors in database
- Display issues in UI

**Diagnosis:**

**Check database encoding:**
```sql
-- In SQL Server:
SELECT DATABASEPROPERTYEX('OshManagement', 'Collation')
-- Should support UTF-8
```

**Check SAP encoding:**
```
Transaction: SE16N
Table: PA0002
View names with special characters
If correct in SAP → Encoding issue in transfer
```

**Solutions:**

**Fix C# encoding:**
```csharp
// In SapODataService.cs:
var content = await response.Content.ReadAsStringAsync();

// Should be:
var content = await response.Content.ReadAsStringAsync(Encoding.UTF8);
```

**Fix database column:**
```sql
ALTER TABLE Employees
ALTER COLUMN FirstName NVARCHAR(50) -- Use NVARCHAR not VARCHAR
```

---

## Performance Issues

### Issue: Sync Takes Too Long

**Symptoms:**
- Sync job runs for 30+ minutes
- Timeout exceptions
- Users report delays

**Diagnosis:**

**Measure sync time:**
```
1. Note start time
2. Run sync
3. Note end time
4. Calculate duration
```

**Identify bottleneck:**
```
SAP side:
- Transaction: ST05 (SQL trace)
- Check for slow queries

C# side:
- Add logging with timestamps
- Check which step is slow:
  - Fetching from SAP
  - Mapping data
  - Database insert/update
```

**Solutions:**

**Use batch processing:**
```csharp
var batchSize = 1000;
for (int i = 0; i < sapEmployees.Count; i += batchSize)
{
    var batch = sapEmployees.Skip(i).Take(batchSize);
    await ProcessBatchAsync(batch);
    await _context.SaveChangesAsync(); // Commit per batch
}
```

**Use bulk insert:**
```csharp
// Instead of individual Add():
_context.BulkInsert(employees); // Use EFCore.BulkExtensions
```

**Use incremental sync:**
```csharp
// Only sync changed employees:
var lastSync = await GetLastSyncDateAsync();
var changedEmployees = await _sapService.GetChangedEmployeesAsync(lastSync);
// Process only changed records
```

---

## Data Quality Issues

### Issue: Employee Count Mismatch

**Symptoms:**
- SAP has 5000 employees
- OSH has 4500 employees
- 500 employees missing

**Diagnosis:**

**Count in SAP:**
```
Transaction: SE16N
Table: ZOSH_EMPLOYEE_DATA
Execute
Note count (e.g., 5000)
```

**Count in OSH:**
```sql
SELECT COUNT(*) FROM Employees WHERE EmploymentStatus = 'Active'
-- Returns 4500
```

**Find missing:**
```sql
-- Find SAP employees not in OSH:
SELECT PERNR FROM ZOSH_EMPLOYEE_DATA
WHERE PERNR NOT IN (SELECT PayrollNo FROM Employees)
```

**Solutions:**

**Check for errors in sync log:**
```
Review C# application logs
Look for "Error processing employee" messages
Fix data quality issues causing failures
Re-run sync
```

**Check mapping coverage:**
```
Some employees may be in plants without mapping
Add missing mappings
Re-run sync
```

---

## Emergency Procedures

### Emergency: Production Sync Breaking OSH

**Immediate Actions:**
1. Disable sync in SAP
2. Stop Hangfire job
3. Assess damage
4. Plan fix

**Disable Sync:**
```
Transaction: SE16N
Table: ZOSH_CONFIG
Change OSH_SYNC_ENABLED to FALSE
Save
```

**Stop Hangfire Job:**
```csharp
// In OSH application:
RecurringJob.RemoveIfExists("sap-employee-sync");
```

**Or in appsettings.json:**
```json
"SapIntegration": {
  "Enabled": false
}
```

**Restart application to apply**

---

### Emergency: Data Corruption

**Symptoms:**
- Wrong employees in wrong stations
- Data overwritten incorrectly

**Recovery:**

**Restore from backup:**
```sql
-- Restore Employees table from last good backup
RESTORE DATABASE OshManagement 
FROM DISK = 'E:\Backups\OshManagement_20250116.bak'
WITH REPLACE
```

**Or manual fix:**
```sql
-- If specific data corrupted, fix manually:
UPDATE Employees 
SET StationId = 101, DepartmentId = 201
WHERE PayrollNo IN ('00012345', '00012346')
```

---

## Getting Help

### Internal Escalation

**Level 1: Check Logs**
- SAP: ST22 (dumps), SM21 (system log), SM37 (job log)
- C#: Application logs, Hangfire dashboard

**Level 2: Your Team**
- SAP ABAP Developer (for program issues)
- C# Developer (for application issues)
- DBA (for database issues)

**Level 3: SAP Basis Team**
- Transport issues
- Authorization issues
- System performance

**Level 4: Vendor Support**
- SAP Support (for SAP bugs)
- Microsoft Support (for .NET issues)

### External Resources

**SAP Community:**
- https://community.sap.com
- Search for error messages
- Ask questions in forums

**Stack Overflow:**
- Tag: [sap] [odata] [rfc]
- Many SAP .NET connector questions answered

**Documentation:**
- SAP Help Portal: https://help.sap.com
- SAP .NET Connector Guide
- OData Protocol Specification

---

## Preventive Measures

### Avoid Issues Before They Happen

**1. Monitor Proactively:**
- Set up alerts for sync failures
- Monitor sync duration trends
- Track error rates

**2. Test Thoroughly:**
- Test in DEV before QAS
- Test in QAS before PROD
- Test with real data volumes

**3. Document Everything:**
- Keep runbook updated
- Document all customizations
- Note all mappings

**4. Regular Maintenance:**
- Review error logs weekly
- Update mappings when org changes
- Optimize slow queries
- Archive old sync logs

**5. Communication:**
- Alert users before changes
- Coordinate with HR on data changes
- Keep stakeholders informed

---

**Remember:** Most issues are caused by:
1. Authorization (40%)
2. Data quality (30%)
3. Configuration (20%)
4. Network/connectivity (10%)

Always check the simple things first!
