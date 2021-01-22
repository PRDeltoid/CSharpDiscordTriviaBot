using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaBot
{
    public class QuestionSet : IQuestionSet
    {
        List<QuestionModel> questions = new List<QuestionModel>();

        public QuestionSet(List<QuestionModel> questions)
        {
            this.questions = questions;
        }

        public int QuestionCount { get => questions?.Count ?? 0; }

        public List<QuestionModel> AsList()
        {
            return questions;
        }

        public QuestionModel GetQuestion(int index)
        {
            return questions[index];
        }
    }
}
