using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAPIDemoNew.Models;

namespace WebAPIDemoNew.Data
{
    public class ApplicationDBContext(DbContextOptions options):IdentityDbContext(options)
    {
        public DbSet<Villa> Villas { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<VillaAmenities> VillaAmenities { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    }
}
