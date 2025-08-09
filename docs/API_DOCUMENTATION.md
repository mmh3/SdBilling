# School District Billing System - API Documentation

## Overview
This document provides comprehensive API documentation for the School District Billing System's MVC controllers and endpoints.

## Base URL
- Development: `https://localhost:5001` or `http://localhost:5000`
- Production: [Your production URL]

## Authentication
Currently, the system does not implement authentication. All endpoints are publicly accessible.

---

## Controllers & Endpoints

### HomeController
**Base Route:** `/Home`

#### GET /Home/Index
- **Description:** Application home page
- **Returns:** Main dashboard view
- **Parameters:** None
- **Response:** HTML view

#### GET /Home/Privacy
- **Description:** Privacy policy page
- **Returns:** Privacy policy view
- **Parameters:** None
- **Response:** HTML view

---

### StudentsController
**Base Route:** `/Students`

#### GET /Students
- **Description:** Display paginated list of all students with filtering options
- **Returns:** Student index view with search and filter capabilities
- **Parameters:** None (filtering handled via query parameters)
- **Response:** HTML view with StudentIndexView model
- **Business Logic:** 
  - Loads all students from database
  - Provides filtering by charter school, school district, grade
  - Supports search by name or state student number

#### GET /Students/Create
- **Description:** Display form to create a new student
- **Returns:** Student creation form
- **Parameters:** None
- **Response:** HTML view with StudentView model
- **Dependencies:** 
  - Loads all charter schools for dropdown
  - Loads all school districts for dropdown

#### POST /Students/Create
- **Description:** Create a new student record
- **Parameters:** StudentView model (form data)
- **Validation:** 
  - State student number required
  - First and last name required
  - Valid AUN for school district
  - Valid grade (K, 1-12)
  - Valid date of birth
  - Valid district entry date
- **Returns:** Redirect to Index on success, form with errors on failure
- **Business Logic:**
  - Validates student data using Student.IsValid()
  - Checks for duplicate state student numbers
  - Creates new student record

#### GET /Students/Edit/{id}
- **Description:** Display form to edit existing student
- **Parameters:** 
  - `id` (int): Student UID
- **Returns:** Student edit form
- **Response:** HTML view with StudentView model
- **Error Handling:** Returns NotFound if student doesn't exist

#### POST /Students/Edit/{id}
- **Description:** Update existing student record
- **Parameters:** 
  - `id` (int): Student UID
  - StudentView model (form data)
- **Validation:** Same as Create
- **Returns:** Redirect to Index on success, form with errors on failure
- **Business Logic:**
  - Updates existing student record
  - Maintains audit trail

#### GET /Students/Delete/{id}
- **Description:** Display confirmation page for student deletion
- **Parameters:** 
  - `id` (int): Student UID
- **Returns:** Delete confirmation view
- **Error Handling:** Returns NotFound if student doesn't exist

#### POST /Students/Delete/{id}
- **Description:** Delete student record
- **Parameters:** 
  - `id` (int): Student UID
- **Returns:** Redirect to Index
- **Business Logic:** Permanently removes student from database

#### POST /Students/Import
- **Description:** Bulk import students from Excel file
- **Parameters:** 
  - Excel file upload
  - Charter school selection
- **File Format:** Excel file with specific column headers
- **Returns:** Import results view
- **Business Logic:**
  - Validates Excel file format
  - Processes each row for student data
  - Handles duplicate detection
  - Provides import summary

---

### CharterSchoolsController
**Base Route:** `/CharterSchools`

#### GET /CharterSchools
- **Description:** Display list of all charter schools
- **Returns:** Charter school index view
- **Parameters:** None
- **Response:** HTML view with list of CharterSchool models

#### GET /CharterSchools/Create
- **Description:** Display form to create new charter school
- **Returns:** Charter school creation form
- **Parameters:** None
- **Response:** HTML view with CharterSchool model

#### POST /CharterSchools/Create
- **Description:** Create new charter school record
- **Parameters:** CharterSchool model (form data)
- **Validation:**
  - Name required (max 255 characters)
  - Phone optional (max 255 characters)
  - Address fields optional
