using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SchoolDistrictBilling.Models
{
    public class PaymentView
    {
        public PaymentView() { }
        public PaymentView(List<CharterSchool> charterSchools, List<SchoolDistrict> schoolDistricts)
        {
            CharterSchools = charterSchools.Select(cs => cs.Name).ToList();
            SchoolDistricts = schoolDistricts.Select(sd => sd.Name).ToList();
        }
        public PaymentView(CharterSchool charterSchool, SchoolDistrict schoolDistrict, Payment payment)
        {
            CharterSchool = charterSchool;
            SchoolDistrict = schoolDistrict;

            PaymentUid = payment.PaymentUid;
            CharterSchoolUid = payment.CharterSchoolUid;
            CharterSchoolName = charterSchool.Name;
            SchoolDistrictUid = payment.SchoolDistrictUid;
            SchoolDistrictName = schoolDistrict.Name;
            Date = payment.Date;
            CheckNo = payment.CheckNo;
            Amount = payment.Amount;
            PaidBy = payment.PaidBy;
        }
        public PaymentView(List<CharterSchool> charterSchools, List<SchoolDistrict> schoolDistricts, Payment payment)
        {
            CharterSchools = charterSchools.Select(cs => cs.Name).ToList();
            SchoolDistricts = schoolDistricts.Select(sd => sd.Name).ToList();

            PaymentUid = payment.PaymentUid;
            CharterSchoolUid = payment.CharterSchoolUid;
            CharterSchoolName = charterSchools.Where(cs => cs.CharterSchoolUid == CharterSchoolUid).FirstOrDefault().Name;
            SchoolDistrictUid = payment.SchoolDistrictUid;
            SchoolDistrictName = schoolDistricts.Where(sd => sd.SchoolDistrictUid == SchoolDistrictUid).FirstOrDefault().Name;
            Date = payment.Date;
            CheckNo = payment.CheckNo;
            Amount = payment.Amount;
            PaidBy = payment.PaidBy;
        }

        public List<string> CharterSchools { get; set; }
        public List<string> SchoolDistricts { get; set; }
        public List<string> PaidByList { get; set; } = new List<string> { "School", "PDE" };

        public CharterSchool CharterSchool { get; set; }
        public SchoolDistrict SchoolDistrict { get; set; }

        [Display(Name = "Charter School")]
        public string CharterSchoolName { get; set; }
        [Display(Name = "School District")]
        public string SchoolDistrictName { get; set; }

        public int PaymentUid { get; set; }
        public int CharterSchoolUid { get; set; }
        public int SchoolDistrictUid { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;
        [Display(Name = "Check #")]
        public int CheckNo { get; set; }
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Amount { get; set; }
        [Display(Name = "Paid By")]
        public string PaidBy { get; set; }
    }
}
