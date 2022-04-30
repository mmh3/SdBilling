using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OfficeOpenXml;
using SchoolDistrictBilling.Data;
using SchoolDistrictBilling.Models;
using SchoolDistrictBilling.Services;

namespace SchoolDistrictBilling.Reports
{
    public class PdeCsrStudentList
    {
        private AppDbContext _dbContext { get; set; }
        private string _rootPath { get; set; }

        public PdeCsrStudentList(AppDbContext context, string rootPath)
        {
            _dbContext = context;
            _rootPath = rootPath;
        }

        public string Generate(ReportCriteriaView criteria)
        {
            var fileName = string.Empty;
            int invoiceMonth = DateTime.ParseExact(criteria.Month, "MMMM", CultureInfo.CurrentCulture).Month;
            var schedules = _dbContext.GetCharterSchoolSchedules(criteria.CharterSchoolUid, invoiceMonth, int.Parse(criteria.Year));

            using (ExcelPackage studentList = new ExcelPackage())
            {
                ExcelWorksheet sheet = studentList.Workbook.Worksheets.Add("Sheet1");

                PopulateHeaderRow(sheet, criteria);

                foreach (var sdName in criteria.SelectedSchoolDistricts)
                {
                    if (!string.IsNullOrEmpty(sdName))
                    {
                        var schoolDistrict = _dbContext.SchoolDistricts.Where(sd => sd.Name == sdName).FirstOrDefault();
                        var students = _dbContext.GetStudents(criteria.CharterSchoolUid, schoolDistrict.Aun);

                        if (criteria.IsYearEndRecon)
                        {
                            AddStudentsToReconciliation(sheet, schoolDistrict, students, schedules, int.Parse(criteria.Year));
                        }
                        else
                        {
                            AddStudentsToInvoice(sheet, schoolDistrict, students, schedules, criteria.Month, int.Parse(criteria.Year));
                        }

                        // Add an audit record that we generated this report.
                        ReportType reportType = ReportType.Invoice;
                        if (criteria.IsYearEndRecon)
                        {
                            reportType = ReportType.YearEnd;
                        }
                        _dbContext.Add(new ReportHistory(reportType, criteria.CharterSchoolUid, schoolDistrict.SchoolDistrictUid, criteria.SendTo, criteria.Month, int.Parse(criteria.Year)));
                    }
                }

                sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

                var charterSchoolName = _dbContext.CharterSchools.Find(criteria.CharterSchoolUid).Name;
                fileName = FileSystemServices.SaveReportFile(FileType.PdeStudentList, criteria, studentList, charterSchoolName, _rootPath);
            }

            return fileName;
        }

        private void AddStudentsToInvoice(ExcelWorksheet sheet, SchoolDistrict schoolDistrict, List<Student> students, List<CharterSchoolSchedule> schedules, string invoiceMonth, int year)
        {
            List<int> invoiceMonths = new List<int>() { 7, 8, 9, 10, 11, 12, 1, 2, 3, 4, 5 };
            int intInvoiceMonth = DateTime.ParseExact(invoiceMonth, "MMMM", CultureInfo.CurrentCulture).Month;
            List<string> invoiceMonthNames = new List<string>() { "JUL", "AUG", "SEP", "OCT", "NOV", "DEC", "JAN", "FEB", "MAR", "APR", "MAY" };

            int row = sheet.Dimension.End.Row + 1;
            foreach (var student in students)
            {
                var schedule = schedules.Where(s => s.AppliesToGrade(student.Grade)).FirstOrDefault();

                int monthIndex = 0;
                if (student.DistrictEntryDate > schedule.FirstDay)
                {
                    monthIndex = invoiceMonths.IndexOf(((DateTime)student.DistrictEntryDate).Month);
                }
                int invoiceMonthIndex = invoiceMonths.IndexOf(intInvoiceMonth);

                for (int i = monthIndex; i <= invoiceMonthIndex; i++)
                {
                    int currentMonthYear = (i <= 5 && invoiceMonthIndex > 5) ? year - 1 : year;
                    var firstDayOfMonth = new DateTime(currentMonthYear, invoiceMonths[i], 1);
                    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

                    // If the student exited before this month started, we're done with this student - break.
                    if (student.ExitDate != null && student.ExitDate < firstDayOfMonth)
                    {
                        break;
                    }

                    // If the student's district entry is after the last day of the month, they didn't start yet - skip this month.
                    if (student.DistrictEntryDate > lastDayOfMonth)
                    {
                        continue;
                    }

                    bool isSped = (student.PriorIepDate != null && student.CurrentIepDate != null) || student.CurrentIepDate < lastDayOfMonth;
                    AddStudentRow(sheet, row, student, schoolDistrict, invoiceMonthNames[i], isSped, 0);
                    row++;
                }
            }
        }

