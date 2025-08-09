using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Townsward.database
{
    public static class DbManager
    {
        private const string BasePath = "db";
        private const string DbPrefix = "database-";
        private const string DbExtension = ".sqlite";
        private const string CurrentDbVersion = "0001"; 

        private static readonly Dictionary<ulong, string> _dbPaths = new();

        public static void CreateDatabase(ulong guildId)
        {
            string folderPath = Path.Combine(BasePath, guildId.ToString());
            string expectedDbName = $"{DbPrefix}{CurrentDbVersion}{DbExtension}";
            string expectedDbPath = Path.Combine(folderPath, expectedDbName);

            Directory.CreateDirectory(folderPath);

            // Check for existing DB
            var existingDb = Directory.GetFiles(folderPath, $"{DbPrefix}*.sqlite")
                                      .FirstOrDefault();

            if (existingDb != null && Path.GetFileName(existingDb) == expectedDbName)
            {
                Console.WriteLine($"[DB] Guild {guildId} already has up-to-date DB.");
                _dbPaths[guildId] = expectedDbPath;
                return;
            }

            // If outdated, back up and migrate
            if (existingDb != null && Path.GetFileName(existingDb) != expectedDbName)
            {
                Console.WriteLine($"[DB] Outdated DB detected for guild {guildId}: {Path.GetFileName(existingDb)}");

                string backupPath = existingDb.Replace(".sqlite", "-backup.sqlite");
                File.Move(existingDb, backupPath);
                Console.WriteLine($"[DB] Backed up old DB to {Path.GetFileName(backupPath)}");

                // Create new DB
                _dbPaths[guildId] = expectedDbPath;
                CreateAndInitSchema(expectedDbPath);

                // Transfer data
                TransferData(backupPath, expectedDbPath);

                // Clean up backup
                File.Delete(backupPath);
                Console.WriteLine($"[DB] Completed migration for guild {guildId}");
            }
            else
            {
                // No DB exists
                _dbPaths[guildId] = expectedDbPath;
                CreateAndInitSchema(expectedDbPath);
                Console.WriteLine($"[DB] Created fresh DB for guild {guildId}");
            }
        }

        private static void CreateAndInitSchema(string dbPath)
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            models.Player.CreateTable(connection);
        }

        private static void TransferData(string oldDbPath, string newDbPath)
        {
            Console.WriteLine("[DB] Starting table data transfer...");

            using var oldConn = new SqliteConnection($"Data Source={oldDbPath}");
            using var newConn = new SqliteConnection($"Data Source={newDbPath}");

            oldConn.Open();
            newConn.Open();

            // Let each model handle its own transfer logic
            models.Player.TransferData(oldConn, newConn);
        }

        public static SqliteConnection GetConnection(ulong guildId)
        {
            if (!_dbPaths.TryGetValue(guildId, out var path))
                throw new InvalidOperationException($"[DB] No database path for guild {guildId}");

            var connection = new SqliteConnection($"Data Source={path}");
            connection.Open();
            return connection;
        }

        public static bool IsInitialized(ulong guildId) => _dbPaths.ContainsKey(guildId);
    }
}
