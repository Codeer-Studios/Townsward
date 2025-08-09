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
    }
}
