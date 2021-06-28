using System.Collections.Generic;
using System.Linq;

namespace TriviaBot.Services
{
    public class LifetimeScorekeeperService : ILifetimeScorekeeper
    {
        readonly Database _database;

        public LifetimeScorekeeperService(Database database)
        {
            _database = database;
        }

        public void AddLifetimeScore(ulong userId, uint score)
        {
            var user = _database.Scores.GetRow(userId);
            if (user == null)
            {
                // If user does not exist, create them
                _database.Scores.AddRow(new UserLifetimeScoreModel { UserID = userId, Score = score });
            }
            else
            {
                _database.Scores.UpdateRow(new UserLifetimeScoreModel { Score = user.Score + score }, userId);
            }
        }

        public void AddLifetimeWin(ulong userId)
        {
            var user = _database.Scores.GetRow(userId);
            if (user == null)
            {
                // If user does not exist, create them
                _database.Scores.AddRow(new UserLifetimeScoreModel { UserID = userId, Wins = 1});
            }
            else
            {
                _database.Scores.UpdateRow(new UserLifetimeScoreModel { Wins = user.Wins + 1 }, userId);
            }
        }

        public List<UserLifetimeScoreModel> GetTopScores(int numberOfScores = 10)
        {
            // Prevent more than 25 top scores from being requested
            if(numberOfScores > 25)
            {
                numberOfScores = 10;
            }
            return _database.Scores.Cast<UserLifetimeScoreModel>().OrderBy(x => x.Score).Take(numberOfScores).ToList();
        }
    }
}
