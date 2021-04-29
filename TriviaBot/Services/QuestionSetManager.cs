using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TriviaBot
{
    public class QuestionSetManager : IQuestionSetManager
    {
        #region Members 
        int currentQuestionIndex = 0;
        #endregion

        #region Properties
        public IQuestionSet QuestionSet { get; internal set; }
        public QuestionModel CurrentQuestion { get => QuestionSet.GetQuestion(currentQuestionIndex); }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets a new questionset from OpenTDB
        /// </summary>
        /// <param name="questionCount">The number of questions to pull. Try to keep this from being too large</param>
        /// <param name="difficulty">a string representing difficulty. Easy, medium, hard on OpenTDB</param>
        /// <param name="callback">A function to call when the questionset is ready. Must accept an IQuestionSet.</param>
        async public void GetNewQuestionSet(int questionCount, string difficulty, Action<IQuestionSet> callback)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Query the trivia DB for some questions
                    HttpResponseMessage response = await client.GetAsync(ComposeURL(questionCount, difficulty));
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var json = JsonConvert.DeserializeObject(responseBody);
                    ResponseModel responseObject = JsonConvert.DeserializeObject<ResponseModel>(responseBody);
                    if(responseObject.ResponseCode == 0 && responseObject.Results.Count > 0)
                    {
                        //Collate the questions into a single list
                        foreach(QuestionModel question in responseObject.Results) {
                            question.Answers = new List<string>(question.IncorrectAnswers)
                            {
                                question.CorrectAnswer
                            };

                            // Shuffle the list
                            question.Answers.Shuffle();
                            // Find the answer in the list and mark it's index
                            question.AnswerNumber = question.Answers.FindIndex(x => x == question.CorrectAnswer);
                        }
                        // Wrap the questions in a QuestionSet
                        QuestionSet = new QuestionSet(responseObject.Results);
                        currentQuestionIndex = 0;
                        callback(QuestionSet);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Gets the next question in the current trivia session
        /// </summary>
        /// <returns>A <c>QuestionModel</c> representing the question</returns>
        public QuestionModel GetNextQuestion()
        {
            // Check if we've run out of questions
            if(currentQuestionIndex+1 > QuestionSet.QuestionCount)
            {
                OutOfQuestions?.Invoke(this, null);
                return null;
            }

            // If we have questions, grab a new one
            currentQuestionIndex += 1;
            return QuestionSet.GetQuestion(currentQuestionIndex);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Composes an API request URL with a question count and optional difficulty
        /// </summary>
        /// <param name="questionCount">The number of questions to pull</param>
        /// <param name="difficulty">The difficulty. Can be "easy", "medium" or "hard"</param>
        /// <returns>The composed URL</returns>
        private string ComposeURL(int questionCount, string difficulty)
        {
            // Compose a URL with question count and optional difficulty
            string v = $"https://opentdb.com/api.php?amount={ questionCount }&type=multiple";
            if(difficulty != null)
            {
                v += $"&difficulty ={ difficulty }";
            }
            return v;
        }
        #endregion

        public event EventHandler OutOfQuestions;
    }

    /// <summary>
    /// A model which represents a response from OpenTDB
    /// </summary>
    class ResponseModel
    {
        public int ResponseCode { get; set; }
        public List<QuestionModel> Results { get; set; }
    }
}
