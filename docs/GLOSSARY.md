# School District Billing System - Glossary of Terms

## Overview
This glossary provides definitions for all technical, educational, and business terms used in the School District Billing System. Terms are organized alphabetically with cross-references where applicable.

---

## A

### ADM (Average Daily Membership)
**Definition:** A calculation used to determine school funding based on student enrollment and attendance. Represents the average number of students enrolled in a school over a specific period.  
**Context:** Used in Pennsylvania school funding formulas to determine state aid allocations.  
**Related Terms:** Attendance, Enrollment, Funding Formula

### API (Application Programming Interface)
**Definition:** A set of protocols and tools for building software applications, allowing different software components to communicate.  
**Context:** Future enhancement for integrating with external systems like school district financial systems.  
**Related Terms:** Integration, Web Services

### ASP.NET Core
**Definition:** Microsoft's open-source web framework for building modern web applications and APIs.  
**Context:** The primary technology framework used to build the School District Billing System.  
**Related Terms:** MVC, C#, Framework

### Attendance
**Definition:** The number of days a student was present and enrolled in school during a specific period.  
**Context:** Used to calculate prorated billing when students enroll or exit mid-month.  
**Related Terms:** ADM, Days Attended, Enrollment Period

### AUN (Administrative Unit Number)
**Definition:** A unique nine-digit identifier assigned by the Pennsylvania Department of Education to each school district and charter school.  
**Context:** Used to identify which school district is responsible for paying tuition for each student.  
**Example:** 123456789  
**Related Terms:** School District, PDE, District of Residence

### Audit Trail
**Definition:** A chronological record of all system activities, changes, and transactions for compliance and security purposes.  
**Context:** Required for financial audits and regulatory compliance in educational billing.  
**Related Terms:** Compliance, Report History, Logging

---

## B

### Basic Education Funding
**Definition:** The primary funding stream for Pennsylvania public schools, including charter schools, based on student enrollment and district wealth.  
**Context:** The source of funds that school districts use to pay charter school tuition.  
**Related Terms:** Funding Formula, State Aid, Tuition

### Billing Cycle
**Definition:** The regular schedule for generating and sending invoices, typically monthly in the charter school context.  
**Context:** Charter schools bill school districts monthly for students enrolled during that month.  
**Related Terms:** Monthly Invoice, Invoice Generation

### Billing Rate
**Definition:** The daily amount charged for each student's education, set by the school district of residence.  
**Context:** Rates vary by district and are typically higher for special education students.  
**Related Terms:** Regular Rate, Special Education Rate, Tuition Rate

### Business Rules
**Definition:** Specific constraints and logic that govern how the system processes data and performs calculations.  
**Context:** Includes validation rules for student data, billing calculations, and compliance requirements.  
**Related Terms:** Validation, Data Integrity, System Logic

---

## C

### C# (C-Sharp)
**Definition:** Microsoft's object-oriented programming language used for building .NET applications.  
**Context:** The primary programming language used to develop the School District Billing System.  
**Related Terms:** .NET, ASP.NET Core, Programming Language

### Charter School
**Definition:** A publicly funded, independently operated school that operates under a charter granted by an authorizing entity.  
**Context:** The primary client type for the billing system, which bills school districts for enrolled students.  
**Related Terms:** Public School, Independent School, Charter Authorization

### CSR (Charter School Report)
**Definition:** Standardized reports that charter schools must submit to the Pennsylvania Department of Education.  
**Context:** The system generates several CSR reports including Student List, Direct Payment, and Tuition Rate reports.  
**Related Terms:** PDE Reports, Compliance Reporting, State Reporting

### CRUD Operations
**Definition:** Create, Read, Update, Delete - the four basic operations for managing data in a database.  
**Context:** All major entities in the system (students, schools, districts) support full CRUD operations.  
**Related Terms:** Database Operations, Data Management

---

## D

### Database Migration
**Definition:** The process of updating database schema and structure as the application evolves.  
**Context:** Entity Framework Core migrations are used to manage database changes in the billing system.  
**Related Terms:** Entity Framework, Schema Changes, Version Control

