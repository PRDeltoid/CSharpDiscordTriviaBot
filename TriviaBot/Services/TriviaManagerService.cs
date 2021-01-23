using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaBot.Services
{
    public class TriviaManagerService : ITriviaManagerService
    {
        readonly IQuestionSetManager questionSetManager;
        string correctAnswer;

        public TriviaManagerService(IQuestionSetManager questionSetManager) {
            this.questionSetManager = questionSetManager;
        }
        #region Properties
        public bool IsRunning { get; set; }
        #endregion

        #region Private Method
        #endregion

        #region Public Methods
        public async void CheckAnswer(SocketMessage rawMessage)
        {
            // If we've gotten this far, our message is almost definitely an attempt at a 1-4 answer.
            // Figure out if they answered correctly.
            if (rawMessage.Content == (questionSetManager.CurrentQuestion.AnswerNumber + 1).ToString())
            {
                //TODO: Check if answer is correct
                QuestionAnswered?.Invoke(this, new QuestionAnsweredEventArgs(questionSetManager.CurrentQuestion, rawMessage.Author));
                questionSetManager.GetNextQuestion();
                QuestionReady?.Invoke(this, new QuestionEventArgs(questionSetManager.CurrentQuestion));
            }
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
        public class QuestionAnsweredEventArgs : EventArgs
        {
            public QuestionModel Question { get; }
            public SocketUser User { get; }

            public QuestionAnsweredEventArgs(QuestionModel question, SocketUser user)
            {
                Question = question;
                User = user;
            }
        }
    }
}
