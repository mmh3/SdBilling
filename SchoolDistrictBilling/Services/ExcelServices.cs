using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using SchoolDistrictBilling.Data;
using SchoolDistrictBilling.Models;

namespace SchoolDistrictBilling.Services
{
    public class ExcelServices
    {
        public ExcelServices()
        {
        }

        public static List<SchoolDistrictRateView> ImportSchoolDistrictRates(AppDbContext context, List<string> fileNames)
        {
            List<SchoolDistrictRateView> rates = new List<SchoolDistrictRateView>();

            foreach (var fileName in fileNames)
            {
                byte[] bin = File.ReadAllBytes(fileName);

                List<string> columns = new List<string>();

                //create a new Excel package in a memorystream
                using (MemoryStream stream = new MemoryStream(bin))
                {
                    using (ExcelPackage excelPackage = new ExcelPackage(stream))
                    {
                        //loop all worksheets
                        //TODO: What's the deal with the second worksheet? Is it safe to only import the first worksheet?
                        //foreach (ExcelWorksheet worksheet in excelPackage.Workbook.Worksheets)
                        //{
                        ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();

                            //loop all rows. Start with the second row
                            for (int i = worksheet.Dimension.Start.Row; i <= worksheet.Dimension.End.Row; i++)
                            {
                                SchoolDistrictRateView rate = new SchoolDistrictRateView();
                                SchoolDistrict sd = null;

                                //loop all columns in a row
                                for (int j = worksheet.Dimension.Start.Column; j <= worksheet.Dimension.End.Column; j++)
                                {
                                    if (worksheet.Cells[i, j].Value != null)
                                    {
                                        if (i == 1)
                                        {
                                            columns.Add(worksheet.Cells[i, j].Value.ToString());
                                        }
                                        else
                                        {
                                            // TODO: This feels confusing. Is there a better way to do this with the objects and updating/inserting? What if the columns are in a different order?
                                            switch (columns[j-1].ToString())
                                            {
                                                case "AUN":
                                                    rate.SchoolDistrict.Aun = worksheet.Cells[i, j].Value.ToString();
                                                    sd = context.SchoolDistricts.FirstOrDefault(s => s.Aun == rate.SchoolDistrict.Aun);

                                                    if (sd == null)
                                                    {
                                                        sd = new SchoolDistrict
                                                        {
                                                            Aun = rate.SchoolDistrict.Aun
                                                        };
                                                    }
                                                    else
                                                    {
                                                        rate.SchoolDistrict.SchoolDistrictUid = sd.SchoolDistrictUid;
                                                    }
                                                    break;

                                                case "School District":
                                                    rate.SchoolDistrict.Name = worksheet.Cells[i, j].Value.ToString();

                                                    if (sd != null)
                                                    {
                                                        sd.Name = rate.SchoolDistrict.Name;
                                                    }
                                                    break;

                                                case "County":
                                                    rate.SchoolDistrict.County = worksheet.Cells[i, j].Value.ToString();

                                                    if (sd != null)
                                                    {
                                                        sd.County = rate.SchoolDistrict.County;
                                                    }

                                                    if (rate.SchoolDistrict.SchoolDistrictUid == 0)
                                                    {
                                                        context.SchoolDistricts.Add(sd);
                                                    }
                                                    context.SaveChanges();

                                                    break;

                                                default:
                                                    if (columns[j-1].ToString().Contains("Nonspecial"))
                                                    {
                                                        //TODO: Why are all the rates coming through as whole dollar amounts?
                                                        rate.SchoolDistrictRate.NonSpedRate = Convert.ToDecimal(worksheet.Cells[i, j].Value);
                                                    }
                                                    else if(columns[j-1].ToString().Contains("Special"))
                                                    {
                                                        rate.SchoolDistrictRate.SpedRate = Convert.ToDecimal(worksheet.Cells[i, j].Value);
                                                    }
                                                    else if (columns[j-1].ToString().Contains("Month"))
                                                    {
                                                        //TODO: Display this without the timestamp
                                                        rate.SchoolDistrictRate.EffectiveDate = DateTime.Parse(worksheet.Cells[i, j].Value.ToString());
                                                    }

                                                    break;
                                            }
                                        }
                                    }
                                }

                                if (i > 1)
                                {
                                    SchoolDistrictRate sdr = context.SchoolDistrictRates.FirstOrDefault(r => r.SchoolDistrictUid == sd.SchoolDistrictUid && r.EffectiveDate == rate.SchoolDistrictRate.EffectiveDate);
                                    if (sdr == null)
                                    {
                                        sdr = new SchoolDistrictRate(rate);
                                        sdr.SchoolDistrictUid = sd.SchoolDistrictUid;
                                        context.SchoolDistrictRates.Add(sdr);
                                    }
                                    else
                                    {
                                        sdr.NonSpedRate = rate.SchoolDistrictRate.NonSpedRate;
                                        sdr.SpedRate = rate.SchoolDistrictRate.SpedRate;
                                    }
                                }
                            }
                        //}
                    }
                }
            }

            context.SaveChanges();

            return rates;
        }

