using System.IO;

namespace TriviaBot
{
    public class Database
    {
        public ScoresTable Scores;

        public Database()
        {
            Scores = new ScoresTable();

            CreateDatabaseIfNotExists();
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
