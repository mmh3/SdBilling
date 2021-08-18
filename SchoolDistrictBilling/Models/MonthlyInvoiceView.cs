using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolDistrictBilling.Models
{
    public class MonthlyInvoiceView
    {
        public MonthlyInvoiceView() { }
        public MonthlyInvoiceView(List<CharterSchool> charterSchools)
        {
            CharterSchools = charterSchools;
        }

        public List<CharterSchool> CharterSchools { get; set; }
        public List<string> SendToList { get; set; } = new List<string> { "School", "PDE" };
        public List<string> Months { get; set; } = new List<string> { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        public List<string> Years { get; set; } = new List<string> { "2019", "2020", "2021", "2022", "2023", "2024", "2025", "2026", "2027", "2028", "2029", "2030" };
        public string CurrentMonth { get; set; } = DateTime.Now.ToString("MMMM");
        public string CurrentYear { get; set; } = DateTime.Now.ToString("yyyy");
        [Display(Name ="Charter School")]
        public int CharterSchoolUid { get; set; }
        [Display(Name = "Submit To")]
        public string SendTo { get; set; }
        [Display(Name = "Month")]
        public string Month { get; set; }
        public string Year { get; set; }
    }
}
