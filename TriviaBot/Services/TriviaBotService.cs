using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TriviaBot.Services;

namespace TriviaBot.Services
{
    /// <summary>
    /// A module which responds trivia commands
    /// </summary>
    public class TriviaBotService : ITriviaBotService
    {
        #region Members
        readonly IQuestionSetManager _questionSetManager;
        private readonly IScoreKeeperService _scoreKeeper;
        private IChannel _channel;
        private IChatService _chatService;
        readonly Timer messageSendTimer;
        readonly Queue<string> messageSendingQueue;
        readonly Dictionary<ulong, bool> hasAnsweredCurrentQuestion;
        readonly Dictionary<ulong, ulong> hasVotedToSkip;
        readonly Timer _questionTimer;
        readonly int questionTimerLength = 15;
        #endregion

        #region Properties
        public bool IsRunning { get; set; }
        #endregion
        public TriviaBotService(IChannel channel, IChatService chatService, IScoreKeeperService scoreKeeper, IQuestionSetManager questionSetManager)
        {
            _channel = channel;
            _chatService = chatService;
            _questionSetManager = questionSetManager;
            _scoreKeeper = scoreKeeper;

            hasAnsweredCurrentQuestion = new Dictionary<ulong, bool>();
            hasVotedToSkip = new Dictionary<ulong, ulong>();

            // The timer which represents how often to send messages
            messageSendTimer = new Timer
            {
                AutoReset = true,
                Enabled = true,
                Interval = 500
            };
            messageSendTimer.Elapsed += MessageTimer_Elapsed;
            // A queue which holds our to-be-sent messages
            messageSendingQueue = new Queue<string>();

            _questionTimer = new Timer();
            _questionTimer.AutoReset = false;
            _questionTimer.Enabled = false;
            _questionTimer.Elapsed += QuestionTimer_Elapsed;
            _questionTimer.Interval = questionTimerLength * 1000;
        }

        #region Public Methods
        public async void CheckAnswer(SocketMessage rawMessage)
        {
            // If we arn't running, don't bother checking
            if (!IsRunning) { return; }

            // If we've gotten this far, our message is almost definitely an attempt at a 1-4 answer.
            // Figure out if the user has already tried to answer this question
            if (hasAnsweredCurrentQuestion.ContainsKey(rawMessage.Author.Id))
            {
                return;
            }
            else
            {
                hasAnsweredCurrentQuestion.Add(rawMessage.Author.Id, true);
            }

            // Figure out if they answered correctly.
            // Since our answer indexes are 0-3, just add 1 to get the corrisponding answer attempt
            if (rawMessage.Content == (_questionSetManager.CurrentQuestion.AnswerNumber + 1).ToString())
            {
                hasAnsweredCurrentQuestion.Clear();
                QuestionAnswered(rawMessage.Author);
                // Give them a point
                _scoreKeeper.AddScore(rawMessage.Author, 1);
                // And ask the next question
                GetNextQuestion();
            }
        }

        public void Start(int numberOfQuestions, string difficulty)
        {
            IsRunning = true;
            // Get rid of any answered questions
            hasAnsweredCurrentQuestion.Clear();
            // Get rid of old scores
            _scoreKeeper.ResetScores();
            _questionSetManager.GetNewQuestionSet(numberOfQuestions, difficulty, questionset =>
            {
                QuestionReady(_questionSetManager.CurrentQuestion);
                StartQuestionTimer();
            });
            messageSendingQueue.Enqueue("Trivia Starting");
        }

        public async void Stop()
        {
            IsRunning = false;
            _questionTimer.Enabled = false;
            messageSendingQueue.Enqueue("Trivia Stopping");

            await PrintScores(_scoreKeeper.Scores);
        }

        public void VoteSkip(IUser user)
        {
            if (hasVotedToSkip.ContainsKey(user.Id) == false)
            {
                hasVotedToSkip.Add(user.Id, user.Id);
                messageSendingQueue.Enqueue($"{ user.Username} has voted to skip");
                if (hasVotedToSkip.Count >= 3)
                {
                    QuestionSkipped();
                }
            }
        }
        #endregion

        #region Private Methods
        private void GetNextQuestion()
        {
            StopQuestionTimer();
            // if there are not more questions, alert any listeners and bail early
            if (_questionSetManager.GetNextQuestion() == null)
            {
                OutOfQuestions();
                return;
            }
            // Remove any skip votes before we show the next question
            hasVotedToSkip.Clear();
            QuestionReady(_questionSetManager.CurrentQuestion);

            //Start a timer to skip the question after a set amount of seconds
            StartQuestionTimer();
        }

        private void OutOfQuestions()
        {
            messageSendingQueue.Enqueue("No more questions! Trivia Stopping");
            PrintScores(_scoreKeeper.Scores);
        }

        private void QuestionAnswered(IUser user)
        {
            messageSendingQueue.Enqueue($"{user.Username} got the correct answer!");
        }

        private void QuestionReady(QuestionModel question)
        {
            string questionPrompt = $"```{question.Question}\n1:{question.Answers[0]}\n2:{question.Answers[1]}\n3:{question.Answers[2]}\n4:{question.Answers[3]}```";
            questionPrompt = WebUtility.HtmlDecode(questionPrompt);
            Console.WriteLine($"Answer: {question.CorrectAnswer }");

            messageSendingQueue.Enqueue(questionPrompt);
        }

        private void QuestionSkipped()
        {
            messageSendingQueue.Enqueue("Skipping Question");
            _questionSetManager.GetNextQuestion();
        }

        private void QuestionTimedOut()
        {
            messageSendingQueue.Enqueue("Ran out of time! Next question.");
            GetNextQuestion();
        }

        /// <summary>
        /// Sends given scores to the message queue
        /// </summary>
        /// <param name="scores">A list representing user scores</param>
        private async Task PrintScores(List<UserScoreModel> scores)
        {
            string scoresString = "Score:\n";
            if (scores.Count > 0)
            {
                foreach (UserScoreModel score in scores)
                {
                    scoresString += $"{ score.Username } - { score.Score }\n";
                }
            }
            messageSendingQueue.Enqueue(scoresString);
        }

        private void MessageTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // If we have messages to send, send 'em!
            if (messageSendingQueue.Count > 0)
            {
                _chatService.SendMessage(_channel, messageSendingQueue.Dequeue());
            }
        }

        private void QuestionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            QuestionTimedOut();
        }

        private void StartQuestionTimer()
        {
            _questionTimer.Enabled = true;
        }

        private void StopQuestionTimer()
        {
            _questionTimer.Enabled = false;
        }
        #endregion

        #region EventArgs
        public class QuestionEventArgs : EventArgs
        {
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
        #endregion
    }
}
