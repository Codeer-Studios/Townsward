using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Townsward.database.models
{
    public class Player
    {
        public int Id { get; set; }
        public ulong DiscordUserId { get; set; }

        public int Xp { get; set; }
        public int Gold { get; set; }

        public static void CreateTable(SqliteConnection connection)
        {
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

        public static void TransferData(SqliteConnection oldConn, SqliteConnection newConn)
        {
            // Check if table exists in old DB
            using var checkCmd = oldConn.CreateCommand();
            checkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Players'";
            using var reader = checkCmd.ExecuteReader();

            if (!reader.HasRows)
            {
                Console.WriteLine("[DB] ⚠️ No Players table in old DB. Skipping Player transfer.");
                return;
            }

            // Transfer rows
            using var selectCmd = oldConn.CreateCommand();
            selectCmd.CommandText = "SELECT DiscordUserId, Xp, Gold, Level FROM Players";
            using var selectReader = selectCmd.ExecuteReader();

            while (selectReader.Read())
            {
                using var insertCmd = newConn.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO Players (DiscordUserId, Xp, Gold, Level)
                    VALUES ($uid, $xp, $gold, $level)
                ";
                insertCmd.Parameters.AddWithValue("$uid", selectReader.GetInt64(0));
                insertCmd.Parameters.AddWithValue("$xp", selectReader.GetInt32(1));
                insertCmd.Parameters.AddWithValue("$gold", selectReader.GetInt32(2));
                insertCmd.Parameters.AddWithValue("$level", selectReader.GetInt32(3));
                insertCmd.ExecuteNonQuery();
            }

            Console.WriteLine("[DB] ✅ Players table data transferred.");
        }
    }
}
