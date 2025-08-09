using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Townsward.Commands
{
    public class AdminCommands : ApplicationCommandModule
    {
        [SlashCommand("create-db", "Create a new SQLite database for this server.")]
        public async Task CreateDatabaseCommand(InteractionContext context)
        {
            if (!context.Member.Permissions.HasPermission(Permissions.Administrator))
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("❌ You must be a server administrator to use this command."));
                return;
            }

            // Simulate creating the SQLite DB
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent($"✅ SQLite database created for **{context.Guild.Name}** (not really, this is just a placeholder)."));
        }
    }
}
