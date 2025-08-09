# School District Billing System - Business Context & Domain Knowledge

## Business Overview

### Purpose and Mission
The School District Billing System serves as a comprehensive solution for managing the complex financial relationships between charter schools and Pennsylvania school districts. The system automates billing processes, ensures compliance with state regulations, and provides accurate reporting for the Pennsylvania Department of Education (PDE).

### Key Stakeholders

#### Primary Users
- **Charter School Administrators:** Manage student enrollment, generate invoices, track payments
- **Charter School Business Managers:** Handle billing operations, reconciliation, and financial reporting
- **OmniVest Staff:** Provide billing services to multiple charter schools

#### Secondary Stakeholders
- **School District Business Offices:** Receive invoices and make payments
- **Pennsylvania Department of Education (PDE):** Receives compliance reports
- **Auditors:** Review billing accuracy and compliance

### Business Problem Solved
Charter schools in Pennsylvania receive funding from the school districts where their students reside. This creates a complex billing scenario where:
- Each student may come from a different school district
- Billing rates vary by district and change over time
- Special education students have different (higher) rates
- Monthly invoicing and year-end reconciliation are required
- State compliance reporting is mandatory
- Payment tracking and follow-up is essential

## Pennsylvania Charter School Funding Model

### Legal Framework
Charter schools in Pennsylvania are funded through a combination of:
- **Basic Education Funding:** Based on student enrollment and district rates
- **Special Education Funding:** Higher rates for students with IEPs
- **Transportation Funding:** Separate from tuition (not handled by this system)

### Funding Flow
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ State of PA     │───►│ School District │───►│ Charter School  │
│ Allocates Funds │    │ Receives Funds  │    │ Bills District  │
│ to Districts    │    │ for Students    │    │ for Students    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Billing Rates
- **Regular Education Rate:** Daily rate per student for general education
- **Special Education Rate:** Enhanced daily rate for students with IEPs
- **Rate Determination:** Set by each school district, varies significantly
- **Rate Changes:** Typically annual, but can change mid-year

## Key Business Processes

### Student Enrollment Management

#### Student Data Requirements
- **State Student Number:** Unique identifier assigned by PDE
- **Demographics:** Name, address, date of birth, grade level
- **District of Residence:** Determines which district pays tuition
- **Entry/Exit Dates:** Determines billing periods
- **Special Education Status:** IEP dates determine special education billing

#### Enrollment Workflow
1. **Student Enrolls:** Charter school receives enrollment application
2. **Verification:** Confirm student's district of residence (AUN)
3. **Data Entry:** Enter student information into billing system
4. **Status Tracking:** Monitor enrollment status changes
5. **Exit Processing:** Record exit date when student leaves

### Monthly Billing Process

#### Billing Cycle Timeline
- **Month End:** Calculate attendance for completed month
- **First Week:** Generate invoices and student lists
- **Second Week:** Deliver invoices to school districts
- **Payment Period:** Districts have 30-60 days to pay
- **Follow-up:** Track payments and follow up on overdue amounts

#### Invoice Components
- **Student List:** All students from that district enrolled during the month
- **Attendance Calculation:** Days attended vs. days in session
- **Rate Application:** Current rates for regular and special education
- **Total Calculation:** Students × rates × attendance percentage

#### Special Considerations
- **Mid-Month Enrollment:** Prorated billing for partial month attendance
- **Special Education Changes:** Students entering/exiting special education
- **Calendar Variations:** Different grade levels may have different schedules
- **Holiday Adjustments:** Non-school days don't count toward billing

### Year-End Reconciliation

#### Purpose
- **Accuracy Verification:** Ensure all billing was correct throughout the year
- **State Compliance:** Meet PDE reporting requirements
- **Audit Preparation:** Provide documentation for financial audits
- **Payment Reconciliation:** Match payments received to invoices sent

#### Reconciliation Components
- **Annual Student Summary:** Complete enrollment history for each student
- **Days Attended Report:** Monthly breakdown of attendance by student
- **Payment Summary:** All payments received from each district
- **Rate History:** All rate changes throughout the year
- **Variance Analysis:** Differences between monthly billing and year-end totals

### PDE Compliance Reporting

#### Required Reports
- **CSR Student List:** Comprehensive student enrollment data
- **Direct Payment Report:** Payments made directly to charter schools
- **Tuition Rate Report:** Rates charged by each school district
- **Student List Reconciliation:** Verification of student data accuracy

#### Reporting Timeline
- **Monthly Reports:** Due within 30 days of month end
- **Annual Reports:** Due by specific PDE deadlines
- **Audit Reports:** As requested by PDE or auditors

#### Compliance Requirements
- **Data Accuracy:** All student information must be accurate and complete
- **Timeliness:** Reports must be submitted by required deadlines
- **Format Standards:** Reports must follow PDE-specified formats
- **Retention:** Records must be maintained for specified periods

## Business Rules and Calculations

### Attendance Calculations

#### Basic Formula
```
Monthly Billing = (Days Attended / Days in Session) × Daily Rate × Number of Students
```

#### Special Education Calculation
```
Regular Ed Billing = Regular Students × Regular Rate × Attendance Percentage
Special Ed Billing = Special Ed Students × Special Ed Rate × Attendance Percentage
Total Billing = Regular Ed Billing + Special Ed Billing
```

#### Attendance Scenarios
- **Full Month Attendance:** Student enrolled entire month = 1.0 attendance
- **Mid-Month Entry:** Student starts mid-month = prorated attendance
- **Mid-Month Exit:** Student leaves mid-month = prorated attendance
- **Special Ed Change:** Student IEP status changes = split calculation