- **Returns:** Redirect to Index on success, form with errors on failure

#### GET /CharterSchools/Edit/{id}
- **Description:** Display form to edit existing charter school
- **Parameters:** 
  - `id` (int): Charter School UID
- **Returns:** Charter school edit form
- **Error Handling:** Returns NotFound if charter school doesn't exist

#### POST /CharterSchools/Edit/{id}
- **Description:** Update existing charter school record
- **Parameters:** 
  - `id` (int): Charter School UID
  - CharterSchool model (form data)
- **Validation:** Same as Create
- **Returns:** Redirect to Index on success, form with errors on failure

#### GET /CharterSchools/Delete/{id}
- **Description:** Display confirmation page for charter school deletion
- **Parameters:** 
  - `id` (int): Charter School UID
- **Returns:** Delete confirmation view
- **Error Handling:** Returns NotFound if charter school doesn't exist

#### POST /CharterSchools/Delete/{id}
- **Description:** Delete charter school record
- **Parameters:** 
  - `id` (int): Charter School UID
- **Returns:** Redirect to Index
- **Business Logic:** 
  - Checks for dependent records (students, schedules)
  - May prevent deletion if dependencies exist

#### GET /CharterSchools/Calendar/{id}
- **Description:** Display and manage charter school calendar/schedule
- **Parameters:** 
  - `id` (int): Charter School UID
- **Returns:** Calendar management view
- **Business Logic:**
  - Displays school schedules by grade level
  - Shows school days, holidays, breaks
  - Allows schedule modifications

---

### SchoolDistrictsController
**Base Route:** `/SchoolDistricts`

#### GET /SchoolDistricts
- **Description:** Display list of all school districts
- **Returns:** School district index view
- **Parameters:** None
- **Response:** HTML view with list of SchoolDistrict models

#### GET /SchoolDistricts/Create
- **Description:** Display form to create new school district
- **Returns:** School district creation form
- **Parameters:** None
- **Response:** HTML view with SchoolDistrict model

#### POST /SchoolDistricts/Create
- **Description:** Create new school district record
- **Parameters:** SchoolDistrict model (form data)
- **Validation:**
  - Name required (max 255 characters)
  - AUN required and unique (max 255 characters)
  - Phone optional (max 255 characters)
  - Address fields optional
- **Returns:** Redirect to Index on success, form with errors on failure

#### GET /SchoolDistricts/Edit/{id}
- **Description:** Display form to edit existing school district
- **Parameters:** 
  - `id` (int): School District UID
- **Returns:** School district edit form
- **Error Handling:** Returns NotFound if school district doesn't exist

#### POST /SchoolDistricts/Edit/{id}
- **Description:** Update existing school district record
- **Parameters:** 
  - `id` (int): School District UID
  - SchoolDistrict model (form data)
- **Validation:** Same as Create
- **Returns:** Redirect to Index on success, form with errors on failure

#### GET /SchoolDistricts/Delete/{id}
- **Description:** Display confirmation page for school district deletion
- **Parameters:** 
  - `id` (int): School District UID
- **Returns:** Delete confirmation view
- **Error Handling:** Returns NotFound if school district doesn't exist

#### POST /SchoolDistricts/Delete/{id}
- **Description:** Delete school district record
- **Parameters:** 
  - `id` (int): School District UID
- **Returns:** Redirect to Index
- **Business Logic:** 
  - Checks for dependent records (students, rates, payments)
  - May prevent deletion if dependencies exist

---

### SchoolDistrictRatesController
**Base Route:** `/SchoolDistrictRates`

#### GET /SchoolDistrictRates
- **Description:** Display list of all billing rates by school district
- **Returns:** School district rates index view
- **Parameters:** None
- **Response:** HTML view with list of SchoolDistrictRateView models
- **Business Logic:**
  - Shows current and historical rates
  - Groups by school district
  - Displays effective date ranges

#### GET /SchoolDistrictRates/Edit/{id}
- **Description:** Display form to edit billing rate
- **Parameters:** 
  - `id` (int): School District Rate UID
