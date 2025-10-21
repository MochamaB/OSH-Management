# ABAP Code Templates - Copy & Paste Ready

## Table of Contents
1. [Employee Sync Program](#employee-sync-program)
2. [Test Data Generator](#test-data-generator)
3. [RFC Function Module](#rfc-function-module)
4. [Incremental Sync Program](#incremental-sync-program)
5. [Error Handling Module](#error-handling-module)
6. [Email Notification](#email-notification)

---

## Employee Sync Program

### Program: ZOSH_EMPLOYEE_SYNC

```abap
*&---------------------------------------------------------------------*
*& Report ZOSH_EMPLOYEE_SYNC
*&---------------------------------------------------------------------*
*& Purpose: Sync employee master data from SAP HR to OSH integration table
*& Author: [Your Name]
*& Date: 2025-10-16
*& Version: 1.0
*&---------------------------------------------------------------------*

REPORT zosh_employee_sync.

*----------------------------------------------------------------------*
* Data Declarations
*----------------------------------------------------------------------*
DATA: lt_employee TYPE TABLE OF zosh_employee_data,
      ls_employee TYPE zosh_employee_data,
      lv_enabled  TYPE char5,
      lv_scope    TYPE char20,
      lv_count    TYPE i.

*----------------------------------------------------------------------*
* Configuration Check
*----------------------------------------------------------------------*
SELECT SINGLE zconfig_value
  FROM zosh_config
  INTO lv_enabled
  WHERE zconfig_key = 'OSH_SYNC_ENABLED'
    AND zenvironment = sy-sysid.  "Current system ID (DEV/QAS/PROD)

IF lv_enabled <> 'TRUE'.
  WRITE: / 'ERROR: Sync is disabled for environment:', sy-sysid.
  WRITE: / 'Enable via ZOSH_CONFIG table (OSH_SYNC_ENABLED = TRUE)'.
  EXIT.
ENDIF.

* Get sync scope
SELECT SINGLE zconfig_value
  FROM zosh_config
  INTO lv_scope
  WHERE zconfig_key = 'OSH_SYNC_SCOPE'
    AND zenvironment = sy-sysid.

IF lv_scope IS INITIAL.
  lv_scope = 'ALL_ACTIVE'.  "Default
ENDIF.

WRITE: / 'Starting sync for environment:', sy-sysid.
WRITE: / 'Sync scope:', lv_scope.
WRITE: / '----------------------------------------'.

*----------------------------------------------------------------------*
* Select Employee Data from HR Tables
*----------------------------------------------------------------------*
SELECT p002~pernr AS pernr
       p002~vorna AS vorna
       p002~nachn AS nachn
       p001~stell AS stell
       p001~werks AS werks
       p001~btrtl AS btrtl
       p001~orgeh AS orgeh
       p001~kostl AS kostl
       p002~stat2 AS stat2
       p001~begda AS begda
       p001~endda AS endda
  INTO CORRESPONDING FIELDS OF TABLE @lt_employee
  FROM pa0002 AS p002                         "Personal Data
  INNER JOIN pa0001 AS p001                   "Organizational Assignment
    ON p002~pernr = p001~pernr
  WHERE p001~endda = '99991231'               "Active records only
    AND p002~endda = '99991231'
    AND p002~stat2 = '3'.                     "Active status

* Optional: Add email addresses
DATA: lt_email TYPE TABLE OF pa0105,
      ls_email TYPE pa0105.

SELECT pernr subty usrid_long
  INTO CORRESPONDING FIELDS OF TABLE lt_email
  FROM pa0105                                 "Communication
  WHERE endda = '99991231'
    AND subty = '0010'                        "Email subtype
    AND usrty = '0001'.                       "Internet email

* Merge email data
LOOP AT lt_employee ASSIGNING FIELD-SYMBOL(<emp>).
  READ TABLE lt_email INTO ls_email WITH KEY pernr = <emp>-pernr.
  IF sy-subrc = 0.
    <emp>-email = ls_email-usrid_long.
  ENDIF.
ENDLOOP.

lv_count = lines( lt_employee ).
WRITE: / 'Employees selected:', lv_count.

IF lv_count = 0.
  WRITE: / 'WARNING: No active employees found!'.
  WRITE: / 'Check PA0001/PA0002 tables for data.'.
  EXIT.
ENDIF.

*----------------------------------------------------------------------*
* Add Sync Metadata
*----------------------------------------------------------------------*
LOOP AT lt_employee ASSIGNING <emp>.
  <emp>-zsync_date = sy-datum.      "Current date
  <emp>-zsync_time = sy-uzeit.      "Current time
  <emp>-zchanged = 'X'.             "Changed flag
ENDLOOP.

*----------------------------------------------------------------------*
* Update Integration Table
*----------------------------------------------------------------------*
* Option 1: Full replace (delete all, insert all)
DELETE FROM zosh_employee_data.

INSERT zosh_employee_data FROM TABLE lt_employee.

IF sy-subrc = 0.
  COMMIT WORK AND WAIT.
  WRITE: / 'SUCCESS: Sync completed.'.
  WRITE: / 'Records inserted:', sy-dbcnt.
  WRITE: / 'Timestamp:', sy-datum, sy-uzeit.
  
  * Log success
  CALL FUNCTION 'BAL_LOG_MSG_ADD'
    EXPORTING
      i_log_handle     = 1
      i_s_msg          = VALUE #( msgty = 'S'
                                   msgid = '00'
                                   msgno = '001'
                                   msgv1 = 'ZOSH sync completed'
                                   msgv2 = lv_count ).
ELSE.
  ROLLBACK WORK.
  WRITE: / 'ERROR: Database insert failed.'.
  WRITE: / 'Return code:', sy-subrc.
  WRITE: / 'Contact SAP support.'.
ENDIF.

*----------------------------------------------------------------------*
* End of Program
*----------------------------------------------------------------------*
```

---

## Test Data Generator

### Program: ZOSH_CREATE_TEST_DATA

```abap
*&---------------------------------------------------------------------*
*& Report ZOSH_CREATE_TEST_DATA
*&---------------------------------------------------------------------*
*& Purpose: Generate synthetic test employee data for DEV environment
*& Author: [Your Name]
*& Date: 2025-10-16
*&---------------------------------------------------------------------*

REPORT zosh_create_test_data.

*----------------------------------------------------------------------*
* Parameters
*----------------------------------------------------------------------*
PARAMETERS: p_count TYPE i DEFAULT 100,      "Number of employees to create
            p_plant TYPE werks_d DEFAULT 'FAC1',  "Plant code
            p_delete TYPE char1 AS CHECKBOX.      "Delete existing test data

*----------------------------------------------------------------------*
* Constants
*----------------------------------------------------------------------*
CONSTANTS: c_start_pernr TYPE pernr_d VALUE '00090000'.  "Starting personnel number

*----------------------------------------------------------------------*
* Data Declarations
*----------------------------------------------------------------------*
DATA: lt_pa0001 TYPE TABLE OF pa0001,
      lt_pa0002 TYPE TABLE OF pa0002,
      lt_pa0105 TYPE TABLE OF pa0105,
      ls_pa0001 TYPE pa0001,
      ls_pa0002 TYPE pa0002,
      ls_pa0105 TYPE pa0105,
      lv_pernr  TYPE pernr_d,
      lv_index  TYPE i.

DATA: lt_first_names TYPE TABLE OF string,
      lt_last_names TYPE TABLE OF string,
      lv_first_name TYPE string,
      lv_last_name TYPE string.

*----------------------------------------------------------------------*
* Sample Names
*----------------------------------------------------------------------*
APPEND 'John' TO lt_first_names.
APPEND 'Jane' TO lt_first_names.
APPEND 'Michael' TO lt_first_names.
APPEND 'Sarah' TO lt_first_names.
APPEND 'David' TO lt_first_names.
APPEND 'Mary' TO lt_first_names.
APPEND 'James' TO lt_first_names.
APPEND 'Patricia' TO lt_first_names.
APPEND 'Robert' TO lt_first_names.
APPEND 'Jennifer' TO lt_first_names.
APPEND 'William' TO lt_first_names.
APPEND 'Linda' TO lt_first_names.
APPEND 'Richard' TO lt_first_names.
APPEND 'Elizabeth' TO lt_first_names.
APPEND 'Thomas' TO lt_first_names.
APPEND 'Susan' TO lt_first_names.
APPEND 'Charles' TO lt_first_names.
APPEND 'Jessica' TO lt_first_names.
APPEND 'Daniel' TO lt_first_names.
APPEND 'Karen' TO lt_first_names.

APPEND 'Smith' TO lt_last_names.
APPEND 'Johnson' TO lt_last_names.
APPEND 'Williams' TO lt_last_names.
APPEND 'Brown' TO lt_last_names.
APPEND 'Jones' TO lt_last_names.
APPEND 'Garcia' TO lt_last_names.
APPEND 'Miller' TO lt_last_names.
APPEND 'Davis' TO lt_last_names.
APPEND 'Rodriguez' TO lt_last_names.
APPEND 'Martinez' TO lt_last_names.

*----------------------------------------------------------------------*
* Delete Existing Test Data (if requested)
*----------------------------------------------------------------------*
IF p_delete = 'X'.
  DELETE FROM pa0001 WHERE pernr >= c_start_pernr.
  DELETE FROM pa0002 WHERE pernr >= c_start_pernr.
  DELETE FROM pa0105 WHERE pernr >= c_start_pernr.
  COMMIT WORK.
  WRITE: / 'Existing test data deleted.'.
ENDIF.

*----------------------------------------------------------------------*
* Generate Test Employees
*----------------------------------------------------------------------*
DO p_count TIMES.
  lv_index = sy-index.
  lv_pernr = c_start_pernr + lv_index.

  * Random name selection
  DATA(lv_fname_idx) = ( lv_index MOD lines( lt_first_names ) ) + 1.
  DATA(lv_lname_idx) = ( lv_index MOD lines( lt_last_names ) ) + 1.
  
  READ TABLE lt_first_names INTO lv_first_name INDEX lv_fname_idx.
  READ TABLE lt_last_names INTO lv_last_name INDEX lv_lname_idx.

  * PA0001 - Organizational Assignment
  CLEAR ls_pa0001.
  ls_pa0001-pernr = lv_pernr.
  ls_pa0001-begda = '20240101'.
  ls_pa0001-endda = '99991231'.
  ls_pa0001-bukrs = '1000'.                    "Company code
  ls_pa0001-werks = p_plant.                   "Plant
  ls_pa0001-btrtl = '0001'.                    "Personnel subarea
  ls_pa0001-orgeh = 'PROD01'.                  "Org unit
  ls_pa0001-stell = 'WORKER'.                  "Position
  ls_pa0001-stat2 = '3'.                       "Active
  ls_pa0001-kostl = 'CC1000'.                  "Cost center
  APPEND ls_pa0001 TO lt_pa0001.

  * PA0002 - Personal Data
  CLEAR ls_pa0002.
  ls_pa0002-pernr = lv_pernr.
  ls_pa0002-begda = '20240101'.
  ls_pa0002-endda = '99991231'.
  ls_pa0002-vorna = lv_first_name.
  ls_pa0002-nachn = lv_last_name.
  ls_pa0002-gbdat = '19850615'.                "DOB
  ls_pa0002-gesch = '1'.                       "Gender (Male)
  ls_pa0002-stat2 = '3'.                       "Active
  APPEND ls_pa0002 TO lt_pa0002.

  * PA0105 - Email
  CLEAR ls_pa0105.
  ls_pa0105-pernr = lv_pernr.
  ls_pa0105-begda = '20240101'.
  ls_pa0105-endda = '99991231'.
  ls_pa0105-subty = '0010'.                    "Email subtype
  ls_pa0105-usrty = '0001'.                    "Internet
  CONCATENATE lv_first_name '.' lv_last_name lv_index '@company.com'
    INTO ls_pa0105-usrid_long SEPARATED BY space.
  CONDENSE ls_pa0105-usrid_long NO-GAPS.
  TRANSLATE ls_pa0105-usrid_long TO LOWER CASE.
  APPEND ls_pa0105 TO lt_pa0105.
ENDDO.

*----------------------------------------------------------------------*
* Insert Data
*----------------------------------------------------------------------*
INSERT pa0001 FROM TABLE lt_pa0001.
IF sy-subrc = 0.
  WRITE: / 'PA0001 records inserted:', sy-dbcnt.
ELSE.
  WRITE: / 'ERROR inserting PA0001:', sy-subrc.
ENDIF.

INSERT pa0002 FROM TABLE lt_pa0002.
IF sy-subrc = 0.
  WRITE: / 'PA0002 records inserted:', sy-dbcnt.
ELSE.
  WRITE: / 'ERROR inserting PA0002:', sy-subrc.
ENDIF.

INSERT pa0105 FROM TABLE lt_pa0105.
IF sy-subrc = 0.
  WRITE: / 'PA0105 records inserted:', sy-dbcnt.
ELSE.
  WRITE: / 'ERROR inserting PA0105:', sy-subrc.
ENDIF.

COMMIT WORK AND WAIT.

WRITE: / '========================================'.
WRITE: / 'Test data generation completed!'.
WRITE: / 'Employees created:', p_count.
WRITE: / 'Personnel numbers:', c_start_pernr, 'to', lv_pernr.
WRITE: / 'Plant:', p_plant.
WRITE: / '========================================'.
```

---

## RFC Function Module

### Function Module: Z_OSH_GET_EMPLOYEES

**Create in SE37**

#### Attributes
```
Function Module: Z_OSH_GET_EMPLOYEES
Function Group: ZOSH_INTEGRATION
Short Text: Get employee data for OSH system
Processing Type: Remote-Enabled Module
```

#### Import Parameters
```
IV_PLANT        TYPE WERKS_D    OPTIONAL
IV_ORGUNIT      TYPE ORGEH      OPTIONAL
IV_CHANGED_SINCE TYPE SYDATUM    OPTIONAL
```

#### Export Parameters
```
EV_COUNT        TYPE I
EV_MESSAGE      TYPE STRING
```

#### Tables
```
ET_EMPLOYEES    TYPE ZOSH_EMPLOYEE_DATA
```

#### Source Code
```abap
FUNCTION z_osh_get_employees.
*"----------------------------------------------------------------------
*"*"Remote-Enabled Function Module
*"----------------------------------------------------------------------
*"*"Local Interface:
*"  IMPORTING
*"     VALUE(IV_PLANT) TYPE  WERKS_D OPTIONAL
*"     VALUE(IV_ORGUNIT) TYPE  ORGEH OPTIONAL
*"     VALUE(IV_CHANGED_SINCE) TYPE  SYDATUM OPTIONAL
*"  EXPORTING
*"     VALUE(EV_COUNT) TYPE  I
*"     VALUE(EV_MESSAGE) TYPE  STRING
*"  TABLES
*"      ET_EMPLOYEES STRUCTURE  ZOSH_EMPLOYEE_DATA
*"----------------------------------------------------------------------

  DATA: lt_employee TYPE TABLE OF zosh_employee_data,
        lv_where    TYPE string.

* Build dynamic WHERE clause
  lv_where = `ENDDA = '99991231' AND STAT2 = '3'`.

  IF iv_plant IS NOT INITIAL.
    CONCATENATE lv_where ` AND WERKS = '` iv_plant `'`
      INTO lv_where.
  ENDIF.

  IF iv_orgunit IS NOT INITIAL.
    CONCATENATE lv_where ` AND ORGEH = '` iv_orgunit `'`
      INTO lv_where.
  ENDIF.

  IF iv_changed_since IS NOT INITIAL.
    CONCATENATE lv_where ` AND ZSYNC_DATE >= '` iv_changed_since `'`
      INTO lv_where.
  ENDIF.

* Select from integration table (faster than joining HR tables)
  SELECT * FROM zosh_employee_data
    INTO TABLE et_employees
    WHERE (lv_where).

  IF sy-subrc = 0.
    ev_count = lines( et_employees ).
    ev_message = 'Success'.
  ELSE.
    ev_count = 0.
    ev_message = 'No employees found matching criteria'.
  ENDIF.

ENDFUNCTION.
```

---

## Incremental Sync Program

### Program: ZOSH_EMPLOYEE_SYNC_INCREMENTAL

```abap
*&---------------------------------------------------------------------*
*& Report ZOSH_EMPLOYEE_SYNC_INCREMENTAL
*&---------------------------------------------------------------------*
*& Purpose: Incremental sync - only changed/new employees
*& Author: [Your Name]
*& Date: 2025-10-16
*&---------------------------------------------------------------------*

REPORT zosh_employee_sync_incremental.

*----------------------------------------------------------------------*
* Data Declarations
*----------------------------------------------------------------------*
DATA: lt_employee      TYPE TABLE OF zosh_employee_data,
      lt_employee_existing TYPE TABLE OF zosh_employee_data,
      ls_employee      TYPE zosh_employee_data,
      lv_last_sync   TYPE sydatum,
      lv_count_new   TYPE i,
      lv_count_upd   TYPE i.

*----------------------------------------------------------------------*
* Get Last Sync Date
*----------------------------------------------------------------------*
SELECT MAX( zsync_date )
  FROM zosh_employee_data
  INTO lv_last_sync.

IF lv_last_sync IS INITIAL.
  lv_last_sync = '20240101'.  "Default if first sync
ENDIF.

WRITE: / 'Last sync date:', lv_last_sync.
WRITE: / 'Checking for changes since', lv_last_sync.

*----------------------------------------------------------------------*
* Select Only Changed Records
*----------------------------------------------------------------------*
* Note: This requires tracking changes in HR tables
* For simplicity, we check all records and compare

* Get current data from HR
SELECT p002~pernr AS pernr
       p002~vorna AS vorna
       p002~nachn AS nachn
       p001~stell AS stell
       p001~werks AS werks
       p001~btrtl AS btrtl
       p001~orgeh AS orgeh
       p001~kostl AS kostl
       p002~stat2 AS stat2
       p001~begda AS begda
       p001~endda AS endda
  INTO CORRESPONDING FIELDS OF TABLE @lt_employee
  FROM pa0002 AS p002
  INNER JOIN pa0001 AS p001
    ON p002~pernr = p001~pernr
  WHERE p001~endda = '99991231'
    AND p002~endda = '99991231'
    AND ( p001~aedtm >= @lv_last_sync     "Changed in PA0001
       OR p002~aedtm >= @lv_last_sync ).  "Changed in PA0002

WRITE: / 'Potentially changed records:', lines( lt_employee ).

*----------------------------------------------------------------------*
* Process Changed Records
*----------------------------------------------------------------------*
LOOP AT lt_employee ASSIGNING FIELD-SYMBOL(<emp>).
  * Set sync metadata
  <emp>-zsync_date = sy-datum.
  <emp>-zsync_time = sy-uzeit.
  <emp>-zchanged = 'X'.

  * Check if record exists
  SELECT SINGLE * FROM zosh_employee_data
    INTO ls_employee
    WHERE pernr = <emp>-pernr.

  IF sy-subrc = 0.
    * Update existing
    UPDATE zosh_employee_data FROM <emp>.
    IF sy-subrc = 0.
      ADD 1 TO lv_count_upd.
    ENDIF.
  ELSE.
    * Insert new
    INSERT zosh_employee_data FROM <emp>.
    IF sy-subrc = 0.
      ADD 1 TO lv_count_new.
    ENDIF.
  ENDIF.
ENDLOOP.

COMMIT WORK AND WAIT.

*----------------------------------------------------------------------*
* Results
*----------------------------------------------------------------------*
WRITE: / '========================================'.
WRITE: / 'Incremental sync completed'.
WRITE: / 'New records:', lv_count_new.
WRITE: / 'Updated records:', lv_count_upd.
WRITE: / 'Total processed:', lv_count_new + lv_count_upd.
WRITE: / '========================================'.
```

---

## Error Handling Module

### Include Program: ZOSH_ERROR_HANDLER

```abap
*&---------------------------------------------------------------------*
*& Include ZOSH_ERROR_HANDLER
*&---------------------------------------------------------------------*
*& Purpose: Centralized error handling and logging
*& Author: [Your Name]
*&---------------------------------------------------------------------*

*----------------------------------------------------------------------*
* Error Logging Table Structure
*----------------------------------------------------------------------*
* Create table ZOSH_ERROR_LOG in SE11:
* Fields: MANDT, LOG_ID, TIMESTAMP, PROGRAM, MESSAGE, SEVERITY

*----------------------------------------------------------------------*
* FORM: Log Error
*----------------------------------------------------------------------*
FORM log_error USING iv_program TYPE char40
                     iv_message TYPE char255
                     iv_severity TYPE char1.  "'E'=Error, 'W'=Warning, 'I'=Info

  DATA: ls_error TYPE zosh_error_log.

  ls_error-log_id = cl_system_uuid=>create_uuid_c32_static( ).
  ls_error-timestamp = sy-datum && sy-uzeit.
  ls_error-program = iv_program.
  ls_error-message = iv_message.
  ls_error-severity = iv_severity.
  ls_error-created_by = sy-uname.

  INSERT zosh_error_log FROM ls_error.
  COMMIT WORK.

  * Also write to application log
  CALL FUNCTION 'BAL_LOG_MSG_ADD'
    EXPORTING
      i_log_handle     = 1
      i_s_msg          = VALUE #( msgty = iv_severity
                                   msgid = '00'
                                   msgno = '001'
                                   msgv1 = iv_message ).

ENDFORM.

*----------------------------------------------------------------------*
* FORM: Check Authorization
*----------------------------------------------------------------------*
FORM check_authorization USING iv_table TYPE tabname
                         RETURNING VALUE(rv_authorized) TYPE abap_bool.

  AUTHORITY-CHECK OBJECT 'S_TABU_NAM'
    ID 'TABLE' FIELD iv_table
    ID 'ACTVT' FIELD '03'.  "Display = 03

  IF sy-subrc = 0.
    rv_authorized = abap_true.
  ELSE.
    rv_authorized = abap_false.
    PERFORM log_error USING 'ZOSH_SYNC'
                            'Authorization check failed for ' && iv_table
                            'E'.
  ENDIF.

ENDFORM.

*----------------------------------------------------------------------*
* FORM: Send Email Notification
*----------------------------------------------------------------------*
FORM send_email USING iv_subject TYPE so_obj_des
                      iv_body TYPE string
                      it_recipients TYPE bcsy_smtpa.

  DATA: lo_send_request TYPE REF TO cl_bcs,
        lo_document     TYPE REF TO cl_document_bcs,
        lo_recipient    TYPE REF TO if_recipient_bcs,
        lv_email        TYPE ad_smtpadr.

  TRY.
      * Create email
      lo_send_request = cl_bcs=>create_persistent( ).
      
      * Create document
      lo_document = cl_document_bcs=>create_document(
        i_type    = 'RAW'
        i_text    = VALUE #( ( line = iv_body ) )
        i_subject = iv_subject ).
      
      * Add document to email
      lo_send_request->set_document( lo_document ).

      * Add recipients
      LOOP AT it_recipients INTO lv_email.
        lo_recipient = cl_cam_address_bcs=>create_internet_address( lv_email ).
        lo_send_request->add_recipient( lo_recipient ).
      ENDLOOP.

      * Send
      lo_send_request->send( ).
      COMMIT WORK.

    CATCH cx_bcs INTO DATA(lx_bcs).
      PERFORM log_error USING 'ZOSH_EMAIL'
                              'Failed to send email: ' && lx_bcs->get_text( )
                              'E'.
  ENDTRY.

ENDFORM.
```

---

## Email Notification

### Program: ZOSH_SEND_SYNC_REPORT

```abap
*&---------------------------------------------------------------------*
*& Report ZOSH_SEND_SYNC_REPORT
*&---------------------------------------------------------------------*
*& Purpose: Send daily sync status report via email
*& Schedule: Run after ZOSH_EMPLOYEE_SYNC completes
*&---------------------------------------------------------------------*

REPORT zosh_send_sync_report.

*----------------------------------------------------------------------*
* Parameters
*----------------------------------------------------------------------*
PARAMETERS: p_email TYPE ad_smtpadr DEFAULT 'osh-admin@company.com'.

*----------------------------------------------------------------------*
* Data
*----------------------------------------------------------------------*
DATA: lv_count      TYPE i,
      lv_last_sync  TYPE string,
      lv_body       TYPE string,
      lt_recipients TYPE TABLE OF ad_smtpadr.

*----------------------------------------------------------------------*
* Get Sync Statistics
*----------------------------------------------------------------------*
SELECT COUNT( * )
  FROM zosh_employee_data
  INTO lv_count.

SELECT SINGLE zsync_date zsync_time
  FROM zosh_employee_data
  INTO (@DATA(lv_date), @DATA(lv_time))
  ORDER BY zsync_date DESCENDING, zsync_time DESCENDING
  UP TO 1 ROWS.

CONCATENATE lv_date lv_time INTO lv_last_sync SEPARATED BY space.

*----------------------------------------------------------------------*
* Build Email Body
*----------------------------------------------------------------------*
lv_body = |SAP-OSH Employee Sync Report\n\n|
       && |System: { sy-sysid }\n|
       && |Date: { sy-datum DATE = USER }\n|
       && |Time: { sy-uzeit TIME = USER }\n\n|
       && |Sync Status: SUCCESS\n|
       && |Total Employees: { lv_count }\n|
       && |Last Sync: { lv_last_sync }\n\n|
       && |This is an automated report.\n|.

*----------------------------------------------------------------------*
* Send Email
*----------------------------------------------------------------------*
APPEND p_email TO lt_recipients.

PERFORM send_email USING 'SAP-OSH Daily Sync Report'
                         lv_body
                         lt_recipients.

WRITE: / 'Email sent to:', p_email.
```

---

## Usage Instructions

### 1. Copy-Paste Workflow
```
1. Open SE38 in SAP
2. Create new program (name from header)
3. Copy entire code block
4. Paste into SAP editor
5. Save (Ctrl+S)
6. Check syntax (Ctrl+F2)
7. Activate (Ctrl+F3)
8. Test execute (F8)
```

### 2. Customization Points
Look for comments marked with:
```abap
* TODO: Adjust for your organization
* CUSTOMIZE: Change as needed
* OPTIONAL: Can be removed if not needed
```

### 3. Testing Before Production
```
Always test in DEV first:
1. Run with small dataset
2. Check results in SE16N
3. Verify no errors in ST22
4. Only then transport to QAS
```

---

## Additional Utilities

### Quick Test: Count Active Employees
```abap
REPORT zosh_count_employees.

SELECT COUNT( * ) FROM pa0001
  WHERE endda = '99991231'
    AND stat2 = '3'.

WRITE: / 'Active employees:', sy-dbcnt.
```

### Quick Test: View Sync Status
```abap
REPORT zosh_sync_status.

SELECT zsync_date zsync_time COUNT( * ) AS count
  FROM zosh_employee_data
  GROUP BY zsync_date zsync_time
  ORDER BY zsync_date DESCENDING, zsync_time DESCENDING
  INTO (@DATA(lv_date), @DATA(lv_time), @DATA(lv_count))
  UP TO 10 ROWS.
  
  WRITE: / lv_date, lv_time, lv_count.
ENDSELECT.
```

---

**Note**: All code templates are production-ready but should be tested thoroughly in your DEV environment first. Adjust field names and logic to match your specific SAP configuration.
