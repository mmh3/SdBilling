using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            var charterSchoolName = context.CharterSchools.Find(criteria.CharterSchoolUid).Name;
            var headerDateRange = "For the Months of July " + GetStartYear(criteria.Month, criteria.Year) + " to " + criteria.Month + " " + criteria.Year;
            var currentDateString = DateTime.Now.Date.ToString("MM/dd/yyyy");

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

                    if (criteria.SendTo == "School")
                    {
                        invoiceSheet.Cells["N6"].Value = currentDateString;
                        invoiceSheet.Cells["N7"].Value = currentDateString;
                    }
                    else
                    {
                        // If we're generating this invoice straight to PDE and it hasn't been sent to the SD yet,
                        // set the prep and SD dates to the day before.
                        invoiceSheet.Cells["N6"].Value = DateTime.Now.Date.AddDays(-1).ToString("MM/dd/yyyy");
                        invoiceSheet.Cells["N7"].Value = DateTime.Now.Date.AddDays(-1).ToString("MM/dd/yyyy");
                        invoiceSheet.Cells["N8"].Value = currentDateString;

                        unipayFile = GenerateUnipayRequest(criteria, rootPath, charterSchoolName);
                    }
                    invoiceSheet.Cells["N8"].Value = string.Empty;
                    studentSheet.Cells["K6"].Value = currentDateString;

                    // Get the list of school districts we'll be billing - this will dictate the number of reports we're creating
                    var schoolDistricts = context.Students.Where(s => s.CharterSchoolUid == criteria.CharterSchoolUid)
                                                          .Select(x => x.Aun)
                                                          .Distinct().ToList();

                    int unipayRow = 10;
                    foreach (var aun in schoolDistricts)
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
                        var students = context.Students.Where(s => s.CharterSchoolUid == criteria.CharterSchoolUid && s.Aun == aun)
                            .OrderBy(x => x.Grade).ThenBy(x => x.LastName).ThenBy(x => x.FirstName).ToList();

                        // Get the list of holidays for this charter school.
                        var holidays = context.CharterSchoolScheduleDates.ToList();

                        // Get the school district billing rate record for this school district.
                        var schoolDistrictRate = context.SchoolDistrictRates.Where(r => r.SchoolDistrictUid == schoolDistrict.SchoolDistrictUid)
                            .OrderByDescending(x => x.EffectiveDate).FirstOrDefault();

                        PopulateInvoiceSheet(invoiceSheet, criteria, students, holidays, schoolDistrictRate);
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

                            PopulatePayments(paymentSheet, context, criteria, schoolDistrict);
                            invoicePayments.Save();
                        }
                    }
                }

                return Directory.EnumerateFiles(GetReportPath(rootPath, criteria, charterSchoolName));
            }
        }

        private static void PopulatePayments(ExcelWorksheet invoiceSheet, AppDbContext context, MonthlyInvoiceView criteria, SchoolDistrict schoolDistrict)
        {
            var startDate = new DateTime(int.Parse(GetStartYear(criteria.Month, criteria.Year)), 7, 1);
            var monthInt = DateTime.ParseExact(criteria.Month, "MMMM", CultureInfo.CurrentCulture).Month;
            var endDate = new DateTime(int.Parse(criteria.Year), monthInt, 1).AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

            // Get all of the payments for the current school year for the given charter school from the given school district.
            var payments = context.Payments.Where(p => p.CharterSchoolUid == criteria.CharterSchoolUid &&
                                                       p.SchoolDistrictUid == schoolDistrict.SchoolDistrictUid &&
                                                       p.Date >= startDate && p.Date <= endDate).OrderBy(p => p.Date);

            int currentMonth = 0;
            int rowIncrement = 0;
            foreach (var payment in payments)
            {
                // Get the row for the payment based on date.
                int row = GetInvoicePaymentRow(payment.Date);

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

        private static FileInfo GenerateUnipayRequest(MonthlyInvoiceView criteria, string rootPath, string charterSchoolName)
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

        private static bool UpdateInvoiceForPde(AppDbContext context, MonthlyInvoiceView criteria, string rootPath, string charterSchoolName, out List<string> files)
        {
            bool result = false;
            List<string> fileNames = new List<string>();

            var unipayFile = GenerateUnipayRequest(criteria, rootPath, charterSchoolName);
            fileNames.Add(unipayFile.FullName);

            int unipayRow = 10;
            foreach(var sdName in criteria.SelectedSchoolDistricts)
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

        private static void PopulateInvoiceSheet(ExcelWorksheet sheet, MonthlyInvoiceView criteria, List<Student> students, List<CharterSchoolScheduleDate> holidays, SchoolDistrictRate rate)
        {
            PopulateInvoiceMonthlyAmounts(criteria, sheet, students, rate, holidays);
            PopulateAdm(sheet, rate);
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
            sheet.Cells["D" +  firstRow].Value = student.FirstName + " " + student.LastName;
            sheet.Cells["D" + secondRow].Value = student.AddressStreet;
            sheet.Cells["D" + fourthRow].Value = student.AddressCity + ", " + student.AddressState;
            sheet.Cells["E" + fourthRow].Value = student.AddressZip;
            sheet.Cells["F" +  firstRow].Value = student.Dob;
            sheet.Cells["F" +  thirdRow].Value = student.Grade;

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

        private static void PopulateAdm(ExcelWorksheet sheet, SchoolDistrictRate rate)
        {
            sheet.Cells["M12"].Value = rate.NonSpedRate;
            sheet.Cells["M13"].Value = rate.SpedRate;
        }

        private static void PopulateInvoiceMonthlyAmounts(MonthlyInvoiceView criteria, ExcelWorksheet sheet, List<Student> students, SchoolDistrictRate rate, List<CharterSchoolScheduleDate> holidays)
        {
            int invoiceMonth = DateTime.ParseExact(criteria.Month, "MMMM", CultureInfo.CurrentCulture).Month;
            List<int> invoiceMonths = new List<int>() { 7, 8, 9, 10, 11, 12, 1, 2, 3, 4, 5 };

            int monthIndex = invoiceMonths.IndexOf(invoiceMonth);
            for (int i = 0; i <= monthIndex; i++)
            {
                PopulateInvoiceMonthlyStudents(sheet, students, holidays, invoiceMonths[i], Int32.Parse(criteria.Year));
            }
        }

        private static void PopulateInvoiceMonthlyStudents(ExcelWorksheet sheet, List<Student> students, List<CharterSchoolScheduleDate> holidays, int month, int year)
        {
            decimal nonSpedStudents = 0;
            decimal spedStudents = 0;

            foreach (var student in students)
            {
                // TODO: Is sped flag the same as IEP flag or they're different?
                if (student.IepFlag == "Y")
                {
                    spedStudents += student.GetMonthlyAttendanceValue(month, year, holidays);
                }
                else
                {
                    nonSpedStudents += student.GetMonthlyAttendanceValue(month, year, holidays);
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

        private static int GetInvoicePaymentRow(DateTime date)
        {
            switch (date.Month)
            {
                case 7:
                    return 19;

                case 8:
                    return 20;

                case 9:
                    return 21;

                case 10:
                    return 22;

                case 11:
                    return 23;

                case 12:
                    return 24;

                case 1:
                    return 25;

                case 2:
                    return 26;

                case 3:
                    return 27;

                case 4:
                    return 28;

                case 5:
                    return 29;

                case 6:
                    return 30;

                default:
                    throw new Exception("Invalid payment date");
            }
        }

        private static FileInfo SaveExcelFile(ReportType type, ExcelPackage excel, string rootPath, MonthlyInvoiceView criteria, string charterSchoolName, string schoolDistrictName = null)
        {
            string fileName = GetReportFileName(type, rootPath, criteria, charterSchoolName, schoolDistrictName);

            Directory.CreateDirectory(GetReportPath(rootPath, criteria, charterSchoolName));
            FileInfo outputFile = new FileInfo(fileName);
            excel.SaveAs(outputFile);

            return outputFile;
        }

        private static string GetReportFileName(ReportType type, string rootPath, MonthlyInvoiceView criteria, string charterSchoolName, string schoolDistrictName)
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
            else if (type == ReportType.Unipay)
            {
                outputFilePath = outputFilePath + "/" + charterSchoolName + "Unipay.xlsx";
            }
            else
            {
                throw new Exception("Invalid report type passed to GetReportFileName.");
            }

            return outputFilePath;
        }
        private static string GetReportPath(string rootPath, MonthlyInvoiceView criteria, string charterSchoolName)
        {
            return rootPath + "/reports/" + criteria.Year + "/" + criteria.Month + "/" + Regex.Replace(charterSchoolName, @"\s+", "");
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
        Unipay
    }

    enum SubmitTo
    {
        SchoolDistrict,
        PDE
    }
}