### Rate Management

#### Rate Effective Dating
- **Effective Date:** Date new rate becomes active
- **Historical Rates:** Previous rates maintained for reconciliation
- **Rate Changes:** Can occur at any time, typically annually
- **Billing Impact:** Rate in effect on billing date is used

#### Rate Validation
- **Positive Values:** All rates must be greater than zero
- **Reasonable Ranges:** Rates typically between $10,000-$25,000 annually
- **Special Ed Premium:** Special education rates typically 1.5-3x regular rates
- **District Variations:** Rates vary significantly between districts

### Payment Processing

#### Payment Matching
- **Invoice Identification:** Match payments to specific invoices
- **Partial Payments:** Handle payments less than invoice amount
- **Overpayments:** Handle payments greater than invoice amount
- **Multiple Invoices:** Single payment covering multiple invoices

#### Payment Tracking
- **Payment Date:** When payment was received
- **Check Number:** For audit trail and bank reconciliation
- **Amount:** Exact payment amount received
- **Enrollment Month:** Which billing period payment covers

## Regulatory Environment

### Pennsylvania Department of Education (PDE)

#### Oversight Responsibilities
- **Charter Authorization:** Approves charter school operations
- **Funding Oversight:** Monitors proper use of public funds
- **Academic Standards:** Ensures educational quality
- **Financial Reporting:** Requires detailed financial reports

#### Compliance Requirements
- **Student Data Reporting:** Accurate enrollment and attendance data
- **Financial Reporting:** Detailed revenue and expenditure reports
- **Audit Compliance:** Cooperation with state and federal audits
- **Record Retention:** Maintain records for specified periods

### Federal Requirements

#### IDEA Compliance
- **Special Education:** Proper identification and services for students with disabilities
- **IEP Requirements:** Individual Education Programs must be properly documented
- **Funding Compliance:** Special education funding must be used appropriately

#### FERPA Compliance
- **Student Privacy:** Protect student educational records
- **Data Security:** Secure storage and transmission of student data
- **Access Controls:** Limit access to authorized personnel only

## Industry-Specific Terminology

### Educational Terms
- **AUN (Administrative Unit Number):** Unique identifier for each school district
- **Charter School:** Publicly funded, independently operated school
- **IEP (Individual Education Program):** Plan for special education students
- **LEA (Local Education Agency):** School district or charter school
- **PDE:** Pennsylvania Department of Education
- **SPED:** Special Education
- **State Student Number:** Unique identifier for each student in Pennsylvania

### Financial Terms
- **ADM (Average Daily Membership):** Student enrollment metric
- **Basic Education Funding:** Primary funding stream for schools
- **Reconciliation:** Process of verifying billing accuracy
- **Tuition Rate:** Daily rate charged for student enrollment
- **Unipay:** Electronic payment system used by some districts

### System-Specific Terms
- **Days Attended:** Actual days student was enrolled and school was in session
- **Days in Session:** Total school days in the billing period
- **District Entry Date:** When student enrolled in charter school
- **Exit Date:** When student left charter school
- **Grade Level:** Student's academic grade (K, 1-12)

## Business Challenges and Solutions

### Common Challenges

#### Data Accuracy
- **Challenge:** Ensuring student information is accurate and up-to-date
- **Solution:** Regular data validation and verification processes
- **System Support:** Built-in validation rules and error checking

#### Rate Management
- **Challenge:** Tracking rate changes across multiple districts
- **Solution:** Effective-dated rate system with historical tracking
- **System Support:** Automated rate lookup based on billing date

#### Payment Tracking
- **Challenge:** Matching payments to invoices across multiple districts
- **Solution:** Detailed payment tracking with reference information
- **System Support:** Payment matching and reconciliation tools

#### Compliance Reporting
- **Challenge:** Meeting various state and federal reporting requirements
- **Solution:** Automated report generation with standardized formats
- **System Support:** Template-based reporting with data validation

### Best Practices

#### Data Management
- **Regular Backups:** Protect against data loss
- **Data Validation:** Verify accuracy before processing
- **Audit Trails:** Maintain complete history of changes
- **Access Controls:** Limit system access to authorized users

#### Financial Controls
- **Segregation of Duties:** Separate billing and payment functions
- **Regular Reconciliation:** Monthly and annual reconciliation processes
- **Documentation:** Maintain supporting documentation for all transactions
- **Review Procedures:** Regular review of billing accuracy

#### Compliance Management
- **Deadline Tracking:** Monitor all reporting deadlines
- **Quality Assurance:** Review reports before submission
- **Record Retention:** Maintain records per regulatory requirements
- **Training:** Keep staff updated on regulatory changes

## Success Metrics

### Financial Metrics
- **Collection Rate:** Percentage of invoices paid within terms
- **Billing Accuracy:** Percentage of invoices without errors
- **Processing Time:** Time from month-end to invoice delivery
- **Payment Cycle:** Average time from invoice to payment

### Operational Metrics
- **Data Quality:** Percentage of student records with complete information
- **Report Timeliness:** Percentage of reports submitted on time
- **System Uptime:** Availability of billing system
- **User Satisfaction:** Feedback from charter school users

### Compliance Metrics
- **Audit Results:** Number of audit findings or exceptions
- **Regulatory Compliance:** Percentage of requirements met
- **Report Accuracy:** Percentage of reports accepted without revision
- **Deadline Performance:** Percentage of deadlines met

This business context provides the foundation for understanding the complex regulatory and financial environment in which the School District Billing System operates. The system must balance accuracy, compliance, and efficiency while serving the needs of multiple stakeholders in the Pennsylvania charter school ecosystem.
