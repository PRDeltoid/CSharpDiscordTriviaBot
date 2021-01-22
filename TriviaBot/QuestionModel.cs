using Newtonsoft.Json;
using System.Collections.Generic;

namespace TriviaBot
{
    public class QuestionModel
    {
        public string Question { get; set; }
        [JsonProperty(PropertyName = "correct_answer")]
        public string CorrectAnswer { get; set; }
        [JsonProperty(PropertyName = "incorrect_answers")]
        public List<string> IncorrectAnswers { get; set; }
        public string Category { get; set; }
        public string Difficulty { get; set; }
    }
}