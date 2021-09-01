using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolDistrictBilling.Models
{
    [Table("charter_school_schedule")]
    public class CharterSchoolSchedule
    {
        public CharterSchoolSchedule() { }
        public CharterSchoolSchedule(CharterSchool school)
        {
            CharterSchoolUid = school.CharterSchoolUid;
            StartGrade = "K";
            EndGrade = "12";
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
        public DateTime FirstDay { get; set; }

        [Column("last_day")]
        public DateTime LastDay { get; set; }
    }
}
