using Microsoft.EntityFrameworkCore;
using Townsward.database.models;

namespace Townsward.database
{
    public class TownswardDbContext : DbContext
    {
        public TownswardDbContext(DbContextOptions<TownswardDbContext> options) : base(options) { }

        public DbSet<Player> players { get; set; }
        public DbSet<Guild> guilds { get; set; }
    }
}
