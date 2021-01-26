using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace TriviaBot.Services
{
    public class TriviaManagerService : ITriviaManagerService
    {
        readonly IQuestionSetManager questionSetManager;
        readonly IScoreKeeperService scoreKeeper;
        readonly Dictionary<ulong, bool> hasAnsweredCurrentQuestion;
        readonly Dictionary<ulong, ulong> hasVotedToSkip;
        readonly Timer questionTimer;
        readonly int questionTimerLength = 15;

        public TriviaManagerService(IQuestionSetManager questionSetManager, IScoreKeeperService scoreKeeper) {
            this.questionSetManager = questionSetManager;
            this.scoreKeeper = scoreKeeper;

            hasAnsweredCurrentQuestion = new Dictionary<ulong, bool>();
            hasVotedToSkip = new Dictionary<ulong, ulong>();
            questionTimer = new Timer();
            questionTimer.AutoReset = false;
            questionTimer.Enabled = false;
            questionTimer.Elapsed += QuestionTimer_Elapsed;
            questionTimer.Interval = questionTimerLength * 1000;
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
            // Figure out if the user has already tried to answer this question
            if(hasAnsweredCurrentQuestion.ContainsKey(rawMessage.Author.Id))
            {
                return;
            } else
            {
                hasAnsweredCurrentQuestion.Add(rawMessage.Author.Id, true);
            }

            // Figure out if they answered correctly.
            // Since our answer indexes are 0-3, just add 1 to get the corrisponding answer attempt
            if (rawMessage.Content == (questionSetManager.CurrentQuestion.AnswerNumber + 1).ToString())
            {
                hasAnsweredCurrentQuestion.Clear();
                QuestionAnswered?.Invoke(this, new QuestionAnsweredEventArgs(questionSetManager.CurrentQuestion, rawMessage.Author));
                // Give them a point
                scoreKeeper.AddScore(rawMessage.Author, 1);
                // And ask the next question
                GetNextQuestion();
            }
        }

        public async void Start(int numberOfQuestions, string difficulty)
        {
            IsRunning = true;
            // Get rid of any answered questions
            hasAnsweredCurrentQuestion.Clear();
            // Get rid of old scores
            scoreKeeper.ResetScores();
            questionSetManager.GetNewQuestionSet(numberOfQuestions, difficulty, questionset => {
                QuestionReady?.Invoke(this, new QuestionEventArgs(questionSetManager.CurrentQuestion));
                StartQuestionTimer();
            });
        }

        public async void Stop()
        {
            IsRunning = false;
            TriviaStopped?.Invoke(this, new GameOverEventArgs(scoreKeeper.Scores));
        }

        public void VoteSkip(ulong voterId)
        {
            if(hasVotedToSkip.ContainsKey(voterId) == false)
            {
                hasVotedToSkip.Add(voterId, voterId);
                if(hasVotedToSkip.Count >= 3)
                {
                    QuestionSkipped?.Invoke(this, null);
                    GetNextQuestion();
                }
            }
        }
        #endregion

        #region Private Methods
        private void GetNextQuestion()
        {
            StopQuestionTimer();
            // if there are not more questions, alert any listeners and bail early
            if(questionSetManager.GetNextQuestion() == null)
            {
                OutOfQuestions?.Invoke(this, new GameOverEventArgs(scoreKeeper.Scores));
                return;
            }
            // Remove any skip votes before we show the next question
            hasVotedToSkip.Clear();
            QuestionReady?.Invoke(this, new QuestionEventArgs(questionSetManager.CurrentQuestion));

            //Start a timer to skip the question after a set amount of seconds
            StartQuestionTimer();
        }

        private void StartQuestionTimer()
        {
            questionTimer.Enabled = true;
        }

        private void StopQuestionTimer()
        {
            questionTimer.Enabled = false;
        }

        private void QuestionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            QuestionTimedOut?.Invoke(this, e);
            GetNextQuestion();
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
            public List<UserScoreModel> Scores { get; }

            public GameOverEventArgs(List<UserScoreModel> scores)
            {
                Scores = scores;
            }
        }
    }
}
