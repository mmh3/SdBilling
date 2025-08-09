# School District Billing System - Debugging & Troubleshooting Guide

## Overview
This guide provides systematic approaches to debugging and troubleshooting common issues in the School District Billing System, specifically designed to help LLMs diagnose and resolve problems effectively.

---

## Common Error Patterns and Solutions

### Database Connection Issues

#### Symptoms
- "Unable to connect to any of the specified MySQL hosts"
- "Access denied for user"
- "Unknown database"

#### Diagnostic Steps
```csharp
// Test basic connectivity
try
{
    var connectionString = Configuration.GetConnectionString("Db");
    using (var connection = new MySqlConnection(connectionString))
    {
        connection.Open();
        Console.WriteLine("Database connection successful");
        
        // Test basic query
        var command = new MySqlCommand("SELECT COUNT(*) FROM students", connection);
        var count = command.ExecuteScalar();
        Console.WriteLine($"Student count: {count}");
    }
}
catch (MySqlException ex)
{
    Console.WriteLine($"MySQL Error {ex.Number}: {ex.Message}");
    // Error 1045: Access denied
    // Error 1049: Unknown database
    // Error 2003: Can't connect to MySQL server
}
```

#### Solutions
1. **Check connection string format:**
   ```json
   "Server=localhost;Database=omni_test;Uid=username;Pwd=password;CharSet=utf8mb4;"
   ```

2. **Verify MySQL service is running:**
   ```bash
   # macOS
   brew services list | grep mysql
   brew services start mysql
   
   # Linux
   systemctl status mysql
   sudo systemctl start mysql
   
   # Windows
   net start mysql80
   ```

3. **Test database credentials:**
   ```bash
   mysql -u username -p database_name
   ```

### Entity Framework Migration Issues

#### Symptoms
- "No migrations configuration type was found"
- "Unable to create an object of type 'AppDbContext'"
- "The model backing the context has changed"

#### Diagnostic Commands
```bash
# Check current migration status
dotnet ef migrations list

# Check if database is up to date
dotnet ef database update --dry-run

# Generate SQL script for pending migrations
dotnet ef migrations script
```

#### Solutions
```bash
# Reset migrations (CAUTION: Data loss)
dotnet ef database drop
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update

# Add new migration for model changes
dotnet ef migrations add DescriptiveName
dotnet ef database update
```

### Excel Processing Errors

#### Symptoms
- "The file is not a valid Excel file"
- "Index was outside the bounds of the array"
- "Object reference not set to an instance"

#### Debugging Excel Issues
```csharp
public void DebugExcelFile(string filePath)
{
    try
    {
        Console.WriteLine($"File exists: {File.Exists(filePath)}");
        Console.WriteLine($"File size: {new FileInfo(filePath).Length} bytes");
        
        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            Console.WriteLine($"Worksheets count: {package.Workbook.Worksheets.Count}");
            
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet != null)
            {
                Console.WriteLine($"Worksheet name: {worksheet.Name}");
                Console.WriteLine($"Dimensions: {worksheet.Dimension?.Address}");
                Console.WriteLine($"Row count: {worksheet.Dimension?.End.Row}");
                Console.WriteLine($"Column count: {worksheet.Dimension?.End.Column}");
                
                // Check first few cells
                for (int row = 1; row <= Math.Min(3, worksheet.Dimension?.End.Row ?? 0); row++)
                {
                    for (int col = 1; col <= Math.Min(5, worksheet.Dimension?.End.Column ?? 0); col++)
                    {
                        var value = worksheet.Cells[row, col].Value;
                        Console.WriteLine($"Cell [{row},{col}]: {value} (Type: {value?.GetType().Name})");
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Excel processing error: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}
```

### Attendance Calculation Issues

#### Symptoms
- Incorrect billing amounts
- Students showing zero attendance
- Prorated calculations seem wrong

