using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using SchoolDistrictBilling.Data;

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
        [StringLength(2)]
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

        public int GetAttendanceCount(AppDbContext context, int year, out DateTime lastDayOfYear)
        {
            if (DistrictEntryDate == null)
            {
                throw new Exception("Student " + StateStudentNo + " does not have a district entry date.");
            }

            var schedule = context.GetCharterSchoolSchedule(CharterSchoolUid, Grade, year);
            lastDayOfYear = schedule.LastDay.Date;

            if (DistrictEntryDate >= schedule.FirstDay && (ExitDate != null && ExitDate <= schedule.LastDay))
            {
                return schedule.GetSchoolDays(context, (DateTime)DistrictEntryDate, (DateTime)ExitDate);
            }
            else if (DistrictEntryDate >= schedule.FirstDay)
            {
                return schedule.GetSchoolDays(context, (DateTime)DistrictEntryDate, schedule.LastDay);
            }
            else if (ExitDate != null && ExitDate <= schedule.LastDay)
            {
                return schedule.GetSchoolDays(context, schedule.FirstDay, (DateTime)ExitDate);
            }
            else
            {
                return schedule.GetSchoolDays(context, schedule.FirstDay, schedule.LastDay);
            }
        }

        public int GetDaysInSession(AppDbContext context, int year)
        {
            if (DistrictEntryDate == null)
            {
                throw new Exception("Student " + StateStudentNo + " does not have a district entry date.");
            }

            var schedule = context.GetCharterSchoolSchedule(CharterSchoolUid, Grade, year);
            return schedule.GetSchoolDays(context, schedule.FirstDay, schedule.LastDay);
        }

        public decimal GetMonthlyAttendanceValue(AppDbContext context, int month, int year)
        {
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            if (DistrictEntryDate == null)
            {
                throw new Exception("Student " + StateStudentNo + " does not have a district entry date.");
            }

            if (DistrictEntryDate <= firstDayOfMonth && (ExitDate == null || ExitDate >= lastDayOfMonth))
            {
                // If the student attended this school for the whole month, they're a full student.
                return 1;
            }
            else
            {
                var schedule = context.GetCharterSchoolSchedule(CharterSchoolUid, Grade, month, year);
                if (schedule == null)
                {
                    return 0;
                }

                var daysInMonth = schedule.GetSchoolDays(context, firstDayOfMonth, lastDayOfMonth);

                //TODO: Account for scenario where student entered and exited withint the month!!!
                // Student started mid-month
                if (DistrictEntryDate >= firstDayOfMonth)
                {
                    return decimal.Round(schedule.GetSchoolDays(context, (DateTime)DistrictEntryDate, lastDayOfMonth) / (decimal)daysInMonth, 3);
                }
                // Student exited mid-month
                else
                {
                    return decimal.Round(schedule.GetSchoolDays(context, firstDayOfMonth, (DateTime)ExitDate) / (decimal)daysInMonth, 3);
                }
            }
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
