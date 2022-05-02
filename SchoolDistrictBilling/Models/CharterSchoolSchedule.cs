using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using SchoolDistrictBilling.Data;

namespace SchoolDistrictBilling.Models
{
    [Table("charter_school_schedule")]
    public class CharterSchoolSchedule
    {
        private int _totalDaysInYear = 0;
        private Dictionary<string, int> _totalDaysInMonth = new Dictionary<string, int>();

        public CharterSchoolSchedule()
        {
            FirstDay = DateTime.Today.Date;
            LastDay = DateTime.Today.Date;
        }
        public CharterSchoolSchedule(CharterSchool school)
        {
            CharterSchoolUid = school.CharterSchoolUid;
            StartGrade = "K";
            EndGrade = "12";

            FirstDay = DateTime.Today.Date;
            LastDay = DateTime.Today.Date;
        }

        [Column("charter_school_schedule_uid")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [DisplayName("Id")]
        public int CharterSchoolScheduleUid { get; set; }

        [Column("charter_school_uid")]
        [Required]
        public int CharterSchoolUid { get; set; }

        [Column("start_grade")]
        [Required]
        [StringLength(2)]
        public string StartGrade { get; set; }

        [Column("end_grade")]
        [Required]
        [StringLength(2)]
        public string EndGrade { get; set; }

        [Column("first_day")]
        [DataType(DataType.Date)]
        public DateTime FirstDay { get; set; }

        [Column("last_day")]
        [DataType(DataType.Date)]
        public DateTime LastDay { get; set; }

        public bool AppliesToGrade(string grade)
        {
            int.TryParse(grade, out int intGrade);
            int.TryParse(StartGrade, out int intStartGrade);
            int.TryParse(EndGrade, out int intEndGrade);

            return intGrade >= intStartGrade && intGrade <= intEndGrade;
        }

        public int GetSchoolDays(AppDbContext context, DateTime from, DateTime to, bool isFullMonth = false, bool isFullYear = false)
        {
            // If we're calcuating for the full year and we've already done it before, just return the days.
            if (isFullYear && _totalDaysInYear > 0)
            {
                return _totalDaysInYear;
            }

            // If we're calculating for the full month and we already have it in our dictionary, use that value.
            if (isFullMonth && _totalDaysInMonth.TryGetValue(from.ToString("MMMM"), out int daysInMonth))
            {
                return daysInMonth;
            }

            var totalDays = 0;

            for (var date = from.Date; date <= to; date = date.AddDays(1))
            {
                // If this date is before the first day of the school year or after the last day of the school year, skip it.
                if (date.Date < FirstDay.Date || date.Date > LastDay.Date)
                    continue;

                // If this is a holiday, skip it.
                if (context.CharterSchoolScheduleDates.Any(d => d.CharterSchoolScheduleUid == CharterSchoolScheduleUid && d.Date.Date == date.Date))
                    continue;

                // If this is a weekday, add to the total days.
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    totalDays++;
            }

            // If calculating for the full year, save off the number of days so we can shortcut the logic next time.
            if (isFullYear)
            {
                _totalDaysInYear = totalDays;
            }

            // If calcuating for the full month, add an entry to our dictionary with the days in the month.
            if (isFullMonth)
            {
                _totalDaysInMonth.Add(from.ToString("MMMM"), totalDays);
            }

            return totalDays;
        }
    }
}