#### Debugging Attendance Logic
```csharp
public void DebugAttendanceCalculation(Student student, int month, int year)
{
    Console.WriteLine($"=== Debugging Attendance for {student.StateStudentNo} ===");
    Console.WriteLine($"Student: {student.FirstName} {student.LastName}");
    Console.WriteLine($"Grade: {student.Grade}");
    Console.WriteLine($"Entry Date: {student.DistrictEntryDate}");
    Console.WriteLine($"Exit Date: {student.ExitDate}");
    Console.WriteLine($"IEP Flag: {student.IepFlag}");
    Console.WriteLine($"Current IEP Date: {student.CurrentIepDate}");
    
    var schedule = _context.GetCharterSchoolSchedule(student.CharterSchoolUid, student.Grade, month, year);
    if (schedule == null)
    {
        Console.WriteLine("ERROR: No schedule found for this student/grade/period");
        return;
    }
    
    Console.WriteLine($"Schedule: {schedule.FirstDay:MM/dd/yyyy} - {schedule.LastDay:MM/dd/yyyy}");
    Console.WriteLine($"Days in Session: {schedule.DaysInSession}");
    
    DateTime monthStart = new DateTime(year, month, 1);
    DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);
    Console.WriteLine($"Month Period: {monthStart:MM/dd/yyyy} - {monthEnd:MM/dd/yyyy}");
    
    // Check if student was enrolled during this period
    bool wasEnrolled = student.DistrictEntryDate <= monthEnd && 
                      (student.ExitDate == null || student.ExitDate >= monthStart);
    Console.WriteLine($"Was Enrolled: {wasEnrolled}");
    
    if (wasEnrolled)
    {
        student.GetMonthlyAttendanceValue(_context, month, year,
            out decimal spedAttendance, out decimal nonSpedAttendance,
            out decimal spedDays, out decimal nonSpedDays, out decimal daysInSession);
        
        Console.WriteLine($"Special Ed Attendance: {spedAttendance:F3} ({spedDays} days)");
        Console.WriteLine($"Regular Ed Attendance: {nonSpedAttendance:F3} ({nonSpedDays} days)");
        Console.WriteLine($"Total Days in Session: {daysInSession}");
    }
}
```

---

## Performance Debugging

### Slow Database Queries

#### Identifying Slow Queries
```csharp
// Enable EF Core logging in appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}

// Or programmatically
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
}
```

#### Common Performance Issues
```csharp
// BAD: N+1 query problem
var students = _context.Students.ToList();
foreach (var student in students)
{
    var district = _context.SchoolDistricts.First(d => d.Aun == student.Aun); // Executes for each student
}

// GOOD: Use Include or Join
var studentsWithDistricts = _context.Students
    .Join(_context.SchoolDistricts, s => s.Aun, d => d.Aun, (s, d) => new { Student = s, District = d })
    .ToList();

// BAD: Loading unnecessary data
var allStudents = _context.Students.ToList(); // Loads all columns for all students

// GOOD: Project only needed data
var studentSummary = _context.Students
    .Select(s => new { s.StateStudentNo, s.FirstName, s.LastName, s.Grade })
    .ToList();
```

### Memory Issues

#### Debugging Memory Usage
```csharp
public void MonitorMemoryUsage(string operation)
{
    var beforeMemory = GC.GetTotalMemory(false);
    var stopwatch = Stopwatch.StartNew();
    
    try
    {
        // Perform operation
        PerformOperation();
    }
    finally
    {
        stopwatch.Stop();
        var afterMemory = GC.GetTotalMemory(false);
        var memoryUsed = afterMemory - beforeMemory;
        
        Console.WriteLine($"Operation: {operation}");
        Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Memory used: {memoryUsed / 1024 / 1024:F2} MB");
        
        // Force garbage collection to see actual memory usage
        GC.Collect();
        GC.WaitForPendingFinalizers();
        var finalMemory = GC.GetTotalMemory(true);
        Console.WriteLine($"Memory after GC: {(finalMemory - beforeMemory) / 1024 / 1024:F2} MB");
    }
}
```

---

## File System Issues

### Permission Problems

