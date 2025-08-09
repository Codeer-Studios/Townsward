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
            // Respond immediately to avoid timeout
            await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            try
            {
                if (!context.Member.Permissions.HasPermission(Permissions.Administrator))
                {
                    await context.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent("❌ You must be a server administrator to use this command."));
                    return;
                }

                var db = DbManager.Context;

                var existingGuild = await db.guilds.FirstOrDefaultAsync(g => g.discordGuildId == context.Guild.Id);

                if (existingGuild != null)
                {
                    await context.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent("⚠️ This server is already registered in the database."));
                    return;
                }

                db.guilds.Add(new Guild
                {
                    discordGuildId = context.Guild.Id,
                    name = context.Guild.Name
                });

                await db.SaveChangesAsync();

                await context.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"✅ Database initialized for **{context.Guild.Name}**."));
            }
            catch (Exception ex)
            {
                await context.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"❌ An error occurred: `{ex.Message}`"));
            }
        }
    }
}
