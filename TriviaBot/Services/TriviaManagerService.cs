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
        readonly IScoreKeeperService scoreKeeper;
        string correctAnswer;

        public TriviaManagerService(IQuestionSetManager questionSetManager, IScoreKeeperService scoreKeeper) {
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
            // If we arn't running, don't bother checking
            if(!IsRunning) { return; }

            // If we've gotten this far, our message is almost definitely an attempt at a 1-4 answer.
            // Figure out if they answered correctly.
            // Since our answer indexes are 0-3, just add 1 to get the corrisponding answer attempt
            if (rawMessage.Content == (questionSetManager.CurrentQuestion.AnswerNumber + 1).ToString())
            {
                QuestionAnswered?.Invoke(this, new QuestionAnsweredEventArgs(questionSetManager.CurrentQuestion, rawMessage.Author));
                // Give them a point
                scoreKeeper.AddScore(rawMessage.Author, 1);
                // And ask the next question
                // if there are not more questions, alert any listeners and bail early
                if(questionSetManager.GetNextQuestion() == null)
                {
                    OutOfQuestions?.Invoke(this, new GameOverEventArgs(scoreKeeper.Scores));
                    return;
                }
                QuestionReady?.Invoke(this, new QuestionEventArgs(questionSetManager.CurrentQuestion));
            }
        }

        public async void Start()
        {
            IsRunning = true;
            questionSetManager.GetNewQuestionSet(10, questionset => {
                QuestionReady?.Invoke(this, new QuestionEventArgs(questionSetManager.CurrentQuestion));
            });
        }

        public async void Stop()
        {
            IsRunning = false;
            TriviaStopped?.Invoke(this, new GameOverEventArgs(scoreKeeper.Scores));
        }
        #endregion

        #region Events
        public event EventHandler TriviaStarted;
        public event EventHandler TriviaStopped;
        public event EventHandler OutOfQuestions;
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

        public class GameOverEventArgs : EventArgs
        {
            public Dictionary<ulong, int> Scores { get; }

            public GameOverEventArgs(Dictionary<ulong, int> scores)
            {
                Scores = scores;
            }
        }
    }
}
