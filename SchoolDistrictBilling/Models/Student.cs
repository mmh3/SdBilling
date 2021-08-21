using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolDistrictBilling.Models
{
    [Table("student")]
    public class Student
    {
        public Student()
        {
        }

        [Column("student_uid")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int StudentUid { get; set; }

        [Column("state_student_no")]
        [Required]
        [StringLength(255)]
        public string StateStudentNo { get; set; }

        [Column("first_name")]
        [Required]
        [StringLength(255)]
        public string FirstName { get; set; }

        [Column("last_name")]
        [Required]
        [StringLength(255)]
        public string LastName { get; set; }

        [Column("address_street")]
        [StringLength(255)]
        public string AddressStreet { get; set; }

        [Column("address_city")]
        [StringLength(255)]
        public string AddressCity { get; set; }

        [Column("address_state")]
        [StringLength(2)]
        public string AddressState { get; set; }

        [Column("address_zip")]
        [StringLength(255)]
        public string AddressZip { get; set; }

        [Column("dob")]
        public DateTime Dob { get; set; }

        [Column("grade")]
        [StringLength(1)]
        public string Grade { get; set; }

        [Column("district_entry_date")]
        public DateTime DistrictEntryDate { get; set; }

        [Column("exit_date")]
        public DateTime? ExitDate { get; set; }

        [Column("sped_flag")]
        [StringLength(1)]
        public string SpedFlag { get; set; }

        [Column("iep_flag")]
        [StringLength(1)]
        public string IepFlag { get; set; }

        [Column("current_iep_date")]
        public DateTime? CurrentIepDate { get; set; }

        [Column("prior_iep_date")]
        public DateTime? PriorIepDate { get; set; }

        [Column("charter_school_uid")]
        [Required]
        public int CharterSchoolUid { get; set; }

        [Column("aun")]
        [Required]
        [StringLength(255)]
        public string Aun { get; set; }
    }
}
