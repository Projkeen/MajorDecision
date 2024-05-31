using MajorDecision.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MajorDecision.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Decision> Decisions { get; set; }

        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    base.OnModelCreating(builder);
        //    builder.Entity<IdentityRole>().HasData(new IdentityRole
        //    {
        //        Name = "User",
        //        NormalizedName = "USER",
        //        Id = Guid.NewGuid().ToString(),
        //        ConcurrencyStamp = Guid.NewGuid().ToString()
        //    });

        //    builder.Entity<IdentityRole>().HasData(new IdentityRole
        //    {
        //        Name = "Admin",
        //        NormalizedName = "ADMIN",
        //        Id = Guid.NewGuid().ToString(),
        //        ConcurrencyStamp = Guid.NewGuid().ToString()
        //    });
        //}

    }
}
