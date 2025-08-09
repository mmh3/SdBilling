# School District Billing System - Workflow Documentation

## Overview
This document provides detailed step-by-step workflows for all major business processes in the School District Billing System. Each workflow includes user actions, system processes, decision points, and error handling procedures.

---

## Student Management Workflows

### Student Enrollment Workflow

#### Process Overview
**Trigger:** New student enrolls in charter school  
**Frequency:** Ongoing throughout school year  
**Primary Users:** Charter school administrators, data entry staff  
**Duration:** 15-30 minutes per student  

#### Detailed Steps

##### 1. Initial Student Information Gathering
**User Actions:**
- Collect enrollment application from student/parent
- Verify student's district of residence
- Obtain previous school records
- Confirm special education status (if applicable)

**Required Information:**
- Student's full legal name
- Date of birth
- Current address (determines district of residence)
- Previous school attended
- Grade level
- Special education documentation (IEP, if applicable)

##### 2. District Verification Process
**User Actions:**
1. Navigate to `/Students/Create`
2. Look up school district by name or AUN
3. Verify district information matches student's address

**System Process:**
- Validates AUN exists in school district table
- Displays district contact information for verification
- Checks for active billing rates for the district

**Decision Point:** Is the district in the system?
- **Yes:** Continue to step 3
- **No:** Add district first (see District Setup Workflow)

##### 3. Student Data Entry
**User Actions:**
1. Enter student information in create form:
   - State Student Number (if known)
   - First Name and Last Name
   - Address information
   - Date of Birth
   - Grade Level
   - District Entry Date
   - Charter School selection

**System Validation:**
- Checks for duplicate state student numbers
- Validates grade level (K, 1-12)
- Ensures entry date is not in future
- Verifies all required fields are completed

##### 4. Special Education Processing
**If student has IEP:**
1. Set IEP Flag to "Y"
2. Enter Current IEP Date
3. Enter Prior IEP Date (if applicable)
4. Upload IEP documentation (future enhancement)

**System Process:**
- Calculates special education billing eligibility
- Flags student for special education rate billing
- Sets up attendance tracking for dual rates

##### 5. Final Validation and Save
**System Process:**
1. Runs comprehensive validation using `Student.IsValid()`
2. Checks business rules:
   - Valid AUN reference
   - Proper date relationships
   - Required field completion
   - Grade level validation

**Success Path:**
- Student record created
- Redirect to student index
- Success message displayed

**Error Path:**
- Display validation errors
- Return to form with entered data
- Highlight problematic fields

#### Error Handling Scenarios

##### Duplicate State Student Number
**Error:** "State student number already exists in system"
**Resolution Steps:**
1. Check if existing record is for same student
2. If same student, update existing record instead
3. If different student, verify state student number accuracy
4. Contact PDE if number conflict persists

##### Invalid District AUN
**Error:** "District of residence is not a valid AUN number"
**Resolution Steps:**
1. Verify AUN with school district
2. Check PDE district directory
3. Add district to system if valid but missing
4. Correct AUN if transcription error

##### Missing Required Information
**Error:** Various field-specific validation messages
**Resolution Steps:**
1. Contact student/parent for missing information
2. Check previous school records
3. Use temporary values if necessary (flag for follow-up)
4. Complete entry when information becomes available

---

### Student Update Workflow

#### Process Overview
**Trigger:** Student information changes, IEP status changes, address changes  
**Frequency:** As needed throughout school year  
**Primary Users:** Charter school administrators  
**Duration:** 5-15 minutes per update  

#### Detailed Steps

##### 1. Identify Update Need
**Common Triggers:**
- Student moves (address change)
- IEP status change
- Grade level change (promotion/retention)
- Name change (legal name change)
- Exit from school

##### 2. Access Student Record
**User Actions:**
1. Navigate to `/Students`
2. Search for student by name or state number
3. Click "Edit" on student record

**System Process:**
- Loads current student data
- Displays edit form with existing values
- Shows related information (billing history, etc.)

##### 3. Make Required Changes
**For Address Changes:**
- Update address fields
- Verify if district of residence changes
- If district changes, update AUN field
- Note: May affect billing for current month

**For IEP Changes:**
- Update IEP flag and dates
- Document reason for change
- Note: Affects billing rate calculations

