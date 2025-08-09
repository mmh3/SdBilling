# School District Billing System - LLM Development Guide

## Overview
This document provides specific guidance for Large Language Models (LLMs) working with the School District Billing System codebase. It includes code patterns, examples, common tasks, and development conventions to help LLMs understand and work effectively with the system.

---

## Code Patterns and Conventions

### Controller Pattern
All controllers follow a consistent pattern:

```csharp
public class ExampleController : Controller
{
    private IWebHostEnvironment _hostEnvironment;
    private readonly AppDbContext _context;

    public ExampleController(IWebHostEnvironment environment, AppDbContext db)
    {
        _hostEnvironment = environment;
        _context = db;
    }

    // GET: Example
    public IActionResult Index()
    {
        return View(new ExampleIndexView(_context));
    }

    // GET: Example/Create
    public IActionResult Create()
    {
        var view = new ExampleView()
        {
            // Populate dropdowns and related data
            RelatedEntities = _context.RelatedEntities.ToList()
        };
        return View(view);
    }

    // POST: Example/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExampleView viewModel)
    {
        if (ModelState.IsValid)
        {
            var entity = new Example(viewModel, _context);
            
            if (entity.IsValid(_context, out string errorMessage))
            {
                _context.Examples.Add(entity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("", errorMessage);
            }
        }

        // Repopulate dropdowns on error
        viewModel.RelatedEntities = _context.RelatedEntities.ToList();
        return View(viewModel);
    }
}
```

### Model Validation Pattern
All domain models implement validation:

```csharp
public class Example
{
    // Properties with data annotations
    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    // Business validation method
    public bool IsValid(AppDbContext context, out string errorMessage)
    {
        errorMessage = null;

        // Check required fields
        if (string.IsNullOrEmpty(Name))
        {
            errorMessage = "Name is required.";
            return false;
        }

        // Check business rules
        if (context.Examples.Any(e => e.Name == Name && e.Id != Id))
        {
            errorMessage = "Name must be unique.";
            return false;
        }

        return true;
    }
}
```

### Database Query Patterns
Common query patterns used throughout the system:

```csharp
// Get related data with filtering
public List<Student> GetStudents(int charterSchoolUid, string aun)
{
    return Students.Where(s => s.CharterSchoolUid == charterSchoolUid && s.Aun == aun)
                   .OrderBy(x => x.Grade)
                   .ThenBy(x => x.LastName)
                   .ThenBy(x => x.FirstName)
                   .ToList();
}

// Get effective-dated records
public SchoolDistrictRate GetSchoolDistrictRate(int schoolDistrictUid, DateTime asOfDate)
{
    return SchoolDistrictRates.Where(r => r.SchoolDistrictUid == schoolDistrictUid &&
                                          r.EffectiveDate <= asOfDate)
                              .OrderByDescending(x => x.EffectiveDate)
                              .FirstOrDefault();
}

// Get records within date range
public List<Payment> GetPayments(int charterSchoolUid, int schoolDistrictUid, DateTime startDate, DateTime endDate)
{
    return Payments.Where(p => p.CharterSchoolUid == charterSchoolUid &&
                               p.SchoolDistrictUid == schoolDistrictUid &&
                               p.Date >= startDate && p.Date <= endDate)
                   .OrderBy(p => p.Date)
                   .ToList();
}
```

---

## Common Development Tasks

### Adding a New Entity

1. **Create the Model Class:**
```csharp
[Table("new_entity")]
public class NewEntity
{
    [Column("new_entity_uid")]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Required]
    public int NewEntityUid { get; set; }

    [Column("name")]
    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    // Add validation method
    public bool IsValid(AppDbContext context, out string errorMessage)
    {
        // Implement validation logic
    }
}
```

2. **Add to DbContext:**
```csharp
public DbSet<NewEntity> NewEntities { get; set; }
```

3. **Create Migration:**
```bash
dotnet ef migrations add AddNewEntity
dotnet ef database update
```

4. **Create Controller:**
```csharp
public class NewEntitiesController : Controller
{
    // Follow standard controller pattern
}
```

### Adding a New Report

