# Days Attended Report Implementation Plan

## Overview
This document outlines the implementation plan for adding a new "Days Attended" report to the Year-end Reconciliation process. The report will show monthly attendance days for each student throughout the school year, organized by school district.

## Analysis of Required Output
Based on the screenshot provided, the report should include:
- **Header**: School year, charter school name, and school district name
- **Column Headers**: Student names (Last Name, First Name) and monthly columns (Jul-23 through Jun-24)
- **Data Rows**: Each student with their attendance days per month
- **Summary**: Total instructional days column and grand total at bottom
- **Format**: Excel spreadsheet with proper formatting and borders

## Implementation Steps

### 1. Update Enums and Constants

#### 1.1 Add New FileType Enum Value
**File**: `SchoolDistrictBilling/Reports/FileType.cs`
```csharp
public enum FileType
{
    Invoice,
    Student,
    ReconStudent,
    Unipay,
    YearEnd,
    PdeStudentList,
    PdeDirectPayment,
    PdeTuitionRate,
    PdeStudentListReconciliation,
    DaysAttended  // Add this new value
}
```

### 2. Update FileSystemServices

#### 2.1 Add File Naming Logic for Days Attended Report
**File**: `SchoolDistrictBilling/Services/FileSystemServices.cs`

Add case in `GetReportFileName` method:
```csharp
case FileType.DaysAttended:
    return Path.Combine(outputFilePath, criteria.Year + Regex.Replace(schoolDistrictName, @"\s+", "") + "DaysAttended.xlsx");
```

### 3. Create Days Attended Report Class

