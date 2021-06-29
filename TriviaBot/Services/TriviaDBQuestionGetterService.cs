using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TriviaBot.Services
{
    public class TriviaDBQuestionGetterService : IQuestionGetterService
    {
        public async Task<List<QuestionModel>> GetQuestions(uint questionCount, string difficulty)
        {
            using (HttpClient client = new HttpClient())
            {
                // Query the trivia DB for some questions
                HttpResponseMessage response = await client.GetAsync(ComposeURL(questionCount, difficulty));
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject(responseBody);
                ResponseModel responseObject = JsonConvert.DeserializeObject<ResponseModel>(responseBody);
                if (responseObject.ResponseCode == 0 && responseObject.Results.Count > 0)
                {
                    //Collate the questions into a single list
                    foreach (QuestionModel question in responseObject.Results)
                    {
                        question.Answers = new List<string>(question.IncorrectAnswers)
                        {
                            question.CorrectAnswer
                        };

                        // Shuffle the list
                        question.Answers.Shuffle();
                        // Find the answer in the list and mark it's index
                        question.AnswerNumber = question.Answers.FindIndex(x => x == question.CorrectAnswer);
                    }

                    return responseObject.Results;
                }

                return new List<QuestionModel>();
            }
        }

        #region Private Methods
        /// <summary>
        /// Composes an API request URL with a question count and optional difficulty
        /// </summary>
        /// <param name="questionCount">The number of questions to pull</param>
        /// <param name="difficulty">The difficulty. Can be "easy", "medium" or "hard"</param>
        /// <returns>The composed URL</returns>
        private string ComposeURL(uint questionCount, string difficulty)
        {
            // Compose a URL with question count and optional difficulty
            string v = $"https://opentdb.com/api.php?amount={ questionCount }&type=multiple";
            if (difficulty != null)
            {
                v += $"&difficulty ={ difficulty }";
            }
            return v;
        }
        #endregion


        /// <summary>
        /// A model which represents a response from OpenTDB
        /// </summary>
        class ResponseModel
        {
            public int ResponseCode { get; set; }
            public List<QuestionModel> Results { get; set; }
        }
    }
}