### Days Attended
**Definition:** The actual number of school days a student was enrolled and present during a billing period.  
**Context:** Used to calculate prorated billing for students who enroll or exit mid-month.  
**Related Terms:** Attendance, Days in Session, Prorated Billing

### Days in Session
**Definition:** The total number of school days in a given period according to the charter school's calendar.  
**Context:** Used as the denominator in attendance percentage calculations for billing.  
**Related Terms:** School Calendar, Days Attended, Attendance Percentage

### District Entry Date
**Definition:** The date a student first enrolled in the charter school.  
**Context:** Used to determine when billing should begin and for prorated calculations.  
**Related Terms:** Enrollment Date, Entry Date, Student Enrollment

### District of Residence
**Definition:** The school district where a student lives, which is responsible for paying the student's charter school tuition.  
**Context:** Determined by the student's home address and used to identify the paying district.  
**Related Terms:** AUN, School District, Residence Address

---

## E

### Entity Framework Core
**Definition:** Microsoft's object-relational mapping (ORM) framework for .NET applications.  
**Context:** Used for all database operations in the billing system, providing data access and migration capabilities.  
**Related Terms:** ORM, Database Access, LINQ

### EPPlus
**Definition:** A .NET library for reading and writing Excel files without requiring Microsoft Office.  
**Context:** Used for generating Excel reports, invoices, and importing data from Excel files.  
**Related Terms:** Excel Processing, Report Generation, File Import

### Exit Date
**Definition:** The date a student left the charter school.  
**Context:** Used to determine when billing should end and for prorated calculations.  
**Related Terms:** Withdrawal Date, Student Exit, Enrollment Period

---

## F

### FERPA (Family Educational Rights and Privacy Act)
**Definition:** Federal law that protects the privacy of student education records.  
**Context:** The billing system must comply with FERPA requirements for handling student data.  
**Related Terms:** Student Privacy, Data Protection, Compliance

### Funding Formula
**Definition:** The mathematical calculation used to determine how much funding each school receives.  
**Context:** Pennsylvania's funding formula determines the rates that districts pay to charter schools.  
**Related Terms:** Basic Education Funding, State Aid, Tuition Rates

---

## G

### Grade Level
**Definition:** The academic level of a student, typically ranging from Kindergarten (K) through 12th grade.  
**Context:** Used for organizing students and may affect billing rates or school calendar assignments.  
**Valid Values:** K, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12  
**Related Terms:** Academic Level, Student Classification

---

## I

### IDEA (Individuals with Disabilities Education Act)
**Definition:** Federal law ensuring students with disabilities receive appropriate public education.  
**Context:** Students covered under IDEA typically have IEPs and are billed at special education rates.  
**Related Terms:** Special Education, IEP, Disability Services

### IEP (Individual Education Program)
**Definition:** A written plan for students with disabilities that outlines special education services and goals.  
**Context:** Students with current IEPs are billed at special education rates, which are typically higher.  
**Related Terms:** Special Education, IDEA, Disability Services

### Invoice
**Definition:** A bill sent to a school district requesting payment for students enrolled from that district.  
**Context:** Generated monthly for each district with enrolled students, includes detailed student lists and calculations.  
**Related Terms:** Bill, Monthly Invoice, Billing

---

## L

### LEA (Local Education Agency)
**Definition:** A public board of education or other public authority that maintains administrative control of public schools.  
**Context:** Both school districts and charter schools are considered LEAs for reporting purposes.  
**Related Terms:** School District, Charter School, Educational Authority

### LINQ (Language Integrated Query)
**Definition:** A Microsoft .NET Framework component that adds query capabilities to .NET languages.  
**Context:** Used extensively in the billing system for database queries through Entity Framework.  
**Related Terms:** Database Queries, Entity Framework, Data Access

---

## M

### Monthly Invoice
**Definition:** The regular bill sent to school districts each month for students enrolled from that district.  
**Context:** The primary revenue generation mechanism for charter schools in the billing system.  
**Related Terms:** Invoice, Billing Cycle, Revenue

