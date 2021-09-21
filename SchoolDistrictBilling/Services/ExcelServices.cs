using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SchoolDistrictBilling.Data;
using SchoolDistrictBilling.Models;

namespace SchoolDistrictBilling.Services
{
    public class ExcelServices
    {
        public ExcelServices() { }

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
                                        switch (columns[j - 1].ToString())
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
                                                if (columns[j - 1].ToString().Contains("Nonspecial"))
                                                {
                                                    //TODO: Why are all the rates coming through as whole dollar amounts?
                                                    rate.SchoolDistrictRate.NonSpedRate = Convert.ToDecimal(worksheet.Cells[i, j].Value);
                                                }
                                                else if (columns[j - 1].ToString().Contains("Special"))
                                                {
                                                    rate.SchoolDistrictRate.SpedRate = Convert.ToDecimal(worksheet.Cells[i, j].Value);
                                                }
                                                else if (columns[j - 1].ToString().Contains("Month"))
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
                    }
                }
            }

            context.SaveChanges();

            return rates;
        }

        public static List<Student> ImportStudents(AppDbContext context, List<string> fileNames, int charterSchoolUid)
        {
            List<Student> students = new List<Student>();

            foreach (var fileName in fileNames)
            {
                byte[] bin = File.ReadAllBytes(fileName);

                List<string> columns = new List<string>();

                //create a new Excel package in a memorystream
                using (MemoryStream stream = new MemoryStream(bin))
                {
                    using (ExcelPackage excelPackage = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();

                        //loop all rows. Start with the second row
                        for (int i = worksheet.Dimension.Start.Row; i <= worksheet.Dimension.End.Row; i++)
                        {
                            Student student = new Student();
                            student.CharterSchoolUid = charterSchoolUid;

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
                                        // TODO: This feels confusing. Is there a better way to do this with the objects and updating/inserting? What
                                        // if the columns are in a different order or have slightly different names?
                                        switch (columns[j - 1].ToLower())
                                        {
                                            case "state student #":
                                                student.StateStudentNo = worksheet.Cells[i, j].Value.ToString();
                                                break;

                                            case "aun":
                                                student.Aun = worksheet.Cells[i, j].Value.ToString();
                                                break;

                                            case "first name":
                                                student.FirstName = worksheet.Cells[i, j].Value.ToString();
                                                break;

                                            case "last name":
                                                student.LastName = worksheet.Cells[i, j].Value.ToString();
                                                break;

                                            case "street":
                                                student.AddressStreet = worksheet.Cells[i, j].Value.ToString();
                                                break;

                                            case "city":
                                                student.AddressCity = worksheet.Cells[i, j].Value.ToString();
                                                break;

                                            case "state":
                                                student.AddressState = worksheet.Cells[i, j].Value.ToString();
                                                break;

                                            case "zip":
                                                student.AddressZip = worksheet.Cells[i, j].Value.ToString();
                                                break;

                                            case "dob":
                                                student.Dob = DateTime.Parse(worksheet.Cells[i, j].Value.ToString());
                                                break;

                                            case "grade":
                                                student.Grade = worksheet.Cells[i, j].Value.ToString();
                                                break;

                                            case "district entry date":
                                                student.DistrictEntryDate = DateTime.Parse(worksheet.Cells[i, j].Value.ToString());
                                                break;

                                            case "exit date":
                                                student.ExitDate = DateTime.Parse(worksheet.Cells[i, j].Value.ToString());
                                                break;

                                            case "iep":
                                                student.IepFlag = worksheet.Cells[i, j].Value.ToString();
                                                break;

                                            case "current iep date":
                                                student.CurrentIepDate = DateTime.Parse(worksheet.Cells[i, j].Value.ToString());
                                                break;

                                            case "prior iep date":
                                                student.PriorIepDate = DateTime.Parse(worksheet.Cells[i, j].Value.ToString());
                                                break;

                                            default:

                                                break;
                                        }
                                    }
                                }
                            }

                            if (i > 1)
                            {
                                Student existingStudent = context.Students.FirstOrDefault(s => s.StateStudentNo == student.StateStudentNo && s.CharterSchoolUid == student.CharterSchoolUid);
                                if (existingStudent == null)
                                {
                                    context.Students.Add(student);
                                }
                                else
                                {
                                    existingStudent.CopyPropertiesFrom(student);
                                }
                            }
                        }
                    }
                }
            }

            context.SaveChanges();

            return students;
        }

        public static IEnumerable<string> GenerateYearEndRecon(AppDbContext context, string rootPath, ReportCriteriaView criteria)
        {
            var fileNames = new List<string>();

            FileInfo reconTemplate = new FileInfo(rootPath + "/reportTemplates/YearEndReconciliation.xlsx");
            var charterSchoolName = context.CharterSchools.Find(criteria.CharterSchoolUid).Name;

            using (ExcelPackage reconciliation = new ExcelPackage(reconTemplate))
            {
                ExcelWorksheet reconSheet = reconciliation.Workbook.Worksheets.FirstOrDefault();

                //TODO: Generate for PDE

                //Replace header information
                reconSheet.Cells["E1"].Value = charterSchoolName;
                PopulatePrepDates(reconSheet, criteria.SendTo, "H5");

                // Get the list of school district AUNs we're reconciling.
                var sdAuns = context.GetAunsForCharterSchool(criteria.CharterSchoolUid);

                foreach(var aun in sdAuns)
                {
                    var schoolDistrict = context.SchoolDistricts.Where(sd => sd.Aun == aun).FirstOrDefault();
                    var schoolDistrictName = schoolDistrict.Name;

                    // If we're sending to PDE and we're here, only generate invoices for the selected SDs.
                    if (criteria.SendTo == "PDE" && !criteria.SelectedSchoolDistricts.Contains(schoolDistrictName))
                    {
                        continue;
                    }

                    // Set school district information
                    reconSheet.Cells["A5"].Value = aun;
                    reconSheet.Cells["A6"].Value = schoolDistrictName;

                    PopulateAdm(reconSheet, context.GetSchoolDistrictRate(schoolDistrict.SchoolDistrictUid), "G10");

                    // Select all students for this charter school and school district.
                    var students = context.GetStudents(criteria.CharterSchoolUid, aun);


                    var nonSpedStudents = students.Where(s => string.IsNullOrEmpty(s.IepFlag) || s.IepFlag.ToUpper() == "N");
                    int nonSpedStudentCount = 0;
                    int nonSpedStudentMembershipDays = 0;
                    int nonSpedStudentTotalDaysInSession = 0;
                    foreach (var student in nonSpedStudents)
                    {
                        nonSpedStudentCount++;
                        nonSpedStudentMembershipDays += student.GetAttendanceCount(context, int.Parse(criteria.Year));
                        if (nonSpedStudentTotalDaysInSession == 0)
                        {
                            nonSpedStudentTotalDaysInSession = student.GetDaysInSession(context, int.Parse(criteria.Year));
                        }
                    }

                    var spedStudents = students.Where(s => s.IepFlag.ToUpper() == "Y");
                    int spedStudentCount = 0;
                    int spedStudentMembershipDays = 0;
                    int spedStudentTotalDaysInSession = 0;
                    foreach (var student in spedStudents)
                    {
                        spedStudentCount++;
                        spedStudentMembershipDays += student.GetAttendanceCount(context, int.Parse(criteria.Year));
                        if (spedStudentTotalDaysInSession == 0)
                        {
                            spedStudentTotalDaysInSession = student.GetDaysInSession(context, int.Parse(criteria.Year));
                        }
                    }

                    if (nonSpedStudentTotalDaysInSession == 0) nonSpedStudentTotalDaysInSession = spedStudentTotalDaysInSession;
                    if (spedStudentTotalDaysInSession == 0) spedStudentTotalDaysInSession = nonSpedStudentTotalDaysInSession;

                    reconSheet.Cells["C10"].Value = nonSpedStudentCount;
                    reconSheet.Cells["C11"].Value = spedStudentCount;
                    reconSheet.Cells["D10"].Value = nonSpedStudentMembershipDays;
                    reconSheet.Cells["D11"].Value = spedStudentMembershipDays;
                    reconSheet.Cells["E10"].Value = nonSpedStudentTotalDaysInSession;
                    reconSheet.Cells["E11"].Value = spedStudentTotalDaysInSession;

                    var reconFile = SaveExcelFile(ReportType.YearEnd, reconciliation, rootPath, criteria, charterSchoolName, schoolDistrictName);
                    fileNames.Add(reconFile.FullName);

                    // Payments can insert rows and mess up the sheet for future iterations of the loop. Re-open the recon sheet
                    // and apply to that instead of doing it before saving.
                    using (ExcelPackage reconPayments = new ExcelPackage(reconFile))
                    {
                        ExcelWorksheet paymentSheet = reconPayments.Workbook.Worksheets.FirstOrDefault();

                        PopulateReconPayments(paymentSheet, context, criteria, schoolDistrict);
                        reconPayments.Save();
                    }
                }
            }

            return fileNames;
        }

        private static void PopulatePrepDates(ExcelWorksheet sheet, string submitTo, string startCell)
        {
            var currentDateString = DateTime.Now.Date.ToString("MM/dd/yyyy");
            var prepDateCell = startCell;

            // "Date Sent to SD" and "Date Sent to PDE" are in the same column as "Prep Date" but need to increment row.
            var cellCharArray = startCell.ToCharArray();
            var sdDateCell = cellCharArray[0] + (int.Parse(startCell.Remove(0, 1)) + 1).ToString();
            var pdeDateCell = cellCharArray[0] + (int.Parse(startCell.Remove(0, 1)) + 2).ToString();

            if (submitTo == "School")
            {
                sheet.Cells[prepDateCell].Value = currentDateString;
                sheet.Cells[sdDateCell].Value = currentDateString;
                sheet.Cells[pdeDateCell].Value = string.Empty;
            }
            else if (submitTo == "PDE")
            {
                // If we're generating this report straight to PDE and it hasn't been sent to the SD yet,
                // set the prep and SD dates to the day before.
                sheet.Cells[prepDateCell].Value = DateTime.Now.Date.AddDays(-1).ToString("MM/dd/yyyy");
                sheet.Cells[sdDateCell].Value = DateTime.Now.Date.AddDays(-1).ToString("MM/dd/yyyy");
                sheet.Cells[pdeDateCell].Value = currentDateString;
            }
            else
            {
                throw new Exception("Invalid 'Submit To' value for report.");
            }
        }

        public static IEnumerable<string> GenerateMonthlyInvoice(AppDbContext context, string rootPath, ReportCriteriaView criteria)
        {
            var charterSchoolName = context.CharterSchools.Find(criteria.CharterSchoolUid).Name;
            var headerDateRange = "For the Months of July " + GetStartYear(criteria.Month, criteria.Year) + " to " + criteria.Month + " " + criteria.Year;

            if (criteria.SendTo == SubmitTo.PDE.ToString())
            {
                List<string> files = new List<string>();
                if (UpdateInvoiceForPde(context, criteria, rootPath, charterSchoolName, out files))
                {
                    return files;
                }
            }

            FileInfo invoiceTemplate = new FileInfo(rootPath + "/reportTemplates/MonthlyInvoice.xlsx");
            FileInfo studentTemplate = new FileInfo(rootPath + "/reportTemplates/MonthlyIndividualStudent.xlsx");

            using (ExcelPackage invoice = new ExcelPackage(invoiceTemplate))
            {
                using (ExcelPackage student = new ExcelPackage(studentTemplate))
                {
                    ExcelWorksheet invoiceSheet = invoice.Workbook.Worksheets.FirstOrDefault();
                    ExcelWorksheet studentSheet = student.Workbook.Worksheets.FirstOrDefault();
                    FileInfo unipayFile = null;

                    // Replace header information
                    invoiceSheet.Cells["H1"].Value = charterSchoolName;
                    studentSheet.Cells["F1"].Value = charterSchoolName;
                    invoiceSheet.Cells["H4"].Value = headerDateRange;
                    studentSheet.Cells["F4"].Value = headerDateRange;
                    PopulatePrepDates(invoiceSheet, criteria.SendTo, "N6");

                    if (criteria.SendTo == SubmitTo.PDE.ToString())
                    {
                        unipayFile = GenerateUnipayRequest(criteria, rootPath, charterSchoolName);
                    }
                    invoiceSheet.Cells["N8"].Value = string.Empty;
                    studentSheet.Cells["K6"].Value = DateTime.Now.Date.ToString("MM/dd/yyyy");

                    // Get the list of school district AUNs we'll be billing - this will dictate the number of reports we're creating
                    var sdAuns = context.GetAunsForCharterSchool(criteria.CharterSchoolUid);

                    int unipayRow = 10;
                    foreach (var aun in sdAuns)
                    {
                        var schoolDistrict = context.SchoolDistricts.Where(sd => sd.Aun == aun).FirstOrDefault();
                        var schoolDistrictName = schoolDistrict.Name;

                        // If we're sending to PDE and we're here, only generate invoices for the selected SDs.
                        if (criteria.SendTo == "PDE" && !criteria.SelectedSchoolDistricts.Contains(schoolDistrictName))
                        {
                            continue;
                        }

                        // Set school district information
                        invoiceSheet.Cells["A6"].Value = aun;
                        studentSheet.Cells["B5"].Value = aun;
                        invoiceSheet.Cells["A7"].Value = schoolDistrictName;
                        studentSheet.Cells["B6"].Value = schoolDistrictName;

                        // Select all students for this charter school and school district.
                        var students = context.GetStudents(criteria.CharterSchoolUid, aun);
                        // Get the school district billing rate record for this school district.
                        var schoolDistrictRate = context.GetSchoolDistrictRate(schoolDistrict.SchoolDistrictUid);

                        PopulateInvoiceSheet(context, invoiceSheet, criteria, students, schoolDistrictRate);
                        PopulateStudentSheet(studentSheet, students);

                        if (unipayFile != null)
                        {
                            AddSchoolToUnipayRequest(unipayFile, invoiceSheet, unipayRow++, schoolDistrict);
                        }

                        var invoiceFile = SaveExcelFile(ReportType.Invoice, invoice, rootPath, criteria, charterSchoolName, schoolDistrictName);
                        SaveExcelFile(ReportType.Student, student, rootPath, criteria, charterSchoolName, schoolDistrictName);

                        // Payments can insert rows and mess up the sheet for future iterations of the loop. Re-open the invoice
                        // and apply to that instead of doing it before saving.
                        using (ExcelPackage invoicePayments = new ExcelPackage(invoiceFile))
                        {
                            ExcelWorksheet paymentSheet = invoicePayments.Workbook.Worksheets.FirstOrDefault();

                            PopulateInvoicePayments(paymentSheet, context, criteria, schoolDistrict);
                            invoicePayments.Save();
                        }
                    }
                }

                return Directory.EnumerateFiles(GetReportPath(ReportType.Invoice, rootPath, criteria, charterSchoolName));
            }
        }

        private static void PopulateReconPayments(ExcelWorksheet sheet, AppDbContext context, ReportCriteriaView criteria, SchoolDistrict schoolDistrict)
        {
            var startDate = new DateTime(int.Parse(criteria.Year) - 1, 7, 1);
            var endDate = new DateTime(int.Parse(criteria.Year), 7, 1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

            // Get all of the payments for the current school year for the given charter school from the given school district.
            var payments = context.GetPayments(criteria.CharterSchoolUid, schoolDistrict.SchoolDistrictUid, startDate, endDate);

            int currentMonth = 0;
            int rowIncrement = 0;
            foreach (var payment in payments)
            {
                // Get the row for the payment based on date.
                int row = GetPaymentRow(payment.Date, 16);

                if (currentMonth == 0)
                {
                    currentMonth = payment.Date.Month;
                }
                else if (payment.Date.Month == currentMonth)
                {
                    rowIncrement++;
                    row = row + rowIncrement;

                    sheet.InsertRow(row, 1);
                    sheet.Cells["D" + row.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells["E" + row.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    sheet.Cells["E" + row.ToString()].Style.Numberformat.Format = "$###,###,##0.00";
                    sheet.Cells["F" + row.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    sheet.Cells["F" + row.ToString()].Style.Numberformat.Format = "$###,###,##0.00";
                    sheet.Cells["G" + row.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    sheet.Cells["G" + row.ToString()].Style.Numberformat.Format = "$###,###,##0.00";
                }
                else
                {
                    currentMonth = payment.Date.Month;
                    row = row + rowIncrement;
                }

                if (!string.IsNullOrEmpty(payment.CheckNo.ToString())) sheet.Cells["D" + row.ToString()].Value = payment.CheckNo;

                if (payment.Amount < 0)
                {
                    sheet.Cells["G" + row.ToString()].Value = Math.Abs(payment.Amount);
                }
                else
                {
                    if (payment.PaidBy == "School")
                    {
                        sheet.Cells["E" + row.ToString()].Value = payment.Amount;
                    }
                    else
                    {
                        sheet.Cells["F" + row.ToString()].Value = payment.Amount;
                    }
                }
            }
        }

        private static void PopulateInvoicePayments(ExcelWorksheet invoiceSheet, AppDbContext context, ReportCriteriaView criteria, SchoolDistrict schoolDistrict)
        {
            var startDate = new DateTime(int.Parse(GetStartYear(criteria.Month, criteria.Year)), 7, 1);
            var monthInt = DateTime.ParseExact(criteria.Month, "MMMM", CultureInfo.CurrentCulture).Month;
            var endDate = new DateTime(int.Parse(criteria.Year), monthInt, 1).AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

            // Get all of the payments for the date range for the given charter school from the given school district.
            var payments = context.GetPayments(criteria.CharterSchoolUid, schoolDistrict.SchoolDistrictUid, startDate, endDate);

            int currentMonth = 0;
            int rowIncrement = 0;
            foreach (var payment in payments)
            {
                // Get the row for the payment based on date.
                int row = GetPaymentRow(payment.Date, 19);

                if (currentMonth == 0)
                {
                    currentMonth = payment.Date.Month;
                }
                else if (payment.Date.Month == currentMonth)
                {
                    rowIncrement++;
                    row = row + rowIncrement;

                    invoiceSheet.InsertRow(row, 1);
                    invoiceSheet.Cells["F" + row.ToString()].Style.Numberformat.Format = "mm-dd-yy";
                    invoiceSheet.Cells["E" + row.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    invoiceSheet.Cells["F" + row.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    invoiceSheet.Cells["G" + row.ToString() + ":H" + row.ToString()].Merge = true;
                    invoiceSheet.Cells["I" + row.ToString() + ":J" + row.ToString()].Merge = true;
                    invoiceSheet.Cells["K" + row.ToString() + ":L" + row.ToString()].Merge = true;
                    invoiceSheet.Cells["G" + row.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    invoiceSheet.Cells["G" + row.ToString()].Style.Numberformat.Format = "$###,###,##0.00";
                    invoiceSheet.Cells["I" + row.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    invoiceSheet.Cells["I" + row.ToString()].Style.Numberformat.Format = "$###,###,##0.00";
                    invoiceSheet.Cells["K" + row.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    invoiceSheet.Cells["K" + row.ToString()].Style.Numberformat.Format = "$###,###,##0.00";
                }
                else
                {
                    currentMonth = payment.Date.Month;
                    row = row + rowIncrement;
                }

                if (!string.IsNullOrEmpty(payment.CheckNo.ToString())) invoiceSheet.Cells["E" + row.ToString()].Value = payment.CheckNo;
                invoiceSheet.Cells["F" + row.ToString()].Value = payment.Date.Date;

                if (payment.Amount < 0)
                {
                    invoiceSheet.Cells["K" + row.ToString()].Value = Math.Abs(payment.Amount);
                }
                else
                {
                    if (payment.PaidBy == "School")
                    {
                        invoiceSheet.Cells["G" + row.ToString()].Value = payment.Amount;
                    }
                    else
                    {
                        invoiceSheet.Cells["I" + row.ToString()].Value = payment.Amount;
                    }
                }
            }
        }

        private static FileInfo GenerateUnipayRequest(ReportCriteriaView criteria, string rootPath, string charterSchoolName)
        {
            FileInfo unipayRequestFile = null;
            FileInfo unipayTemplate = new FileInfo(rootPath + "/reportTemplates/MonthlyUnipayRequest.xlsx");
            var headerDateRange = "For the Months of July " + GetStartYear(criteria.Month, criteria.Year) + " to " + criteria.Month + " " + criteria.Year;

            using (ExcelPackage unipay = new ExcelPackage(unipayTemplate))
            {
                ExcelWorksheet unipaySheet = unipay.Workbook.Worksheets.FirstOrDefault();

                unipaySheet.Cells["D1"].Value = charterSchoolName;
                unipaySheet.Cells["D3"].Value = "                                        " + headerDateRange;
                unipaySheet.Cells["D4"].Value = "                                        Submission for " + criteria.Month + " " + criteria.Year + " Unipay";
                unipaySheet.Cells["F6"].Value = DateTime.Now.Date.ToString("MM/dd/yyyy");

                unipayRequestFile = SaveExcelFile(ReportType.Unipay, unipay, rootPath, criteria, charterSchoolName);
            }

            return unipayRequestFile;
        }

        private static void AddSchoolToUnipayRequest(FileInfo unipayFile, ExcelWorksheet invoiceSheet, int unipayRow, SchoolDistrict schoolDistrict)
        {
            using (ExcelPackage unipay = new ExcelPackage(unipayFile))
            {
                ExcelWorksheet unipaySheet = unipay.Workbook.Worksheets.FirstOrDefault();

                unipaySheet.Cells["C" + unipayRow.ToString()].Value = schoolDistrict.Aun;
                unipaySheet.Cells["D" + unipayRow.ToString()].Value = schoolDistrict.Name;

                invoiceSheet.Cells["N35"].Calculate();
                unipaySheet.Cells["F" + unipayRow.ToString()].Value = invoiceSheet.Cells["N35"].Value;

                unipay.Save();
            }
        }

        private static bool UpdateInvoiceForPde(AppDbContext context, ReportCriteriaView criteria, string rootPath, string charterSchoolName, out List<string> files)
        {
            bool result = false;
            List<string> fileNames = new List<string>();

            var unipayFile = GenerateUnipayRequest(criteria, rootPath, charterSchoolName);
            fileNames.Add(unipayFile.FullName);

            int unipayRow = 10;
            foreach (var sdName in criteria.SelectedSchoolDistricts)
            {
                if (!string.IsNullOrEmpty(sdName))
                {
                    var invoiceFileName = GetReportFileName(ReportType.Invoice, rootPath, criteria, charterSchoolName, sdName);
                    if (!File.Exists(invoiceFileName))
                    {
                        // If the file doesn't exist, return false so we know we have to generate it now.
                        files = fileNames;
                        return false;
                    }

                    using (ExcelPackage invoice = new ExcelPackage(new FileInfo(invoiceFileName)))
                    {
                        // Set the 'Date Sent to PDE' field on the invoice.
                        ExcelWorksheet invoiceSheet = invoice.Workbook.Worksheets.FirstOrDefault();
                        invoiceSheet.Cells["N8"].Value = DateTime.Now.Date.ToString("MM/dd/yyyy");
                        invoice.Save();

                        // Add the school to the unipay request
                        var schoolDistrict = context.SchoolDistricts.Where(sd => sd.Name == sdName).FirstOrDefault();
                        AddSchoolToUnipayRequest(unipayFile, invoiceSheet, unipayRow++, schoolDistrict);

                        // Add this invoice and student template to the list of files that were updated.
                        result = true;
                        fileNames.Add(invoiceFileName);
                        fileNames.Add(GetReportFileName(ReportType.Student, rootPath, criteria, charterSchoolName, sdName));
                    }
                }
            }

            files = fileNames;
            return result;
        }

        private static void PopulateInvoiceSheet(AppDbContext context, ExcelWorksheet sheet, ReportCriteriaView criteria, List<Student> students, SchoolDistrictRate rate)
        {
            PopulateInvoiceMonthlyAmounts(context, criteria, sheet, students, rate);
            PopulateAdm(sheet, rate, "M12");
        }

        private static void PopulateStudentSheet(ExcelWorksheet sheet, List<Student> students)
        {
            //TODO: this is just for testing for now
            //for (int j = 0; j < 24; j++)
            //{
            //    students.Add(students[0]);
            //}

            for (int i = 0; i < students.Count(); i++)
            {
                AddStudent(sheet, students[i], i);
            }
        }

        private static void AddStudent(ExcelWorksheet sheet, Student student, int counter)
        {
            // Student data starts at row 12. Each additional student moves down by 4 rows.
            // After 8, copy the whole sheet over again so add 46 for each sheet.
            var sheetNum = Math.Floor((decimal)(counter / 8));

            if (counter % 8 == 0)
            {
                var copyStartRow = (sheetNum * 46 + 1).ToString();
                var copyEndRow = (sheetNum * 46 + 93).ToString();
                sheet.Cells["A1:K46"].Copy(sheet.Cells["A" + copyStartRow + ":K" + copyEndRow]);
                ClearCopiedSheet(sheet, sheetNum);
            }

            var row = (sheetNum * 46) + 12 + ((counter % 8) * 4);
            var firstRow = row.ToString();
            var secondRow = (row + 1).ToString();
            var thirdRow = (row + 2).ToString();
            var fourthRow = (row + 3).ToString();

            sheet.Cells["C" + secondRow].Value = student.StateStudentNo;
            sheet.Cells["D" + firstRow].Value = student.FirstName + " " + student.LastName;
            sheet.Cells["D" + secondRow].Value = student.AddressStreet;
            sheet.Cells["D" + fourthRow].Value = student.AddressCity + ", " + student.AddressState;
            sheet.Cells["E" + fourthRow].Value = student.AddressZip;
            sheet.Cells["F" + firstRow].Value = student.Dob;
            sheet.Cells["F" + thirdRow].Value = student.Grade;

            sheet.Cells["G" + secondRow].Value = ""; //Submitted date
            sheet.Cells["H" + secondRow].Value = student.DistrictEntryDate;
            sheet.Cells["I" + secondRow].Value = student.ExitDate;
            sheet.Cells["J" + secondRow].Value = student.IepFlag == "Y" ? "Yes" : "No";
            sheet.Cells["K" + secondRow].Value = student.CurrentIepDate;
            sheet.Cells["K" + fourthRow].Value = student.PriorIepDate;
        }

        private static void ClearCopiedSheet(ExcelWorksheet sheet, decimal sheetNum)
        {
            for (int i = 0; i < 8; i++)
            {
                var row = (sheetNum * 46) + 12 + ((i % 8) * 4);
                var firstRow = row.ToString();
                var secondRow = (row + 1).ToString();
                var thirdRow = (row + 2).ToString();
                var fourthRow = (row + 3).ToString();

                sheet.Cells["C" + secondRow].Value = string.Empty;
                sheet.Cells["D" + firstRow].Value = string.Empty;
                sheet.Cells["D" + secondRow].Value = string.Empty;
                sheet.Cells["D" + fourthRow].Value = string.Empty;
                sheet.Cells["E" + fourthRow].Value = string.Empty;
                sheet.Cells["F" + firstRow].Value = string.Empty;
                sheet.Cells["F" + thirdRow].Value = string.Empty;

                sheet.Cells["G" + secondRow].Value = string.Empty;
                sheet.Cells["H" + secondRow].Value = string.Empty;
                sheet.Cells["I" + secondRow].Value = string.Empty;
                sheet.Cells["J" + secondRow].Value = string.Empty;
                sheet.Cells["K" + secondRow].Value = string.Empty;
                sheet.Cells["K" + fourthRow].Value = string.Empty;
            }
        }

        private static void PopulateAdm(ExcelWorksheet sheet, SchoolDistrictRate rate, string startCell)
        {
            var nonSpedCell = startCell;

            // Increment row to get the spedCell address.
            var cellCharArray = startCell.ToCharArray();
            var spedCell = cellCharArray[0] + (int.Parse(startCell.Remove(0, 1)) + 1).ToString();

            sheet.Cells[nonSpedCell].Value = rate.NonSpedRate;
            sheet.Cells[spedCell].Value = rate.SpedRate;
        }

        private static void PopulateInvoiceMonthlyAmounts(AppDbContext context, ReportCriteriaView criteria, ExcelWorksheet sheet, List<Student> students, SchoolDistrictRate rate)
        {
            int invoiceMonth = DateTime.ParseExact(criteria.Month, "MMMM", CultureInfo.CurrentCulture).Month;
            List<int> invoiceMonths = new List<int>() { 7, 8, 9, 10, 11, 12, 1, 2, 3, 4, 5 };

            int monthIndex = invoiceMonths.IndexOf(invoiceMonth);
            for (int i = 0; i <= monthIndex; i++)
            {
                PopulateInvoiceMonthlyStudents(context, sheet, students, invoiceMonths[i], Int32.Parse(criteria.Year));
            }
        }

        private static void PopulateInvoiceMonthlyStudents(AppDbContext context, ExcelWorksheet sheet, List<Student> students, int month, int year)
        {
            decimal nonSpedStudents = 0;
            decimal spedStudents = 0;
            //var charterSchoolSchedule = context.GetCharterSchoolSchedule(students.First().CharterSchoolUid, month, year);

            foreach (var student in students)
            {
                // TODO: Is sped flag the same as IEP flag or they're different?
                if (student.IepFlag == "Y")
                {
                    spedStudents += student.GetMonthlyAttendanceValue(context, month, year);
                }
                else
                {
                    nonSpedStudents += student.GetMonthlyAttendanceValue(context, month, year);
                }
            }

            sheet.Cells[GetInvoiceMonthCell(month, false)].Value = nonSpedStudents;
            sheet.Cells[GetInvoiceMonthCell(month, true)].Value = spedStudents;
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

        private static int GetPaymentRow(DateTime date, int startRow)
        {
            switch (date.Month)
            {
                case 7:
                    return startRow;

                case 8:
                    return startRow + 1;

                case 9:
                    return startRow + 2;

                case 10:
                    return startRow + 3;

                case 11:
                    return startRow + 4;

                case 12:
                    return startRow + 5;

                case 1:
                    return startRow + 6;

                case 2:
                    return startRow + 7;

                case 3:
                    return startRow + 8;

                case 4:
                    return startRow + 9;

                case 5:
                    return startRow + 10;

                case 6:
                    return startRow + 11;

                default:
                    throw new Exception("Invalid payment date");
            }
        }

        private static FileInfo SaveExcelFile(ReportType type, ExcelPackage excel, string rootPath, ReportCriteriaView criteria, string charterSchoolName, string schoolDistrictName = null)
        {
            string fileName = GetReportFileName(type, rootPath, criteria, charterSchoolName, schoolDistrictName);

            Directory.CreateDirectory(GetReportPath(type, rootPath, criteria, charterSchoolName));
            FileInfo outputFile = new FileInfo(fileName);
            excel.SaveAs(outputFile);

            return outputFile;
            //ConvertFileToPdf(fileName);
        }

        private static void ConvertFileToPdf(string fileName)
        {
            HttpClient httpClient = new HttpClient();

            var urlFile = fileName.Replace(":/", "--");
            urlFile = urlFile.Replace("/", "-");
            urlFile = urlFile.Replace(".xlsx", string.Empty);
            string url = "http://localhost/excel-to-pdf/api/Conversion/Convert/" + fileName;

            Task.Run(() => httpClient.PostAsync(url, new StringContent(null)).GetAwaiter().GetResult());
        }

        private static string GetReportFileName(ReportType type, string rootPath, ReportCriteriaView criteria, string charterSchoolName, string schoolDistrictName)
        {
            string outputFilePath = GetReportPath(type, rootPath, criteria, charterSchoolName);

            switch (type)
            {
                case ReportType.Invoice:
                    return Path.Combine(outputFilePath, criteria.Month + criteria.Year + Regex.Replace(schoolDistrictName, @"\s+", "") + ".xlsx");

                case ReportType.Student:
                    return Path.Combine(outputFilePath, criteria.Month + criteria.Year + Regex.Replace(schoolDistrictName, @"\s+", "") + "Student.xlsx");

                case ReportType.Unipay:
                    return Path.Combine(outputFilePath, Regex.Replace(charterSchoolName, @"\s+", "") + "Unipay.xlsx");

                case ReportType.YearEnd:
                    return Path.Combine(outputFilePath, criteria.Year + Regex.Replace(schoolDistrictName, @"\s+", "") + "YearEnd.xlsx");

                default:
                    throw new Exception("Invalid report type passed to GetReportFileName.");
            }
        }
        private static string GetReportPath(ReportType type, string rootPath, ReportCriteriaView criteria, string charterSchoolName)
        {
            if (type == ReportType.YearEnd)
            {
                return Path.Combine(new string[] { rootPath, "reports", Regex.Replace(charterSchoolName, @"\s+", ""), criteria.Year });
            }
            else
            {
                return Path.Combine(new string[] { rootPath, "reports", Regex.Replace(charterSchoolName, @"\s+", ""), criteria.Year, criteria.Month });
            }
        }

        private static string GetStartYear(string month, string currentYear)
        {
            var secondHalfMonths = new List<string> { "July", "August", "September", "October", "November", "December" };
            if (secondHalfMonths.Contains(month))
            {
                return currentYear;
            }
            else
            {
                return (int.Parse(currentYear) - 1).ToString();
            }
        }
    }

    enum ReportType
    {
        Invoice,
        Student,
        Unipay,
        YearEnd
    }

    enum SubmitTo
    {
        SchoolDistrict,
        PDE
    }
}