1. **Create Report Class:**
```csharp
public class NewReport
{
    private AppDbContext _dbContext { get; set; }
    private string _rootPath { get; set; }

    public NewReport(AppDbContext context, string rootPath)
    {
        _dbContext = context;
        _rootPath = rootPath;
    }

    public string Generate(ReportCriteriaView criteria)
    {
        using (ExcelPackage package = new ExcelPackage())
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");
            
            // Populate data
            PopulateData(worksheet, criteria);
            
            // Save file
            var fileName = FileSystemServices.GetReportFileName(
                FileType.NewReport, _rootPath, criteria, "ReportName");
            
            FileInfo outputFile = new FileInfo(fileName);
            package.SaveAs(outputFile);
            
            return fileName;
        }
    }
}
```

2. **Add to ExcelServices:**
```csharp
public static List<string> GenerateNewReport(AppDbContext context, string rootPath, ReportCriteriaView criteria)
{
    var files = new List<string>();
    var report = new NewReport(context, rootPath);
    
    foreach (var item in criteria.SelectedItems)
    {
        var fileName = report.Generate(criteria);
        files.Add(fileName);
    }
    
    return files;
}
```

### Adding Business Logic

1. **Complex Calculations (like attendance):**
```csharp
public void GetMonthlyAttendanceValue(AppDbContext context, int month, int year, 
    out decimal spedAttendance, out decimal nonSpedAttendance, 
    out decimal spedDays, out decimal nonSpedDays, out decimal daysInSession)
{
    var schedule = context.GetCharterSchoolSchedule(CharterSchoolUid, Grade, month, year);
    DateTime firstDayOfMonth = new DateTime(year, month, 1);
    DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
    
    GetPeriodAttendanceValues(context, schedule, firstDayOfMonth, lastDayOfMonth, 
        out spedAttendance, out spedDays, out nonSpedAttendance, out nonSpedDays, out daysInSession);
}
```

2. **Data Import Logic:**
```csharp
public static List<EntityView> ImportEntities(AppDbContext context, List<string> fileNames)
{
    List<EntityView> entities = new List<EntityView>();

    foreach (var fileName in fileNames)
    {
        byte[] bin = File.ReadAllBytes(fileName);
        
        using (MemoryStream stream = new MemoryStream(bin))
        using (ExcelPackage excelPackage = new ExcelPackage(stream))
        {
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();
            
            // Process each row
            for (int i = 2; i <= worksheet.Dimension.End.Row; i++) // Skip header
            {
                var entity = ProcessRow(worksheet, i, context);
                if (entity != null)
                {
                    entities.Add(entity);
                }
            }
        }
    }
    
    return entities;
}
```

---

## Key Business Logic Patterns

### Attendance Calculations
The system has complex attendance calculation logic that handles:

```csharp
// Key scenarios to understand:
// 1. Full month attendance
if (DidAttendForEntirePeriod(startDate, endDate))
{
    if (IsSpedOnDate(startDate))
    {
        spedAttendanceAdm = 1;
        nonSpedAttendanceAdm = 0;
    }
    else
    {
        spedAttendanceAdm = 0;
        nonSpedAttendanceAdm = 1;
    }
}

// 2. Mid-month entry/exit
else if (DistrictEntryDate > startDate && ExitDate < endDate)
{
    var attendedDays = schedule.GetSchoolDays(context, (DateTime)DistrictEntryDate, (DateTime)ExitDate);
    spedAttendanceAdm = decimal.Round(attendedDays / daysInSession, 3);
}

// 3. IEP status changes during month
if (IsSpedOnDate(startDate) != IsSpedOnDate(endDate))
{
    // Split calculation between sped and non-sped periods
    spedDays = schedule.GetSchoolDays(context, (DateTime)CurrentIepDate, endDate);
    nonSpedDays = daysInSession - spedDays;
    
    spedAttendanceAdm = decimal.Round(spedDays / daysInSession, 3);
    nonSpedAttendanceAdm = 1 - spedAttendanceAdm;
}
```

### Rate Management
Effective-dated rates are handled consistently:

