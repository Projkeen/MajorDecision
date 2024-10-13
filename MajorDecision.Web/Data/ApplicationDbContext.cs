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
        public DbSet<Answers> Answers { get; set; }

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

        //manual list of answers. You may add a new sentences in Admin account
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Answers>().HasData(
                new Answers { Id = 1, Answer = "May be" },
                new Answers { Id = 2, Answer = "More likely" },
                new Answers { Id = 3, Answer = "Ask this question tomorrow" },
                new Answers { Id = 4, Answer = "What do you think?" },
                new Answers { Id = 5, Answer = "I can not answer" },
                new Answers { Id = 6, Answer = "..." }
                );

        }
    }
}
