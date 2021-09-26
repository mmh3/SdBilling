using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolDistrictBilling.Models
{
    public class ReportCriteriaView
    {
        public ReportCriteriaView() { }
        public ReportCriteriaView(List<CharterSchool> charterSchools)
        {
            CharterSchools = charterSchools;
        }

        public List<CharterSchool> CharterSchools { get; set; }
        public List<SchoolDistrict> SchoolDistricts { get; set; }
        public SelectList SchoolDistrictList { get; set; }
        public List<string> SendToList { get; set; } = new List<string> { "School", "PDE" };
        public List<string> Months { get; set; } = new List<string> { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        public List<string> Years { get; set; } = new List<string> { "2019", "2020", "2021", "2022", "2023", "2024", "2025", "2026", "2027", "2028", "2029", "2030" };
        public List<string> YearEndYears { get; set; } = new List<string> { "2021-2022", "2022-2023", "2023-2024", "2024-2025", "2025-2026", "2026-2027", "2027-2028", "2028-2029", "2029-2030" };
        public string CurrentMonth { get; set; } = DateTime.Now.ToString("MMMM");
        public string CurrentYear { get; set; } = DateTime.Now.ToString("yyyy");
        [Display(Name ="Charter School")]
        public int CharterSchoolUid { get; set; }
        [Display(Name = "Submit To")]
        public string SendTo { get; set; }
        [Display(Name = "Month")]
        public string Month { get; set; }
        public string Year { get; set; }
        [BindProperty]
        [Display(Name = "School District(s)")]
        public string[] SelectedSchoolDistricts { get; set; }

        public void OnGet()
        {
            SchoolDistrictList = new SelectList(SchoolDistricts, nameof(SchoolDistrict.SchoolDistrictUid), nameof(SchoolDistrict.Name));
        }

        public DateTime LastDayOfMonth()
        {
            // Get the int representation of the month
            var month = DateTime.ParseExact(Month, "MMMM", CultureInfo.CurrentCulture).Month;

            return new DateTime(int.Parse(Year), month, 1).AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        }
    }
}
