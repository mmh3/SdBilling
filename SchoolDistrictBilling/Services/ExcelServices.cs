using System;
using System.Collections.Generic;
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

                    //Replce header information
                    invoiceSheet.Cells["H1"].Value = charterSchoolName;
                    studentSheet.Cells["F1"].Value = charterSchoolName;
                    //TODO: What the heck is wrong with H4??? Why does it not set sometimes...
                    invoiceSheet.Cells["H4"].Value = headerDateRange;
                    studentSheet.Cells["F4"].Value = headerDateRange;
                    //TODO: format these without time
                    invoiceSheet.Cells["N6"].Value = currentDate;
                    invoiceSheet.Cells["N7"].Value = currentDate;
                    invoiceSheet.Cells["N8"].Value = string.Empty;
                    studentSheet.Cells["K6"].Value = currentDate;

                    //Get the list of school districts we'll be billing - this will dictate the number of reports we're creating
                    var schoolDistricts = context.Students.Where(s => s.CharterSchoolUid == criteria.CharterSchoolUid)
                                                          .Select(x => x.Aun)
                                                          .Distinct().ToList();

                    //Select all students for this charter school
                    //var students = context.Students.Where(s => s.CharterSchoolUid == criteria.CharterSchoolUid)
                    //    .OrderBy(x => x.Aun).ThenBy(x => x.Grade).ThenBy(x => x.LastName).ThenBy(x => x.FirstName);

                    foreach (var aun in schoolDistricts)
                    {
                        string schoolDistrictName = context.SchoolDistricts.Where(sd => sd.Aun == aun).FirstOrDefault().Name;

                        invoiceSheet.Cells["A6"].Value = aun;
                        studentSheet.Cells["B5"].Value = aun;
                        invoiceSheet.Cells["A7"].Value = schoolDistrictName;
                        studentSheet.Cells["B6"].Value = schoolDistrictName;

                        SaveExcelFile(ReportType.Invoice, invoice, rootPath, criteria, charterSchoolName, schoolDistrictName);
                        SaveExcelFile(ReportType.Student, student, rootPath, criteria, charterSchoolName, schoolDistrictName);
                    }
                }

                return Directory.EnumerateFiles(GetReportPath(rootPath, criteria, charterSchoolName));
            }
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
