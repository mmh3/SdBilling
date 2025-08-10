using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
                int studentCount = PopulateStudentData(worksheet, students, criteria);
                ApplyFormatting(worksheet, studentCount);
                
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
            
            // Merge cells for header (now extends to column O due to added counter column)
            worksheet.Cells["A1:O1"].Merge = true;
            worksheet.Cells["A2:O2"].Merge = true;
            
            // Center align headers
            worksheet.Cells["A1:O2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A1:O2"].Style.Font.Bold = true;
        }

        private void PopulateColumnHeaders(ExcelWorksheet worksheet, ReportCriteriaView criteria)
        {
            int startRow = 4;
            
            // Counter column (no header - column A)
            // worksheet.Cells[startRow, 1].Value = ""; // Leave empty as requested
            
            // Student name headers (columns B and C)
            worksheet.Cells[startRow, 2].Value = "Last Name";
            worksheet.Cells[startRow, 3].Value = "First Name";
            
            // Month headers (now columns D through O)
            var months = GetSchoolYearMonths(criteria.Year);
            for (int i = 0; i < months.Count; i++)
            {
                worksheet.Cells[startRow + 1, i + 4].Value = months[i];
            }
            
            // Total column (column P) - header is above the total days row
            worksheet.Cells[startRow - 1, 16].Value = "Instructional Days";
            
            // Make headers bold
            worksheet.Cells[startRow, 1, startRow + 1, 16].Style.Font.Bold = true;
        }

        private int PopulateStudentData(ExcelWorksheet worksheet, List<Student> students, ReportCriteriaView criteria)
        {
            int currentRow = 6;
            int year = int.Parse(criteria.Year);
            int studentCounter = 1;
            
            students = students.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToList();
            
            foreach (var student in students)
            {
                student.GetYearlyAttendanceValue(_dbContext, year, out decimal yearlySpedDays, out decimal yearlyNonSpedDays);
                if (yearlySpedDays + yearlyNonSpedDays == 0)
                {
                    continue;
                }
                
                // Counter column (column A)
                worksheet.Cells[currentRow, 1].Value = studentCounter;
                
                // Student names (now columns B and C)
                worksheet.Cells[currentRow, 2].Value = student.LastName;
                worksheet.Cells[currentRow, 3].Value = student.FirstName;
                
                decimal totalDays = 0;
                decimal totalDaysInSession = 0;
                
                // Get attendance for each month (now columns D through O)
                var months = GetSchoolYearMonthNumbers(year);
                for (int i = 0; i < months.Count; i++)
                {
                    var monthlyDays = GetStudentMonthlyAttendance(student, months[i].month, months[i].year, out decimal daysInSession);
                    worksheet.Cells[currentRow, i + 4].Value = monthlyDays;

                    // For the first student, also populate the header row with the number days in the session for the school.
                    if (currentRow == 6)
                    {
                        worksheet.Cells[4, i + 4].Value = daysInSession;
                    }

                    totalDays += monthlyDays;
                    totalDaysInSession += daysInSession;
                }
                
                // Total column (now column P)
                worksheet.Cells[currentRow, 16].Value = totalDays;

                if (currentRow == 6)
                {
                    worksheet.Cells[4, 16].Value = totalDaysInSession;
                }

                currentRow++;
                studentCounter++;
            }
            
            // Add grand total row
            if (studentCounter > 1)
            {
                AddGrandTotalRow(worksheet, currentRow, studentCounter);
                return studentCounter;
            }

            return 0;
        }

        private decimal GetStudentMonthlyAttendance(Student student, int month, int year, out decimal daysInSession)
        {
            try
            {
                student.GetMonthlyAttendanceValue(_dbContext, month, year, out decimal spedAttendance, out decimal nonSpedAttendance, out decimal spedDays, out decimal nonSpedDays, out daysInSession);
                return Math.Ceiling(spedDays + nonSpedDays);
            }
            catch
            {
                daysInSession = 0;
                return 0;
            }
        }

        private void AddGrandTotalRow(ExcelWorksheet worksheet, int row, int studentCount)
        {   
            // Calculate column totals (skip counter, name columns - start from column D which is month data)
            // Only doing this for the total instructional days column right now but left the for loop in case they want to do it for all at some point.
            for (int col = 16; col <= 16; col++)
            {
                var formula = $"SUM({worksheet.Cells[5, col].Address}:{worksheet.Cells[row - 1, col].Address})";
                worksheet.Cells[row, col].Formula = formula;
            }
        }

        private void ApplyFormatting(ExcelWorksheet worksheet, int studentCount)
        {
            // Apply borders to data area (now extends to column P)
            int lastRow = 4 + studentCount;

            //Underline the month column headers
            worksheet.Cells[5, 4, 5, 15].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            //Underline the final Instructional Days value and double underline the total.
            worksheet.Cells[lastRow, 16, lastRow, 16].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[lastRow + 1, 16, lastRow + 1, 16].Style.Border.Bottom.Style = ExcelBorderStyle.Double;
            
            // Auto-fit columns (commented out due to libgdiplus dependency)
            //worksheet.Cells.AutoFitColumns();
            
            // Set minimum column widths
            for (int col = 1; col <= 16; col++)
            {
                if (worksheet.Column(col).Width < 8)
                    worksheet.Column(col).Width = 8;
            }
            
            // Set counter column to be narrower since it just contains numbers and name and instructional days columns to be wider.
            worksheet.Column(1).Width = 5;
            worksheet.Column(2).Width = 20;
            worksheet.Column(3).Width = 20;
            worksheet.Column(16).Width = 15;

            // Center all values except the names.
            worksheet.Cells[4, 1, lastRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[4, 4, lastRow + 1, 16].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
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
