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
                await context.CreateResponseAsync("❌ You must be an admin to run this command.");
                return;
            }

            var created = DbManager.CreateGuildDirectory(context.Guild.Id);

            if (created)
            {
                await context.CreateResponseAsync($"✅ Created data folder for **{context.Guild.Name}**.");
            }
            else
            {
                await context.CreateResponseAsync($"⚠️ Data folder for **{context.Guild.Name}** already exists.");
            }
        }
    }
}
