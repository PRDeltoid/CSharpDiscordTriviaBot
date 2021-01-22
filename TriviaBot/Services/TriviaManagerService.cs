using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaBot.Services
{
    public class TriviaManagerService : ITriviaManager
    {
        readonly IQuestionSetManager questionSetManager;

        public TriviaManagerService(IQuestionSetManager questionSetManager) {
            this.questionSetManager = questionSetManager;
        }
        #region Properties
        public bool IsRunning { get; set; }
        #endregion

        #region Public Methods
        public async void CheckAnswer(SocketMessage rawMessage)
        {
            if(rawMessage.Content.Length > 1) { return; }

            string messageText = rawMessage.Content.ToLower();
            if(messageText != "a" && messageText != "b" && messageText != "c" && messageText != "d") { return; }

            //TODO: Check if answer is correct
            QuestionAnswered?.Invoke(this, new QuestionEventArgs(questionSetManager.CurrentQuestion));
            questionSetManager.GetNextQuestion();
        }

        public async void Start()
        {
            questionSetManager.GetNewQuestionSet(10, questionset => {
                QuestionReady?.Invoke(this, new QuestionEventArgs(questionSetManager.CurrentQuestion));
            });
        }

        public async void Stop()
        {
        }
        #endregion

        #region Events
        public event EventHandler TriviaStarted;
        public event EventHandler TriviaStopped;
        public event EventHandler QuestionReady;
        public event EventHandler QuestionAnswered;
        public event EventHandler QuestionTimedOut;
        public event EventHandler QuestionSkipped;
        #endregion

        public class QuestionEventArgs : EventArgs {
            public QuestionModel Question { get; }

            public QuestionEventArgs(QuestionModel question)
            {
                Question = question;
            }
        }
    }
}
