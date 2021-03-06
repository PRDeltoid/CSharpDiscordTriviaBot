﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaBot
{
    public class UserLifetimeScoreModel
    {
        [KeyColumn]
        public ulong Key { get; set; }
        [ColumnName("UserID")]
        public ulong PlayerId { get; set; }
        [ColumnName("GuildID")]
        public ulong GuildId { get; set; }
        public uint Score { get; set; }
    }
}