**For Exit Processing:**
- Enter exit date
- Verify final attendance calculations
- Generate final billing if mid-month exit

##### 4. Validation and Save
**System Process:**
- Re-runs all validation rules
- Checks for data consistency
- Validates business rule compliance
- Updates audit trail

**Impact Analysis:**
- System calculates billing impact of changes
- Identifies affected invoices
- Flags need for billing adjustments

---

## Billing Rate Management Workflows

### Rate Import Workflow

#### Process Overview
**Trigger:** Annual rate updates from school districts  
**Frequency:** Annually, typically before school year starts  
**Primary Users:** OmniVest billing staff  
**Duration:** 2-4 hours for all districts  

#### Detailed Steps

##### 1. Prepare Rate Data
**User Actions:**
- Collect rate information from all school districts
- Verify rate effective dates
- Prepare Excel file with standardized format

**Required Excel Columns:**
- AUN (Administrative Unit Number)
- School District (Name)
- County
- Regular Rate (Daily rate for regular education)
- Special Ed Rate (Daily rate for special education)
- Effective Date

##### 2. Data Validation
**Pre-Import Checks:**
- Verify all AUNs are valid
- Ensure rates are reasonable (typically $30-80 per day)
- Confirm effective dates are logical
- Check for missing or duplicate entries

##### 3. Import Process
**User Actions:**
1. Navigate to `/SchoolDistrictRates`
2. Click "Import Rates" button
3. Select prepared Excel file
4. Review import preview
5. Confirm import

**System Process:**
- Reads Excel file using EPPlus library
- Validates each row of data
- Creates or updates school district records
- Creates new rate records with effective dates
- Maintains historical rate data

##### 4. Post-Import Verification
**User Actions:**
- Review imported rates for accuracy
- Spot-check several districts
- Verify effective dates are correct
- Test billing calculations with new rates

**System Validation:**
- Ensures no overlapping effective dates
- Validates rate relationships (special ed > regular)
- Confirms all districts have current rates

#### Error Handling

##### Invalid Rate Values
**Error:** "Rate must be a positive number"
**Resolution:**
- Check source data for formatting issues
- Verify decimal places are correct
- Confirm currency symbols are removed

##### Missing School Districts
**System Response:** Creates new district records automatically
**User Verification Required:**
- Review auto-created districts
- Add missing contact information
- Verify district names and AUNs

---

## Monthly Billing Workflow

### Invoice Generation Process

#### Process Overview
**Trigger:** End of each month  
**Frequency:** Monthly  
**Primary Users:** Charter school business managers  
**Duration:** 2-4 hours depending on number of districts  

#### Detailed Steps

##### 1. Month-End Preparation
**User Actions:**
- Verify all student enrollment changes are entered
- Confirm attendance data is accurate
- Review any mid-month entries or exits
- Check for rate changes during the month

**System Verification:**
- Validate student data completeness
- Confirm school calendar is accurate
- Verify billing rates are current

##### 2. Generate Monthly Invoices
**User Actions:**
1. Navigate to `/MonthlyInvoice`
2. Select charter school
3. Select billing month and year
4. Choose school districts to bill
5. Click "Generate Invoices"

**System Process:**
1. **Student Attendance Calculation:**
   - For each student, calculate days attended
   - Account for entry/exit dates within month
   - Separate regular and special education attendance
   - Handle mid-month IEP status changes

2. **Rate Application:**
   - Look up current rates for each district
   - Apply appropriate rates based on student status
   - Calculate prorated amounts for partial attendance

3. **Invoice Creation:**
   - Generate Excel invoice for each district
   - Include detailed student list
   - Show attendance calculations
   - Calculate total amounts due

4. **File Management:**
   - Save invoices to temporary directory
   - Create ZIP archive of all invoices
   - Generate audit trail records

##### 3. Invoice Review and Validation
**User Actions:**
- Download and review generated invoices
- Verify student lists are accurate
- Check calculation accuracy
- Confirm total amounts are reasonable

**Quality Checks:**
- Compare to previous month for reasonableness
- Verify special education students are properly flagged
- Check for any missing or duplicate students
- Confirm rate applications are correct

