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
            // Build folder and file paths
            string folderPath = Path.Combine(BasePath, guildId.ToString());
            string dbFileName = $"{DbPrefix}{CurrentDbVersion}{DbExtension}";
            string dbPath = Path.Combine(folderPath, dbFileName);

            // Create folder if it doesn't exist
            Directory.CreateDirectory(folderPath);

            // Store path for later retrieval
            _dbPaths[guildId] = dbPath;

            // Create SQLite DB and table
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

            Console.WriteLine($"[DB] Created database for guild {guildId} at {dbPath}");
        }

        public static SqliteConnection GetConnection(ulong guildId)
        {
            if (!_dbPaths.TryGetValue(guildId, out var path))
                throw new InvalidOperationException($"[DB] No database path found for guild {guildId}");

            var connection = new SqliteConnection($"Data Source={path}");
            connection.Open();
            return connection;
        }

        public static bool IsInitialized(ulong guildId) => _dbPaths.ContainsKey(guildId);
    }
}
