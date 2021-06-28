using System.Collections.Generic;

namespace TriviaBot
{
    public interface IQuestionSet
    {
        List<QuestionModel> AsList();
        int QuestionCount { get; }
        QuestionModel GetQuestion(int index);
    }
}
