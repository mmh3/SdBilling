using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using SchoolDistrictBilling.Data;
using SchoolDistrictBilling.Models;
using SchoolDistrictBilling.Services;

namespace SchoolDistrictBilling.Reports
{
    public class DaysAttendedReportSimple
    {
        private AppDbContext _dbContext { get; set; }
        private string _rootPath { get; set; }

        public DaysAttendedReportSimple(AppDbContext context, string rootPath)
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
                
                // Generate the report content WITHOUT graphics-dependent features
                PopulateHeaderSimple(worksheet, criteria, charterSchool.Name, schoolDistrictName);
                PopulateColumnHeaders(worksheet, criteria);
                PopulateStudentData(worksheet, students, criteria);
                // Skip formatting that might require graphics
                
                // Save the file
                var fileName = FileSystemServices.GetReportFileName(FileType.DaysAttended, _rootPath, criteria, charterSchool.Name, schoolDistrictName);
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                
                FileInfo outputFile = new FileInfo(fileName);
                package.SaveAs(outputFile);
                
                return fileName;
            }
        }

        private void PopulateHeaderSimple(ExcelWorksheet worksheet, ReportCriteriaView criteria, string charterSchoolName, string schoolDistrictName)
        {
            // Simple headers without merging or special formatting
            worksheet.Cells["A1"].Value = $"{GetSchoolYearString(criteria.Year)} {charterSchoolName}";
            worksheet.Cells["A2"].Value = $"Instructional Days Attended - {schoolDistrictName}";
            
            // Basic bold formatting only
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Style.Font.Bold = true;
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
            
            // Add grand total row (without borders)
            if (students.Count > 0)
            {
                AddGrandTotalRowSimple(worksheet, currentRow, students.Count);
            }
        }

        private decimal GetStudentMonthlyAttendance(Student student, int month, int year)
        {
            try
            {
                student.GetMonthlyAttendanceValue(_dbContext, month, year, out decimal spedAttendance, out decimal nonSpedAttendance, out decimal spedDays, out decimal nonSpedDays, out decimal daysInSession);
                return Math.Ceiling(spedDays + nonSpedDays);
            }
            catch
            {
                return 0;
            }
        }

        private void AddGrandTotalRowSimple(ExcelWorksheet worksheet, int row, int studentCount)
        {
            // Calculate column totals without borders
            for (int col = 3; col <= 15; col++)
            {
                var formula = $"SUM({worksheet.Cells[5, col].Address}:{worksheet.Cells[4 + studentCount, col].Address})";
                worksheet.Cells[row, col].Formula = formula;
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
