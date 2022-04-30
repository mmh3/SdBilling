using System;
using System.Linq;
using OfficeOpenXml;
using SchoolDistrictBilling.Data;
using SchoolDistrictBilling.Models;
using SchoolDistrictBilling.Services;

namespace SchoolDistrictBilling.Reports
{
    public class PdeCsrTuitionRate
    {
        private AppDbContext _dbContext { get; set; }
        private string _rootPath { get; set; }

        public PdeCsrTuitionRate(AppDbContext context, string rootPath)
        {
            _dbContext = context;
            _rootPath = rootPath;
        }

        public string Generate(ReportCriteriaView criteria)
        {
            var fileName = string.Empty;

            using (ExcelPackage tuitionRates = new ExcelPackage())
            {
                ExcelWorksheet sheet = tuitionRates.Workbook.Worksheets.Add("Sheet1");

                PopulateHeaderRow(sheet);

                foreach (var sdName in criteria.SelectedSchoolDistricts)
                {
                    if (!string.IsNullOrEmpty(sdName))
                    {
                        var schoolDistrict = _dbContext.SchoolDistricts.Where(sd => sd.Name == sdName).FirstOrDefault();
                        var schoolDistrictRate = _dbContext.GetSchoolDistrictRate(schoolDistrict.SchoolDistrictUid, criteria.LastDayOfMonth());

                        AddRate(sheet, criteria, schoolDistrict, schoolDistrictRate);
                    }
                }

                sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

                var charterSchoolName = _dbContext.CharterSchools.Find(criteria.CharterSchoolUid).Name;
                fileName = FileSystemServices.SaveReportFile(FileType.PdeTuitionRate, criteria, tuitionRates, charterSchoolName, _rootPath);
            }

            return fileName;
        }

        private void AddRate(ExcelWorksheet sheet, ReportCriteriaView criteria, SchoolDistrict schoolDistrict, SchoolDistrictRate rate)
        {
            int row = sheet.Dimension.End.Row + 1;

            sheet.Cells["A" + row.ToString()].Value = schoolDistrict.Aun;
            sheet.Cells["B" + row.ToString()].Value = schoolDistrict.Name;
            sheet.Cells["C" + row.ToString()].Style.Numberformat.Format = "mm/dd/yyyy";
            sheet.Cells["C" + row.ToString()].Value = GetSentToSdValue(criteria, schoolDistrict);
            sheet.Cells["D" + row.ToString()].Value = String.Format("{0:0.00}", rate.NonSpedRate);
            sheet.Cells["E" + row.ToString()].Value = String.Format("{0:0.00}", rate.SpedRate);
            sheet.Cells["F" + row.ToString()].Value = rate.ThreeSixtyThreeFlag ? "SD" : "PDE";
        }

        private DateTime? GetSentToSdValue(ReportCriteriaView criteria, SchoolDistrict schoolDistrict)
        {
            ReportType reportType = ReportType.Invoice;
            string month = criteria.Month;

            if (criteria.IsYearEndRecon)
            {
                reportType = ReportType.YearEnd;
                month = null;
            }

            return _dbContext.GetMostRecentSDReportDate(reportType,
                                                        criteria.CharterSchoolUid,
                                                        schoolDistrict.SchoolDistrictUid,
                                                        month,
                                                        int.Parse(criteria.Year));
        }

        private void PopulateHeaderRow(ExcelWorksheet sheet)
        {
            sheet.Cells["A1"].Value = "SD AUN";
            sheet.Cells["B1"].Value = "School District";
            sheet.Cells["C1"].Value = "Date Sent to SD";
            sheet.Cells["D1"].Value = "Nonspecial Rate";
            sheet.Cells["E1"].Value = "Special Rate";
            sheet.Cells["F1"].Value = "Source";
        }
    }
}
