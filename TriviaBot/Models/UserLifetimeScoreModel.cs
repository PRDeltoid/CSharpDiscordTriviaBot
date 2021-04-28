using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaBot
{
    public class UserLifetimeScoreModel
    {
        [KeyColumn]
        public ulong UserID { get; set; }
        public uint Score { get; set; }
        public uint Wins { get; set; }
    }
}
