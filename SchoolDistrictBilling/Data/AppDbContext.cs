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
    }
}