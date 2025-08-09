using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Townsward.database;
using Townsward.database.models;

namespace Townsward.commands
{
    public class AdminCommands : ApplicationCommandModule
    {
        [SlashCommand("create-db", "Initializes database for this server.")]
        public async Task CreateDatabaseCommand(InteractionContext context)
        {
            if (!context.Member.Permissions.HasPermission(Permissions.Administrator))
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("❌ You must be a server administrator to use this command."));
                return;
            }

            var db = DbManager.Context;

            // Check if the guild already exists
            var existingGuild = await db.guilds.FirstOrDefaultAsync(g => g.discordGuildId == context.Guild.Id);

            if (existingGuild != null)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("⚠️ This server is already registered in the database."));
                return;
            }

            // Create a new guild record
            var newGuild = new Guild
            {
                discordGuildId = context.Guild.Id,
                name = context.Guild.Name
            };

            db.guilds.Add(newGuild);
            await db.SaveChangesAsync();

            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent($"✅ Database initialized for **{context.Guild.Name}**."));
        }
    }
}