#### 3.1 Create New Report Generator
**File**: `SchoolDistrictBilling/Reports/DaysAttendedReport.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SchoolDistrictBilling.Data;
using SchoolDistrictBilling.Models;
using SchoolDistrictBilling.Services;

namespace SchoolDistrictBilling.Reports
{
    public class DaysAttendedReport
    {
        private AppDbContext _dbContext { get; set; }
        private string _rootPath { get; set; }

        public DaysAttendedReport(AppDbContext context, string rootPath)
        {
            _dbContext = context;
            _rootPath = rootPath;
        }

        public string Generate(ReportCriteriaView criteria, string schoolDistrictName)
        {
            var charterSchool = _dbContext.CharterSchools.Find(criteria.CharterSchoolUid);
            var schoolDistrict = _dbContext.SchoolDistricts.Where(sd => sd.Name == schoolDistrictName).FirstOrDefault();
            var students = _dbContext.GetStudents(criteria.CharterSchoolUid, schoolDistrict.Aun);
            
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Days Attended");
                
                // Generate the report content
                PopulateHeader(worksheet, criteria, charterSchool.Name, schoolDistrictName);
                PopulateColumnHeaders(worksheet, criteria);
                PopulateStudentData(worksheet, students, criteria);
                ApplyFormatting(worksheet, students.Count);
                
                // Save the file
                var fileName = FileSystemServices.GetReportFileName(FileType.DaysAttended, _rootPath, criteria, charterSchool.Name, schoolDistrictName);
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                
                FileInfo outputFile = new FileInfo(fileName);
                package.SaveAs(outputFile);
                
                return fileName;
            }
        }

        private void PopulateHeader(ExcelWorksheet worksheet, ReportCriteriaView criteria, string charterSchoolName, string schoolDistrictName)
        {
            // Main header
            worksheet.Cells["A1"].Value = $"{GetSchoolYearString(criteria.Year)} {charterSchoolName}";
            worksheet.Cells["A2"].Value = $"Instructional Days Attended - {schoolDistrictName}";
            
            // Merge cells for header
            worksheet.Cells["A1:N1"].Merge = true;
            worksheet.Cells["A2:N2"].Merge = true;
            
            // Center align headers
            worksheet.Cells["A1:N2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A1:N2"].Style.Font.Bold = true;
        }

        private void PopulateColumnHeaders(ExcelWorksheet worksheet, ReportCriteriaView criteria)
        {
            int startRow = 4;
            
            // Student name headers
            worksheet.Cells[startRow, 1].Value = "Last Name";
            worksheet.Cells[startRow, 2].Value = "First Name";
            
            // Month headers
            var months = GetSchoolYearMonths(criteria.Year);
            for (int i = 0; i < months.Count; i++)
            {
                worksheet.Cells[startRow, i + 3].Value = months[i];
            }
            
            // Total column
            worksheet.Cells[startRow, 15].Value = "Instructional Days";
            
            // Make headers bold
            worksheet.Cells[startRow, 1, startRow, 15].Style.Font.Bold = true;
        }

        private void PopulateStudentData(ExcelWorksheet worksheet, List<Student> students, ReportCriteriaView criteria)
        {
            int currentRow = 5;
            int year = int.Parse(criteria.Year);
            
            foreach (var student in students)
            {
                worksheet.Cells[currentRow, 1].Value = student.LastName;
                worksheet.Cells[currentRow, 2].Value = student.FirstName;
                
                decimal totalDays = 0;
                
                // Get attendance for each month
                var months = GetSchoolYearMonthNumbers(year);
                for (int i = 0; i < months.Count; i++)
                {
                    var monthlyDays = GetStudentMonthlyAttendance(student, months[i].month, months[i].year);
                    worksheet.Cells[currentRow, i + 3].Value = monthlyDays;
                    totalDays += monthlyDays;
                }
                
                // Total column
                worksheet.Cells[currentRow, 15].Value = totalDays;
                
                currentRow++;
            }
            
            // Add grand total row
            if (students.Count > 0)
            {
                AddGrandTotalRow(worksheet, currentRow, students.Count);
            }
        }

        private decimal GetStudentMonthlyAttendance(Student student, int month, int year)
        {
            try
            {
                student.GetMonthlyAttendanceValue(_dbContext, month, year, out decimal spedAttendance, out decimal nonSpedAttendance);
                return Math.Ceiling(spedAttendance + nonSpedAttendance);
            }
            catch
            {
                return 0;
            }
        }

        private void AddGrandTotalRow(ExcelWorksheet worksheet, int row, int studentCount)
        {
            // Add border above total row
            worksheet.Cells[row, 1, row, 15].Style.Border.Top.Style = ExcelBorderStyle.Thick;
            
            // Calculate column totals
            for (int col = 3; col <= 15; col++)
            {
                var formula = $"SUM({worksheet.Cells[5, col].Address}:{worksheet.Cells[4 + studentCount, col].Address})";
                worksheet.Cells[row, col].Formula = formula;
            }
        }

        private void ApplyFormatting(ExcelWorksheet worksheet, int studentCount)
        {
            // Apply borders to data area
            int lastRow = 5 + studentCount;
            worksheet.Cells[4, 1, lastRow, 15].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[4, 1, lastRow, 15].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[4, 1, lastRow, 15].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[4, 1, lastRow, 15].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            
            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();
            
            // Set minimum column widths
            for (int col = 1; col <= 15; col++)
            {
                if (worksheet.Column(col).Width < 8)
                    worksheet.Column(col).Width = 8;
            }
        }

        private List<string> GetSchoolYearMonths(string year)
        {
            return new List<string>
            {
                "Jul-" + (int.Parse(year) - 1).ToString().Substring(2),
                "Aug-" + (int.Parse(year) - 1).ToString().Substring(2),
                "Sep-" + (int.Parse(year) - 1).ToString().Substring(2),
                "Oct-" + (int.Parse(year) - 1).ToString().Substring(2),
                "Nov-" + (int.Parse(year) - 1).ToString().Substring(2),
                "Dec-" + (int.Parse(year) - 1).ToString().Substring(2),
                "Jan-" + year.Substring(2),
                "Feb-" + year.Substring(2),
                "Mar-" + year.Substring(2),
                "Apr-" + year.Substring(2),
                "May-" + year.Substring(2),
                "Jun-" + year.Substring(2)
            };
        }

        private List<(int month, int year)> GetSchoolYearMonthNumbers(int endYear)
        {
            int startYear = endYear - 1;
            return new List<(int, int)>
            {
                (7, startYear), (8, startYear), (9, startYear), (10, startYear), (11, startYear), (12, startYear),
                (1, endYear), (2, endYear), (3, endYear), (4, endYear), (5, endYear), (6, endYear)
            };
        }

        private string GetSchoolYearString(string year)
        {
            int endYear = int.Parse(year);
            int startYear = endYear - 1;
            return $"{startYear}/{endYear}";
        }
    }
}
```

