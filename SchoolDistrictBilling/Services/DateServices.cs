using System.Collections.Generic;

namespace SchoolDistrictBilling.Services
{
    public static class DateServices
    {
        public static string GetStartYear(string month, string currentYear)
        {
            var secondHalfMonths = new List<string> { "July", "August", "September", "October", "November", "December" };
            if (secondHalfMonths.Contains(month))
            {
                return currentYear;
            }
            else
            {
                return (int.Parse(currentYear) - 1).ToString();
            }
        }
    }
}
