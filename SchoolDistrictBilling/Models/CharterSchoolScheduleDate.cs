using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolDistrictBilling.Models
{
    [Table("charter_school_schedule_date")]
    public class CharterSchoolScheduleDate
    {
        public CharterSchoolScheduleDate()
        {
        }

        [Column("charter_school_schedule_date_uid")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [DisplayName("Id")]
        public int CharterSchoolScheduleDateUid { get; set; }

        [Column("charter_school_schedule_uid")]
        [Required]
        public int CharterSchoolScheduleUid { get; set; }

        [Column("date")]
        [Required]
        public DateTime Date { get; set; }

        [Column("date_type")]
        [StringLength(1)]
        public string DateType { get; set; }
    }
}
