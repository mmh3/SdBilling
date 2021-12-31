using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using SchoolDistrictBilling.Data;

namespace SchoolDistrictBilling.Models
{
    [Table("payment")]
    public class Payment
    {
        public Payment() { }
        public Payment(PaymentView view, AppDbContext context)
        {
            PaymentUid = view.PaymentUid;
            CharterSchoolUid = context.CharterSchools.Where(cs => cs.Name == view.CharterSchoolName).FirstOrDefault().CharterSchoolUid;
            SchoolDistrictUid = context.SchoolDistricts.Where(sd => sd.Name == view.SchoolDistrictName).FirstOrDefault().SchoolDistrictUid;
            Date = view.Date;
            CheckNo = view.CheckNo;
            Amount = view.Amount;
            PaidBy = view.PaidBy;
        }

        [Column("payment_uid")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [DisplayName("Id")]
        public int PaymentUid { get; set; }

        [Column("charter_school_uid")]
        [Required]
        public int CharterSchoolUid { get; set; }

        [Column("school_district_uid")]
        [Required]
        public int SchoolDistrictUid { get; set; }

        [Column("date")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Column("check_no")]
        [StringLength(255)]
        public string CheckNo { get; set; }

        [Column("amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Amount { get; set; }

        [Column("paid_by")]
        [StringLength(255)]
        public string PaidBy { get; set; }
    }
}
