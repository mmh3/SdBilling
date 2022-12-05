using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SchoolDistrictBilling.Data;
using SchoolDistrictBilling.Models;
using SchoolDistrictBilling.Reports;

namespace SchoolDistrictBilling.Services
{
    public class ExcelServices
    {
        public ExcelServices() { }

        public static List<SchoolDistrictRateView> ImportSchoolDistrictRates(AppDbContext context, List<string> fileNames)
        {
            List<SchoolDistrictRateView> rates = new List<SchoolDistrictRateView>();

            for (int pass = 0; pass < 2; pass++)
            {
                // For performance: To minimize the amount of times we need to update the DB, do 2 passes here. One where we'll build up all of the school
                // districts and then save that once at the end of the pass and then a second pass to create the SD rates with one save at the end. That way
                // all of the records for the rates have valid foreign keys to SD records when they're created (and we don't have to save SDs OTF on every iteration.
                bool createRates = false;
                if (pass > 0)
                {
                    createRates = true;
                }

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
                                                    //context.SaveChanges();

                                                    break;

                                                default:
                                                    if (columns[j - 1].ToString().Contains("Nonspecial"))
                                                    {
                                                        rate.SchoolDistrictRate.NonSpedRate = Convert.ToDecimal(worksheet.Cells[i, j].Value);
                                                    }
                                                    else if (columns[j - 1].ToString().Contains("Special"))
                                                    {
                                                        rate.SchoolDistrictRate.SpedRate = Convert.ToDecimal(worksheet.Cells[i, j].Value);
                                                    }
                                                    else if (columns[j - 1].ToString().Contains("Month"))
                                                    {
                                                        rate.SchoolDistrictRate.EffectiveDate = DateTime.Parse(worksheet.Cells[i, j].Value.ToString());
                                                    }

                                                    break;
                                            }
                                        }
                                    }
                                }

                                if (i > 1 && createRates)
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
            }

            return rates;
        }

        public static List<Student> ImportStudents(AppDbContext context, List<string> fileNames, int charterSchoolUid, out ExcelPackage resultFile)
        {
            int validationErrorCount = 0;
            resultFile = null;
            ExcelWorksheet resultSheet = null;
            List<Student> students = new List<Student>();

            foreach (var fileName in fileNames)
            {
                byte[] bin = File.ReadAllBytes(fileName);

                List<string> columns = new List<string>();
                bool checkForColumnHeaders = false;
                bool isHeadersRow = false;

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
                            checkForColumnHeaders = (columns.Count() == 0);
                            // reset this value if the last row was the headers row
                            if (isHeadersRow) isHeadersRow = false;

                            //loop all columns in a row
                            for (int j = worksheet.Dimension.Start.Column; j <= worksheet.Dimension.End.Column; j++)
                            {
                                if (worksheet.Cells[i, j].Value != null)
                                {
                                    if (checkForColumnHeaders)
                                    {
                                        string cellValue = worksheet.Cells[i, j].Value.ToString();
                                        List<string> columnHeaders = new List<string> { "state_studentnumber", "lastfirst", "street", "city", "state", "zip", "districtofresidence", "district", "dob", "grade_level", "districtentrydate", "vsims start date/date new district", "exit date", "iep (y/n)", "previous iep", "current iep" };

                                        // If we haven't already confirmed that this is the headers row, check to see if the current cell value is one of the headers.
                                        // If it is one of the column headers, set the bool that tells us it is. If it's not, break the current loop and check the next row.
                                        if (!isHeadersRow)
                                        {
                                            if (columnHeaders.Contains(cellValue.ToLower()))
                                            {
                                                isHeadersRow = true;
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }

                                        columns.Add(cellValue);
                                    }
                                    else
                                    {
                                        try
                                        {
                                            // TODO: This feels confusing. Is there a better way to do this with the objects and updating/inserting? What
                                            // if the columns are in a different order or have slightly different names?
                                            switch (columns[j - 1].ToLower())
                                            {
                                                case "state_studentnumber":
                                                    student.StateStudentNo = worksheet.Cells[i, j].Value.ToString();
                                                    break;

                                                case "districtofresidence":
                                                    student.Aun = worksheet.Cells[i, j].Value.ToString(); ;
                                                    break;

                                                case "first name":
                                                    student.FirstName = worksheet.Cells[i, j].Value.ToString();
                                                    break;

                                                case "last name":
                                                    student.LastName = worksheet.Cells[i, j].Value.ToString();
                                                    break;

                                                case "lastfirst":
                                                    var fullName = worksheet.Cells[i, j].Value.ToString();
                                                    student.LastName = fullName.Split(",")[0];
                                                    student.FirstName = fullName.Split(",")[1];
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
                                                    //student.Dob = DateTime.Parse(worksheet.Cells[i, j].Value.ToString());
                                                    var dob = worksheet.Cells[i, j].Value.ToString();
                                                    DateTime dobDateTime;
                                                    if (DateTime.TryParse(dob, out dobDateTime))
                                                    {
                                                        student.Dob = dobDateTime;
                                                    }
                                                    else
                                                    {
                                                        student.Dob = DateTime.Parse("01/01/0001");
                                                    }
                                                    break;

                                                case "grade_level":
                                                    var grade = worksheet.Cells[i, j].Value.ToString();
                                                    if (grade == "0" || grade == "KDG")
                                                    {
                                                        grade = "K";
                                                    }
                                                    student.Grade = grade;
                                                    break;

                                                case "districtentrydate":
                                                case "vsims start date/date new district":
                                                    var entryDate = worksheet.Cells[i, j].Value.ToString();
                                                    if (entryDate == "0/0/0") entryDate = "01/01/0001";

                                                    DateTime entryDateTime;
                                                    if (DateTime.TryParse(entryDate, out entryDateTime))
                                                    {
                                                        student.DistrictEntryDate = entryDateTime;
                                                    }
                                                    else
                                                    {
                                                        student.DistrictEntryDate = DateTime.Parse("01/01/0001");
                                                    }
                                                    break;

                                                case "exit date":
                                                    DateTime exitDate;
                                                    if (DateTime.TryParse(worksheet.Cells[i, j].Value.ToString(), out exitDate))
                                                    {
                                                        student.ExitDate = exitDate;
                                                    }
                                                    break;

                                                case "iep":
                                                case "iep (y/n)":
                                                case "s_pa_stu_x.special_education_iep_code":
                                                    student.IepFlag = worksheet.Cells[i, j].Value.ToString();
                                                    break;

                                                case "current iep date":
                                                case "current iep":
                                                    DateTime currentIep;
                                                    if (DateTime.TryParse(worksheet.Cells[i, j].Value.ToString(), out currentIep))
                                                    {
                                                        student.CurrentIepDate = currentIep;
                                                    }
                                                    break;

                                                case "prior iep date":
                                                case "previous iep":
                                                    DateTime priorIep;
                                                    if (DateTime.TryParse(worksheet.Cells[i, j].Value.ToString(), out priorIep))
                                                    {
                                                        student.PriorIepDate = priorIep;
                                                    }
                                                    break;

                                                default:
                                                    break;
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            throw new Exception($"Error processing row {i.ToString()}, column {columns[j - 1]}. {e.Message}");
                                        }
                                    }
                                }
                            }

                            if (columns.Count() > 0 && !isHeadersRow && !student.IsEmpty())
                            {
                                if (student.IsValid(context, out string studentValidationMessage))
                                {
                                    Student existingStudent = context.Students.FirstOrDefault(s => s.StateStudentNo == student.StateStudentNo
                                                                                                    && s.CharterSchoolUid == student.CharterSchoolUid
                                                                                                    && s.Aun == student.Aun);
                                    if (existingStudent == null)
                                    {
                                        if (string.IsNullOrEmpty(student.Aun)) student.Aun = "0";
                                        context.Students.Add(student);
                                    }
                                    else
                                    {
                                        existingStudent.CopyPropertiesFrom(student);
                                    }
                                }
                                else  // If the student data is not valid, add a record to the import result file that will be downloaded at the end of the process.
                                {
                                    if (resultFile == null)
                                    {
                                        resultFile = new ExcelPackage();
                                        resultSheet = resultFile.Workbook.Worksheets.Add("Sheet1");
                                        resultSheet.Cells["A1"].Value = "Import Row #";
                                        resultSheet.Cells["B1"].Value = "State Student #";
                                        resultSheet.Cells["C1"].Value = "Error Message";
                                    }
                                    validationErrorCount++;
                                    resultSheet.Cells["A" + (validationErrorCount + 1).ToString()].Value = i;
                                    resultSheet.Cells["B" + (validationErrorCount + 1).ToString()].Value = student.StateStudentNo;
                                    resultSheet.Cells["C" + (validationErrorCount + 1).ToString()].Value = studentValidationMessage;
                                    resultSheet.Cells.AutoFitColumns();
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
            criteria.IsYearEndRecon = true;
            criteria.Month = "May";

            var fileNames = new List<string>();

            FileInfo reconTemplate = new FileInfo(rootPath + "/reportTemplates/YearEndReconciliation.xlsx");
            var charterSchool = context.CharterSchools.Find(criteria.CharterSchoolUid);
            var charterSchoolName = charterSchool.Name;

            using (ExcelPackage reconciliation = new ExcelPackage(reconTemplate))
            {
                ExcelWorksheet reconSheet = reconciliation.Workbook.Worksheets.FirstOrDefault();

                //Generate for PDE
                if (criteria.SendTo == SubmitTo.PDE.ToString())
                {
                    List<string> files = new List<string>();

                    files.Add(new PdeCsrStudentList(context, rootPath).Generate(criteria));
                    files.Add(new PdeCsrDirectPayment(context, rootPath).Generate(criteria));
                    files.Add(new PdeCsrTuitionRate(context, rootPath).Generate(criteria));

                    return files;
                }

                //Replace header information
                reconSheet.Cells["E1"].Value = charterSchoolName;
                PopulatePrepDates(reconSheet, criteria.SendTo, "H5");

                // Get the list of school district AUNs we're reconciling.
                var sdAuns = context.GetAunsForCharterSchool(criteria.CharterSchoolUid);

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
                    reconSheet.Cells["A5"].Value = aun;
                    reconSheet.Cells["A6"].Value = schoolDistrictName;

                    // Select all students for this charter school and school district.
                    var students = context.GetStudents(criteria.CharterSchoolUid, aun);




                    ////////////////////////////////////
                    // Generate the student template listing all students for the charter school / school district for the year.
                    // TODO: Move to this to a method so the same thing can be called from monthly invoice and year end recon...
                    // Each iteration, start with the empty student template so we don't have extra pages in here from the previous district.
                    FileInfo studentTemplate = new FileInfo(rootPath + "/reportTemplates/ReconIndividualStudent.xlsx");
                    using (ExcelPackage student = new ExcelPackage(studentTemplate))
                    {
                        ExcelWorksheet studentSheet = student.Workbook.Worksheets.FirstOrDefault();
                        studentSheet.Cells["F1"].Value = charterSchoolName;

                        // Set charter school information, including Remit To w/ address
                        studentSheet.Cells["K1"].Value = charterSchool.Name;
                        studentSheet.Cells["K2"].Value = charterSchool.AddressStreet;
                        studentSheet.Cells["K3"].Value = charterSchool.AddressCity + ", " + charterSchool.AddressState + " " + charterSchool.AddressZip;
                        studentSheet.Cells["K4"].Value = charterSchool.Phone;
                        studentSheet.Cells["K6"].Value = DateTime.Now.Date.ToString("MM/dd/yyyy");

                        // Set school district information
                        studentSheet.Cells["B5"].Value = aun;
                        studentSheet.Cells["B6"].Value = schoolDistrictName;

                        PopulateStudentSheet(context, studentSheet, students, criteria);

                        var reconStudentFile = SaveExcelFile(FileType.ReconStudent, student, rootPath, criteria, charterSchoolName, schoolDistrictName);
                        fileNames.Add(reconStudentFile.FullName);
                    }
                    ///////////////////////////////////




                    decimal nonSpedStudentCount = 0;
                    decimal nonSpedStudentMembershipDays = 0;
                    decimal spedStudentCount = 0;
                    decimal spedStudentMembershipDays = 0;
                    decimal daysInSession = 0;

                    foreach (var student in students)
                    {
                        student.GetYearlyAttendanceValue(context, int.Parse(criteria.Year), out decimal spedAttendance, out decimal spedDays, out decimal nonSpedAttendance, out decimal nonSpedDays, out daysInSession);

                        spedStudentCount += Math.Ceiling(spedAttendance);
                        spedStudentMembershipDays += spedDays;

                        nonSpedStudentCount += Math.Ceiling(nonSpedAttendance);
                        nonSpedStudentMembershipDays += nonSpedDays;
                    }
                    
                    PopulateRates(reconSheet, context.GetSchoolDistrictRate(schoolDistrict.SchoolDistrictUid, new DateTime(int.Parse(criteria.Year), 5, 1)), "G10");

                    reconSheet.Cells["C10"].Value = nonSpedStudentCount;
                    reconSheet.Cells["C11"].Value = spedStudentCount;
                    reconSheet.Cells["D10"].Value = nonSpedStudentMembershipDays;
                    reconSheet.Cells["D11"].Value = spedStudentMembershipDays;
                    reconSheet.Cells["E10"].Value = daysInSession;
                    reconSheet.Cells["E11"].Value = daysInSession;

                    var reconFile = SaveExcelFile(FileType.YearEnd, reconciliation, rootPath, criteria, charterSchoolName, schoolDistrictName);
                    fileNames.Add(reconFile.FullName);
                    // Add an audit record that we generated this report.
                    context.Add(new ReportHistory(ReportType.YearEnd, criteria.CharterSchoolUid, schoolDistrict.SchoolDistrictUid, criteria.SendTo, int.Parse(criteria.Year)));

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
            //using (StreamWriter w = File.AppendText(@"C:\temp\TestingInvoices.txt"))
            //{
            //    w.WriteLine("{0}", DateTime.Now);
            //    w.WriteLine("{0}", "GenerateMonthlyInvoice");
            //}

            var charterSchoolName = context.CharterSchools.Find(criteria.CharterSchoolUid).Name;
            var headerDateRange = "For the Months of July " + DateServices.GetStartYear(criteria.Month, criteria.Year) + " to " + criteria.Month + " " + criteria.Year;

            if (criteria.SendTo == SubmitTo.PDE.ToString())
            {
                List<string> files = new List<string>();

                files.Add(new PdeCsrStudentList(context, rootPath).Generate(criteria));
                files.Add(new PdeCsrDirectPayment(context, rootPath).Generate(criteria));
                files.Add(new PdeCsrTuitionRate(context, rootPath).Generate(criteria));

                return files;
            }

            FileInfo invoiceTemplate = new FileInfo(rootPath + "/reportTemplates/MonthlyInvoice.xlsx");
            FileInfo studentTemplate = new FileInfo(rootPath + "/reportTemplates/MonthlyIndividualStudent.xlsx");

            using (ExcelPackage invoice = new ExcelPackage(invoiceTemplate))
            {
                ExcelWorksheet invoiceSheet = invoice.Workbook.Worksheets.FirstOrDefault();
                FileInfo unipayFile = null;

                // Replace header information
                invoiceSheet.Cells["H1"].Value = charterSchoolName;
                invoiceSheet.Cells["H4"].Value = headerDateRange;
                PopulatePrepDates(invoiceSheet, criteria.SendTo, "N6");

                if (criteria.SendTo == SubmitTo.PDE.ToString())
                {
                    unipayFile = GenerateUnipayRequest(criteria, rootPath, charterSchoolName);
                }
                invoiceSheet.Cells["N8"].Value = string.Empty;

                // Get the list of school district AUNs we'll be billing - this will dictate the number of reports we're creating
                var sdAuns = context.GetAunsForCharterSchool(criteria.CharterSchoolUid);

                int unipayRow = 10;
                foreach (var aun in sdAuns)
                {
                    // if aun is 0 it means the student(s) didn't have a home school district in the data. Skip these students for the reports.
                    if (aun == "0") continue;

                    // Each iteration, start with the empty student template so we don't have extra pages in here from the previous district.
                    using (ExcelPackage student = new ExcelPackage(studentTemplate))
                    {
                        ExcelWorksheet studentSheet = student.Workbook.Worksheets.FirstOrDefault();
                        studentSheet.Cells["F1"].Value = charterSchoolName;
                        studentSheet.Cells["F4"].Value = headerDateRange;
                        studentSheet.Cells["K6"].Value = DateTime.Now.Date.ToString("MM/dd/yyyy");

                        var schoolDistrict = context.SchoolDistricts.Where(sd => sd.Aun == aun).FirstOrDefault();
                        var schoolDistrictName = schoolDistrict.Name;

                        //using (StreamWriter w = File.AppendText(@"C:\temp\TestingInvoices.txt"))
                        //{
                        //    w.WriteLine("{0}", DateTime.Now);
                        //    w.WriteLine("{0}", schoolDistrictName);
                        //}

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
                        var schoolDistrictRate = context.GetSchoolDistrictRate(schoolDistrict.SchoolDistrictUid, criteria.LastDayOfMonth());

                        PopulateInvoiceSheet(context, invoiceSheet, criteria, students, schoolDistrictRate);
                        PopulateStudentSheet(context, studentSheet, students, criteria);

                        if (unipayFile != null)
                        {
                            AddSchoolToUnipayRequest(unipayFile, invoiceSheet, unipayRow++, schoolDistrict);
                        }

                        var invoiceFile = SaveExcelFile(FileType.Invoice, invoice, rootPath, criteria, charterSchoolName, schoolDistrictName);
                        SaveExcelFile(FileType.Student, student, rootPath, criteria, charterSchoolName, schoolDistrictName);
                        // Add an audit record that we generated this report.
                        context.Add(new ReportHistory(ReportType.Invoice, criteria.CharterSchoolUid, schoolDistrict.SchoolDistrictUid, criteria.SendTo, criteria.Month, int.Parse(criteria.Year)));

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

                return Directory.EnumerateFiles(FileSystemServices.GetReportPath(FileType.Invoice, rootPath, criteria, charterSchoolName)/*, "*.pdf"*/);
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

                if (!string.IsNullOrEmpty(payment.CheckNo)) sheet.Cells["D" + row.ToString()].Value = payment.CheckNo;

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
            var startDate = new DateTime(int.Parse(DateServices.GetStartYear(criteria.Month, criteria.Year)), 7, 1);
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

                if (!string.IsNullOrEmpty(payment.CheckNo)) invoiceSheet.Cells["E" + row.ToString()].Value = payment.CheckNo;
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
            var headerDateRange = "For the Months of July " + DateServices.GetStartYear(criteria.Month, criteria.Year) + " to " + criteria.Month + " " + criteria.Year;

            using (ExcelPackage unipay = new ExcelPackage(unipayTemplate))
            {
                ExcelWorksheet unipaySheet = unipay.Workbook.Worksheets.FirstOrDefault();

                unipaySheet.Cells["D1"].Value = charterSchoolName;
                unipaySheet.Cells["D3"].Value = "                                        " + headerDateRange;
                unipaySheet.Cells["D4"].Value = "                                        Submission for " + criteria.Month + " " + criteria.Year + " Unipay";
                unipaySheet.Cells["F6"].Value = DateTime.Now.Date.ToString("MM/dd/yyyy");

                unipayRequestFile = SaveExcelFile(FileType.Unipay, unipay, rootPath, criteria, charterSchoolName);
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
                    var invoiceFileName = FileSystemServices.GetReportFileName(FileType.Invoice, rootPath, criteria, charterSchoolName, sdName);
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
                        fileNames.Add(FileSystemServices.GetReportFileName(FileType.Student, rootPath, criteria, charterSchoolName, sdName));
                    }
                }
            }

            files = fileNames;
            return result;
        }

        private static void PopulateInvoiceSheet(AppDbContext context, ExcelWorksheet sheet, ReportCriteriaView criteria, List<Student> students, SchoolDistrictRate rate)
        {
            PopulateInvoiceMonthlyAmounts(context, criteria, sheet, students, rate);
            PopulateRates(sheet, rate, "M12");
        }

        private static void PopulateStudentSheet(AppDbContext context, ExcelWorksheet sheet, List<Student> students, ReportCriteriaView criteria)
        {
            //TODO: this is just for testing for now
            //for (int j = 0; j < 421; j++)
            //{
            //    students.Add(students[0]);
            //}

            DateTime firstDayOfSchool = context.GetCharterSchoolEarliestFirstDayOfSchool(criteria.CharterSchoolUid, int.Parse(DateServices.GetStartYear(criteria.Month, criteria.Year)));
            var activeStudents = students.Where(s => s.ExitDate == null || s.ExitDate > firstDayOfSchool)
                                         .ToList();

            for (int i = 0; i < activeStudents.Count(); i++)
            {
                AddStudent(context, sheet, activeStudents[i], i, criteria);
            }
        }

        private static void AddStudent(AppDbContext context, ExcelWorksheet sheet, Student student, int counter, ReportCriteriaView criteria)
        {
            // Student data starts at row 12. Each additional student moves down by 4 rows.
            // After 8, copy the whole sheet over again so add 46 for each sheet.
            var sheetNum = Math.Floor((decimal)(counter / 8));

            if (counter % 8 == 0)
            {
                var copyStartRow = (sheetNum * 44 + 1).ToString();
                var copyEndRow = (sheetNum * 44 + 93).ToString();
                sheet.Cells["A1:K44"].Copy(sheet.Cells["A" + copyStartRow + ":K" + copyEndRow]);
                ClearAndFormatCopiedSheet(sheet, sheetNum);
            }

            var row = (sheetNum * 44) + 12 + ((counter % 8) * 4);
            var firstRow = row.ToString();
            var secondRow = (row + 1).ToString();
            var thirdRow = (row + 2).ToString();
            var fourthRow = (row + 3).ToString();

            sheet.Cells["C" + secondRow].Value = student.StateStudentNo;
            sheet.Cells["D" + firstRow].Value = student.FirstName + " " + student.LastName;
            sheet.Cells["D" + secondRow].Value = student.AddressStreet;
            sheet.Cells["D" + fourthRow].Value = student.AddressCity + ", " + student.AddressState;
            sheet.Cells["E" + fourthRow].Value = student.AddressZip;
            sheet.Cells["F" + firstRow].Value = student.Dob.HasValue ? student.Dob.Value.ToString("MM/dd/yyyy") : "";
            sheet.Cells["F" + thirdRow].Value = student.Grade;

            sheet.Cells["G" + secondRow].Value = ""; //Submitted date
            sheet.Cells["H" + secondRow].Value = student.DistrictEntryDate.HasValue ? student.DistrictEntryDate.Value.ToString("MM/dd/yyyy") : "";
            sheet.Cells["I" + secondRow].Value = student.ExitDate.HasValue ? student.ExitDate.Value.ToString("MM/dd/yyyy") : "";

            var iepColumn = "J";
            var iepDateColumn = "K";
            if (criteria.IsYearEndRecon)
            {
                student.GetYearlyAttendanceValue(context, int.Parse(criteria.Year), out decimal spedAttendance, out decimal spedDays, out decimal nonSpedAttendance, out decimal nonSpedDays, out decimal daysInSession);
                sheet.Cells["J" + secondRow].Value = spedDays + nonSpedDays;

                iepColumn = "K";
                iepDateColumn = "L";
            }

            sheet.Cells[iepColumn + secondRow].Value = student.IepFlag == "Y" ? "Yes" : "No";
            sheet.Cells[iepDateColumn + secondRow].Value = student.CurrentIepDate.HasValue ? student.CurrentIepDate.Value.ToString("MM/dd/yyyy") : "";
            sheet.Cells[iepDateColumn + fourthRow].Value = student.PriorIepDate.HasValue ? student.PriorIepDate.Value.ToString("MM/dd/yyyy") : "";
        }

        private static void ClearAndFormatCopiedSheet(ExcelWorksheet sheet, decimal sheetNum)
        {
            int row = (int)(sheetNum * 44) + 1;
            sheet.Row(row).Height = 13;
            sheet.Row(++row).Height = 14;
            sheet.Row(++row).Height = 13;
            sheet.Row(++row).Height = 13;
            sheet.Row(++row).Height = 13;
            sheet.Row(++row).Height = 13;
            sheet.Row(++row).Height = 10;
            sheet.Row(++row).Height = 12;
            sheet.Row(++row).Height = 12;
            sheet.Row(++row).Height = 12;
            sheet.Row(++row).Height = 12;

            for (int i = 0; i < 8; i++)
            {
                var studentFirstRow = (sheetNum * 44) + 12 + ((i % 8) * 4);
                var firstRow = studentFirstRow.ToString();
                var secondRow = (studentFirstRow + 1).ToString();
                var thirdRow = (studentFirstRow + 2).ToString();
                var fourthRow = (studentFirstRow + 3).ToString();

                sheet.Row(int.Parse(firstRow)).Height = 13;
                sheet.Row(int.Parse(secondRow)).Height = 13;
                sheet.Row(int.Parse(thirdRow)).Height = 13;
                sheet.Row(int.Parse(fourthRow)).Height = 13;

                sheet.Cells["B" + secondRow].Value = (sheetNum * 8) + (i + 1);
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

        private static void PopulateRates(ExcelWorksheet sheet, SchoolDistrictRate rate, string startCell)
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
                int year = int.Parse(criteria.Year);
                // If we're generating invoicing for January through June, need to decrement the year when calculating students for July through Dec.
                if (invoiceMonth < 7 && i <= 5)
                {
                    year = year - 1;
                }
                PopulateInvoiceMonthlyStudents(context, sheet, students, invoiceMonths[i], year);
            }
        }

        private static void PopulateInvoiceMonthlyStudents(AppDbContext context, ExcelWorksheet sheet, List<Student> students, int month, int year)
        {
            decimal nonSpedStudents = 0;
            decimal spedStudents = 0;
            //var charterSchoolSchedule = context.GetCharterSchoolSchedule(students.First().CharterSchoolUid, month, year);

            foreach (var student in students)
            {
                student.GetMonthlyAttendanceValue(context, month, year, out decimal spedAttendance, out decimal nonSpedAttendance);

                spedStudents += spedAttendance;
                nonSpedStudents += nonSpedAttendance;
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

        private static FileInfo SaveExcelFile(FileType type, ExcelPackage excel, string rootPath, ReportCriteriaView criteria, string charterSchoolName, string schoolDistrictName = null)
        {
            string fileName = FileSystemServices.GetReportFileName(type, rootPath, criteria, charterSchoolName, schoolDistrictName);

            Directory.CreateDirectory(FileSystemServices.GetReportPath(type, rootPath, criteria, charterSchoolName));
            FileInfo outputFile = new FileInfo(fileName);
            excel.SaveAs(outputFile);

            //ConvertFileToPdf(fileName);

            return outputFile;
        }

        private static void ConvertFileToPdf(string fileName)
        {
            HttpClient httpClient = new HttpClient();

            var urlFile = fileName.Replace(@":\", "~~");
            urlFile = urlFile.Replace(@"\", "~");
            urlFile = urlFile.Replace(".xlsx", string.Empty);
            string url = "http://localhost/excel-to-pdf/api/Conversion/Convert/" + urlFile;

            Thread.Sleep(5000);

            //Task.Run(() => httpClient.PostAsync(url, new StringContent(null)).GetAwaiter().GetResult());
            _ = httpClient.PostAsync(url, null).Result;
        }
    }

    enum SubmitTo
    {
        SchoolDistrict,
        PDE
    }
}