### 4. Update ExcelServices

#### 4.1 Integrate Days Attended Report into Year-End Reconciliation
**File**: `SchoolDistrictBilling/Services/ExcelServices.cs`

Add the following code in the `GenerateYearEndRecon` method, after the existing student report generation (around line 500):

```csharp
// Generate Days Attended Report
var daysAttendedReport = new DaysAttendedReport(context, rootPath);
var daysAttendedFile = daysAttendedReport.Generate(criteria, schoolDistrictName);
fileNames.Add(daysAttendedFile);
```

### 5. Add Required Using Statement

#### 5.1 Update ExcelServices Using Statements
**File**: `SchoolDistrictBilling/Services/ExcelServices.cs`

Add at the top with other using statements:
```csharp
using SchoolDistrictBilling.Reports;
```

### 6. Testing and Validation

#### 6.1 Unit Testing Considerations
- Test with students who have varying enrollment dates
- Test with students who have gaps in attendance
- Test with different school district configurations
- Verify Excel formatting matches the expected output
- Test file naming conventions

#### 6.2 Integration Testing
- Test the complete Year-End Reconciliation process
- Verify the new report is included in the ZIP file download
- Test with multiple school districts
- Verify report generation doesn't break existing functionality

### 7. Error Handling and Edge Cases

#### 7.1 Handle Missing Data
- Students with no attendance data
- Missing charter school schedules
- Invalid date ranges

#### 7.2 Performance Considerations
- Large numbers of students (100+ per district)
- Multiple school districts
- Memory usage for Excel generation

### 8. Documentation Updates

#### 8.1 Update README.md
Add information about the new Days Attended report in the features section.

#### 8.2 Code Comments
Add comprehensive comments to the new DaysAttendedReport class explaining:
- Purpose and usage
- Data sources
- Calculation methods
- Output format

## Implementation Timeline

1. **Phase 1** (Day 1-2): Create enum updates and file system changes
2. **Phase 2** (Day 3-5): Implement DaysAttendedReport class with basic functionality
3. **Phase 3** (Day 6-7): Integrate with ExcelServices and test basic generation
4. **Phase 4** (Day 8-9): Add formatting, error handling, and edge case management
5. **Phase 5** (Day 10): Final testing, documentation, and deployment preparation

## Potential Challenges and Solutions

### Challenge 1: Performance with Large Student Lists
**Solution**: Implement batch processing and optimize database queries

### Challenge 2: Complex Attendance Calculations
**Solution**: Leverage existing Student model methods and add comprehensive error handling

### Challenge 3: Excel Formatting Complexity
**Solution**: Use EPPlus styling features and create reusable formatting methods

### Challenge 4: File Organization
**Solution**: Follow existing file naming conventions and integrate with current archive system

## Success Criteria

- [ ] New report generates successfully for all school districts
- [ ] Report format matches the provided screenshot
- [ ] Integration with Year-End Reconciliation process is seamless
- [ ] Performance is acceptable for typical data volumes
- [ ] Error handling covers edge cases
- [ ] Code follows existing project patterns and standards
- [ ] Documentation is complete and accurate

## Post-Implementation Considerations

1. **Monitoring**: Track report generation performance and error rates
2. **User Feedback**: Collect feedback on report format and usefulness
3. **Maintenance**: Plan for updates to attendance calculation logic if needed
4. **Scalability**: Monitor performance with larger datasets and optimize as needed
