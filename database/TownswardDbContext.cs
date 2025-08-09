using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Townsward.database.models;

namespace Townsward.database
{
    public class TownswardDbContext : DbContext
    {
        public TownswardDbContext(DbContextOptions<TownswardDbContext> options)
            : base(options) { }

        // Tables
        public DbSet<Player> Players { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Optional: Configure table names or constraints here

            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.DiscordUserId).IsUnique(false);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
