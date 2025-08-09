# School District Billing System

A comprehensive ASP.NET Core web application designed to manage billing operations between charter schools and school districts. This system handles student enrollment tracking, billing rate management, invoice generation, and regulatory reporting for Pennsylvania Department of Education (PDE) compliance.

## Overview

The School District Billing System is built for OmniVest to streamline the complex billing processes between charter schools and their associated school districts. The application manages student data, calculates billing amounts based on district-specific rates, generates invoices, and produces various reports required for state compliance.

## Features

### Core Functionality
- **Student Management**: Track student enrollment, demographics, and school district assignments
- **Charter School Administration**: Manage charter school information, contacts, and schedules
- **School District Management**: Maintain school district data, contacts, and billing rates
- **Billing Rate Management**: Configure and track time-sensitive billing rates for each school district
- **Payment Tracking**: Record and monitor payments from school districts

### Reporting & Invoicing
- **Monthly Invoice Generation**: Automated invoice creation based on student enrollment and billing rates
- **Year-End Reconciliation**: Comprehensive year-end reporting and reconciliation processes
  - **Days Attended Reports**: Monthly attendance tracking for each student throughout the school year
- **PDE Compliance Reports**: Generate required reports for Pennsylvania Department of Education
  - Student List Reports
  - Direct Payment Reports
  - Tuition Rate Reports
  - Student List Reconciliation
- **Excel Export**: All reports exportable to Excel format for further analysis

### Data Import/Export
- **Excel Import**: Bulk import of school district rates and student data
- **Report Templates**: Pre-configured Excel templates for consistent reporting
- **Archive Management**: Organized file storage and retrieval system

## Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: MySQL with Entity Framework Core
- **Frontend**: Bootstrap 4, jQuery, HTML5/CSS3
- **Excel Processing**: EPPlus library for Excel file generation and manipulation
- **Architecture**: MVC (Model-View-Controller) pattern

## Project Structure

```
SchoolDistrictBilling/
├── Controllers/           # MVC Controllers
│   ├── CharterSchoolsController.cs
│   ├── StudentsController.cs
│   ├── SchoolDistrictsController.cs
│   ├── PaymentsController.cs
│   └── ...
├── Models/               # Data models and view models
│   ├── Student.cs
│   ├── CharterSchool.cs
│   ├── SchoolDistrict.cs
│   └── ...
├── Views/                # Razor views for UI
│   ├── CharterSchools/
│   ├── Students/
│   ├── SchoolDistricts/
│   └── ...
├── Data/                 # Database context and configurations
│   └── AppDbContext.cs
├── Services/             # Business logic services
│   ├── ExcelServices.cs
│   ├── FileSystemServices.cs
│   └── DateServices.cs
├── Reports/              # Report generation classes
│   ├── PdeCsrStudentList.cs
│   ├── PdeCsrDirectPayment.cs
│   └── ...
└── wwwroot/             # Static files, CSS, JS, images
    ├── css/
    ├── js/
    ├── lib/
    └── reportTemplates/
```

## Prerequisites

- .NET 8.0 SDK
- MySQL Server 8.0+
- Visual Studio 2022 or Visual Studio Code
- Git

## Installation & Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd SdBilling
   ```

2. **Database Setup**
   - Install MySQL Server
   - Create a database named `omni_test` (or update connection string)
   - Update connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "Db": "Server=localhost;Database=omni_test;Uid=your_username;Pwd=your_password"
     }
   }
   ```

3. **Install Dependencies**
   ```bash
   cd SchoolDistrictBilling
   dotnet restore
   ```

4. **Run Database Migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the Application**
   ```bash
   dotnet run
   ```

6. **Access the Application**
   - Navigate to `https://localhost:5001` or `http://localhost:5000`

## Configuration

### Database Connection
Update the connection string in `appsettings.json` to match your MySQL server configuration:

```json
{
  "ConnectionStrings": {
    "Db": "Server=your_server;Database=your_database;Uid=your_username;Pwd=your_password"
  }
}
```

### Development vs Production
- Development settings: `appsettings.Development.json`
- Production settings: `appsettings.json`

## Usage

### Getting Started
1. **Setup Charter Schools**: Add charter school information and contacts
2. **Configure School Districts**: Add school districts and their billing rates
3. **Import Student Data**: Use Excel import functionality to bulk load student information
4. **Generate Reports**: Create monthly invoices and compliance reports
5. **Track Payments**: Record payments received from school districts

### Key Workflows

#### Monthly Billing Process
1. Navigate to Monthly Invoice section
2. Select charter school and reporting month
3. Choose target school districts
4. Generate invoices and student lists
5. Export to Excel for distribution

#### Year-End Reconciliation
1. Access Year-End Reconciliation module
2. Select charter school and academic year
3. Generate reconciliation reports
4. Export final reports for state submission

#### PDE Reporting
1. Use PDE-specific report generators
2. Select appropriate report type (Student List, Direct Payment, etc.)
3. Configure date ranges and criteria
4. Generate and export reports in required format

## Key Models

- **Student**: Core student information including demographics and enrollment data
- **CharterSchool**: Charter school details, contacts, and schedules
- **SchoolDistrict**: School district information and billing rates
- **Payment**: Payment tracking and history
- **ReportHistory**: Audit trail for generated reports

## API Endpoints

The application follows standard MVC routing patterns:
- `/CharterSchools` - Charter school management
- `/Students` - Student data management
- `/SchoolDistricts` - School district administration
- `/Payments` - Payment tracking
- `/MonthlyInvoice` - Invoice generation
- `/YearEndRecon` - Year-end reconciliation

## Development

### Adding New Features
1. Create or modify models in `/Models`
2. Update database context in `/Data/AppDbContext.cs`
3. Add/modify controllers in `/Controllers`
4. Create/update views in `/Views`
5. Add business logic to `/Services`

### Database Changes
1. Modify models
2. Add migration: `dotnet ef migrations add MigrationName`
3. Update database: `dotnet ef database update`

### Testing
- Unit tests can be added in a separate test project
- Integration tests should cover controller actions and database operations

## Deployment

### Production Deployment
1. Update `appsettings.json` with production database connection
2. Build the application: `dotnet build --configuration Release`
3. Publish: `dotnet publish --configuration Release`
4. Deploy to web server (IIS, Apache, etc.)
5. Ensure MySQL server is accessible from production environment

### Environment Variables
Consider using environment variables for sensitive configuration:
- Database connection strings
- API keys
- File storage paths

## Security Considerations

- Database credentials should be stored securely
- Implement proper authentication and authorization
- Validate all user inputs
- Use HTTPS in production
- Regular security updates for dependencies

## Troubleshooting

### Common Issues
1. **Database Connection Errors**: Verify MySQL server is running and connection string is correct
2. **Excel Export Issues**: Ensure EPPlus library is properly installed
3. **File Permission Errors**: Check write permissions for report output directories

### Logging
The application uses ASP.NET Core's built-in logging. Check logs for detailed error information.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

[Specify license information]

## Support

For technical support or questions about the School District Billing System, please contact the development team or create an issue in the repository.

## Changelog

### Version History
- **Current**: ASP.NET Core 8.0 implementation with MySQL support
- Features include comprehensive reporting, Excel integration, and PDE compliance tools

---

*This application is designed specifically for charter school billing operations in Pennsylvania and includes state-specific reporting requirements.*
