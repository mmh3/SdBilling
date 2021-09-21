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

        public int GetSchoolDays(AppDbContext context, DateTime from, DateTime to)
        {
            var totalDays = 0;

            //TODO: Is this going to be a performance problem for the year end reconciliation? Any more efficient way to do this?
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

            return totalDays;
        }
    }
}