        public static IEnumerable<string> GenerateMonthlyInvoice(AppDbContext context, string rootPath, MonthlyInvoiceView criteria)
        {
            FileInfo invoiceTemplate = new FileInfo(rootPath + "/reportTemplates/MonthlyInvoice.xlsx");
            FileInfo studentTemplate = new FileInfo(rootPath + "/reportTemplates/MonthlyIndividualStudent.xlsx");

            using (ExcelPackage invoice = new ExcelPackage(invoiceTemplate))
            {
                string charterSchoolName = context.CharterSchools.Find(criteria.CharterSchoolUid).Name;

                using (ExcelPackage student = new ExcelPackage(studentTemplate))
                {
                    ExcelWorksheet invoiceSheet = invoice.Workbook.Worksheets.FirstOrDefault();
                    ExcelWorksheet studentSheet = student.Workbook.Worksheets.FirstOrDefault();

                    string headerDateRange = "For the Months of July " + criteria.Year + " to " + criteria.Month + " " + criteria.Year;
                    string currentDate = DateTime.Now.Date.ToString("MM/dd/yyyy");

                    // Replace header information
                    invoiceSheet.Cells["H1"].Value = charterSchoolName;
                    studentSheet.Cells["F1"].Value = charterSchoolName;
                    invoiceSheet.Cells["H4"].Value = headerDateRange;
                    studentSheet.Cells["F4"].Value = headerDateRange;
                    invoiceSheet.Cells["N6"].Value = currentDate;
                    invoiceSheet.Cells["N7"].Value = currentDate;
                    invoiceSheet.Cells["N8"].Value = string.Empty;
                    studentSheet.Cells["K6"].Value = currentDate;

                    // Get the list of school districts we'll be billing - this will dictate the number of reports we're creating
                    var schoolDistricts = context.Students.Where(s => s.CharterSchoolUid == criteria.CharterSchoolUid)
                                                          .Select(x => x.Aun)
                                                          .Distinct().ToList();

                    foreach (var aun in schoolDistricts)
                    {
                        var schoolDistrict = context.SchoolDistricts.Where(sd => sd.Aun == aun).FirstOrDefault();
                        var schoolDistrictName = schoolDistrict.Name;

                        // Set school district information
                        invoiceSheet.Cells["A6"].Value = aun;
                        studentSheet.Cells["B5"].Value = aun;
                        invoiceSheet.Cells["A7"].Value = schoolDistrictName;
                        studentSheet.Cells["B6"].Value = schoolDistrictName;

                        // Select all students for this charter school and school district.
                        var students = context.Students.Where(s => s.CharterSchoolUid == criteria.CharterSchoolUid && s.Aun == aun)
                            .OrderBy(x => x.Grade).ThenBy(x => x.LastName).ThenBy(x => x.FirstName);

                        // Get the school district billing rate record for this school district.

                        var schoolDistrictRate = context.SchoolDistrictRates.Where(r => r.SchoolDistrictUid == schoolDistrict.SchoolDistrictUid)
                            .OrderByDescending(x => x.EffectiveDate).FirstOrDefault();

                        PopulateInvoiceMonthlyAmounts(criteria, invoiceSheet, students.ToList(), schoolDistrictRate);

                        // Add a new student row to the student template.
                        //studentSheet.Cells["B40:K43"].Copy(studentSheet.Cells["B44:K47"]);
                        studentSheet.Cells[40, 2, 43, 11].Copy(studentSheet.Cells[44, 2, 47, 11]);

                        SaveExcelFile(ReportType.Invoice, invoice, rootPath, criteria, charterSchoolName, schoolDistrictName);
                        SaveExcelFile(ReportType.Student, student, rootPath, criteria, charterSchoolName, schoolDistrictName);
                    }
                }

                return Directory.EnumerateFiles(GetReportPath(rootPath, criteria, charterSchoolName));
            }
        }

