using MajorDecision.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace MajorDecision.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Decision> Decisions { get; set; }
    }
}
