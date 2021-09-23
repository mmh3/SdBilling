using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolDistrictBilling.Models
{
    [Table("school_district")]
    public class SchoolDistrict
    {
        public SchoolDistrict()
        {
        }

        [Column("school_district_uid")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [DisplayName("Id")]
        public int SchoolDistrictUid { get; set; }

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Column("address_uid")]
        public int? AddressUid { get; set; }

        [Column("county")]
        public string County { get; set; }

        [Column("aun")]
        [Required]
        [StringLength(255)]
        public string Aun { get; set; }

        [Column("phone")]
        [StringLength(255)]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

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
    }
}
