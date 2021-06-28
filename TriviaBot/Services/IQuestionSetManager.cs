using System;

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
