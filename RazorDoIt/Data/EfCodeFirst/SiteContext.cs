using System.Data.Entity;
using System.Data.Entity.Database;
using RazorDoIt.Models;

namespace RazorDoIt.Data.EfCodeFirst
{
    public class SiteContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public SiteContext()
            : base("RazorDoIt"){}
    }
}