        private void AddStudentsToReconciliation(ExcelWorksheet sheet, SchoolDistrict schoolDistrict, List<Student> students, List<CharterSchoolSchedule> schedules, int year)
        {
            int row = sheet.Dimension.End.Row + 1;
            foreach (var student in students)
            {
                student.GetYearlyAttendanceValue(_dbContext, year, out decimal spedAttendance, out decimal nonSpedAttendance);

                if (spedAttendance > 0)
                {
                    AddStudentRow(sheet, row, student, schoolDistrict, "REC", true, spedAttendance);
                    row++;
                }

                if (nonSpedAttendance > 0)
                {
                    AddStudentRow(sheet, row, student, schoolDistrict, "REC", false, nonSpedAttendance);
                    row++;
                }
            }
        }

        private void AddStudentRow(ExcelWorksheet sheet, int row, Student student, SchoolDistrict schoolDistrict, string month, bool IsSped, decimal adm)
        {
            sheet.Cells["A" + row.ToString()].Value = student.StateStudentNo;
            sheet.Cells["B" + row.ToString()].Value = month;
            sheet.Cells["C" + row.ToString()].Value = student.Aun;
            sheet.Cells["D" + row.ToString()].Value = schoolDistrict.Name;
            sheet.Cells["E" + row.ToString()].Value = IsSped ? "SP" : "NS";
            sheet.Cells["F" + row.ToString()].Style.Numberformat.Format = "mm-dd-yyyy";
            sheet.Cells["F" + row.ToString()].Value = student.Dob;
            sheet.Cells["G" + row.ToString()].Value = student.Grade;
            sheet.Cells["H" + row.ToString()].Value = student.AddressStreet;
            sheet.Cells["I" + row.ToString()].Value = "";
            sheet.Cells["J" + row.ToString()].Value = student.AddressCity;
            sheet.Cells["K" + row.ToString()].Value = student.AddressState;
            sheet.Cells["L" + row.ToString()].Value = student.AddressZip;
            sheet.Cells["M" + row.ToString()].Style.Numberformat.Format = "mm-dd-yyyy";
            sheet.Cells["M" + row.ToString()].Value = student.CurrentIepDate;
            sheet.Cells["N" + row.ToString()].Style.Numberformat.Format = "mm-dd-yyyy";
            sheet.Cells["N" + row.ToString()].Value = student.PriorIepDate;
            sheet.Cells["O" + row.ToString()].Value = "Y";
            sheet.Cells["P" + row.ToString()].Style.Numberformat.Format = "mm-dd-yyyy";
            sheet.Cells["P" + row.ToString()].Value = student.DistrictEntryDate;
            sheet.Cells["Q" + row.ToString()].Style.Numberformat.Format = "mm-dd-yyyy";
            sheet.Cells["Q" + row.ToString()].Value = student.ExitDate;

            if (adm > 0)
            {
                sheet.Cells["R" + row.ToString()].Value = adm;
            }
        }

        private void PopulateHeaderRow(ExcelWorksheet sheet, ReportCriteriaView criteria)
        {
            sheet.Cells["A1"].Value = "PAsecureID";
            sheet.Cells["B1"].Value = "Enrollment Month";
            sheet.Cells["C1"].Value = "SD AUN";
            sheet.Cells["D1"].Value = "School District";
            sheet.Cells["E1"].Value = "Student Type";
            sheet.Cells["F1"].Value = "DOB";
            sheet.Cells["G1"].Value = "Grade";
            sheet.Cells["H1"].Value = "Home Address 1";
            sheet.Cells["I1"].Value = "Home Address 2";
            sheet.Cells["J1"].Value = "Home Address City";
            sheet.Cells["K1"].Value = "Home Address State Code";
            sheet.Cells["L1"].Value = "Home Address Postal Code";
            sheet.Cells["M1"].Value = "Current IEP Date";
            sheet.Cells["N1"].Value = "Prior IEP Date";
            sheet.Cells["O1"].Value = "CSSENF Indicator";
            sheet.Cells["P1"].Value = "First Day Educated";
            sheet.Cells["Q1"].Value = "Last Day Educated";

            if (criteria.IsYearEndRecon)
            {
                sheet.Cells["R1"].Value = "ADM";
            }
        }
    }
}
