using Discord.WebSocket;
using System.Collections.Generic;

namespace TriviaBot.Services
{
    public interface IScoreKeeperService
    {
        List<UserScoreModel> Scores { get; }

        void AddScore(SocketUser user, int score);
        void ResetScores();
        int Count { get; }
    }
}