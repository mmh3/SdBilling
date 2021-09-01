namespace SchoolDistrictBilling.Models
{
    public class CalendarEvent
    {
        public int CharterSchoolScheduleUid { get; set; }
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public bool AllDay { get; set; }
    }
}
