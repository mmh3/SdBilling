using System.Linq;
using SchoolDistrictBilling.Data;

namespace SchoolDistrictBilling.Models
{
    public class StudentView
    {
        public StudentView() { }
        public StudentView(AppDbContext context, Student student)
        {
            Student = student;
            CharterSchool = context.CharterSchools.Find(student.CharterSchoolUid);
            SchoolDistrict = context.SchoolDistricts.Where(sd => sd.Aun == student.Aun).FirstOrDefault();
        }

        public Student Student { get; set; } = new Student();
        public CharterSchool CharterSchool { get; set; } = new CharterSchool();
        public SchoolDistrict SchoolDistrict { get; set; } = new SchoolDistrict();
    }
}