```csharp
// Always get the most recent rate effective on or before the target date
public SchoolDistrictRate GetSchoolDistrictRate(int schoolDistrictUid, DateTime asOfDate)
{
    return SchoolDistrictRates.Where(r => r.SchoolDistrictUid == schoolDistrictUid &&
                                          r.EffectiveDate <= asOfDate)
                              .OrderByDescending(x => x.EffectiveDate)
                              .FirstOrDefault();
}
```

### File Management
All file operations follow this pattern:

```csharp
// Generate files in temp directory
var tempDir = Path.Combine(_hostEnvironment.WebRootPath, "temp");
var fileName = Path.Combine(tempDir, $"report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");

// Create ZIP archive for download
var archivePath = Path.Combine(_hostEnvironment.WebRootPath, "archive.zip");
if (File.Exists(archivePath)) File.Delete(archivePath);

// Copy files to temp and create archive
files.ForEach(f => File.Copy(f, Path.Combine(tempDir, Path.GetFileName(f))));
ZipFile.CreateFromDirectory(tempDir, archivePath);

return File("/archive.zip", "application/zip", "download.zip");
```

---

## Error Handling Patterns

### Model Validation Errors
```csharp
if (!ModelState.IsValid)
{
    // Repopulate any dropdown data
    viewModel.CharterSchools = _context.CharterSchools.ToList();
    viewModel.SchoolDistricts = _context.SchoolDistricts.ToList();
    return View(viewModel);
}

// Business rule validation
if (!entity.IsValid(_context, out string errorMessage))
{
    ModelState.AddModelError("", errorMessage);
    // Repopulate dropdowns and return view
}
```

### Database Operation Errors
```csharp
try
{
    _context.Entities.Add(entity);
    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}
catch (Exception ex)
{
    ModelState.AddModelError("", "An error occurred while saving. Please try again.");
    // Log the exception
    // Repopulate view data and return
}
```

### File Operation Errors
```csharp
try
{
    byte[] bin = File.ReadAllBytes(fileName);
    // Process file
}
catch (FileNotFoundException)
{
    ModelState.AddModelError("", "File not found. Please check the file path.");
}
catch (UnauthorizedAccessException)
{
    ModelState.AddModelError("", "Access denied. Please check file permissions.");
}
catch (Exception ex)
{
    ModelState.AddModelError("", "Error processing file. Please try again.");
}
```

---

## Database Migration Patterns

### Adding New Columns
```csharp
// In migration Up() method
migrationBuilder.AddColumn<string>(
    name: "new_column",
    table: "existing_table",
    type: "varchar(255)",
    maxLength: 255,
    nullable: true);

// In migration Down() method
migrationBuilder.DropColumn(
    name: "new_column",
    table: "existing_table");
```

### Adding New Tables
```csharp
// In migration Up() method
migrationBuilder.CreateTable(
    name: "new_table",
    columns: table => new
    {
        new_table_uid = table.Column<int>(type: "int", nullable: false)
            .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
        name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
        created_date = table.Column<DateTime>(type: "datetime(6)", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_new_table", x => x.new_table_uid);
    });
```

---

## Testing Patterns

### Unit Test Structure
```csharp
[TestClass]
public class StudentTests
{
    private AppDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [TestMethod]
    public void Student_IsValid_ReturnsTrueForValidStudent()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var student = new Student
        {
            StateStudentNo = "123456789",
            FirstName = "John",
            LastName = "Doe",
            Grade = "5",
            Dob = DateTime.Parse("2010-01-01"),
            DistrictEntryDate = DateTime.Parse("2023-08-15"),
            Aun = "123456789"
        };

        // Add required related data
        context.SchoolDistricts.Add(new SchoolDistrict { Aun = "123456789", Name = "Test District" });
        context.SaveChanges();

        // Act
        var result = student.IsValid(context, out string errorMessage);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(errorMessage);
    }
}
```

---

## Performance Considerations

