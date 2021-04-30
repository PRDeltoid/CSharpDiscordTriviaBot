using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriviaBot.Services;

namespace TriviaBot.Modules
{
    public class TriviaBotModule : ModuleBase<SocketCommandContext>, ITriviaBotModule
    {
        private readonly DiscordSocketClient _discord;
        private readonly TriviaBotServiceFactory _triviaBotFactory;
        private readonly ILifetimeScorekeeper _lifetimeScorekeeper;

        static Dictionary<ulong, ITriviaBotService> TriviaBots { get; set; } = new Dictionary<ulong, ITriviaBotService>();

        public TriviaBotModule(DiscordSocketClient discord, TriviaBotServiceFactory triviaBotFactory, ILifetimeScorekeeper lifetimeScorekeeper)
        {
            _discord = discord;
            _triviaBotFactory = triviaBotFactory;
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

            var triviaBot = _triviaBotFactory.CreateTriviaBotService();
            triviaBot.Channel = Context.Channel;
            TriviaBots.Add(Context.Channel.Id, triviaBot);

            // 10 questions by default
            uint numberOfQuestions = Properties.Settings.Default.DefaultNumberQuestions;
            if (args.Length > 0 && uint.TryParse(args[0], out uint parsedNumber) && numberOfQuestions < Properties.Settings.Default.MaxNumberQuestions)
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
            TriviaBots.TryGetValue(Context.Channel.Id, out ITriviaBotService bot);
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
            TriviaBots.TryGetValue(Context.Channel.Id, out ITriviaBotService bot);
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
            string scoreString = "```" +
                                 "Top Scores:\n" +
                                 "User          Score   Wins\n" +
                                 "----------------------------\n";
            foreach (UserLifetimeScoreModel score in scores)
            {
                string username = _discord.GetUser(score.UserID).Username;
                scoreString += $"{username,-15}  {score.Score,-6}  {score.Wins,-4}\n";
            }

            scoreString += "```";
            ReplyAsync(scoreString);
            return null;
        }

        public void CheckAnswer(SocketMessage rawMessage)
        {
            TriviaBots.TryGetValue(rawMessage.Channel.Id, out ITriviaBotService bot);
            if (bot != null)
            {
                bot.CheckAnswer(rawMessage);
            }
        }
        #endregion
    }
}
