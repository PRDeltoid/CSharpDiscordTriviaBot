using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriviaBot;

namespace TriviaBot.Services
{
    public interface ILifetimeScorekeeper
    {
        void AddLifetimeScore(ulong id, uint score);
        List<UserLifetimeScoreModel> GetTopScores(int numberOfScores = 10);
        void AddLifetimeWin(ulong userId);
    }
}
