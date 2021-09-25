using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolDistrictBilling.Models
{
    [Table("school_district_contact")]
    public class SchoolDistrictContact
    {
        public SchoolDistrictContact() { }

        [Column("school_district_contact_uid")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [DisplayName("Id")]
        public int SchoolDistrictContactUid { get; set; }

        [Column("school_district_uid")]
        [Required]
        public int SchoolDistrictUid { get; set; }

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
