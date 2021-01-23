using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaBot.Services
{
    public class ScoreKeeperService : IScoreKeeperService
    {
        readonly Dictionary<ulong, int> scores;

        public Dictionary<ulong, int> Scores { get => scores; }

        public ScoreKeeperService()
        {
            scores = new Dictionary<ulong, int>();
        }

        public void AddScore(SocketUser user, int score)
        {
            if (scores.ContainsKey(user.Id))
            {
                scores[user.Id] += score;
            }
            else
            {
                scores.Add(user.Id, score);
            }
        }

    }
}
