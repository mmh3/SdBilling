using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolDistrictBilling.Models
{
    [Table("charter_school_contact")]
    public class CharterSchoolContact
    {
        public CharterSchoolContact() { }

        [Column("charter_school_contact_uid")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [DisplayName("Id")]
        public int CharterSchoolContactUid { get; set; }

        [Column("charter_school_uid")]
        [Required]
        public int CharterSchoolUid { get; set; }

        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("phone")]
        [StringLength(255)]
        public string Phone { get; set; }

        [Column("email")]
        [StringLength(255)]
        public string Email { get; set; }

        [Column("primary_flag")]
        [StringLength(1)]
        [DisplayName("Primary")]
        public string PrimaryFlag { get; set; } = "N";
    }
}
