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

        async public void GetNewQuestionSet(int questionCount, Action<IQuestionSet> callback)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(ComposeURL(questionCount));
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine(responseBody);
                    var json = JsonConvert.DeserializeObject(responseBody);
                    ResponseModel responseObject = JsonConvert.DeserializeObject<ResponseModel>(responseBody);
                    if(responseObject.ResponseCode == 0 && responseObject.Results.Count > 0)
                    {
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

        private string ComposeURL(int questionCount)
        {
            return $"https://opentdb.com/api.php?amount={ questionCount }&type=multiple";
        }

        public event EventHandler OutOfQuestions;
    }

    class ResponseModel
    {
        public int ResponseCode { get; set; }
        public List<QuestionModel> Results { get; set; }
    }

}
