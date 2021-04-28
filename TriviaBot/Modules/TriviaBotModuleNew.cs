using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriviaBot.Services;

namespace TriviaBot.Services
{
    public class TriviaBotModuleNew : ModuleBase<SocketCommandContext>, ITriviaBotModule
    {
        private readonly DiscordSocketClient _discord;
        private readonly ILifetimeScorekeeper _lifetimeScorekeeper;

        static Dictionary<ulong, TriviaBotService> TriviaBots { get; set; } = new Dictionary<ulong, TriviaBotService>();

        public TriviaBotModuleNew(DiscordSocketClient discord, ILifetimeScorekeeper lifetimeScorekeeper) //, CommandService commandService)
        {
            _discord = discord;
            _lifetimeScorekeeper = lifetimeScorekeeper;
        }


        #region Public Methods
        [Command("tstart")]
        [Alias("trivia start")]
        public Task TriviaStartAsync(params string[] args)
        {
            // Make sure only one triviabot is running per server so we can ignore multiple channel issues
            if (TriviaBots.ContainsKey(Context.Channel.Id))
            {
                Context.Channel.SendMessageAsync("Sorry, cannot start TriviaBot here. There is already one running in this channel.");
                return null;
            }

            //TODO: Create factory to generate trivia bots using DI'd services/managers
            var triviaBot = new TriviaBotService(Context.Channel, new ChatService(_discord),new ScoreKeeperService(_discord), new LifetimeScorekeeperService(), new QuestionSetManager());
            TriviaBots.Add(Context.Channel.Id, triviaBot);

            // 10 questions by default
            // Hardcoded 20 question max at the moment (over 20 is invalid and becomes default)
            //TODO: Load max questions from settings
            int numberOfQuestions = 10;
            if (args.Length > 0 && int.TryParse(args[0], out int parsedNumber) && numberOfQuestions < 20)
            {
                numberOfQuestions = parsedNumber;
            }

            // Determine difficulty. Trivia Manager interprets null as any difficulty.
            string difficulty = null;
            if (args.Length > 1 && (args[1] == "easy" || args[1] == "medium" || args[1] == "hard"))
            {
                difficulty = args[1];
            }
            triviaBot.Start(numberOfQuestions, difficulty);
            return Task.CompletedTask;
        }

        [Command("tstop")]
        [Alias("trivia stop")]
        public Task TriviaStopAsync()
        {
            TriviaBots.TryGetValue(Context.Channel.Id, out TriviaBotService bot);
            if (bot != null)
            {
                bot.Stop();
            }
            return null;
        }

        [Command("tskip")]
        [Alias("trivia skip")]
        public Task TriviaSkipAsync(IUser user = null)
        {
            user = user ?? Context.User;
            // Exit early if no user is passed
            if (user == null) { return null; }
            TriviaBots.TryGetValue(Context.Channel.Id, out TriviaBotService bot);
            if (bot != null)
            {
                bot.VoteSkip(Context.User);
            }
            return null;
        }

        [Command("ttop")]
        [Alias("trivia top")]
        public Task TriviaPrintTopScoresAsync(IUser user = null, int numberOfScores = 10)
        {
            user = user ?? Context.User;
            // Exit early if no user is passed
            if (user == null) { return null; }

            var scores = _lifetimeScorekeeper.GetTopScores(numberOfScores);
            string scoreString = "Top Scores:\n--------------";
            foreach(UserLifetimeScoreModel score in scores)
            {
                scoreString += $"{score.PlayerId} - {score.Score} - {score.Wins}\n";
            }

            ReplyAsync(scoreString);
            return null;
        }

        public void CheckAnswer(SocketMessage rawMessage)
        {
            TriviaBots.TryGetValue(rawMessage.Channel.Id, out TriviaBotService bot);
            if (bot != null)
            {
                bot.CheckAnswer(rawMessage);
            }
        }
        #endregion
    }
}
