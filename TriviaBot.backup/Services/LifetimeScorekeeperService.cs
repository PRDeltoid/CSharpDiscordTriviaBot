using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace TriviaBot.Services
{
    class LifetimeScorekeeperService : ILifetimeScorekeeper
    {
        Database Database { get; } = new Database();
        //_discord.GetUser(score.Key)
        public void AddLifetimeScore(ulong id, ulong guildId, uint score)
        {
            var user = Database.Scores.GetRow(id);
            if (user != null)
            {
                // If user does not exist, create them
                Database.Scores.AddRow(new UserLifetimeScoreModel { PlayerId = id, GuildId = guildId, Score = score });
            }
            else
            {
                //user.Score += 1; 
            }
        }

        public List<UserLifetimeScoreModel> GetTopScores(int numberOfScores = 10)
        {
            return Database.Scores.OrderBy(x => x.Score).Take(numberOfScores).ToList();
        }
    }
}
