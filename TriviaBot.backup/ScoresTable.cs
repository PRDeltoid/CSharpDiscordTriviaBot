﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace TriviaBot
{
    public class ScoresTable : DatabaseTable<UserLifetimeScoreModel, ulong>
    {
        public ScoresTable() : base("Scores") { }
    }
}
