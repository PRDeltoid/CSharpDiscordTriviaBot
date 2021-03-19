using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaBot
{
    public interface IQuestionSet
    {
        List<QuestionModel> AsList();
        int QuestionCount { get; }
        QuestionModel GetQuestion(int index);
    }
}
