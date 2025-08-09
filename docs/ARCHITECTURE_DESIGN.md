# School District Billing System - Architecture & Design Documentation

## System Overview

The School District Billing System is a web-based ASP.NET Core MVC application designed to manage the complex billing relationships between charter schools and Pennsylvania school districts. The system handles student enrollment tracking, billing calculations, invoice generation, and regulatory compliance reporting.

## Architecture Patterns

### Model-View-Controller (MVC)
The application follows the standard ASP.NET Core MVC pattern:

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│     Models      │    │   Controllers   │    │     Views       │
│                 │    │                 │    │                 │
│ • Student       │◄───┤ • StudentsCtrl  ├───►│ • Student Forms │
│ • CharterSchool │    │ • CharterSchools│    │ • Lists/Grids   │
│ • SchoolDistrict│    │ • Payments      │    │ • Reports       │
│ • Payment       │    │ • Reports       │    │ • Layouts       │
│ • Rates         │    │                 │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Layered Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  Controllers, Views, ViewModels, Client-side JavaScript    │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                    Business Logic Layer                     │
│     Services, Domain Models, Business Rules                │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                    Data Access Layer                        │
│        Entity Framework Core, AppDbContext                 │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                    Database Layer                           │
│                    MySQL Database                          │
└─────────────────────────────────────────────────────────────┘
```

## Technology Stack

### Backend Technologies
- **Framework:** ASP.NET Core 8.0
- **Language:** C# 12
- **Database:** MySQL 8.0+
- **ORM:** Entity Framework Core
- **Dependency Injection:** Built-in ASP.NET Core DI Container

### Frontend Technologies
- **UI Framework:** Bootstrap 4
- **JavaScript:** jQuery
- **Templating:** Razor Views
- **CSS:** Custom CSS with Bootstrap

### External Libraries
- **Excel Processing:** EPPlus (OfficeOpenXml)
- **File Compression:** System.IO.Compression
- **Date/Time:** Built-in .NET DateTime

### Development Tools
- **IDE:** Visual Studio 2022 / Visual Studio Code
- **Version Control:** Git
- **Package Manager:** NuGet
- **Build System:** MSBuild

## Project Structure

```
SchoolDistrictBilling/
├── Controllers/              # MVC Controllers
│   ├── HomeController.cs
│   ├── StudentsController.cs
│   ├── CharterSchoolsController.cs
│   ├── SchoolDistrictsController.cs
│   ├── PaymentsController.cs
│   ├── MonthlyInvoiceController.cs
│   └── YearEndReconController.cs
├── Models/                   # Domain Models & ViewModels
│   ├── Student.cs
│   ├── CharterSchool.cs
│   ├── SchoolDistrict.cs
│   ├── Payment.cs
│   ├── SchoolDistrictRate.cs
│   └── ViewModels/
├── Views/                    # Razor Views
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── Error.cshtml
│   ├── Students/
│   ├── CharterSchools/
│   └── [Other Controllers]/
├── Data/                     # Data Access Layer
│   └── AppDbContext.cs
├── Services/                 # Business Logic Services
│   ├── ExcelServices.cs
│   ├── FileSystemServices.cs
│   └── DateServices.cs
├── Reports/                  # Report Generation Classes
│   ├── PdeCsrStudentList.cs
│   ├── DaysAttendedReport.cs
│   └── [Other Reports]/
├── wwwroot/                  # Static Files
│   ├── css/
│   ├── js/
│   ├── lib/
│   ├── reportTemplates/
│   └── temp/
├── Properties/
├── appsettings.json
├── Program.cs
└── Startup.cs
```

## Design Patterns

### Repository Pattern (Implicit)
While not explicitly implemented as separate repository classes, the Entity Framework DbContext serves as a repository pattern implementation:

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Student> Students { get; set; }
    public DbSet<CharterSchool> CharterSchools { get; set; }
    
    // Custom query methods
    public List<string> GetAunsForCharterSchool(int charterSchoolUid)
    public List<Student> GetStudents(int charterSchoolUid, string aun)
}
```