        private static void PopulateInvoiceMonthlyAmounts(MonthlyInvoiceView criteria, ExcelWorksheet sheet, List<Student> students, SchoolDistrictRate rate)
        {
            int invoiceMonth = DateTime.ParseExact(criteria.Month, "MMMM", CultureInfo.CurrentCulture).Month;
            List<int> invoiceMonths = new List<int>() { 7, 8, 9, 10, 11, 12, 1, 2, 3, 4, 5 };

            int monthIndex = invoiceMonths.IndexOf(invoiceMonth);
            for (int i = 0; i <= monthIndex; i++)
            {
                PopulateInvoiceMonthAmount(sheet, students, rate, invoiceMonths[i], Int32.Parse(criteria.Year));
            }
        }

        private static void PopulateInvoiceMonthAmount(ExcelWorksheet sheet, List<Student> students, SchoolDistrictRate rate, int month, int year)
        {
            decimal nonSpedTotal = 0;
            decimal spedTotal = 0;
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            foreach (var student in students)
            {
                if (student.DistrictEntryDate <= firstDayOfMonth &&
                    (student.ExitDate == null || student.ExitDate >= lastDayOfMonth))
                {
                    // If the student attended this school for the whole month, just take district rate / 12.
                    // TODO: Is sped flag the same as IEP flag or they're different?
                    if (student.IepFlag == "Y")
                    {
                        spedTotal += (rate.SpedRate / 12);
                    }
                    else
                    {
                        nonSpedTotal += (rate.NonSpedRate / 12);
                    }
                }
                else
                {
                    // TODO: If the student enrolled or exited mid-month, need to prorate the amount.

                }
            }

            sheet.Cells[GetInvoiceMonthCell(month, false)].Value = nonSpedTotal;
            sheet.Cells[GetInvoiceMonthCell(month, true)].Value = spedTotal;
        }

        private static string GetInvoiceMonthCell(int month, bool specialEducation)
        {
            string cell = string.Empty;

            switch (month)
            {
                case 7:
                    cell = "B";
                    break;

                case 8:
                    cell = "C";
                    break;

                case 9:
                    cell = "D";
                    break;

                case 10:
                    cell = "E";
                    break;

                case 11:
                    cell = "F";
                    break;

                case 12:
                    cell = "G";
                    break;

                case 1:
                    cell = "H";
                    break;

                case 2:
                    cell = "I";
                    break;

                case 3:
                    cell = "J";
                    break;

                case 4:
                    cell = "K";
                    break;

                case 5:
                    cell = "L";
                    break;

                default:
                    throw new Exception("No cell for May on the Monthly Invoice Template");
            }


            if (specialEducation)
            {
                cell += "13";
            }
            else
            {
                cell += "12";
            }

            return cell;
        }

        private static void SaveExcelFile(ReportType type, ExcelPackage excel, string rootPath, MonthlyInvoiceView criteria, string charterSchoolName, string schoolDistrictName)
        {
            string outputFilePath = GetReportPath(rootPath, criteria, charterSchoolName);
            if (type == ReportType.Invoice)
            {
                outputFilePath = outputFilePath + "/" + criteria.Month + criteria.Year + Regex.Replace(schoolDistrictName, @"\s+", "") + ".xlsx";
            }
            else if (type == ReportType.Student)
            {
                outputFilePath = outputFilePath + "/" + criteria.Month + criteria.Year + Regex.Replace(schoolDistrictName, @"\s+", "") + "Student.xlsx";
            }
            else
            {
                throw new Exception("Invalid report type passed to SaveExcelFile.");
            }

            Directory.CreateDirectory(GetReportPath(rootPath, criteria, charterSchoolName));
            FileInfo outputFile = new FileInfo(outputFilePath);
            excel.SaveAs(outputFile);
        }

        private static string GetReportPath(string rootPath, MonthlyInvoiceView criteria, string charterSchoolName)
        {
            return rootPath + "/reports/" + criteria.Year + "/" + criteria.Month + "/" + Regex.Replace(charterSchoolName, @"\s+", "");
        }
    }

    enum ReportType
    {
        Invoice,
        Student
    }
}
