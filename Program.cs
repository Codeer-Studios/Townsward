using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Townsward.commands;
using Townsward.config;
using Townsward.database;

namespace Townsward
{
    internal class Program
    {

        private static DiscordClient client { get; set; }
        private static CommandsNextExtension commands { get; set; }

        public static async Task Main(string[] args)
        {

            // Load configuration
            var configLoader = new ConfigLoader();
            BotConfig config = await configLoader.ReadAsync();

            // Setup Discord client
            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = config.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            client = new DiscordClient(discordConfig);

            var slash = client.UseSlashCommands();

            slash.RegisterCommands<AdminCommands>();

            client.Ready += Client_Ready;


            // Connect the bot
            await client.ConnectAsync();

            EnsureAllDatabasesUpToDate();

            await Task.Delay(-1);
        }

        private static Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
        {
            Console.WriteLine("[INFO] Bot is connected and ready.");
            return Task.CompletedTask;
        }

        public static void EnsureAllDatabasesUpToDate()
        {
            var basePath = "db";

            if (!Directory.Exists(basePath))
                return;

            var guildDirs = Directory.GetDirectories(basePath);

            foreach (var dir in guildDirs)
            {
                var folderName = Path.GetFileName(dir);

                if (ulong.TryParse(folderName, out ulong guildId))
                {
                    Console.WriteLine($"[DB] Checking DB for guild {guildId}...");
                    DbManager.EnsureDatabaseUpToDate(guildId);
                }
                else
                {
                    Console.WriteLine($"[DB WARN] Skipping invalid guild folder: {folderName}");
                }
            }
        }

    }
}