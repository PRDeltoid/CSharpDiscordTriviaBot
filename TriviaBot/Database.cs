using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
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

            CreateDatabaseIfNotExists();
            //CreateTableIfNotExists(Scores);
        }

        private void CreateTableIfNotExists(DatabaseTable<object, object> table)
        {
            // TODO: Finish this
            var colNames = table.GetAllColumnNames(true);
            var createTableString = "CREATE TABLE IF NOT EXISTS {table.TableName} ("; 
            foreach(string col in colNames)
            {
                //createTableString += $"{col}"
            }
            throw new NotImplementedException();
        }

        private void CreateDatabaseIfNotExists()
        {
            if (File.Exists("trivia.db"))
            {
                return;
            } else
            {
                File.Create("trivia.db");
            }
        }
    }
}
