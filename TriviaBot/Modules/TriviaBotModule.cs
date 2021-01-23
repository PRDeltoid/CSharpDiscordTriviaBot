using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
        private readonly DiscordSocketClient _discord;

        public TriviaModule(IServiceProvider services, ITriviaManagerService triviaManager)
        {
            _triviaManager = triviaManager;
            _discord = services.GetRequiredService<DiscordSocketClient>();
        }

        #region Private Methods
        private async void _triviaManager_TriviaStarted(object sender, EventArgs e)
        {
            await ReplyAsync("Trivia Starting");
        }

        private async void _triviaManager_TriviaStopped(object sender, EventArgs e)
        {
            await ReplyAsync("Trivia Stopped");
            var scores = (GameOverEventArgs)e;
            await PrintScores(scores.Scores);
        }

        private async void _triviaManager_OutOfQuestions(object sender, EventArgs e)
        {
            await ReplyAsync("No more questions! Trivia Stopping");
            var scores = (GameOverEventArgs)e;
            await PrintScores(scores.Scores);
        }

        private async void _triviaManager_QuestionAnswered(object sender, System.EventArgs e)
        {
            var user = ((QuestionAnsweredEventArgs)e).User;
            await ReplyAsync($"{user.Username} got the correct answer!");
        }

        private async void _triviaManager_QuestionReady(object sender, System.EventArgs e)
        {
            QuestionModel question = ((QuestionEventArgs)e).Question;
            string questionPrompt = $"```{question.Question}:\n1:{question.Answers[0]}\n2:{question.Answers[1]}\n3:{question.Answers[2]}\n4:{question.Answers[3]}```";

            Console.WriteLine($"Answer: {question.CorrectAnswer }");

            await ReplyAsync(questionPrompt);
        }
        #endregion

        [Command("tstart")]
        [Alias("trivia start")]
        public Task TriviaStartAsync()
        {
            _triviaManager.Start();
            _triviaManager.QuestionReady += _triviaManager_QuestionReady;
            _triviaManager.QuestionAnswered += _triviaManager_QuestionAnswered;
            _triviaManager.OutOfQuestions += _triviaManager_OutOfQuestions;
            _triviaManager.TriviaStopped += _triviaManager_TriviaStopped;
            _triviaManager.TriviaStarted += _triviaManager_TriviaStarted;
            return null;
        }

        private async Task PrintScores(Dictionary<ulong, int> scores) 
        {
            string scoresString = "Score:\n";
            if (scores.Count > 0)
            {
                foreach (KeyValuePair<ulong, int> score in scores)
                {
                    scoresString += $"{ _discord.GetUser(score.Key).Username } - { score.Value }\n";
                }
            }
            await ReplyAsync($"```{ scoresString}```");
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
        public Task TriviaSkipAsync()
            => ReplyAsync("Skipping Question");
    }
}
