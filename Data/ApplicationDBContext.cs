using Microsoft.EntityFrameworkCore;
using TMS.Models;

namespace TMS.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
            this.Database.SetCommandTimeout(300);
        }

        public DbSet<User> users { get; set; }
        public DbSet<Tasks> tasks { get; set; }
    }
}
