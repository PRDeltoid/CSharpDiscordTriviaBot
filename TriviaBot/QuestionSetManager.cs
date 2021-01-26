using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TriviaBot
{
    public class QuestionSetManager : IQuestionSetManager
    {
        int currentQuestionIndex = 0;
        public IQuestionSet QuestionSet { get; internal set; }
        public QuestionModel CurrentQuestion { get => QuestionSet.GetQuestion(currentQuestionIndex); }

        async public void GetNewQuestionSet(int questionCount, string difficulty, Action<IQuestionSet> callback)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(ComposeURL(questionCount, difficulty));
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine(responseBody);
                    var json = JsonConvert.DeserializeObject(responseBody);
                    ResponseModel responseObject = JsonConvert.DeserializeObject<ResponseModel>(responseBody);
                    if(responseObject.ResponseCode == 0 && responseObject.Results.Count > 0)
                    {
                        foreach(QuestionModel question in responseObject.Results) {
                            question.Answers = new List<string>(question.IncorrectAnswers)
                            {
                                question.CorrectAnswer
                            };

                            question.Answers.Shuffle();
                            question.AnswerNumber = question.Answers.FindIndex(x => x == question.CorrectAnswer);
                        }
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

        public QuestionModel GetNextQuestion()
        {
            if(currentQuestionIndex+1 > QuestionSet.QuestionCount)
            {
                OutOfQuestions?.Invoke(this, null);
                return null;
            }

            currentQuestionIndex += 1;
            return QuestionSet.GetQuestion(currentQuestionIndex);
        }

        private string ComposeURL(int questionCount, string difficulty)
        {
            string v = $"https://opentdb.com/api.php?amount={ questionCount }&type=multiple";
            if(difficulty != null)
            {
                v += $"&difficulty ={ difficulty }";
            }
            return v;
        }

        public event EventHandler OutOfQuestions;
    }

    class ResponseModel

    {
        public int ResponseCode { get; set; }
        public List<QuestionModel> Results { get; set; }
    }

}