##### 4. Invoice Delivery
**User Actions:**
- Extract invoices from ZIP file
- Email invoices to school district contacts
- Mail hard copies if required
- Update delivery tracking

**System Process:**
- Records invoice generation in report history
- Updates invoice status to "Sent"
- Creates audit trail for compliance

#### Calculation Examples

##### Regular Education Student - Full Month
```
Student: John Smith, Grade 5, Regular Education
District: Sample School District (AUN: 123456789)
Month: September 2024
Days in Session: 20 days
Days Attended: 20 days (full month)
Regular Rate: $45.67 per day

Calculation: 20 days × $45.67 = $913.40
```

##### Special Education Student - Mid-Month Entry
```
Student: Jane Doe, Grade 3, Special Education
District: Example District (AUN: 987654321)
Month: October 2024
Days in Session: 22 days
Entry Date: October 15th
Days Attended: 10 days (partial month)
Special Ed Rate: $89.23 per day

Calculation: 10 days × $89.23 = $892.30
```

##### Student with IEP Status Change
```
Student: Mike Johnson, Grade 7
District: Test District (AUN: 555666777)
Month: November 2024
Days in Session: 18 days
IEP Start Date: November 10th
Regular Rate: $52.34 per day
Special Ed Rate: $98.76 per day

Regular Ed Days: 7 days (Nov 1-9)
Special Ed Days: 11 days (Nov 10-30)

Calculation: 
Regular: 7 days × $52.34 = $366.38
Special Ed: 11 days × $98.76 = $1,086.36
Total: $1,452.74
```

---

## Year-End Reconciliation Workflow

### Annual Reconciliation Process

#### Process Overview
**Trigger:** End of school year (typically June)  
**Frequency:** Annually  
**Primary Users:** Charter school business managers, OmniVest staff  
**Duration:** 1-2 weeks for complete reconciliation  

#### Detailed Steps

##### 1. Data Preparation Phase
**User Actions:**
- Ensure all monthly billing is complete
- Verify all payments are recorded
- Confirm all student exits are processed
- Review any billing adjustments made during year

**System Verification:**
- Check for incomplete student records
- Verify all months have been billed
- Confirm payment matching is complete
- Validate rate history for entire year

##### 2. Generate Reconciliation Reports
**User Actions:**
1. Navigate to `/YearEndRecon`
2. Select charter school
3. Select school year (e.g., "2023-2024")
4. Choose school districts for reconciliation
5. Click "Generate Reconciliation"

**System Process:**
1. **Annual Attendance Calculation:**
   - Calculate total days attended for each student
   - Account for all enrollment changes during year
   - Separate regular and special education days
   - Handle multiple IEP status changes

2. **Rate History Application:**
   - Apply correct rates based on effective dates
   - Handle mid-year rate changes
   - Calculate total annual billing per student
   - Summarize by district and education type

3. **Payment Reconciliation:**
   - Match all payments to invoices
   - Identify overpayments and underpayments
   - Calculate outstanding balances
   - Generate payment summary reports

4. **Variance Analysis:**
   - Compare monthly billing totals to annual calculation
   - Identify and explain any differences
   - Generate adjustment recommendations
   - Create supporting documentation

##### 3. Days Attended Report Generation
**System Process:**
- Creates detailed monthly breakdown for each student
- Shows attendance percentage by month
- Identifies months with partial attendance
- Calculates annual attendance totals

**Report Components:**
- Student identification information
- Monthly attendance grid (12 months)
- Special education status by month
- Annual totals and averages

##### 4. Reconciliation Review and Validation
**User Actions:**
- Review reconciliation reports for accuracy
- Compare to monthly billing records
- Investigate any significant variances
- Verify payment matching is complete

**Quality Assurance Steps:**
- Spot-check calculations for accuracy
- Verify student data consistency
- Confirm rate applications are correct
- Review special education status changes

##### 5. Final Report Preparation
**User Actions:**
- Generate final reconciliation package
- Prepare supporting documentation
- Create executive summary
- Package for delivery to school districts

**System Process:**
- Creates comprehensive ZIP archive
- Includes all supporting reports
- Generates audit trail documentation
- Updates report history records

---

## Payment Processing Workflow

### Payment Recording Process

