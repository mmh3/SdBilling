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
    }
}
