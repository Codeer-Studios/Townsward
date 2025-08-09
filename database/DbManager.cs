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
        private const string CurrentDbVersion = "0002"; 

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

            using var command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS Players (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    DiscordUserId INTEGER NOT NULL,
                    Xp INTEGER NOT NULL DEFAULT 0,
                    Gold INTEGER NOT NULL DEFAULT 0,
                    Level INTEGER NOT NULL DEFAULT 1
                );
            ";
            command.ExecuteNonQuery();
        }

        private static void TransferData(string oldDbPath, string newDbPath)
        {
            Console.WriteLine("[DB] Migrating data from old DB...");

            using var oldConn = new SqliteConnection($"Data Source={oldDbPath}");
            using var newConn = new SqliteConnection($"Data Source={newDbPath}");

            oldConn.Open();
            newConn.Open();

            // Ensure old table exists
            using var checkCmd = oldConn.CreateCommand();
            checkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Players'";
            using var reader = checkCmd.ExecuteReader();
            if (!reader.HasRows)
            {
                Console.WriteLine("[DB] ⚠️ No Players table found in old DB. Skipping migration.");
                return;
            }

            using var selectCmd = oldConn.CreateCommand();
            selectCmd.CommandText = "SELECT DiscordUserId, Xp, Gold, Level FROM Players";
            using var selectReader = selectCmd.ExecuteReader();

            while (selectReader.Read())
            {
                using var insertCmd = newConn.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO Players (DiscordUserId, Xp, Gold, Level)
                    VALUES ($uid, $xp, $gold, $lvl)";
                insertCmd.Parameters.AddWithValue("$uid", selectReader.GetInt64(0));
                insertCmd.Parameters.AddWithValue("$xp", selectReader.GetInt32(1));
                insertCmd.Parameters.AddWithValue("$gold", selectReader.GetInt32(2));
                insertCmd.Parameters.AddWithValue("$lvl", selectReader.GetInt32(3));
                insertCmd.ExecuteNonQuery();
            }

            Console.WriteLine("[DB] Player data migrated.");
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