### MVC (Model-View-Controller)
**Definition:** An architectural pattern that separates application logic into three interconnected components.  
**Context:** The billing system follows MVC pattern with Models (data), Views (UI), and Controllers (logic).  
**Related Terms:** Architecture Pattern, ASP.NET Core, Software Design

### MySQL
**Definition:** An open-source relational database management system.  
**Context:** The database system used to store all billing system data including students, schools, and financial records.  
**Related Terms:** Database, Relational Database, Data Storage

---

## O

### OmniVest
**Definition:** The company that provides billing services to charter schools using this system.  
**Context:** The primary user organization that operates the billing system for multiple charter school clients.  
**Related Terms:** Service Provider, Billing Services

### ORM (Object-Relational Mapping)
**Definition:** A programming technique for converting data between incompatible type systems in object-oriented programming languages.  
**Context:** Entity Framework Core serves as the ORM for the billing system.  
**Related Terms:** Entity Framework, Database Mapping, Data Access

---

## P

### Payment
**Definition:** Money received from a school district in response to an invoice.  
**Context:** Tracked in the system to monitor accounts receivable and cash flow.  
**Related Terms:** Accounts Receivable, Cash Flow, Invoice

### PDE (Pennsylvania Department of Education)
**Definition:** The state agency responsible for overseeing public education in Pennsylvania.  
**Context:** Charter schools must submit various reports to PDE, which the billing system generates.  
**Related Terms:** State Agency, Compliance Reporting, Educational Oversight

### Prorated Billing
**Definition:** Billing calculation that adjusts the amount based on partial enrollment periods.  
**Context:** Used when students enroll or exit mid-month, billing only for days actually attended.  
**Related Terms:** Partial Month, Days Attended, Billing Calculation

---

## R

### Razor
**Definition:** Microsoft's view engine for ASP.NET Core that allows embedding C# code in HTML.  
**Context:** Used for all user interface pages in the billing system.  
**Related Terms:** View Engine, HTML, User Interface

### Reconciliation
**Definition:** The process of verifying that billing records match actual enrollment and payment data.  
**Context:** Performed monthly and annually to ensure billing accuracy and compliance.  
**Related Terms:** Verification, Audit, Accuracy Check

### Regular Education Rate
**Definition:** The daily billing rate for students who do not receive special education services.  
**Context:** The standard rate used for most students, typically lower than special education rates.  
**Related Terms:** Billing Rate, Tuition Rate, Standard Rate

### Report History
**Definition:** A record of all reports generated by the system, including when, by whom, and for what purpose.  
**Context:** Maintains audit trail for compliance and tracks report delivery to school districts.  
**Related Terms:** Audit Trail, Compliance, Report Tracking

---

## S

### School Calendar
**Definition:** The schedule of school days, holidays, and breaks for a charter school.  
**Context:** Used to calculate days in session for billing purposes and may vary by grade level.  
**Related Terms:** School Schedule, Days in Session, Academic Calendar

### School District
**Definition:** A local administrative unit that operates public schools within a specific geographic area.  
**Context:** The entities that pay charter school tuition for students residing in their boundaries.  
**Related Terms:** LEA, Public School District, Paying District

### Special Education Rate
**Definition:** The enhanced daily billing rate for students who receive special education services.  
**Context:** Typically 1.5 to 3 times higher than regular education rates to cover additional service costs.  
**Related Terms:** IEP Rate, Enhanced Rate, Special Education

### State Student Number
**Definition:** A unique identifier assigned to each student by the Pennsylvania Department of Education.  
**Context:** Used as the primary identifier for students in the billing system and for state reporting.  
**Related Terms:** Student ID, PDE Number, Unique Identifier

### Student
**Definition:** An individual enrolled in a charter school who generates billing to their district of residence.  
**Context:** The central entity in the billing system around which all calculations and reporting revolve.  
**Related Terms:** Pupil, Enrolled Student, Charter School Student

---

## T