### Efficient Queries
```csharp
// Use Include for related data when needed
var studentsWithDistricts = _context.Students
    .Include(s => s.SchoolDistrict)
    .Where(s => s.CharterSchoolUid == charterSchoolUid)
    .ToList();

// Use projection for large datasets
var studentSummary = _context.Students
    .Where(s => s.CharterSchoolUid == charterSchoolUid)
    .Select(s => new { s.StateStudentNo, s.FirstName, s.LastName, s.Grade })
    .ToList();

// Use AsNoTracking for read-only operations
var readOnlyStudents = _context.Students
    .AsNoTracking()
    .Where(s => s.CharterSchoolUid == charterSchoolUid)
    .ToList();
```

### Memory Management for Large Reports
```csharp
// Process large datasets in chunks
const int batchSize = 1000;
var totalStudents = _context.Students.Count();

for (int skip = 0; skip < totalStudents; skip += batchSize)
{
    var batch = _context.Students
        .Skip(skip)
        .Take(batchSize)
        .ToList();
    
    ProcessBatch(batch);
}
```

---

## Security Patterns

### Input Validation
```csharp
// Always validate user input
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(StudentView viewModel)
{
    // Model validation
    if (!ModelState.IsValid)
    {
        return View(viewModel);
    }

    // Business rule validation
    var student = new Student(viewModel);
    if (!student.IsValid(_context, out string errorMessage))
    {
        ModelState.AddModelError("", errorMessage);
        return View(viewModel);
    }

    // Proceed with creation
}
```

### SQL Injection Prevention
```csharp
// Always use parameterized queries (Entity Framework handles this)
var students = _context.Students
    .Where(s => s.StateStudentNo == userInput) // Safe - parameterized
    .ToList();

// Never use string concatenation for SQL
// BAD: var sql = $"SELECT * FROM students WHERE state_student_no = '{userInput}'";
```

---

## Common Debugging Scenarios

### Database Connection Issues
```csharp
// Check connection string in appsettings.json
// Verify MySQL service is running
// Test connection with simple query:
try
{
    var count = _context.Students.Count();
    // Connection is working
}
catch (Exception ex)
{
    // Log connection error details
}
```

### Excel Generation Issues
```csharp
// Common issues and solutions:
// 1. File permissions - ensure temp directory is writable
// 2. Template not found - verify template path
// 3. Memory issues with large datasets - process in chunks

try
{
    using (var package = new ExcelPackage())
    {
        // Excel operations
    }
}
catch (InvalidDataException)
{
    // Template file is corrupted or invalid format
}
catch (UnauthorizedAccessException)
{
    // File permission issues
}
```

### Attendance Calculation Debugging
```csharp
// Add logging to complex calculations
public void GetMonthlyAttendanceValue(/* parameters */)
{
    var schedule = context.GetCharterSchoolSchedule(CharterSchoolUid, Grade, month, year);
    
    // Log key values for debugging
    Console.WriteLine($"Student: {StateStudentNo}, Schedule: {schedule?.FirstDay} - {schedule?.LastDay}");
    Console.WriteLine($"Entry: {DistrictEntryDate}, Exit: {ExitDate}");
    
    // Continue with calculation
}
```

---

## Code Generation Templates

### Controller Template
When creating new controllers, follow this template:

```csharp
public class [EntityName]Controller : Controller
{
    private IWebHostEnvironment _hostEnvironment;
    private readonly AppDbContext _context;

    public [EntityName]Controller(IWebHostEnvironment environment, AppDbContext db)
    {
        _hostEnvironment = environment;
        _context = db;
    }

    // Standard CRUD operations
    public IActionResult Index() { /* Implementation */ }
    public IActionResult Create() { /* Implementation */ }
    [HttpPost] public async Task<IActionResult> Create([EntityName]View viewModel) { /* Implementation */ }
    public IActionResult Edit(int id) { /* Implementation */ }
    [HttpPost] public async Task<IActionResult> Edit(int id, [EntityName]View viewModel) { /* Implementation */ }
    public IActionResult Delete(int id) { /* Implementation */ }
    [HttpPost] public async Task<IActionResult> Delete(int id) { /* Implementation */ }
}
```

This guide provides LLMs with the specific patterns, conventions, and examples needed to work effectively with your School District Billing System codebase.
