using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SchoolDistrictBilling.Models;
using SchoolDistrictBilling.Reports;

namespace SchoolDistrictBilling.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<CharterSchool> CharterSchools { get; set; }
        public DbSet<SchoolDistrict> SchoolDistricts { get; set; }
        public DbSet<SchoolDistrictRate> SchoolDistrictRates { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<CharterSchoolSchedule> CharterSchoolSchedules { get; set; }
        public DbSet<CharterSchoolScheduleDate> CharterSchoolScheduleDates { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<CharterSchoolContact> CharterSchoolContacts { get; set; }
        public DbSet<SchoolDistrictContact> SchoolDistrictContacts { get; set; }
        public DbSet<ReportHistory> ReportHistoryRecords { get; set; }


        // Get the list of school district AUNs for a given charter school.
        public List<string> GetAunsForCharterSchool(int charterSchoolUid)
        {
            return Students.Where(s => s.CharterSchoolUid == charterSchoolUid)
                           .Select(x => x.Aun)
                           .Distinct()
                           .ToList();
        }

        // Get a list of the students for the given charter school and school district.
        public List<Student> GetStudents(int charterSchoolUid, string aun)
        {
            return Students.Where(s => s.CharterSchoolUid == charterSchoolUid && s.Aun == aun)
                           .OrderBy(x => x.Grade)
                           .ThenBy(x => x.LastName)
                           .ThenBy(x => x.FirstName)
                           .ToList();
        }

        // Get the school district billing rate record for this school district.
        public SchoolDistrictRate GetSchoolDistrictRate(int schoolDistrictUid, DateTime asOfDate)
        {
            return SchoolDistrictRates.Where(r => r.SchoolDistrictUid == schoolDistrictUid &&
                                                  r.EffectiveDate <= asOfDate)
                                      .OrderByDescending(x => x.EffectiveDate)
                                      .FirstOrDefault();
        }

        // Get a list of payments within a date range for the given charter school from the given school district.
        public List<Payment> GetPayments(int charterSchoolUid, int schoolDistrictUid, DateTime startDate, DateTime endDate)
        {
            
            return Payments.Where(p => p.CharterSchoolUid == charterSchoolUid &&
                                       p.SchoolDistrictUid == schoolDistrictUid &&
                                       p.Date >= startDate && p.Date <= endDate)
                           .OrderBy(p => p.Date)
                           .ToList();
        }

        // Get the charter school schedule for the given month, school and grade
        public CharterSchoolSchedule GetCharterSchoolSchedule(int charterSchoolUid, string grade, int month, int year)
        {
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            //TODO: test all scenarios to make sure grade comparison is working here.
            var schedules = CharterSchoolSchedules.Where(s => s.CharterSchoolUid == charterSchoolUid &&
                                                             s.FirstDay.Date <= lastDayOfMonth &&
                                                             s.LastDay.Date >= firstDayOfMonth)
                                                  .ToList();

            return schedules.Where(s => s.AppliesToGrade(grade)).FirstOrDefault();
        }

        // Get the charter school schedule for the given year, school and grade
        public CharterSchoolSchedule GetCharterSchoolSchedule(int charterSchoolUid, string grade, int year)
        {
            //TODO: test all scenarios to make sure grade comparison is working here.
            var schedules = CharterSchoolSchedules.Where(s => s.CharterSchoolUid == charterSchoolUid &&
                                                             s.FirstDay.Year == year - 1 &&
                                                             s.LastDay.Year == year)
                                                  .ToList();

            return schedules.Where(s => s.AppliesToGrade(grade)).FirstOrDefault();
        }

        // Get all charter school schedules for the given month.
        public List<CharterSchoolSchedule> GetCharterSchoolSchedules(int charterSchoolUid, int month, int year)
        {
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            return CharterSchoolSchedules.Where(s => s.CharterSchoolUid == charterSchoolUid &&
                                                     s.FirstDay.Date <= lastDayOfMonth &&
                                                     s.LastDay.Date >= firstDayOfMonth)
                                         .ToList();
        }

        // Get the date of the most recent report for the given report type / charter school / school district combination where it was sent to the SD.
        public DateTime? GetMostRecentSDReportDate(ReportType type, int charterSchoolUid, int schoolDistrictUid, string month, int year)
        {
            var report = ReportHistoryRecords.Where(r => r.CharterSchoolUid == charterSchoolUid &&
                                                         r.SchoolDistrictUid == schoolDistrictUid &&
                                                         r.ReportType == type.Value &&
                                                         r.ReportMonth == month &&
                                                         r.ReportYear == year &&
                                                         r.SendTo == "School")
                                             .OrderByDescending(d => d.RunDate)
                                             .FirstOrDefault();

            if (report == null)
            {
                return null;
            }
            else
            {
                return report.RunDate;
            }
        }
    }
}