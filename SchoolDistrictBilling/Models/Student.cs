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

        public void GetMonthlyAttendanceValue(AppDbContext context, int month, int year, out decimal spedAttendance, out decimal nonSpedAttendance)
        {
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            if (DistrictEntryDate == null)
            {
                throw new Exception("Student " + StateStudentNo + " does not have a district entry date.");
            }

            if (DidAttendForEntirePeriod(firstDayOfMonth, lastDayOfMonth))
            {
                // If the student attended this school for the whole month, they're a full student. Check if we
                // need to split the month between sped and non-sped buckets.
                if (IsSpedOnDate(firstDayOfMonth) == IsSpedOnDate(lastDayOfMonth))
                {
                    if (IsSpedOnDate(firstDayOfMonth))
                    {
                        spedAttendance = 1;
                        nonSpedAttendance = 0;
                    }
                    else
                    {
                        spedAttendance = 0;
                        nonSpedAttendance = 1;
                    }
                }
                else
                {
                    var schedule = context.GetCharterSchoolSchedule(CharterSchoolUid, Grade, month, year);
                    if (schedule == null)
                    {
                        spedAttendance = nonSpedAttendance = 0;
                        return;
                    }

                    var daysInMonth = schedule.GetSchoolDays(context, firstDayOfMonth, lastDayOfMonth);

                    //TODO: Handle the situation where the student exited sped during the month - will this
                    // be based on a sped exit date field or the prior IEP just expiring after a year?
                    spedAttendance = decimal.Round(schedule.GetSchoolDays(context, (DateTime)CurrentIepDate, lastDayOfMonth) / (decimal)daysInMonth, 3);
                    nonSpedAttendance = 1 - spedAttendance;
                }
            }
            else
            {
                var schedule = context.GetCharterSchoolSchedule(CharterSchoolUid, Grade, month, year);
                if (schedule == null)
                {
                    spedAttendance = nonSpedAttendance = 0;
                    return;
                }

                var daysInMonth = schedule.GetSchoolDays(context, firstDayOfMonth, lastDayOfMonth);

                //TODO: Account for scenario where student entered and exited within the month!!!
                // Student started mid-month
                if (DistrictEntryDate >= firstDayOfMonth)
                {
                    if (IsSpedOnDate(firstDayOfMonth) == IsSpedOnDate(lastDayOfMonth))
                    {
                        if (IsSpedOnDate(firstDayOfMonth))
                        {
                            spedAttendance = decimal.Round(schedule.GetSchoolDays(context, (DateTime)DistrictEntryDate, lastDayOfMonth) / (decimal)daysInMonth, 3);
                            nonSpedAttendance = 0;
                        }
                        else
                        {
                            spedAttendance = 0;
                            nonSpedAttendance = decimal.Round(schedule.GetSchoolDays(context, (DateTime)DistrictEntryDate, lastDayOfMonth) / (decimal)daysInMonth, 3);
                        }
                    }
                    else
                    {
                        //TODO: Handle the situation where the student exited sped during the month - will this
                        // be based on a sped exit date field or the prior IEP just expiring after a year?
                        //******TODO: Is this going to calculate completely correctly or is it going to count the Current IEP Start Date in BOTH???
                        spedAttendance = decimal.Round(schedule.GetSchoolDays(context, (DateTime)CurrentIepDate, lastDayOfMonth) / (decimal)daysInMonth, 3);
                        nonSpedAttendance = decimal.Round(schedule.GetSchoolDays(context, (DateTime)DistrictEntryDate, (DateTime)CurrentIepDate) / (decimal)daysInMonth, 3);
                    }
                }
                // Student exited mid-month
                else
                {
                    if (IsSpedOnDate(firstDayOfMonth) == IsSpedOnDate(lastDayOfMonth))
                    {
                        if (IsSpedOnDate(firstDayOfMonth))
                        {
                            spedAttendance = decimal.Round(schedule.GetSchoolDays(context, firstDayOfMonth, (DateTime)ExitDate) / (decimal)daysInMonth, 3);
                            nonSpedAttendance = 0;
                        }
                        else
                        {
                            spedAttendance = 0;
                            nonSpedAttendance = decimal.Round(schedule.GetSchoolDays(context, firstDayOfMonth, (DateTime)ExitDate) / (decimal)daysInMonth, 3);
                        }
                    }
                    else
                    {
                        //TODO: Handle the situation where the student exited sped during the month - will this
                        // be based on a sped exit date field or the prior IEP just expiring after a year?
                        //******TODO: Is this going to calculate completely correctly or is it going to count the Current IEP Start Date in BOTH???
                        spedAttendance = decimal.Round(schedule.GetSchoolDays(context, (DateTime)CurrentIepDate, (DateTime)ExitDate) / (decimal)daysInMonth, 3);
                        nonSpedAttendance = decimal.Round(schedule.GetSchoolDays(context, firstDayOfMonth, (DateTime)CurrentIepDate) / (decimal)daysInMonth, 3);
                    }
                }
            }
        }

        private bool IsSpedOnDate(DateTime date)
        {
            //TODO: Hand the situation where the student exited sped!!! - will this be based on a sped exit date
            // or the prior IEP just expiring after a year?
            if (CurrentIepDate == null && PriorIepDate == null)
            {
                return false;
            }

            if (CurrentIepDate > date && PriorIepDate == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool DidAttendForEntirePeriod(DateTime periodStart, DateTime periodEnd)
        {
            if (DistrictEntryDate <= periodStart && (ExitDate == null || ExitDate >= periodEnd))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void CopyPropertiesFrom(Student student)
        {
            if (!string.IsNullOrEmpty(student.Aun)) Aun = student.Aun;
            if (!string.IsNullOrEmpty(student.FirstName)) FirstName = student.FirstName;
            if (!string.IsNullOrEmpty(student.LastName)) LastName = student.LastName;
            AddressStreet = student.AddressStreet;
            AddressCity = student.AddressCity;
            AddressState = student.AddressState;
            AddressZip = student.AddressZip;
            if (student.Dob != null && student.Dob != DateTime.Parse("01/01/0001")) Dob = student.Dob;
            Grade = student.Grade;
            if (student.DistrictEntryDate != null && student.DistrictEntryDate != DateTime.Parse("01/01/0001")) DistrictEntryDate = student.DistrictEntryDate;
            ExitDate = student.ExitDate;
            IepFlag = student.IepFlag;
            CurrentIepDate = student.CurrentIepDate;
            PriorIepDate = student.PriorIepDate;
            CharterSchoolUid = student.CharterSchoolUid;
        }
    }
}
