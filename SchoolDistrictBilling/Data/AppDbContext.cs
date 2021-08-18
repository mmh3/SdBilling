using Microsoft.EntityFrameworkCore;
using SchoolDistrictBilling.Models;

namespace SchoolDistrictBilling.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<CharterSchool> CharterSchools { get; set; }
        public DbSet<SchoolDistrict> SchoolDistricts { get; set; }
        public DbSet<SchoolDistrictRate> SchoolDistrictRates { get; set; }
        public DbSet<Student> Students { get; set; }
    }
}