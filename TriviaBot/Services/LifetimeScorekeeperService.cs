using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord;

namespace TriviaBot.Services
{
    public class LifetimeScorekeeperService : ILifetimeScorekeeper
    {
        Database Database { get; } = new Database();

        public void AddLifetimeScore(ulong userId, uint score)
        {
            var user = Database.Scores.GetRow(userId);
            if (user == null)
            {
                // If user does not exist, create them
                Database.Scores.AddRow(new UserLifetimeScoreModel { PlayerId = userId, Score = score });
            }
            else
            {
                Database.Scores.UpdateRow(new UserLifetimeScoreModel { Score = user.Score + score }, userId);
            }
        }

        public void AddLifetimeWin(ulong userId)
        {
            var user = Database.Scores.GetRow(userId);
            if (user == null)
            {
                // If user does not exist, create them
                Database.Scores.AddRow(new UserLifetimeScoreModel { PlayerId = userId, Wins = 1});
            }
            else
            {
                Database.Scores.UpdateRow(new UserLifetimeScoreModel { Score = user.Wins + 1 }, userId);
            }
        }

        public List<UserLifetimeScoreModel> GetTopScores(int numberOfScores = 10)
        {
            // Prevent more than 25 top scores from being requested
            if(numberOfScores > 25)
            {
                numberOfScores = 10;
            }
            return Database.Scores.Cast<UserLifetimeScoreModel>().OrderBy(x => x.Score).Take(numberOfScores).ToList();
        }
    }
}