#### Process Overview
**Trigger:** Payment received from school district  
**Frequency:** Ongoing throughout year  
**Primary Users:** Charter school business staff  
**Duration:** 5-10 minutes per payment  

#### Detailed Steps

##### 1. Payment Receipt
**User Actions:**
- Receive payment (check, electronic transfer, etc.)
- Identify which charter school and district
- Determine which invoice(s) payment covers
- Note payment date and amount

##### 2. Payment Entry
**User Actions:**
1. Navigate to `/Payments/Create`
2. Select charter school
3. Select school district
4. Enter payment details:
   - Payment date
   - Check number (if applicable)
   - Payment amount
   - Enrollment month covered
   - Comments/notes

**System Validation:**
- Ensures payment amount is positive
- Validates payment date is not in future
- Confirms charter school and district combination exists
- Checks for reasonable payment amounts

##### 3. Invoice Matching
**User Actions:**
- Identify which invoice(s) the payment covers
- Note if payment is partial, full, or overpayment
- Document any payment discrepancies
- Update invoice status if fully paid

**System Process:**
- Links payment to specific invoices
- Calculates remaining balance
- Updates payment status tracking
- Generates payment confirmation

##### 4. Reconciliation Updates
**System Process:**
- Updates accounts receivable balances
- Adjusts outstanding invoice totals
- Creates audit trail for payment
- Flags any payment discrepancies for review

---

## Report Generation Workflows

### PDE Compliance Reporting

#### Process Overview
**Trigger:** Monthly and annual PDE reporting requirements  
**Frequency:** Monthly and annually  
**Primary Users:** Charter school administrators  
**Duration:** 1-2 hours per reporting period  

#### Detailed Steps

##### 1. Report Preparation
**User Actions:**
- Verify all student data is current and accurate
- Confirm all billing has been completed
- Review any data quality issues
- Gather supporting documentation

##### 2. Generate PDE Reports
**System Process:**
- Creates CSR Student List reports
- Generates Direct Payment reports
- Produces Tuition Rate reports
- Creates Student List Reconciliation reports

**Report Validation:**
- Verifies data completeness
- Checks for formatting compliance
- Validates calculation accuracy
- Confirms deadline requirements

##### 3. Report Review and Submission
**User Actions:**
- Review generated reports for accuracy
- Compare to previous submissions
- Make any necessary corrections
- Submit to PDE through required channels

**Quality Assurance:**
- Cross-reference with source data
- Verify compliance with PDE requirements
- Check for any missing information
- Confirm submission deadlines are met

---

## Error Handling and Recovery Procedures

### Common Error Scenarios

#### Data Validation Errors
**Scenario:** Invalid student data prevents processing
**Recovery Steps:**
1. Identify specific validation failures
2. Correct source data issues
3. Re-run validation process
4. Verify corrections are successful

#### Billing Calculation Errors
**Scenario:** Incorrect attendance or rate calculations
**Recovery Steps:**
1. Identify affected students/districts
2. Recalculate using correct parameters
3. Generate corrected invoices
4. Notify affected school districts
5. Update payment tracking accordingly

#### Report Generation Failures
**Scenario:** System unable to generate required reports
**Recovery Steps:**
1. Check system resources and file permissions
2. Verify data integrity
3. Clear temporary files
4. Restart report generation process
5. Contact technical support if issues persist

#### Payment Matching Issues
**Scenario:** Unable to match payment to correct invoice
**Recovery Steps:**
1. Review payment documentation
2. Contact school district for clarification
3. Research invoice history
4. Make manual adjustments if necessary
5. Document resolution for audit trail

### Escalation Procedures

#### Technical Issues
1. **Level 1:** User troubleshooting and basic fixes
2. **Level 2:** System administrator intervention
3. **Level 3:** Developer/vendor support
4. **Level 4:** Emergency procedures and workarounds

#### Business Issues
1. **Level 1:** Charter school staff resolution
2. **Level 2:** OmniVest management review
3. **Level 3:** School district coordination
4. **Level 4:** PDE or legal consultation

This workflow documentation provides comprehensive guidance for all major business processes in the School District Billing System. Each workflow includes specific steps, decision points, and error handling procedures to ensure consistent and accurate processing.
