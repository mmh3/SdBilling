using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using SchoolDistrictBilling.Data;
using SchoolDistrictBilling.Models;
using SchoolDistrictBilling.Services;

namespace SchoolDistrictBilling.Reports
{
    public class PdeCsrDirectPayment
    {
        private AppDbContext _dbContext { get; set; }
        private string _rootPath { get; set; }

        public PdeCsrDirectPayment(AppDbContext context, string rootPath)
        {
            _dbContext = context;
            _rootPath = rootPath;
        }

        public string Generate(ReportCriteriaView criteria)
        {
            var fileName = string.Empty;

            using (ExcelPackage directPayment = new ExcelPackage())
            {
                ExcelWorksheet sheet = directPayment.Workbook.Worksheets.Add("Sheet1");

                PopulateHeaderRow(sheet);

                foreach (var sdName in criteria.SelectedSchoolDistricts)
                {
                    if (!string.IsNullOrEmpty(sdName))
                    {
                        var schoolDistrict = _dbContext.SchoolDistricts.Where(sd => sd.Name == sdName).FirstOrDefault();
                        var startDate = new DateTime(int.Parse(DateServices.GetStartYear(criteria.Month, criteria.Year)), 7, 1);
                        var payments = _dbContext.GetPayments(criteria.CharterSchoolUid, schoolDistrict.SchoolDistrictUid, startDate, criteria.LastDayOfMonth());

                        AddPayments(sheet, schoolDistrict, payments);
                    }
                }

                sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

                var charterSchoolName = _dbContext.CharterSchools.Find(criteria.CharterSchoolUid).Name;
                fileName = FileSystemServices.SaveReportFile(FileType.PdeDirectPayment, criteria, directPayment, charterSchoolName, _rootPath);
            }

            return fileName;
        }

        private void AddPayments(ExcelWorksheet sheet, SchoolDistrict schoolDistrict, List<Payment> payments)
        {
            int row = sheet.Dimension.End.Row + 1;
            foreach (var payment in payments)
            {
                // Only including payments by school districts here.
                if (payment.PaidBy == "PDE") continue;

                sheet.Cells["A" + row.ToString()].Value = schoolDistrict.Aun;
                sheet.Cells["B" + row.ToString()].Value = schoolDistrict.Name;
                sheet.Cells["C" + row.ToString()].Value = payment.CheckNo;
                sheet.Cells["D" + row.ToString()].Style.Numberformat.Format = "mm/dd/yyyy";
                sheet.Cells["D" + row.ToString()].Value = payment.Date;
                sheet.Cells["E" + row.ToString()].Value = payment.EnrollmentMonth;
                sheet.Cells["F" + row.ToString()].Value = "SD Payment";
                sheet.Cells["G" + row.ToString()].Value = String.Format("{0:0.00}", payment.Amount);
                sheet.Cells["H" + row.ToString()].Value = payment.Comments;
                
                row++;
            }
        }

        private void PopulateHeaderRow(ExcelWorksheet sheet)
        {
            sheet.Cells["A1"].Value = "SD AUN";
            sheet.Cells["B1"].Value = "School District";
            sheet.Cells["C1"].Value = "Transaction ID";
            sheet.Cells["D1"].Value = "Transaction Date";
            sheet.Cells["E1"].Value = "Enrollment Month";
            sheet.Cells["F1"].Value = "Transaction Type";
            sheet.Cells["G1"].Value = "Transaction Amount";
            sheet.Cells["H1"].Value = "Transaction Comment";
        }
    }
}