### Service Layer Pattern
Business logic is encapsulated in service classes:

```csharp
public class ExcelServices
{
    public static List<string> GenerateMonthlyInvoice(...)
    public static List<string> GenerateYearEndRecon(...)
    public static List<SchoolDistrictRateView> ImportSchoolDistrictRates(...)
}
```

### Factory Pattern
Report generation uses factory-like patterns:

```csharp
public class PdeCsrStudentList
{
    public string Generate(ReportCriteriaView criteria)
}
```

### Template Method Pattern
Report generation follows template method pattern:
1. Load template
2. Populate data
3. Apply formatting
4. Save file

### Dependency Injection
ASP.NET Core's built-in DI container manages dependencies:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<AppDbContext>(o => o.UseMySQL(connectionString));
    services.AddControllersWithViews();
}
```

## Data Flow Architecture

### Request Processing Flow

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Browser   │───►│ Controller  │───►│   Service   │───►│  Database   │
│             │    │             │    │             │    │             │
│ HTTP Request│    │ Action      │    │ Business    │    │ MySQL       │
│             │    │ Method      │    │ Logic       │    │             │
└─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘
       ▲                   │                   │                   │
       │                   ▼                   ▼                   ▼
┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Browser   │◄───│    View     │◄───│   Model     │◄───│ Entity      │
│             │    │             │    │             │    │ Framework   │
│ HTTP Response│   │ Razor       │    │ ViewModel   │    │             │
│             │    │ Template    │    │             │    │             │
└─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘
```

### Report Generation Flow

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   User      │───►│ Controller  │───►│ Excel       │
│ Selects     │    │ Validates   │    │ Service     │
│ Criteria    │    │ Input       │    │             │
└─────────────┘    └─────────────┘    └─────────────┘
                           │                   │
                           ▼                   ▼
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│ File        │◄───│ ZIP Archive │◄───│ Template    │
│ Download    │    │ Creation    │    │ Population  │
│             │    │             │    │             │
└─────────────┘    └─────────────┘    └─────────────┘
```

## Security Architecture

### Current Security Model
- **Authentication:** Not currently implemented
- **Authorization:** Not currently implemented
- **Data Protection:** 
  - SQL Injection protection via Entity Framework parameterized queries
  - XSS protection via Razor automatic encoding
  - CSRF protection via anti-forgery tokens

### Recommended Security Enhancements
```csharp
// Future authentication implementation
services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Authorization policies
services.AddAuthorization(options => {
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CharterSchoolUser", policy => 
        policy.RequireRole("Admin", "CharterSchool"));
});
```

## Database Design

### Connection Management
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<AppDbContext>(options =>
        options.UseMySQL(Configuration.GetConnectionString("Db")));
}
```

### Migration Strategy
- Entity Framework Core Code-First migrations
- Version-controlled schema changes
- Automated deployment scripts

### Performance Optimization
- Lazy loading disabled for predictable performance
- Explicit Include() statements for related data
- Pagination for large datasets
- Indexed columns for frequent queries

## File Management Architecture

### File Storage Structure
```
wwwroot/
├── reportTemplates/          # Excel templates
│   ├── MonthlyInvoice.xlsx
│   ├── YearEndReconciliation.xlsx
│   └── [Other templates]
├── temp/                     # Generated files (temporary)
│   └── [Generated reports]
├── archive/                  # Compressed downloads
│   └── [ZIP files]
└── reports/                  # Permanent storage
    └── [Archived reports]
```

### File Processing Pipeline
1. **Template Loading:** Load Excel template from reportTemplates/
2. **Data Population:** Use EPPlus to populate template with database data
3. **File Generation:** Save populated file to temp/ directory
4. **Archive Creation:** Compress multiple files into ZIP archive
5. **Download Delivery:** Serve ZIP file to user
6. **Cleanup:** Remove temporary files after download

