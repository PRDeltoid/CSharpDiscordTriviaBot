using System.Collections.Generic;

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

        /// <summary>
        /// Get the question at index. 
        /// </summary>
        /// <param name="index">The index of the question in the QuestionSet</param>
        /// <returns>The question at index. If no question exists, return null./returns>
        public QuestionModel GetQuestion(int index)
        {
            if (index >= questions.Count) { return null; }
            return questions[index];
        }
    }
}
