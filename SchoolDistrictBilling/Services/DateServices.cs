using System;
using System.Collections.Generic;
using System.Linq;

namespace SchoolDistrictBilling.Services
{
    public static class DateServices
    {
        public static string GetStartYear(string month, string currentYear)
        {
            if (string.IsNullOrEmpty(month) || string.IsNullOrEmpty(currentYear))
            {
                throw new ArgumentException("Month and current year cannot be null or empty");
            }

            if (!int.TryParse(currentYear, out int year))
            {
                throw new ArgumentException("Invalid year format");
            }

            var secondHalfMonths = new List<string> 
            { 
                "July", "August", "September", "October", "November", "December" 
            };

            // Case-insensitive comparison
            if (secondHalfMonths.Contains(month, StringComparer.OrdinalIgnoreCase))
            {
                return currentYear;
            }
            
            return (year - 1).ToString();
        }
    }
}
