# School District Billing System - Data Model Documentation

## Overview
This document provides comprehensive documentation of the data models, relationships, and business constraints in the School District Billing System.

## Entity Relationship Diagram

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   CharterSchool │    │     Student      │    │ SchoolDistrict  │
│                 │    │                  │    │                 │
│ CharterSchoolUid├────┤CharterSchoolUid  │    │SchoolDistrictUid│
│ Name            │    │StateStudentNo    │    │Name             │
│ Phone           │    │FirstName         │    │Aun              │
│ Address...      │    │LastName          │    │County           │
└─────────────────┘    │Aun               ├────┤Phone            │
                       │Grade             │    │Address...       │
                       │Dob               │    └─────────────────┘
                       │DistrictEntryDate │              │
                       │ExitDate          │              │
                       │SpedFlag          │              │
                       │IepFlag           │              │
                       │CurrentIepDate    │              │
                       │PriorIepDate      │              │
                       └──────────────────┘              │
                                                         │
┌─────────────────┐    ┌──────────────────┐              │
│    Payment      │    │SchoolDistrictRate│              │
│                 │    │                  │              │
│PaymentUid       │    │SchoolDistrictUid ├──────────────┘
│CharterSchoolUid ├────┤RegularRate       │
│SchoolDistrictUid├────┤SpecialEdRate     │
│Date             │    │EffectiveDate     │
│CheckNo          │    └──────────────────┘
│Amount           │
│PaidBy           │    ┌──────────────────┐
│EnrollmentMonth  │    │  ReportHistory   │
│Comments         │    │                  │
└─────────────────┘    │ReportHistoryUid  │
                       │CharterSchoolUid  ├────┐
┌─────────────────┐    │SchoolDistrictUid ├────┤
│CharterSchool    │    │ReportType        │    │
│Schedule         │    │ReportMonth       │    │
│                 │    │ReportYear        │    │
│CharterSchoolUid ├────┤RunDate           │    │
│Grade            │    │SendTo            │    │
│FirstDay         │    │FileName          │    │
│LastDay          │    └──────────────────┘    │
│DaysInSession    │                            │
└─────────────────┘    ┌──────────────────┐    │
         │             │CharterSchool     │    │
         │             │Contact           │    │
         └─────────────┤CharterSchoolUid  ├────┘
                       │ContactType       │
                       │FirstName         │
                       │LastName          │
                       │Email             │
                       │Phone             │
                       └──────────────────┘

                       ┌──────────────────┐
                       │SchoolDistrict    │
                       │Contact           │
                       │                  │
                       │SchoolDistrictUid ├────┐
                       │ContactType       │    │
                       │FirstName         │    │
                       │LastName          │    │
                       │Email             │    │
                       │Phone             │    │
                       └──────────────────┘    │
                                              │
                       ┌──────────────────┐    │
                       │CharterSchool     │    │
                       │ScheduleDate      │    │
                       │                  │    │
                       │CharterSchoolUid  ├────┘
                       │Date              │
                       │IsSchoolDay       │
                       │Description       │
                       └──────────────────┘
