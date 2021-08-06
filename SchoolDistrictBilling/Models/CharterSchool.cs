using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolDistrictBilling.Models
{
    [Table("charter_school")]
    public class CharterSchool
    {
        public CharterSchool()
        {
        }

        [Column("charter_school_uid")]
        [Key]
        [DatabaseGenerated (DatabaseGeneratedOption.Identity)]
        [Required]
        [DisplayName("Id")]
        public int CharterSchoolUid { get; set; }

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }
    }
}