## Error Handling Architecture

### Exception Handling Strategy
```csharp
// Global exception handling
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }
}
```

### Validation Architecture
- **Model Validation:** Data annotations on model properties
- **Business Rule Validation:** Custom validation methods in models
- **Client-Side Validation:** jQuery validation with unobtrusive validation
- **Server-Side Validation:** ModelState validation in controllers

## Performance Architecture

### Caching Strategy
- **No explicit caching currently implemented**
- **Recommended additions:**
  - Memory caching for lookup data (school districts, charter schools)
  - Response caching for static content
  - Database query result caching

### Scalability Considerations
- **Database Connection Pooling:** Handled by Entity Framework
- **Stateless Design:** Controllers are stateless for horizontal scaling
- **File Storage:** Local file system (consider cloud storage for scale)
- **Background Processing:** Consider implementing for large report generation

## Integration Architecture

### External System Integration Points
- **Pennsylvania Department of Education (PDE):** Report submission
- **School District Systems:** Potential future integration
- **Accounting Systems:** Potential future integration

### API Design (Future)
```csharp
// Potential REST API endpoints
[ApiController]
[Route("api/[controller]")]
public class StudentsApiController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Student>> GetStudents()
    
    [HttpPost]
    public ActionResult<Student> CreateStudent(Student student)
}
```

## Deployment Architecture

### Development Environment
- **Local Development:** IIS Express or Kestrel
- **Database:** Local MySQL instance
- **File Storage:** Local file system

### Production Environment (Recommended)
- **Web Server:** IIS or Linux with Kestrel
- **Database:** MySQL Server (dedicated instance)
- **File Storage:** Network storage or cloud storage
- **Load Balancing:** Consider for high availability

### Configuration Management
```json
{
  "ConnectionStrings": {
    "Db": "Server=prod-server;Database=billing_prod;Uid=app_user;Pwd=secure_password"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error"
    }
  }
}
```

## Monitoring and Logging

### Current Logging
- **ASP.NET Core Logging:** Built-in logging framework
- **Console Logging:** Development environment
- **File Logging:** Not currently implemented

### Recommended Monitoring
```csharp
// Enhanced logging configuration
public void ConfigureServices(IServiceCollection services)
{
    services.AddLogging(builder =>
    {
        builder.AddConsole();
        builder.AddFile("logs/app-{Date}.txt");
        builder.AddApplicationInsights();
    });
}
```

### Key Metrics to Monitor
- **Application Performance:** Response times, error rates
- **Database Performance:** Query execution times, connection pool usage
- **File Operations:** Report generation times, file sizes
- **User Activity:** Login attempts, report downloads

## Testing Architecture

### Current Testing Status
- **Unit Tests:** Not currently implemented
- **Integration Tests:** Not currently implemented
- **Manual Testing:** Primary testing method

### Recommended Testing Strategy
```csharp
// Unit test example
[TestClass]
public class StudentTests
{
    [TestMethod]
    public void Student_IsValid_ReturnsTrueForValidStudent()
    {
        // Arrange
        var student = new Student { /* valid properties */ };
        
        // Act
        var result = student.IsValid(mockContext, out string error);
        
        // Assert
        Assert.IsTrue(result);
    }
}
```

## Future Architecture Considerations

### Microservices Migration
- **Student Service:** Student management and enrollment
- **Billing Service:** Rate management and calculations
- **Reporting Service:** Report generation and delivery
- **Notification Service:** Email and communication

### Cloud Migration
- **Azure App Service:** Web application hosting
- **Azure SQL Database:** Database hosting
- **Azure Blob Storage:** File storage
- **Azure Service Bus:** Message queuing

### API-First Design
- **REST APIs:** For external integrations
- **GraphQL:** For flexible data querying
- **OpenAPI/Swagger:** API documentation

This architecture provides a solid foundation for the current requirements while allowing for future growth and enhancement. The modular design and separation of concerns make it maintainable and extensible.
