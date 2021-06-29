using System.Collections.Generic;
using System.Threading.Tasks;

namespace TriviaBot.Services
{
    public interface IQuestionGetterService
    {
        /// <summary>
        /// Gets a list of questions of a given difficulty from a trivia source
        /// </summary>
        /// <param name="questionCount">The number of questions to get</param>
        /// <param name="difficulty">A difficulty keyword</param>
        /// <returns>A list of <c>QuestionModel</c>s</returns>
        Task<List<QuestionModel>> GetQuestions(uint questionCount, string difficulty);
    }
}