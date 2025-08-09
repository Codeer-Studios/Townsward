using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Townsward.database.models
{
    public class Player
    {
        public int id {  get; set; }
        public ulong discordUserId { get; set; }
        public ulong guildId { get; set; }

        public string name { get; set; }
        public int xp { get; set; }
        public int gold { get; set; }
    }
}
