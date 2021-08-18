﻿using System;
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
        public decimal NonSpedRate { get; set; }

        [Column("sped_rate")]
        public decimal SpedRate { get; set; }

        [Column("effective_date")]
        public DateTime EffectiveDate { get; set; }

        [Column("notes")]
        [StringLength(255)]
        public string Notes { get; set; }
    }
}