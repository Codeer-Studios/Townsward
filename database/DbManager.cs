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

        public static bool CreateGuildDirectory(ulong guildId)
        {
            try
            {
                var path = Path.Combine(BasePath, guildId.ToString());

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Console.WriteLine($"[DB] Created directory: {path}");
                    return true;
                }

                Console.WriteLine($"[DB] Directory already exists: {path}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB ERROR] Failed to create directory for guild {guildId}: {ex.Message}");
                return false;
            }
        }
    }
}
