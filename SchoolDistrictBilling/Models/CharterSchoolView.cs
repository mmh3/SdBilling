using System.Collections.Generic;

namespace SchoolDistrictBilling.Models
{
    public class CharterSchoolView
    {
        public CharterSchoolView() { }
        public CharterSchoolView(CharterSchool charterSchool)
        {
            CharterSchool = charterSchool;
        }

        public int? CurrentScheduleUid { get; set; }

        public CharterSchool CharterSchool { get; set; } = new CharterSchool();
        public List<CharterSchoolSchedule> CharterSchoolSchedules { get; set; } = new List<CharterSchoolSchedule>();
        public List<CharterSchoolScheduleDate> CharterSchoolScheduleDates { get; set; } = new List<CharterSchoolScheduleDate>();
    }
}