### Tuition Rate
**Definition:** The amount charged per student per day for educational services.  
**Context:** Set by each school district and used to calculate monthly billing amounts.  
**Related Terms:** Billing Rate, Daily Rate, Educational Cost

---

## U

### Unipay
**Definition:** An electronic payment system used by some Pennsylvania school districts.  
**Context:** Some districts use Unipay for charter school payments; the system can generate Unipay request forms.  
**Related Terms:** Electronic Payment, Payment System

---

## V

### Validation
**Definition:** The process of checking data for accuracy, completeness, and compliance with business rules.  
**Context:** Applied to all data entry in the billing system to ensure data integrity and prevent errors.  
**Related Terms:** Data Validation, Business Rules, Error Prevention

### View Model
**Definition:** A class that contains data and logic specifically designed for a particular view or user interface.  
**Context:** Used throughout the billing system to present data in forms and reports.  
**Related Terms:** MVC Pattern, Data Presentation, User Interface

---

## Y

### Year-End Reconciliation
**Definition:** The annual process of verifying billing accuracy and generating comprehensive reports for the entire school year.  
**Context:** Required for compliance and audit purposes, comparing monthly billing to annual totals.  
**Related Terms:** Annual Reconciliation, Compliance Reporting, Audit Support

---

## Acronyms Quick Reference

| Acronym | Full Term | Definition |
|---------|-----------|------------|
| ADM | Average Daily Membership | Student enrollment metric for funding |
| API | Application Programming Interface | Software communication protocol |
| AUN | Administrative Unit Number | Unique school district identifier |
| CSR | Charter School Report | PDE compliance reports |
| FERPA | Family Educational Rights and Privacy Act | Student privacy law |
| IDEA | Individuals with Disabilities Education Act | Special education law |
| IEP | Individual Education Program | Special education plan |
| LEA | Local Education Agency | School district or charter school |
| LINQ | Language Integrated Query | .NET query language |
| MVC | Model-View-Controller | Software architecture pattern |
| ORM | Object-Relational Mapping | Database programming technique |
| PDE | Pennsylvania Department of Education | State education agency |

---

## Common Business Terms

### Billing Terms
- **Accounts Receivable:** Money owed by school districts for invoices sent
- **Aging Report:** Analysis of how long invoices have been outstanding
- **Collection Rate:** Percentage of invoices paid within specified timeframe
- **Outstanding Balance:** Amount still owed on unpaid invoices
- **Payment Terms:** Agreed-upon timeframe for payment (typically 30-60 days)

### Educational Terms
- **Charter Authorization:** Legal permission to operate a charter school
- **Enrollment Period:** Time during which a student is registered at the school
- **School Year:** Academic year, typically running from August/September to June
- **Special Education Services:** Additional support for students with disabilities
- **State Aid:** Funding provided by the state government to schools

### Technical Terms
- **Backup:** Copy of data stored for recovery purposes
- **Database:** Organized collection of structured information
- **Migration:** Process of moving or updating database structure
- **Query:** Request for specific data from a database
- **Validation:** Process of checking data accuracy and completeness

---

## Usage Notes

### Data Entry Standards
- **Dates:** Always use MM/DD/YYYY format
- **Names:** Enter full legal names as they appear on official documents
- **Addresses:** Use complete addresses including ZIP codes
- **Phone Numbers:** Use (XXX) XXX-XXXX format
- **AUN Numbers:** Always use 9-digit format

### Calculation Standards
- **Attendance:** Calculated as days attended divided by days in session
- **Prorated Billing:** Based on actual enrollment days within billing period
- **Rate Application:** Use rate effective on the billing date
- **Rounding:** Financial amounts rounded to nearest cent

### Reporting Standards
- **File Naming:** Use consistent naming convention with date stamps
- **Data Retention:** Maintain records according to regulatory requirements
- **Export Formats:** Excel for detailed reports, PDF for formal documents
- **Version Control:** Track all report versions and changes

This glossary serves as a comprehensive reference for all stakeholders working with the School District Billing System, ensuring consistent understanding and usage of terminology across the organization.
