using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolDistrictBilling.Models
{
    [Table("school_district_rate")]
    public class SchoolDistrictRate
    {
        public SchoolDistrictRate() { }
        public SchoolDistrictRate(SchoolDistrictRateView rate)
        {
            SchoolDistrictRateUid = rate.SchoolDistrictRate.SchoolDistrictUid;
            SchoolDistrictUid = rate.SchoolDistrictRate.SchoolDistrictUid;
            NonSpedRate = rate.SchoolDistrictRate.NonSpedRate;
            SpedRate = rate.SchoolDistrictRate.SpedRate;
            EffectiveDate = rate.SchoolDistrictRate.EffectiveDate;
            Notes = rate.SchoolDistrictRate.Notes;
            ThreeSixtyThreeFlag = rate.SchoolDistrictRate.ThreeSixtyThreeFlag;
        }

        [Column("school_district_rate_uid")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [DisplayName("Id")]
        public int SchoolDistrictRateUid { get; set; }

        [Column("school_district_uid")]
        [Required]
        public int SchoolDistrictUid { get; set; }

        [Column("non_sped_rate")]
        [DisplayName("Non-sped Rate")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal NonSpedRate { get; set; }

        [Column("sped_rate")]
        [DisplayName("Sped Rate")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal SpedRate { get; set; }

        [Column("effective_date")]
        [DisplayName("Effective Date")]
        [DataType(DataType.Date)]
        public DateTime EffectiveDate { get; set; }

        [Column("notes")]
        [StringLength(255)]
        public string Notes { get; set; }

        [Column("363_flag")]
        [DisplayName("363")]
        public bool ThreeSixtyThreeFlag { get; set; }
    }
}
