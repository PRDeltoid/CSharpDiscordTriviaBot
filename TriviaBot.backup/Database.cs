using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TriviaBot.Properties;

namespace TriviaBot
{
    public class Database
    {
        public ScoresTable Scores;

        public Database()
        {
            Scores = new ScoresTable();
        }
    }
}
