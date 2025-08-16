# School District Billing System - User Guide

## Table of Contents
1. [System Overview](#system-overview)
2. [Getting Started](#getting-started)
3. [Charter School Management](#charter-school-management)
4. [School District Management](#school-district-management)
5. [Student Management](#student-management)
6. [Billing Rate Management](#billing-rate-management)
7. [Payment Tracking](#payment-tracking)
8. [Monthly Invoice Generation](#monthly-invoice-generation)
9. [Year-End Reconciliation](#year-end-reconciliation)
10. [Report Generation](#report-generation)
11. [File Management](#file-management)
12. [Troubleshooting](#troubleshooting)

---

## System Overview

The School District Billing System is a comprehensive web application designed to manage billing operations between charter schools and school districts. The system handles student enrollment tracking, billing rate management, invoice generation, and regulatory reporting for Pennsylvania Department of Education (PDE) compliance.

### Key Features
- Student enrollment and demographic tracking
- Charter school and school district administration
- Automated billing calculations based on district-specific rates
- Monthly invoice generation
- Year-end reconciliation reporting
- PDE compliance reporting
- Payment tracking and monitoring
- Excel-based report generation and export

### System Requirements
- Web browser (Chrome, Firefox, Safari, or Edge)
- Internet connection
- Excel or compatible spreadsheet software for viewing reports

---

## Getting Started

### Accessing the System
1. Open your web browser
2. Navigate to the system URL provided by your administrator
3. The system will load the main dashboard

### Navigation
The system uses a standard web interface with:
- **Navigation Menu**: Located at the top of the page
- **Main Content Area**: Displays current page content
- **Action Buttons**: Located throughout the interface for specific functions

### Main Menu Options
- **Home**: System dashboard and overview
- **Charter Schools**: Manage charter school information
- **Students**: Student enrollment and data management
- **School Districts**: School district information and contacts
- **School District Rates**: Billing rate management
- **Payments**: Payment tracking and history
- **Monthly Invoice**: Generate monthly billing invoices
- **Year End Recon**: Year-end reconciliation reporting

---

## Charter School Management

### Adding a New Charter School
1. Click **Charter Schools** in the main menu
2. Click **Create New** button
3. Fill in the required information:
   - Charter School Name
   - Address (Street, City, State, ZIP)
   - Phone Number
   - Contact Information
4. Click **Create** to save

### Editing Charter School Information
1. Navigate to **Charter Schools**
2. Click **Edit** next to the charter school you want to modify
3. Update the necessary fields
4. Click **Save** to apply changes

### Managing Charter School Contacts
1. From the Charter Schools list, click **Details** for the desired school
2. In the Contacts section, click **Add Contact**
3. Enter contact information:
   - Name
   - Title/Position
   - Email Address
   - Phone Number
4. Click **Save Contact**

### Setting Up School Schedules
1. Access the charter school details page
2. Navigate to the **Schedule** section
3. Click **Add Schedule**
4. Configure:
   - School Year
   - Start and End Dates
   - Days in Session
   - Holiday Schedules
5. Save the schedule information

---

## School District Management

### Adding School Districts
1. Click **School Districts** in the main menu
2. Click **Create New**
3. Enter required information:
   - District Name
   - AUN (Administrative Unit Number)
   - Address Information
   - Contact Details
4. Click **Create** to save

### Managing District Contacts
1. From School Districts list, select **Details** for the district
2. In the Contacts section, add or edit contact information
3. Include:
   - Primary Contact Name
   - Business Office Contact
   - Email Addresses
   - Phone Numbers
4. Save contact information

### District Information Management
- Keep district information current and accurate
- Ensure AUN numbers are correct for proper billing
- Maintain up-to-date contact information for invoice delivery

---

## Student Management

### Adding New Students
1. Navigate to **Students** in the main menu
2. Click **Create New Student**
3. Complete the student information form:
   - Personal Information (Name, Date of Birth, Gender)
   - State Student Number
   - School District Assignment (AUN)
   - Charter School Assignment
   - Enrollment Dates
   - Special Education Status
4. Click **Create** to save the student record

### Editing Student Information
1. From the Students list, click **Edit** next to the student
2. Modify necessary fields
3. Pay special attention to:
   - Enrollment and withdrawal dates
   - School district changes
   - Special education status updates
4. Click **Save** to apply changes

### Student Search and Filtering
- Use the search box to find students by name or State Student Number
- Filter by charter school or school district
- Sort by various columns (Name, Enrollment Date, District)

### Managing Student Enrollment
- **Enrollment Date**: When the student started at the charter school
- **Withdrawal Date**: When the student left (if applicable)
- **District Changes**: Update if student moves between districts
- **SPED Status**: Maintain accurate special education classifications

### Bulk Student Import
1. Navigate to **Students**
2. Click **Import Students** (if available)
3. Download the Excel template
4. Fill in student information following the template format
5. Upload the completed file
6. Review and confirm the import

---

## Billing Rate Management

### Understanding Billing Rates
Billing rates determine how much charter schools charge school districts for each student. Rates vary by:
- School District
- Student Type (Regular Education vs. Special Education)
- Time Period (rates can change annually)

### Adding New Billing Rates
1. Go to **School District Rates**
2. Click **Create New Rate**
3. Select the school district
4. Enter rate information:
   - Effective Date (when the rate becomes active)
   - Regular Education Rate (per student)
   - Special Education Rate (per student)
   - Any additional fees or adjustments
4. Click **Save**

### Managing Rate Changes
1. Rates typically change annually
2. Enter new rates with future effective dates
3. The system will automatically use the correct rate based on the billing period
4. Keep historical rates for accurate reconciliation

### Rate Import Process
1. Use the Excel import feature for bulk rate updates
2. Download the rate template
3. Fill in all required fields
4. Upload and verify the data before confirming

---

## Payment Tracking

### Recording Payments
1. Navigate to **Payments**
2. Click **Add Payment**
3. Enter payment details:
   - School District (payer)
   - Charter School (recipient)
   - Payment Amount
   - Payment Date
   - Check Number or Reference
   - Payment Method
4. Click **Save Payment**

### Payment History
- View all payments by date range
- Filter by school district or charter school
- Export payment reports to Excel
- Track outstanding balances

### Payment Reconciliation
- Compare payments received to invoices sent
- Identify overdue accounts
- Generate payment summary reports
- Track partial payments and adjustments

---

## Monthly Invoice Generation

### Generating Monthly Invoices
1. Click **Monthly Invoice** in the main menu
2. Select the charter school
3. Choose the billing month and year
4. Select target school districts (or choose "All")
5. Choose submission destination:
   - **School**: Send directly to school districts
   - **PDE**: Submit to Pennsylvania Department of Education
6. Click **Generate Invoice**

### Invoice Components
Generated invoices include:
- **Invoice Summary**: Total amounts by district
- **Student Lists**: Detailed student enrollment information
- **Billing Calculations**: Rate applications and totals
- **Supporting Documentation**: Attendance and enrollment details

### Invoice Review Process
1. Review generated invoices for accuracy
2. Verify student counts and billing rates
3. Check enrollment dates and special education classifications
4. Confirm district assignments
5. Download and distribute invoices

### Invoice Distribution
- Invoices are generated as Excel files
- Download the ZIP file containing all invoices
- Distribute to appropriate school districts
- Maintain copies for your records

---

## Year-End Reconciliation

### Understanding Year-End Reconciliation
Year-end reconciliation is the process of:
- Summarizing the entire school year's billing
- Reconciling payments received with invoices sent
- Generating required state reporting
- Preparing final documentation for audit purposes

### Generating Year-End Reports
1. Navigate to **Year End Recon**
2. Select the charter school
3. Choose the school year (e.g., "2023-2024")
4. Select school districts to include
5. Choose submission type:
   - **School**: For district distribution
   - **PDE**: For state submission
6. Click **Generate**

### Year-End Report Components
The system generates multiple reports:
- **Reconciliation Summary**: Annual totals by district
- **Student Lists**: Complete enrollment records for the year
- **Days Attended Reports**: Monthly attendance tracking (if enabled)
- **Payment Reconciliation**: Payments vs. invoices comparison
- **PDE Compliance Reports**: Required state reporting formats

### Days Attended Report Configuration
The Days Attended Report can be enabled or disabled:
- Contact your system administrator to enable this feature
- When enabled, provides detailed monthly attendance tracking
- Useful for audit purposes and detailed reconciliation

### Review and Submission Process
1. Download and review all generated reports
2. Verify accuracy of student counts and financial totals
3. Reconcile with your internal records
4. Submit to school districts and/or PDE as required
5. Archive reports for future reference

---

## Report Generation

### Available Report Types

#### Monthly Reports
- **Monthly Invoices**: Billing statements for school districts
- **Student Enrollment Reports**: Current enrollment by district
- **Payment Summary Reports**: Monthly payment tracking

#### Annual Reports
- **Year-End Reconciliation**: Complete annual summary
- **Student List Reports**: Annual enrollment documentation
- **Days Attended Reports**: Monthly attendance tracking
- **Payment Reconciliation**: Annual payment vs. billing comparison

#### PDE Compliance Reports
- **CSR Student List**: Required student enrollment reporting
- **CSR Direct Payment**: Direct payment documentation
- **CSR Tuition Rate**: Rate reporting for state compliance
- **Student List Reconciliation**: Enrollment verification

### Generating Custom Reports
1. Navigate to the appropriate report section
2. Select date ranges and criteria
3. Choose output format (typically Excel)
4. Generate and download reports
5. Review for accuracy before distribution

### Report Export and Distribution
- All reports export to Excel format
- Multiple reports are packaged in ZIP files
- Download and extract files as needed
- Distribute to appropriate recipients
- Maintain copies for record-keeping

---

## File Management

### Understanding File Organization
The system organizes files by:
- Charter School
- School Year
- Report Type
- School District (when applicable)

### Downloading Reports
1. After generating reports, click the download link
2. Save the ZIP file to your computer
3. Extract files to access individual reports
4. Organize files in your local filing system

### File Naming Conventions
Files are named systematically:
- **Invoices**: `[Year][District]Invoice.xlsx`
- **Student Lists**: `[Year][District]Student.xlsx`
- **Reconciliation**: `[Year][District]YearEnd.xlsx`
- **Days Attended**: `[Year][District]DaysAttended.xlsx`

### Archive Management
- Keep copies of all generated reports
- Organize by school year for easy retrieval
- Maintain backup copies of critical reports
- Follow your organization's document retention policies

---

## Troubleshooting

### Common Issues and Solutions

#### Login or Access Problems
**Problem**: Cannot access the system
**Solution**: 
- Verify the correct URL
- Check internet connection
- Contact your system administrator
- Clear browser cache and cookies

#### Student Data Issues
**Problem**: Student not appearing in reports
**Solution**:
- Verify student enrollment dates
- Check school district assignment
- Ensure student is active in the system
- Confirm charter school assignment

#### Billing Rate Problems
**Problem**: Incorrect billing amounts
**Solution**:
- Verify current billing rates are entered
- Check effective dates on rates
- Ensure rates are assigned to correct districts
- Review special education classifications

#### Report Generation Errors
**Problem**: Reports fail to generate or are incomplete
**Solution**:
- Check that all required data is entered
- Verify date ranges are correct
- Ensure students have proper district assignments
- Contact administrator if problems persist

#### Payment Tracking Issues
**Problem**: Payments not reflecting correctly
**Solution**:
- Verify payment dates and amounts
- Check school district assignments
- Ensure payments are posted to correct accounts
- Review payment method and reference numbers

### Getting Help
- Contact your system administrator for technical issues
- Refer to this user guide for operational questions
- Keep records of error messages for troubleshooting
- Document any recurring issues for system improvements

### Best Practices
- **Regular Data Backup**: Ensure important data is backed up regularly
- **Accurate Data Entry**: Double-check all entries for accuracy
- **Timely Updates**: Keep student and rate information current
- **Report Review**: Always review reports before distribution
- **Record Keeping**: Maintain organized files and documentation
- **Training**: Ensure all users are properly trained on system functions

---

## System Maintenance and Updates

### Regular Maintenance Tasks
- Review and update student enrollment status monthly
- Update billing rates annually or as needed
- Reconcile payments regularly
- Archive old reports and data
- Review and clean up inactive records

### Data Quality Management
- Regularly audit student data for accuracy
- Verify school district assignments
- Check for duplicate records
- Ensure enrollment and withdrawal dates are correct
- Maintain accurate special education classifications

### Preparing for Audits
- Keep complete records of all transactions
- Maintain supporting documentation
- Ensure all reports are properly archived
- Document any data corrections or adjustments
- Prepare reconciliation summaries

---

## Contact Information

For technical support or questions about the School District Billing System:

- **System Administrator**: [Contact information to be provided]
- **Technical Support**: [Contact information to be provided]
- **Training Questions**: [Contact information to be provided]

For questions about billing rates, student data, or operational procedures, contact your organization's designated system administrator.

---

*This user guide covers the primary functions of the School District Billing System. For additional features or advanced functionality, consult with your system administrator or refer to supplementary documentation.*

**Document Version**: 1.0  
**Last Updated**: [Current Date]  
**System Version**: ASP.NET Core 8.0
