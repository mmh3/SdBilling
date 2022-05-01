using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
            var schedule = context.GetCharterSchoolSchedule(CharterSchoolUid, Grade, month, year);
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            
            GetPeriodAttendanceValues(context, schedule, firstDayOfMonth, lastDayOfMonth, out spedAttendance, out decimal spedDays, out nonSpedAttendance, out decimal nonSpedDays, out decimal daysInSession);
        }

        public void GetYearlyAttendanceValue(AppDbContext context, int year, out decimal spedAttendance, out decimal nonSpedAttendance)
        {
            var schedule = context.GetCharterSchoolSchedule(CharterSchoolUid, Grade, year);

            GetPeriodAttendanceValues(context, schedule, schedule.FirstDay, schedule.LastDay, out spedAttendance, out decimal spedDays, out nonSpedAttendance, out decimal nonSpedDays, out decimal daysInSession);
        }

        public void GetYearlyAttendanceValue(AppDbContext context, int year, out decimal spedAttendance, out decimal spedDays, out decimal nonSpedAttendance, out decimal nonSpedDays, out decimal daysInSession)
        {
            var schedule = context.GetCharterSchoolSchedule(CharterSchoolUid, Grade, year);

            GetPeriodAttendanceValues(context, schedule, schedule.FirstDay, schedule.LastDay, out spedAttendance, out spedDays, out nonSpedAttendance, out nonSpedDays, out daysInSession);
        }

        private void GetPeriodAttendanceValues(AppDbContext context, CharterSchoolSchedule schedule, DateTime startDate, DateTime endDate, out decimal spedAttendanceAdm, out decimal spedDays, out decimal nonSpedAttendanceAdm, out decimal nonSpedDays, out decimal daysInSession)
        {
            if (DistrictEntryDate == null)
            {
                throw new Exception("Student " + StateStudentNo + " does not have a district entry date.");
            }

            if (schedule == null)
            {
                spedAttendanceAdm = nonSpedAttendanceAdm = spedDays = nonSpedDays = daysInSession = 0;
                return;
            }

            // TODO: If not calculating day count, this isn't necessary here - can we save the calcuation? Or can we just save the
            // days in session on the schedule so it doesn't ever have to recalc???
            daysInSession = schedule.GetSchoolDays(context, startDate, endDate);

            if (DidAttendForEntirePeriod(startDate, endDate))
            {
                // If the student attended this school for the whole period, they're a full student for ADM. Check if we
                // need to split the month between sped and non-sped buckets.
                if (IsSpedOnDate(startDate) == IsSpedOnDate(endDate))
                {
                    if (IsSpedOnDate(startDate))
                    {
                        spedDays = daysInSession;
                        spedAttendanceAdm = 1;

                        nonSpedDays = nonSpedAttendanceAdm = 0;
                    }
                    else
                    {
                        spedDays = spedAttendanceAdm = 0;

                        nonSpedDays = daysInSession;
                        nonSpedAttendanceAdm = 1;
                    }
                }
                else
                {
                    //TODO: Handle the situation where the student exited sped during the month - will this
                    // be based on a sped exit date field or the prior IEP just expiring after a year?
                    spedDays = schedule.GetSchoolDays(context, (DateTime)CurrentIepDate, endDate);
                    nonSpedDays = daysInSession - spedDays;

                    spedAttendanceAdm = decimal.Round(spedDays / daysInSession, 3);
                    nonSpedAttendanceAdm = 1 - spedAttendanceAdm;
                }
            }
            else
            {
                //TODO: Account for scenario where student entered and exited within the month!!!
                // Student started mid-month
                if (DistrictEntryDate >= startDate)
                {
                    if (IsSpedOnDate(startDate) == IsSpedOnDate(endDate))
                    {
                        if (IsSpedOnDate(startDate))
                        {
                            spedDays = schedule.GetSchoolDays(context, (DateTime)DistrictEntryDate, endDate);
                            spedAttendanceAdm = decimal.Round(spedDays / daysInSession, 3);

                            nonSpedDays = nonSpedAttendanceAdm = 0;
                        }
                        else
                        {
                            spedDays = spedAttendanceAdm = 0;

                            nonSpedDays = schedule.GetSchoolDays(context, (DateTime)DistrictEntryDate, endDate);
                            nonSpedAttendanceAdm = decimal.Round(nonSpedDays / daysInSession, 3);
                        }
                    }
                    else
                    {
                        //TODO: Handle the situation where the student exited sped during the month - will this
                        // be based on a sped exit date field or the prior IEP just expiring after a year?
                        spedDays = schedule.GetSchoolDays(context, (DateTime)CurrentIepDate, endDate);
                        spedAttendanceAdm = decimal.Round(spedDays / daysInSession, 3);

                        // Calculate non-sped up to the date before the IEP start date.
                        nonSpedDays = schedule.GetSchoolDays(context, (DateTime)DistrictEntryDate, ((DateTime)CurrentIepDate).AddDays(-1));
                        nonSpedAttendanceAdm = decimal.Round(nonSpedDays / daysInSession, 3);
                    }
                }
                // Student exited mid-month
                else
                {
                    if (IsSpedOnDate(startDate) == IsSpedOnDate(endDate))
                    {
                        if (IsSpedOnDate(startDate))
                        {
                            spedDays = schedule.GetSchoolDays(context, startDate, (DateTime)ExitDate);
                            spedAttendanceAdm = decimal.Round(spedDays / daysInSession, 3);

                            nonSpedDays = nonSpedAttendanceAdm = 0;
                        }
                        else
                        {
                            spedDays = spedAttendanceAdm = 0;

                            nonSpedDays = schedule.GetSchoolDays(context, startDate, (DateTime)ExitDate);
                            nonSpedAttendanceAdm = decimal.Round(nonSpedDays / daysInSession, 3);
                        }
                    }
                    else
                    {
                        //TODO: Handle the situation where the student exited sped during the month - will this
                        // be based on a sped exit date field or the prior IEP just expiring after a year?
                        spedDays = schedule.GetSchoolDays(context, (DateTime)CurrentIepDate, (DateTime)ExitDate);
                        spedAttendanceAdm = decimal.Round(spedDays / daysInSession, 3);

                        // Calculate non-sped up to the date before the IEP start date.
                        nonSpedDays = schedule.GetSchoolDays(context, startDate, ((DateTime)CurrentIepDate).AddDays(-1));
                        nonSpedAttendanceAdm = decimal.Round(nonSpedDays / daysInSession, 3);
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