- **Returns:** Rate edit form
- **Error Handling:** Returns NotFound if rate doesn't exist

#### POST /SchoolDistrictRates/Edit/{id}
- **Description:** Update billing rate record
- **Parameters:** 
  - `id` (int): School District Rate UID
  - SchoolDistrictRateView model (form data)
- **Validation:**
  - Effective date required
  - Regular rate required (decimal)
  - Special education rate required (decimal)
  - Rates must be positive values
- **Returns:** Redirect to Index on success, form with errors on failure
- **Business Logic:**
  - Maintains rate history
  - Ensures no overlapping effective dates

#### GET /SchoolDistrictRates/Delete/{id}
- **Description:** Display confirmation page for rate deletion
- **Parameters:** 
  - `id` (int): School District Rate UID
- **Returns:** Delete confirmation view
- **Error Handling:** Returns NotFound if rate doesn't exist

#### POST /SchoolDistrictRates/Delete/{id}
- **Description:** Delete billing rate record
- **Parameters:** 
  - `id` (int): School District Rate UID
- **Returns:** Redirect to Index
- **Business Logic:** 
  - May prevent deletion of historical rates used in invoices

#### POST /SchoolDistrictRates/Import
- **Description:** Bulk import billing rates from Excel file
- **Parameters:** Excel file upload
- **File Format:** Excel file with columns: AUN, School District, County, Regular Rate, Special Ed Rate, Effective Date
- **Returns:** Import results view
- **Business Logic:**
  - Creates school districts if they don't exist
  - Creates rate records with effective dates
  - Handles rate updates and historical tracking

---

### PaymentsController
**Base Route:** `/Payments`

#### GET /Payments
- **Description:** Display list of all payments with filtering options
- **Returns:** Payments index view
- **Parameters:** Query parameters for filtering
- **Response:** HTML view with list of PaymentView models
- **Business Logic:**
  - Supports filtering by charter school, school district, date range
  - Shows payment history and status

#### GET /Payments/Create
- **Description:** Display form to create new payment record
- **Returns:** Payment creation form
- **Parameters:** None
- **Response:** HTML view with PaymentView model
- **Dependencies:**
  - Loads charter schools for dropdown
  - Loads school districts for dropdown

#### POST /Payments/Create
- **Description:** Create new payment record
- **Parameters:** PaymentView model (form data)
- **Validation:**
  - Charter school required
  - School district required
  - Payment date required
  - Amount required (positive decimal)
- **Returns:** Redirect to Index on success, form with errors on failure

#### GET /Payments/Edit/{id}
- **Description:** Display form to edit existing payment
- **Parameters:** 
  - `id` (int): Payment UID
- **Returns:** Payment edit form
- **Error Handling:** Returns NotFound if payment doesn't exist

#### POST /Payments/Edit/{id}
- **Description:** Update existing payment record
- **Parameters:** 
  - `id` (int): Payment UID
  - PaymentView model (form data)
- **Validation:** Same as Create
- **Returns:** Redirect to Index on success, form with errors on failure

#### GET /Payments/Delete/{id}
- **Description:** Display confirmation page for payment deletion
- **Parameters:** 
  - `id` (int): Payment UID
- **Returns:** Delete confirmation view
- **Error Handling:** Returns NotFound if payment doesn't exist

#### POST /Payments/Delete/{id}
- **Description:** Delete payment record
- **Parameters:** 
  - `id` (int): Payment UID
- **Returns:** Redirect to Index

---

### MonthlyInvoiceController
**Base Route:** `/MonthlyInvoice`

#### GET /MonthlyInvoice
- **Description:** Display monthly invoice generation form
- **Returns:** Invoice criteria form
- **Parameters:** None
- **Response:** HTML view with ReportCriteriaView model
- **Dependencies:** Loads all charter schools

#### GET /MonthlyInvoice/GetSchoolDistricts
- **Description:** AJAX endpoint to get school districts for selected charter school
- **Parameters:** 
  - `charterSchoolUid` (int): Charter School UID
- **Returns:** JSON array of school districts
- **Business Logic:**
  - Returns only districts that have students enrolled in the charter school
  - Used for dynamic dropdown population