```

---

## Core Entities

### Student
**Table:** `student`
**Primary Key:** `student_uid` (auto-increment)

#### Properties
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| student_uid | int | PK, Identity, Required | Unique student identifier |
| state_student_no | varchar(255) | Required, Unique | Pennsylvania state student number |
| first_name | varchar(255) | Required | Student's first name |
| last_name | varchar(255) | Required | Student's last name |
| address_street | varchar(255) | Optional | Street address |
| address_city | varchar(255) | Optional | City |
| address_state | varchar(2) | Optional | State abbreviation |
| address_zip | varchar(255) | Optional | ZIP code |
| dob | date | Optional | Date of birth |
| grade | varchar(2) | Required | Grade level (K, 1-12) |
| district_entry_date | date | Required | Date student entered charter school |
| exit_date | date | Optional | Date student left charter school |
| sped_flag | varchar(1) | Optional | Special education flag |
| iep_flag | varchar(1) | Optional | IEP flag |
| current_iep_date | date | Optional | Current IEP effective date |
| prior_iep_date | date | Optional | Previous IEP date |
| charter_school_uid | int | FK, Required | Reference to charter school |
| aun | varchar(255) | FK, Required | School district AUN |

#### Business Rules
- **State Student Number:** Must be unique across the system
- **Grade Validation:** Must be one of: K, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12
- **Date Validation:** 
  - DOB cannot be in the future
  - District entry date cannot be in the future
  - Exit date must be after entry date (if provided)
- **AUN Validation:** Must reference a valid school district
- **Special Education Logic:**
  - Student is considered special education if they have a current IEP date
  - IEP dates determine billing rate calculations
  - Special education status can change during enrollment

#### Key Methods
- `IsValid()`: Validates all business rules
- `GetMonthlyAttendanceValue()`: Calculates attendance for billing
- `GetYearlyAttendanceValue()`: Calculates annual attendance
- `IsSpedOnDate()`: Determines special education status on specific date
- `DidAttendForEntirePeriod()`: Checks if student was enrolled for full period

#### Relationships
- **Many-to-One:** Student → CharterSchool
- **Many-to-One:** Student → SchoolDistrict (via AUN)

---

### CharterSchool
**Table:** `charter_school`
**Primary Key:** `charter_school_uid` (auto-increment)

#### Properties
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| charter_school_uid | int | PK, Identity, Required | Unique charter school identifier |
| name | varchar(255) | Required | Charter school name |
| phone | varchar(255) | Optional | Primary phone number |
| address_street | varchar(255) | Optional | Street address |
| address_city | varchar(255) | Optional | City |
| address_state | varchar(2) | Optional | State abbreviation |
| address_zip | varchar(255) | Optional | ZIP code |

#### Business Rules
- **Name:** Must be unique and non-empty
- **Contact Information:** At least one form of contact (phone or address) recommended

#### Relationships
- **One-to-Many:** CharterSchool → Student
- **One-to-Many:** CharterSchool → CharterSchoolSchedule
- **One-to-Many:** CharterSchool → CharterSchoolContact
- **One-to-Many:** CharterSchool → Payment
- **One-to-Many:** CharterSchool → ReportHistory

---

### SchoolDistrict
**Table:** `school_district`
**Primary Key:** `school_district_uid` (auto-increment)

#### Properties
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| school_district_uid | int | PK, Identity, Required | Unique school district identifier |
| name | varchar(255) | Required | School district name |
| aun | varchar(255) | Required, Unique | Administrative Unit Number |
| county | varchar(255) | Optional | County name |
| phone | varchar(255) | Optional | Primary phone number |
| address_street | varchar(255) | Optional | Street address |
| address_city | varchar(255) | Optional | City |
| address_state | varchar(2) | Optional | State abbreviation |
| address_zip | varchar(255) | Optional | ZIP code |

#### Business Rules
- **AUN:** Must be unique across the system (Pennsylvania state identifier)
- **Name:** Must be non-empty
- **County:** Used for reporting and organization

#### Relationships
- **One-to-Many:** SchoolDistrict → Student (via AUN)
- **One-to-Many:** SchoolDistrict → SchoolDistrictRate
- **One-to-Many:** SchoolDistrict → SchoolDistrictContact
- **One-to-Many:** SchoolDistrict → Payment
- **One-to-Many:** SchoolDistrict → ReportHistory

---

### SchoolDistrictRate
**Table:** `school_district_rate`
**Primary Key:** `school_district_rate_uid` (auto-increment)

#### Properties
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| school_district_rate_uid | int | PK, Identity, Required | Unique rate identifier |
| school_district_uid | int | FK, Required | Reference to school district |
| regular_rate | decimal(10,2) | Required | Regular education daily rate |
| special_ed_rate | decimal(10,2) | Required | Special education daily rate |
| effective_date | date | Required | Date rate becomes effective |

#### Business Rules
- **Rate Values:** Must be positive decimal values
- **Effective Dating:** 
  - Cannot have overlapping effective dates for same district
  - Most recent effective date (≤ current date) is used for billing
- **Rate Relationship:** Special education rate typically higher than regular rate
- **Historical Tracking:** Old rates maintained for audit and reconciliation

#### Key Methods
- Rate lookup based on effective date
- Historical rate retrieval for reconciliation

#### Relationships
- **Many-to-One:** SchoolDistrictRate → SchoolDistrict

---

### Payment
**Table:** `payment`
**Primary Key:** `payment_uid` (auto-increment)

#### Properties
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| payment_uid | int | PK, Identity, Required | Unique payment identifier |
| charter_school_uid | int | FK, Required | Reference to charter school |
| school_district_uid | int | FK, Required | Reference to school district |
| date | date | Required | Payment date |
| check_no | varchar(255) | Optional | Check number |
| amount | decimal(10,2) | Required | Payment amount |
| paid_by | varchar(255) | Optional | Who made the payment |
| enrollment_month | varchar(255) | Optional | Month payment covers |
| comments | varchar(255) | Optional | Additional notes |

#### Business Rules
- **Amount:** Must be positive value
- **Date:** Cannot be in the future
- **Enrollment Month:** Used to track which billing period payment covers
- **Audit Trail:** All payments maintained for historical tracking

#### Relationships
- **Many-to-One:** Payment → CharterSchool
- **Many-to-One:** Payment → SchoolDistrict

---

### CharterSchoolSchedule
**Table:** `charter_school_schedule`
**Primary Key:** `charter_school_schedule_uid` (auto-increment)

#### Properties
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| charter_school_schedule_uid | int | PK, Identity, Required | Unique schedule identifier |
| charter_school_uid | int | FK, Required | Reference to charter school |
| grade | varchar(255) | Required | Grade levels (e.g., "K-5", "6-8") |
| first_day | date | Required | First day of school year |
| last_day | date | Required | Last day of school year |
| days_in_session | int | Required | Total school days |

#### Business Rules
- **Date Range:** Last day must be after first day
- **Grade Ranges:** Can specify individual grades or ranges
- **School Year:** Typically spans two calendar years (e.g., 2023-2024)
- **Days Calculation:** Used for attendance and billing calculations

#### Key Methods
- `AppliesToGrade()`: Determines if schedule applies to specific grade
- `GetSchoolDays()`: Calculates school days in date range

#### Relationships
- **Many-to-One:** CharterSchoolSchedule → CharterSchool
- **One-to-Many:** CharterSchoolSchedule → CharterSchoolScheduleDate

---

### CharterSchoolScheduleDate
**Table:** `charter_school_schedule_date`
**Primary Key:** `charter_school_schedule_date_uid` (auto-increment)

#### Properties
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| charter_school_schedule_date_uid | int | PK, Identity, Required | Unique schedule date identifier |
| charter_school_uid | int | FK, Required | Reference to charter school |
| date | date | Required | Specific date |
| is_school_day | boolean | Required | Whether this is a school day |
| description | varchar(255) | Optional | Holiday/break description |

#### Business Rules
- **School Days:** Used to override default schedule (holidays, breaks)
- **Attendance Impact:** Non-school days don't count toward attendance
- **Billing Impact:** Affects daily rate calculations

#### Relationships
- **Many-to-One:** CharterSchoolScheduleDate → CharterSchool

---

### ReportHistory
**Table:** `report_history`
**Primary Key:** `report_history_uid` (auto-increment)

#### Properties
| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| report_history_uid | int | PK, Identity, Required | Unique report identifier |
| charter_school_uid | int | FK, Required | Reference to charter school |
| school_district_uid | int | FK, Optional | Reference to school district |
| report_type | varchar(255) | Required | Type of report generated |
| report_month | varchar(255) | Optional | Month for monthly reports |
| report_year | int | Required | Year for report |
| run_date | datetime | Required | When report was generated |
| send_to | varchar(255) | Required | Who report was sent to |
| file_name | varchar(255) | Required | Generated file name |

#### Business Rules
- **Audit Trail:** All report generation logged
- **Report Types:** Monthly Invoice, Year-End Reconciliation, PDE Reports
- **Send To:** Tracks whether sent to school district or state
- **File Tracking:** Links to actual generated files

#### Relationships
- **Many-to-One:** ReportHistory → CharterSchool
- **Many-to-One:** ReportHistory → SchoolDistrict

---

## Supporting Entities

### CharterSchoolContact
**Table:** `charter_school_contact`

#### Properties
- Contact information for charter school personnel
- Multiple contacts per school (billing, administrative, etc.)
- Contact types: Billing, Administrative, Emergency

### SchoolDistrictContact
**Table:** `school_district_contact`

#### Properties
- Contact information for school district personnel
- Multiple contacts per district
- Used for invoice delivery and communication

---

## View Models

### StudentView
Combines Student entity with related data for forms:
- Student properties
- Available charter schools (dropdown)
- Available school districts (dropdown)
- Validation logic

### StudentIndexView
Provides filtered/paginated student list:
- Search functionality
- Filter by charter school, district, grade
- Pagination support

### PaymentView
Combines Payment entity with display names:
- Payment properties
- Charter school name (instead of UID)
- School district name (instead of UID)

### ReportCriteriaView
Captures report generation parameters:
- Charter school selection
- Date/month/year selection
- School district selection (multi-select)
- Report type options

---

## Business Logic & Calculations

### Attendance Calculations
Complex logic in Student model for calculating attendance:

1. **Monthly Attendance:**
   - Based on charter school schedule for specific grade
   - Accounts for student entry/exit dates within month
   - Separates special education vs. regular education days
   - Handles mid-month status changes

2. **Special Education Status:**
   - Determined by Current IEP Date
   - Can change during enrollment period
   - Affects billing rate calculations
   - Historical tracking for reconciliation

3. **Billing Calculations:**
   - Daily rates × attendance days
   - Separate calculations for regular and special education
   - Prorated for partial month enrollment
   - Historical rate lookup for past periods

### Rate Management
- Effective-dated rates allow historical tracking
- Rate changes don't affect past billing
- Supports rate increases/decreases over time
- Audit trail for all rate changes

### Report Generation
- Templates stored in wwwroot/reportTemplates
- Dynamic data population using EPPlus library
- Multiple report formats (invoices, reconciliation, PDE compliance)
- File naming conventions for organization
- Audit logging of all generated reports

---

## Data Integrity Constraints

### Foreign Key Relationships
- Student.CharterSchoolUid → CharterSchool.CharterSchoolUid
- Student.Aun → SchoolDistrict.Aun
- Payment.CharterSchoolUid → CharterSchool.CharterSchoolUid
- Payment.SchoolDistrictUid → SchoolDistrict.SchoolDistrictUid
- SchoolDistrictRate.SchoolDistrictUid → SchoolDistrict.SchoolDistrictUid

### Unique Constraints
- Student.StateStudentNo (unique across system)
- SchoolDistrict.Aun (unique across system)
- CharterSchool.Name (unique across system)

### Check Constraints
- Rates must be positive values
- Dates must be valid and logical (exit after entry, etc.)
- Grade values must be from approved list

### Business Rule Validation
- Implemented in model validation methods
- Enforced at application layer
- Additional database constraints where appropriate

---

## Performance Considerations

### Indexing Strategy
- Primary keys (clustered indexes)
- Foreign key columns
- Frequently queried columns (StateStudentNo, Aun)
- Date columns used in range queries

### Query Optimization
- Entity Framework LINQ queries
- Eager loading for related data
- Pagination for large result sets
- Caching for lookup data (school districts, charter schools)

### Data Volume Estimates
- Students: 1,000-10,000 records per charter school
- Payments: 100-1,000 records per month
- Reports: 50-500 generated per month
- Historical data: 5+ years retention

---

## Migration and Versioning

### Database Migrations
- Entity Framework Core migrations
- Version-controlled schema changes
- Data migration scripts for major changes
- Rollback procedures

### Data Import/Export
- Excel import for bulk data loading
- Export capabilities for reporting
- Data validation during import
- Error handling and reporting

This data model supports the complex billing relationships between charter schools and school districts while maintaining audit trails and supporting Pennsylvania Department of Education compliance requirements.
