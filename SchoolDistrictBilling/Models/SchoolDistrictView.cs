using System.Collections.Generic;
using System.Linq;
using SchoolDistrictBilling.Data;

namespace SchoolDistrictBilling.Models
{
    public class SchoolDistrictView
    {
        public SchoolDistrictView() { }
        public SchoolDistrictView(AppDbContext context, SchoolDistrict schoolDistrict)
        {
            SchoolDistrict = schoolDistrict;
            Contacts = context.SchoolDistrictContacts.Where(c => c.SchoolDistrictUid == schoolDistrict.SchoolDistrictUid).ToList();
        }

        public SchoolDistrict SchoolDistrict { get; set; }
        public List<SchoolDistrictContact> Contacts { get; set; }
    }
}
