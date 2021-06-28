using System.Collections.Generic;

namespace TriviaBot.Services
{
    public interface ILifetimeScorekeeper
    {
        void AddLifetimeScore(ulong id, uint score);
        List<UserLifetimeScoreModel> GetTopScores(int numberOfScores = 10);
        void AddLifetimeWin(ulong userId);
    }
}
