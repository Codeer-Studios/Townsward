using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Townsward.database
{
    public static class DbManager
    {
        private static TownswardDbContext _context;

        public static void Init(string connectionString)
        {
            var options = new DbContextOptionsBuilder<TownswardDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            _context = new TownswardDbContext(options);

            // Optional: apply migrations
            _context.Database.Migrate();
        }

        public static TownswardDbContext Context => _context;
    }
}
