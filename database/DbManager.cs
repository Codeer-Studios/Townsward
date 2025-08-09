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
        private static readonly string BasePath = "db";
        private static readonly Dictionary<ulong, TownswardDbContext> _guildContexts = new();

        private const string CurrentDbVersion = "0001";
        private const string DbPrefix = "database";
        private const string DbExtension = ".sqlite";


        public static bool CreateGuildDirectory(ulong guildId)
        {
            try
            {
                string folderPath = Path.Combine(BasePath, guildId.ToString());
                string dbPath = Path.Combine(folderPath, $"{DbPrefix}{CurrentDbVersion}{DbExtension}");

                var options = new DbContextOptionsBuilder<TownswardDbContext>()
                    .UseSqlite($"Data Source={dbPath}")
                    .Options;

                var context = new TownswardDbContext(options);
                context.Database.Migrate();

                _guildContexts[guildId] = context;

                Console.WriteLine($"[DB] Created database for guild {guildId} at {dbPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB ERROR] Failed to create database for guild {guildId}: {ex.Message}");
                return false;
            }
        }

        private static bool CreateGuildDatabase(ulong guildId)
        {
            try
            {
                var folderPath = Path.Combine(BasePath, guildId.ToString());
                var dbPath = Path.Combine(folderPath, "database.sqlite");

                var options = new DbContextOptionsBuilder<TownswardDbContext>()
                    .UseSqlite($"Data Source={dbPath}")
                    .Options;

                var context = new TownswardDbContext(options);

                // Run any pending migrations (creates DB + tables)
                context.Database.Migrate();

                _guildContexts[guildId] = context;

                Console.WriteLine($"[DB] Created database for guild {guildId} at {dbPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB ERROR] Failed to create database for guild {guildId}: {ex.Message}");
                return false;
            }
        }

        public static TownswardDbContext GetContext(ulong guildId)
        {
            if (!_guildContexts.ContainsKey(guildId))
                throw new InvalidOperationException($"No database initialized for guild {guildId}");

            return _guildContexts[guildId];
        }

        public static bool IsInitialized(ulong guildId)
        {
            return _guildContexts.ContainsKey(guildId);
        }

        public static void EnsureDatabaseUpToDate(ulong guildId)
        {
            string folderPath = Path.Combine(BasePath, guildId.ToString());

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string expectedFile = Path.Combine(folderPath, $"{DbPrefix}{CurrentDbVersion}{DbExtension}");

            // Find existing database file(s)
            var existingDbs = Directory.GetFiles(folderPath, $"{DbPrefix}*.sqlite");

            // Already up to date?
            if (existingDbs.Any(f => f == expectedFile))
            {
                CreateGuildDatabase(guildId); // Just open and cache the context
                return;
            }

            // Handle outdated DBs
            if (existingDbs.Length > 0)
            {
                string oldDb = existingDbs[0];
                string backupPath = oldDb.Replace(".sqlite", "_backup.sqlite");
                File.Move(oldDb, backupPath);

                Console.WriteLine($"[DB] Backed up old database for guild {guildId} to {backupPath}");
            }

            CreateGuildDatabase(guildId);
        }
    }
}
