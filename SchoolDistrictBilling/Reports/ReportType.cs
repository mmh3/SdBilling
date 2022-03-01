namespace SchoolDistrictBilling.Reports
{
    public class ReportType
    {
        private ReportType(string value) { Value = value; }

        public string Value { get; private set; }

        public static ReportType Invoice { get { return new ReportType("Invoice"); } }
        public static ReportType YearEnd { get { return new ReportType("YearEnd"); } }
    }
}