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
        private const string BasePath = "db";
        private const string DbPrefix = "database-";
        private const string DbExtension = ".sqlite";
        private const string CurrentDbVersion = "0001";

        private static readonly Dictionary<ulong, TownswardDbContext> _guildContexts = new();

        public static void EnsureDatabaseUpToDate(ulong guildId)
        {
            string folderPath = Path.Combine(BasePath, guildId.ToString());
            string newDbPath = Path.Combine(folderPath, $"{DbPrefix}{CurrentDbVersion}{DbExtension}");

            Directory.CreateDirectory(folderPath); // Ensure folder exists

            var existingDbs = Directory.GetFiles(folderPath, $"{DbPrefix}*.sqlite");
            var existingDb = existingDbs.FirstOrDefault();

            if (existingDb != null && existingDb == newDbPath)
            {
                // Already up to date
                LoadContext(guildId, newDbPath);
                return;
            }

            if (existingDb != null && existingDb != newDbPath)
            {
                // Outdated version exists — migrate data
                Console.WriteLine($"[DB] Outdated DB found for guild {guildId}: {Path.GetFileName(existingDb)}");

                string oldDbVersion = ExtractVersionFromFilename(existingDb);
                string backupPath = existingDb.Replace(".sqlite", $"-v{oldDbVersion}-backup.sqlite");
                File.Move(existingDb, backupPath);

                Console.WriteLine($"[DB] Backed up old DB to: {Path.GetFileName(backupPath)}");

                CreateAndMigrateNewDb(guildId, newDbPath);
                TransferData(backupPath, newDbPath);
                File.Delete(backupPath);
            }
            else
            {
                // No DB exists — create fresh
                CreateAndMigrateNewDb(guildId, newDbPath);
            }
        }

        private static void CreateAndMigrateNewDb(ulong guildId, string dbPath)
        {
            var options = new DbContextOptionsBuilder<TownswardDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            var context = new TownswardDbContext(options);
            context.Database.Migrate();

            _guildContexts[guildId] = context;
            Console.WriteLine($"[DB] Created new DB for guild {guildId}: {Path.GetFileName(dbPath)}");
        }

        private static void LoadContext(ulong guildId, string dbPath)
        {
            var options = new DbContextOptionsBuilder<TownswardDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            var context = new TownswardDbContext(options);
            context.Database.Migrate();

            _guildContexts[guildId] = context;
            Console.WriteLine($"[DB] Loaded DB for guild {guildId}: {Path.GetFileName(dbPath)}");
        }

        private static void TransferData(string oldDbPath, string newDbPath)
        {
            Console.WriteLine("[DB] Transferring data to new version...");

            var oldOptions = new DbContextOptionsBuilder<TownswardDbContext>()
                .UseSqlite($"Data Source={oldDbPath}")
                .Options;

            var newOptions = new DbContextOptionsBuilder<TownswardDbContext>()
                .UseSqlite($"Data Source={newDbPath}")
                .Options;

            using var oldDb = new TownswardDbContext(oldOptions);
            using var newDb = new TownswardDbContext(newOptions);

            // EXAMPLE: transfer players
            var oldPlayers = oldDb.Players.ToList();
            foreach (var player in oldPlayers)
            {
                // Reset ID to avoid PK conflicts
                player.Id = 0;
                newDb.Players.Add(player);
            }

            newDb.SaveChanges();
            Console.WriteLine("[DB] Data transfer complete.");
        }

        private static string ExtractVersionFromFilename(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path); // e.g. database-0001
            var parts = fileName.Split('-');
            return parts.Length > 1 ? parts[1] : "unknown";
        }

        public static TownswardDbContext GetContext(ulong guildId)
        {
            if (!_guildContexts.TryGetValue(guildId, out var ctx))
                throw new InvalidOperationException($"No database context for guild {guildId}");

            return ctx;
        }

        public static bool IsInitialized(ulong guildId) => _guildContexts.ContainsKey(guildId);
    }
}
