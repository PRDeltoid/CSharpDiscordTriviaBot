using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaBot
{
    public class UserScoreModel
    {
        public ulong Id { get; set; }
        public uint Score { get; set; }
        public string Username{ get; set; }
    }
}
