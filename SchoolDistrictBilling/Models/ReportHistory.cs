using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolDistrictBilling.Reports;

namespace SchoolDistrictBilling.Models
{
    [Table("report_history")]
    public class ReportHistory
    {
        public ReportHistory() { }
        public ReportHistory(ReportType type, int charterSchoolUid, int schoolDistrictUid, string sendTo, int year)
        {
            CharterSchoolUid = charterSchoolUid;
            SchoolDistrictUid = schoolDistrictUid;
            ReportType = type.Value;
            SendTo = sendTo;
            ReportYear = year;
        }

        public ReportHistory(ReportType type, int charterSchoolUid, int schoolDistrictUid, string sendTo, string month, int year)
        {
            CharterSchoolUid = charterSchoolUid;
            SchoolDistrictUid = schoolDistrictUid;
            ReportType = type.Value;
            SendTo = sendTo;
            ReportMonth = month;
            ReportYear = year;
        }

        [Column("report_history_uid")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [DisplayName("Id")]
        public int ReportHistoryUid { get; set; }

        [Column("charter_school_uid")]
        [Required]
        public int CharterSchoolUid { get; set; }

        [Column("school_district_uid")]
        public int SchoolDistrictUid { get; set; }

        [Column("run_date")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime RunDate { get; set; } = DateTime.Today;

        [Column("report_type")]
        [StringLength(255)]
        public string ReportType { get; set; }

        [Column("send_to")]
        [StringLength(255)]
        public string SendTo { get; set; }

        [Column("report_month")]
        [StringLength(255)]
        public string ReportMonth { get; set; }

        [Column("report_year")]
        public int ReportYear { get; set; }
    }
}
