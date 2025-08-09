using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Townsward.database.models
{
    public class Guild
    {
        public int id { get; set; }
        public ulong discordGuildId { get; set; }
        public string name { get; set; }
    }
}