#### Diagnosing File Permissions
```csharp
public void CheckFilePermissions(string directoryPath)
{
    try
    {
        Console.WriteLine($"Checking permissions for: {directoryPath}");
        
        // Check if directory exists
        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine("Directory does not exist");
            return;
        }
        
        // Test read permission
        try
        {
            var files = Directory.GetFiles(directoryPath);
            Console.WriteLine($"Read permission: OK ({files.Length} files found)");
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Read permission: DENIED");
        }
        
        // Test write permission
        var testFile = Path.Combine(directoryPath, "test_write.tmp");
        try
        {
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            Console.WriteLine("Write permission: OK");
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Write permission: DENIED");
        }
        
        // Check available space
        var drive = new DriveInfo(Path.GetPathRoot(directoryPath));
        Console.WriteLine($"Available space: {drive.AvailableFreeSpace / 1024 / 1024 / 1024:F2} GB");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error checking permissions: {ex.Message}");
    }
}
```

### File Locking Issues

#### Detecting File Locks
```csharp
public bool IsFileLocked(string filePath)
{
    try
    {
        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            return false; // File is not locked
        }
    }
    catch (IOException)
    {
        return true; // File is locked
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error checking file lock: {ex.Message}");
        return true; // Assume locked if we can't check
    }
}

public void SafeFileOperation(string filePath, Action<string> operation)
{
    int maxRetries = 5;
    int retryDelay = 1000; // 1 second
    
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            if (!IsFileLocked(filePath))
            {
                operation(filePath);
                return; // Success
            }
        }
        catch (IOException ex) when (attempt < maxRetries)
        {
            Console.WriteLine($"Attempt {attempt} failed: {ex.Message}. Retrying in {retryDelay}ms...");
            Thread.Sleep(retryDelay);
            retryDelay *= 2; // Exponential backoff
        }
    }
    
    throw new IOException($"Could not access file after {maxRetries} attempts: {filePath}");
}
```

---

## Validation and Business Logic Debugging

### Student Validation Issues

#### Debugging Validation Failures
```csharp
public void DebugStudentValidation(Student student, AppDbContext context)
{
    Console.WriteLine($"=== Validating Student {student.StateStudentNo} ===");
    
    // Check required fields
    var requiredFields = new Dictionary<string, object>
    {
        ["StateStudentNo"] = student.StateStudentNo,
        ["FirstName"] = student.FirstName,
        ["LastName"] = student.LastName,
        ["Grade"] = student.Grade,
        ["Aun"] = student.Aun,
        ["DistrictEntryDate"] = student.DistrictEntryDate
    };
    
    foreach (var field in requiredFields)
    {
        bool isEmpty = field.Value == null || 
                      (field.Value is string str && string.IsNullOrWhiteSpace(str));
        Console.WriteLine($"{field.Key}: {field.Value} {(isEmpty ? "❌ EMPTY" : "✅")}");
    }
    
    // Check business rules
    Console.WriteLine("\n=== Business Rule Validation ===");
    
    // Unique state student number
    var duplicateCount = context.Students.Count(s => s.StateStudentNo == student.StateStudentNo && s.StudentUid != student.StudentUid);
    Console.WriteLine($"Duplicate state numbers: {duplicateCount} {(duplicateCount > 0 ? "❌" : "✅")}");
    
    // Valid AUN
    var districtExists = context.SchoolDistricts.Any(d => d.Aun == student.Aun);
    Console.WriteLine($"Valid district AUN: {districtExists} {(districtExists ? "✅" : "❌")}");
    
    // Valid grade
    string[] validGrades = { "K", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
    bool validGrade = validGrades.Contains(student.Grade);
    Console.WriteLine($"Valid grade: {validGrade} {(validGrade ? "✅" : "❌")}");
    
    // Date validation
    bool validDob = student.Dob.HasValue && student.Dob <= DateTime.Today;
    Console.WriteLine($"Valid DOB: {validDob} {(validDob ? "✅" : "❌")}");
    
    bool validEntryDate = student.DistrictEntryDate.HasValue && student.DistrictEntryDate <= DateTime.Today;
    Console.WriteLine($"Valid entry date: {validEntryDate} {(validEntryDate ? "✅" : "❌")}");
    
    if (student.ExitDate.HasValue)
    {
        bool validExitDate = student.ExitDate > student.DistrictEntryDate;
        Console.WriteLine($"Valid exit date: {validExitDate} {(validExitDate ? "✅" : "❌")}");
    }
    
    // Run actual validation
    Console.WriteLine("\n=== Final Validation Result ===");
    bool isValid = student.IsValid(context, out string errorMessage);
    Console.WriteLine($"Overall valid: {isValid} {(isValid ? "✅" : "❌")}");
    if (!isValid)
    {
        Console.WriteLine($"Error: {errorMessage}");
    }
}
```

