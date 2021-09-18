using System.Linq;
using SchoolDistrictBilling.Data;

namespace SchoolDistrictBilling.Models
{
    public class SchoolDistrictRateView
    {
        public SchoolDistrictRateView() { }
        public SchoolDistrictRateView(AppDbContext context, SchoolDistrictRate rate)
        {
            SchoolDistrictRate = rate;
            SchoolDistrict = context.SchoolDistricts.Where(sd => sd.SchoolDistrictUid == rate.SchoolDistrictUid).FirstOrDefault();
        }

        public SchoolDistrict SchoolDistrict { get; set; } = new SchoolDistrict();
        public SchoolDistrictRate SchoolDistrictRate { get; set; } = new SchoolDistrictRate();
    }
}
