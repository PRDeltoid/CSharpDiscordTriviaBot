﻿using System;
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
using static TriviaBot.Services.TriviaManagerService;

namespace TriviaBot.Modules
{
    // Modules must be public and inherit from an IModuleBase
    public class TriviaModule : ModuleBase<SocketCommandContext>
    {
        readonly ITriviaManagerService _triviaManager;
        readonly Timer messageTimer;
        readonly Queue<string> messageQueue;

        public TriviaModule(IServiceProvider services, ITriviaManagerService triviaManager)
        {
            _triviaManager = triviaManager;

            messageTimer = new Timer
            {
                AutoReset = true,
                Enabled = true,
                Interval = 500
            };
            messageTimer.Elapsed += MessageTimer_Elapsed;
            messageQueue = new Queue<string>();
        }

        private void MessageTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(messageQueue.Count > 0)
            {
                ReplyAsync(messageQueue.Dequeue());
            }
        }

        #region Private Methods
        private async void _triviaManager_TriviaStarted(object sender, EventArgs e)
        {
            messageQueue.Enqueue("Trivia Starting");
        }

        private async void _triviaManager_TriviaStopped(object sender, EventArgs e)
        {
            messageQueue.Enqueue("Trivia Stopped");
            var scores = (GameOverEventArgs)e;
            await PrintScores(scores.Scores);
        }

        private async void _triviaManager_OutOfQuestions(object sender, EventArgs e)
        {
            messageQueue.Enqueue("No more questions! Trivia Stopping");
            var scores = (GameOverEventArgs)e;
            await PrintScores(scores.Scores);
        }

        private async void _triviaManager_QuestionAnswered(object sender, System.EventArgs e)
        {
            var user = ((QuestionAnsweredEventArgs)e).User;
            messageQueue.Enqueue($"{user.Username} got the correct answer!");
        }

        private async void _triviaManager_QuestionReady(object sender, System.EventArgs e)
        {
            QuestionModel question = ((QuestionEventArgs)e).Question;
            string questionString = WebUtility.HtmlDecode(question.Question);
            string questionPrompt = $"```{questionString}\n1:{question.Answers[0]}\n2:{question.Answers[1]}\n3:{question.Answers[2]}\n4:{question.Answers[3]}```";

            Console.WriteLine($"Answer: {question.CorrectAnswer }");

            messageQueue.Enqueue(questionPrompt);
        }

        private void _triviaManager_QuestionTimedOut(object sender, EventArgs e)
        {
            messageQueue.Enqueue("Ran out of time! Next question.");
        }

        private void _triviaManager_QuestionSkipped(object sender, EventArgs e)
        {
            messageQueue.Enqueue("Skipping Question");
        }

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
            messageQueue.Enqueue(scoresString);
        }
        #endregion

        #region Public Methods
        [Command("tstart")]
        [Alias("trivia start")]
        public Task TriviaStartAsync(params string[] args)
        {
            // Hardcoded 20 question max at the moment
            // 10 questions by default
            //TODO: Load max questions from settings
            int numberOfQuestions = 10;
            if(args.Length > 0 && int.TryParse(args[0], out int parsedNumber) && numberOfQuestions < 20)
            {
                numberOfQuestions = parsedNumber;
            }

            string difficulty = null;
            if (args.Length > 1 && (args[1] == "easy" || args[1] == "medium" || args[1] == "hard"))
            {
                difficulty = args[1];
            }

            _triviaManager.Start(numberOfQuestions, difficulty);
            _triviaManager.TriviaStarted += _triviaManager_TriviaStarted;
            _triviaManager.QuestionReady += _triviaManager_QuestionReady;
            _triviaManager.QuestionAnswered += _triviaManager_QuestionAnswered;
            _triviaManager.OutOfQuestions += _triviaManager_OutOfQuestions;
            _triviaManager.TriviaStopped += _triviaManager_TriviaStopped;
            _triviaManager.TriviaStarted += _triviaManager_TriviaStarted;
            _triviaManager.QuestionSkipped += _triviaManager_QuestionSkipped;
            _triviaManager.QuestionTimedOut += _triviaManager_QuestionTimedOut;
            return null;
        }


        [Command("tstop")]
        [Alias("trivia stop")]
        public Task TriviaStopAsync()
        {
            _triviaManager.Stop();
            return null;
        }

        [Command("tskip")]
        [Alias("trivia skip")]
        public Task TriviaSkipAsync(IUser user = null)
        {
            user = user ?? Context.User;
            // Exit early if no user is passed
            if (user == null) { return null;  }
            _triviaManager.VoteSkip(user.Id);
            messageQueue.Enqueue($"{ user.Username} has voted to skip");
            return null;
        }
        #endregion
    }
}
