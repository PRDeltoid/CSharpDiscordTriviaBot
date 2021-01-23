using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaBot.Services
{
    public class ScoreKeeperService : IScoreKeeperService
    {
        private readonly Dictionary<ulong, int> scores = new Dictionary<ulong, int>();
        private readonly DiscordSocketClient _discord;

        public ScoreKeeperService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
        }

        public List<UserScoreModel> Scores {
            get
            {
                List<UserScoreModel> scoreList = new List<UserScoreModel>();
                foreach (var score in scores)
                {
                    scoreList.Add(new UserScoreModel { Id = score.Key, Score = score.Value, Username = _discord.GetUser(score.Key).Username });
                }
                return scoreList;
            }
        }

        public void ResetScores()
        {
            scores.Clear();
        }

        public int Count { get => scores.Count; }

        private bool HasScoreForUserId(ulong Id)
        {
            return scores.ContainsKey(Id);
        }

        public void AddScore(SocketUser user, int score)
        {
            if (HasScoreForUserId(user.Id))
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
