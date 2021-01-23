using Discord.WebSocket;
using System.Collections.Generic;

namespace TriviaBot.Services
{
    public interface IScoreKeeperService
    {
        Dictionary<ulong, int> Scores { get; }

        void AddScore(SocketUser user, int score);
    }
}