#### POST /MonthlyInvoice/Generate
- **Description:** Generate monthly invoices and student lists
- **Parameters:** ReportCriteriaView model (form data)
- **Validation:**
  - Charter school required
  - Month required
  - Year required
  - At least one school district selected
- **Returns:** ZIP file download containing all generated reports
- **Business Logic:**
  - Generates invoice Excel files for each selected school district
  - Generates student list reports
  - Creates PDE compliance reports
  - Packages all files into downloadable ZIP
  - Records report generation in audit trail

**Generated Files Include:**
- Monthly invoice spreadsheets
- Individual student lists
- Unipay request forms (if applicable)
- PDE CSR Student List reports

---

### YearEndReconController
**Base Route:** `/YearEndRecon`

#### GET /YearEndRecon
- **Description:** Display year-end reconciliation form
- **Returns:** Reconciliation criteria form
- **Parameters:** None
- **Response:** HTML view with ReportCriteriaView model
- **Dependencies:** Loads all charter schools

#### GET /YearEndRecon/GetSchoolDistricts
- **Description:** AJAX endpoint to get school districts for selected charter school
- **Parameters:** 
  - `charterSchoolUid` (int): Charter School UID
- **Returns:** JSON array of school districts
- **Business Logic:** Same as MonthlyInvoice version

#### POST /YearEndRecon/Generate
- **Description:** Generate year-end reconciliation reports
- **Parameters:** ReportCriteriaView model (form data)
- **Validation:**
  - Charter school required
  - School year required (format: "2023-2024")
  - At least one school district selected
- **Returns:** ZIP file download containing all generated reports
- **Business Logic:**
  - Generates year-end reconciliation spreadsheets
  - Generates annual student attendance reports
  - Creates days attended reports for each month
  - Generates PDE compliance reports for annual submission
  - Packages all files into downloadable ZIP
  - Records report generation in audit trail

**Generated Files Include:**
- Year-end reconciliation spreadsheets
- Days attended reports (monthly breakdown)
- Annual student lists
- PDE CSR reports for year-end submission

---

## Common Response Formats

### Success Responses
- **HTML Views:** Standard MVC views with appropriate models
- **Redirects:** Redirect to Index action after successful operations
- **File Downloads:** Excel files or ZIP archives for reports

### Error Responses
- **Validation Errors:** Form redisplay with ModelState errors
- **Not Found:** 404 response for invalid IDs
- **Server Errors:** 500 response with error page

## Business Rules

### Student Management
- State student numbers must be unique within the system
- Students must have valid school district AUN
- Entry dates cannot be in the future
- Exit dates must be after entry dates
- Grade levels: K, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12

### Billing Rates
- Rates are effective-dated (historical tracking)
- Cannot have overlapping effective dates for same district
- Rates must be positive decimal values
- Special education rates typically higher than regular rates

### Attendance Calculations
- Based on charter school calendar/schedule
- Accounts for student entry/exit dates
- Separates special education vs. regular education days
- Handles mid-month enrollment changes

### Report Generation
- All reports logged in audit trail
- Files stored in temporary directory for download
- ZIP archives created for multi-file reports
- Excel templates used for consistent formatting

## File Upload Requirements

### Student Import Excel Format
- Required columns: State Student No, First Name, Last Name, AUN, Grade, DOB, District Entry Date
- Optional columns: Address fields, IEP information
- Date format: MM/DD/YYYY
- Grade format: K, 1, 2, etc.

### Rate Import Excel Format
- Required columns: AUN, School District, Regular Rate, Special Ed Rate, Effective Date
- Optional columns: County
- Rate format: Decimal (e.g., 12345.67)
- Date format: MM/DD/YYYY

## Security Considerations
- Currently no authentication implemented
- All file uploads should be validated
- SQL injection protection via Entity Framework
- XSS protection via Razor encoding
- CSRF protection via anti-forgery tokens

## Performance Notes
- Large student imports may take significant time
- Report generation can be memory-intensive for large datasets
- Consider implementing background job processing for large operations
- Database queries optimized with appropriate indexes
