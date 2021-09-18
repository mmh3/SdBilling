using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SchoolDistrictBilling.Models
{
    [Table("student")]
    public class Student
    {
        public Student() { }

        [Column("student_uid")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [Display(Name = "Id")]
        public int StudentUid { get; set; }

        [Column("state_student_no")]
        [Required]
        [StringLength(255)]
        [Display(Name = "State #")]
        public string StateStudentNo { get; set; }

        [Column("first_name")]
        [Required]
        [StringLength(255)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Column("last_name")]
        [Required]
        [StringLength(255)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Column("address_street")]
        [StringLength(255)]
        [Display(Name = "Street")]
        public string AddressStreet { get; set; }

        [Column("address_city")]
        [StringLength(255)]
        [Display(Name = "City")]
        public string AddressCity { get; set; }

        [Column("address_state")]
        [StringLength(2)]
        [Display(Name = "State")]
        public string AddressState { get; set; }

        [Column("address_zip")]
        [StringLength(255)]
        [Display(Name = "Zip")]
        public string AddressZip { get; set; }

        [Column("dob")]
        [DataType(DataType.Date)]
        public DateTime? Dob { get; set; }

        [Column("grade")]
        [StringLength(1)]
        //TODO: Make Grade char(2)
        public string Grade { get; set; }

        [Column("district_entry_date")]
        [Display(Name = "Entry Date")]
        [DataType(DataType.Date)]
        public DateTime? DistrictEntryDate { get; set; }

        [Column("exit_date")]
        [Display(Name = "Exit Date")]
        [DataType(DataType.Date)]
        public DateTime? ExitDate { get; set; }

        [Column("sped_flag")]
        [StringLength(1)]
        public string SpedFlag { get; set; }

        [Column("iep_flag")]
        [StringLength(1)]
        [Display(Name = "IEP Flag")]
        public string IepFlag { get; set; }

        [Column("current_iep_date")]
        [Display(Name = "Current IEP Date")]
        [DataType(DataType.Date)]
        public DateTime? CurrentIepDate { get; set; }

        [Column("prior_iep_date")]
        [Display(Name = "Prior IEP Date")]
        [DataType(DataType.Date)]
        public DateTime? PriorIepDate { get; set; }

        [Column("charter_school_uid")]
        [Required]
        [Display(Name = "Charter School")]
        public int CharterSchoolUid { get; set; }

        [Column("aun")]
        [Required]
        [StringLength(255)]
        [Display(Name = "School District")]
        public string Aun { get; set; }

        public decimal GetMonthlyAttendanceValue(int month, int year, List<CharterSchoolScheduleDate> holidays)
        {

            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            if (DistrictEntryDate <= firstDayOfMonth && (ExitDate == null || ExitDate >= lastDayOfMonth))
            {
                // If the student attended this school for the whole month, they're a full student.
                return 1;
            }
            else
            {
                var daysInMonth = GetSchoolDays(firstDayOfMonth, lastDayOfMonth, holidays);

                // TODO: If the student enrolled or exited mid-month, need to prorate their enrollment
                // for the actual days they attended.
                // Student started mid-month
                if (DistrictEntryDate >= firstDayOfMonth)
                {
                    return decimal.Round(GetSchoolDays((DateTime)DistrictEntryDate, lastDayOfMonth, holidays) / daysInMonth, 3);
                }
                // Student exited mid-month
                else
                {
                    DateTime exitDate = (DateTime)ExitDate;
                    return decimal.Round(GetSchoolDays(firstDayOfMonth, exitDate, holidays) / daysInMonth, 3);
                }
            }
        }

        private int GetSchoolDays(DateTime from, DateTime to, List<CharterSchoolScheduleDate> holidays)
        {
            var totalDays = 0;
            for (var date = from; date < to; date = date.AddDays(1))
            {
                // If this is a holiday, skip it.
                if (holidays.Any(d => d.Date == date.Date))
                    continue;

                // If this is a weekday, add to the total days.
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    totalDays++;
            }

            return totalDays;
        }

        public void CopyPropertiesFrom(Student student)
        {
            Aun = student.Aun;
            FirstName = student.FirstName;
            LastName = student.LastName;
            AddressStreet = student.AddressStreet;
            AddressCity = student.AddressCity;
            AddressState = student.AddressState;
            AddressZip = student.AddressZip;
            Dob = student.Dob;
            Grade = student.Grade;
            DistrictEntryDate = student.DistrictEntryDate;
            ExitDate = student.ExitDate;
            IepFlag = student.IepFlag;
            CurrentIepDate = student.CurrentIepDate;
            PriorIepDate = student.PriorIepDate;
            CharterSchoolUid = student.CharterSchoolUid;
        }
    }
}