---

## Report Generation Debugging

### Excel Template Issues

#### Debugging Template Problems
```csharp
public void DebugExcelTemplate(string templatePath)
{
    Console.WriteLine($"=== Debugging Excel Template: {templatePath} ===");
    
    if (!File.Exists(templatePath))
    {
        Console.WriteLine("❌ Template file does not exist");
        return;
    }
    
    try
    {
        using (var package = new ExcelPackage(new FileInfo(templatePath)))
        {
            Console.WriteLine($"✅ Template loaded successfully");
            Console.WriteLine($"Worksheets: {package.Workbook.Worksheets.Count}");
            
            foreach (var worksheet in package.Workbook.Worksheets)
            {
                Console.WriteLine($"  - {worksheet.Name}: {worksheet.Dimension?.Address}");
                
                // Check for common placeholders
                var placeholders = new[] { "{{CharterSchoolName}}", "{{DistrictName}}", "{{Month}}", "{{Year}}" };
                foreach (var placeholder in placeholders)
                {
                    var found = FindPlaceholderInWorksheet(worksheet, placeholder);
                    Console.WriteLine($"    Placeholder '{placeholder}': {(found ? "✅ Found" : "❌ Not found")}");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error loading template: {ex.Message}");
    }
}

private bool FindPlaceholderInWorksheet(ExcelWorksheet worksheet, string placeholder)
{
    if (worksheet.Dimension == null) return false;
    
    for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
    {
        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
        {
            var value = worksheet.Cells[row, col].Value?.ToString();
            if (value?.Contains(placeholder) == true)
            {
                return true;
            }
        }
    }
    return false;
}
```

---

## System Health Checks

### Comprehensive System Check
```csharp
public async Task<SystemHealthReport> PerformSystemHealthCheck()
{
    var report = new SystemHealthReport();
    
    // Database connectivity
    try
    {
        var studentCount = await _context.Students.CountAsync();
        report.DatabaseStatus = $"✅ Connected ({studentCount} students)";
    }
    catch (Exception ex)
    {
        report.DatabaseStatus = $"❌ Database error: {ex.Message}";
    }
    
    // File system access
    var tempDir = Path.Combine(_hostEnvironment.WebRootPath, "temp");
    try
    {
        Directory.CreateDirectory(tempDir);
        var testFile = Path.Combine(tempDir, "health_check.tmp");
        await File.WriteAllTextAsync(testFile, "test");
        File.Delete(testFile);
        report.FileSystemStatus = "✅ File system accessible";
    }
    catch (Exception ex)
    {
        report.FileSystemStatus = $"❌ File system error: {ex.Message}";
    }
    
    // Template files
    var templateDir = Path.Combine(_hostEnvironment.WebRootPath, "reportTemplates");
    var requiredTemplates = new[] { "MonthlyInvoice.xlsx", "YearEndReconciliation.xlsx" };
    var missingTemplates = requiredTemplates.Where(t => !File.Exists(Path.Combine(templateDir, t))).ToList();
    
    if (missingTemplates.Any())
    {
        report.TemplateStatus = $"❌ Missing templates: {string.Join(", ", missingTemplates)}";
    }
    else
    {
        report.TemplateStatus = "✅ All templates present";
    }
    
    // Memory usage
    var memoryUsage = GC.GetTotalMemory(false) / 1024 / 1024;
    report.MemoryUsage = $"Memory: {memoryUsage:F2} MB";
    
    return report;
}

public class SystemHealthReport
{
    public string DatabaseStatus { get; set; }
    public string FileSystemStatus { get; set; }
    public string TemplateStatus { get; set; }
    public string MemoryUsage { get; set; }
    public DateTime CheckTime { get; set; } = DateTime.Now;
}
```

This debugging guide provides systematic approaches to identifying and resolving common issues in the School District Billing System, with specific focus on helping LLMs understand the diagnostic process and implement effective solutions.
