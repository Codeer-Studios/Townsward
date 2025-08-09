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
        private static DiscordClient client;
        private static SlashCommandsExtension slash;

        public static async Task Main(string[] args)
        {
            // Load configuration
            var configLoader = new ConfigLoader();
            BotConfig config = await configLoader.ReadAsync();

            // Setup Discord client
            var discordConfig = new DiscordConfiguration
            {
                Intents = DiscordIntents.All,
                Token = config.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            client = new DiscordClient(discordConfig);

            RegisterEvents();
            RegisterSlashCommands();

            // Connect the bot
            await client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static void RegisterEvents()
        {
            client.Ready += OnClientReady;
            client.GuildCreated += OnGuildCreated;
        }

        private static void RegisterSlashCommands()
        {
            slash = client.UseSlashCommands();
            slash.RegisterCommands<AdminCommands>();
        }

        private static Task OnClientReady(DiscordClient sender, ReadyEventArgs args)
        {
            Console.WriteLine("[INFO] Bot is connected and ready.");
            Console.WriteLine("[BOT] Checking all connected guild databases...");

            foreach (var guild in client.Guilds.Values)
            {
                DbManager.EnsureDatabaseUpToDate(guild.Id);
            }

            Console.WriteLine("[BOT] All guild databases validated.");
            return Task.CompletedTask;
        }

        private static Task OnGuildCreated(DiscordClient sender, GuildCreateEventArgs args)
        {
            Console.WriteLine($"[BOT] Joined or rejoined guild: {args.Guild.Name} ({args.Guild.Id})");

            DbManager.EnsureDatabaseUpToDate(args.Guild.Id);
            return Task.CompletedTask;
        }
    }
}