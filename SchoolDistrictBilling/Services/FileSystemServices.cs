using System;
using System.IO;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using SchoolDistrictBilling.Models;
using SchoolDistrictBilling.Reports;

namespace SchoolDistrictBilling.Services
{
    public static class FileSystemServices
    {
        public static string SaveReportFile(FileType reportType, ReportCriteriaView criteria, ExcelPackage excel, string charterSchoolName, string rootPath)
        {
            string fileName = GetReportFileName(reportType, rootPath, criteria, charterSchoolName);
            Directory.CreateDirectory(GetReportPath(reportType, rootPath, criteria, charterSchoolName));

            FileInfo outputFile = new FileInfo(fileName);
            excel.SaveAs(outputFile);

            return fileName;
        }

        public static string GetReportFileName(FileType type, string rootPath, ReportCriteriaView criteria, string charterSchoolName)
        {
            return GetReportFileName(type, rootPath, criteria, charterSchoolName, string.Empty);
        }

        public static string GetReportFileName(FileType type, string rootPath, ReportCriteriaView criteria, string charterSchoolName, string schoolDistrictName)
        {
            string outputFilePath = GetReportPath(type, rootPath, criteria, charterSchoolName);

            switch (type)
            {
                case FileType.Invoice:
                    return Path.Combine(outputFilePath, criteria.Month + criteria.Year + Regex.Replace(schoolDistrictName, @"\s+", "") + ".xlsx");

                case FileType.Student:
                    return Path.Combine(outputFilePath, criteria.Month + criteria.Year + Regex.Replace(schoolDistrictName, @"\s+", "") + "Student.xlsx");

                case FileType.ReconStudent:
                    return Path.Combine(outputFilePath, criteria.Year + Regex.Replace(schoolDistrictName, @"\s+", "") + "Student.xlsx");

                case FileType.Unipay:
                    return Path.Combine(outputFilePath, Regex.Replace(charterSchoolName, @"\s+", "") + "Unipay.xlsx");

                case FileType.YearEnd:
                    return Path.Combine(outputFilePath, criteria.Year + Regex.Replace(schoolDistrictName, @"\s+", "") + "YearEnd.xlsx");

                case FileType.PdeStudentList:
                    return Path.Combine(outputFilePath, Regex.Replace(charterSchoolName, @"\s+", "") + "PdeCsrStudentList.xlsx");

                case FileType.PdeDirectPayment:
                    return Path.Combine(outputFilePath, Regex.Replace(charterSchoolName, @"\s+", "") + "PdeCsrDirectPayment.xlsx");

                case FileType.PdeTuitionRate:
                    return Path.Combine(outputFilePath, Regex.Replace(charterSchoolName, @"\s+", "") + "PdeCsrTuitionRate.xlsx");

                case FileType.PdeStudentListReconciliation:
                    return Path.Combine(outputFilePath, criteria.Year + Regex.Replace(charterSchoolName, @"\s+", "") + "PdeCsrStudentListRecon.xlsx");

                case FileType.DaysAttended:
                    return Path.Combine(outputFilePath, criteria.Year + Regex.Replace(schoolDistrictName, @"\s+", "") + "DaysAttended.xlsx");

                default:
                    throw new Exception("Invalid report type passed to GetReportFileName.");
            }
        }

        public static string GetReportPath(FileType type, string rootPath, ReportCriteriaView criteria, string charterSchoolName)
        {
            if (type == FileType.YearEnd || type == FileType.PdeStudentListReconciliation || type == FileType.DaysAttended)
            {
                return Path.Combine(new string[] { rootPath, "reports", Regex.Replace(charterSchoolName, @"\s+", ""), criteria.Year });
            }
            else
            {
                var path = Path.Combine(new string[] { rootPath, "reports", Regex.Replace(charterSchoolName, @"\s+", ""), criteria.Year, criteria.Month });
                if (criteria.SendTo == "PDE")
                {
                    return Path.Combine(new string[] { path, "PDE" });
                }
                else
                {
                    return path;
                }
            }
        }
    }
}
