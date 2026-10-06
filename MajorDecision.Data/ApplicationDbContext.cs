using MajorDecision.Web.Data;
using MajorDecision.Web.Models;
using MajorDecision.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MajorDecision.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Decision> Decisions { get; set; }
        public DbSet<AnswerSentence> Answers { get; set; }
        public DbSet<DiscussionPage> DiscussionPages { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<AppUserFriendship> Friends { get; set; }
        public DbSet<Notification> Notifications { get; set; }

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
            builder.Entity<AnswerSentence>().HasData(
                new AnswerSentence { Id = 1, Answer = "May be" },
                new AnswerSentence { Id = 2, Answer = "More likely" },
                new AnswerSentence { Id = 3, Answer = "Ask this question tomorrow" },
                new AnswerSentence { Id = 4, Answer = "What do you think?" },
                new AnswerSentence { Id = 5, Answer = "I can not answer" },
                new AnswerSentence { Id = 6, Answer = "..." }
                );

            builder.Entity<AppUserFriendship>(b =>
            {
                b.HasKey(f => new { f.SenderId, f.ReceiverId });
                b.HasOne(fr => fr.Sender)
                .WithMany() // WithMany(fr => fr.SentFriendRequests)
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(fr => fr.Receiver)
                .WithMany() //WithMany(fr => fr.ReceivedFriendRequests)
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
