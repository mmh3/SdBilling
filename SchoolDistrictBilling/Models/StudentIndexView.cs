using System.Collections.Generic;
using System.Linq;
using SchoolDistrictBilling.Data;

namespace SchoolDistrictBilling.Models
{
    public class StudentIndexView
    {
        public StudentIndexView() { }
        public StudentIndexView(AppDbContext context)
        {
            List<Student> students = context.Students.ToList();
            List<SchoolDistrict> schoolDistricts = context.SchoolDistricts.ToList();
            ImportCharterSchools = context.CharterSchools.ToList();

            schoolDistricts.Add(new SchoolDistrict() { SchoolDistrictUid = 0, Name = "UNKNOWN", Aun = "0" });

            var studentViews = from s in students
                               join sd in schoolDistricts on s.Aun equals sd.Aun
                               join cs in ImportCharterSchools on s.CharterSchoolUid equals cs.CharterSchoolUid
                               select new StudentView
                               {
                                   Student = s,
                                   SchoolDistrict = sd,
                                   CharterSchool = cs
                               };

            Students = studentViews.ToList();
        }

        public List<StudentView> Students { get; set; }
        public List<CharterSchool> ImportCharterSchools { get; set; }
        public int ImportCharterSchoolUid { get; set; }
    }
}
