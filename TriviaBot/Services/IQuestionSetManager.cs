using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaBot
{
    public interface IQuestionSetManager
    {
        QuestionModel CurrentQuestion { get; }
        IQuestionSet QuestionSet { get; }
        void GetNewQuestionSet(uint questionCount, string difficulty, Action<IQuestionSet> callback);
        QuestionModel GetNextQuestion();
    }
}
