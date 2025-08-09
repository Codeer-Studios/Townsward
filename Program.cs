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
            await Task.Delay(-1);
        }

        private static Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
        {
            Console.WriteLine("[INFO] Bot is connected and ready.");
            return Task.CompletedTask;
        }

    }
}