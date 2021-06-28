using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using TriviaBot.Extensions;
using TriviaBot.Services;

namespace TriviaBot.Modules
{
    public class TriviaBotModule : ModuleBase<SocketCommandContext>
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

            ITriviaBotService triviaBot = _triviaBotFactory.CreateTriviaBotService();
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
            bot?.Stop();
            return null;
        }

        [Command("tskip")]
        [Alias("trivia skip")]
        public Task TriviaSkipAsync(IUser user = null)
        {
            user ??= Context.User;
            // Exit early if no user is passed
            if (user == null) { return null; }
            TriviaBots.TryGetValue(Context.Channel.Id, out ITriviaBotService bot);
            bot?.VoteSkip(Context.User);
            return null;
        }

        [Command("ttop")]
        [Alias("trivia top")]
        public Task TriviaPrintTopScoresAsync(IUser user = null, int numberOfScores = 10)
        {
            user ??= Context.User;
            // Exit early if no user is passed
            if (user == null) { return null; }

            int userColLen = 25;
            string userColString = "User";
            int scoreColLen = 8;
            string scoreColString = "Score";
            int winsColLen = 5;
            string winsColString = "Wins";

            var scores = _lifetimeScorekeeper.GetTopScores(numberOfScores);
            string scoreString = "```" +
                                 "Top Scores:\n" +
                                 userColString.PadRight(userColLen) + scoreColString.PadRight(scoreColLen) + winsColString.PadRight(winsColLen) + "\n" +
                                 new string('-', userColLen + scoreColLen + winsColLen) + "\n";

            foreach (UserLifetimeScoreModel score in scores)
            {
                // Get the user's username. If no user with the user ID is found, just return the ID instead
                string username = _discord.GetUser(score.UserID)?.Username ?? score.UserID.ToString();
                scoreString += $"{username.PadRight(userColLen)}{score.Score.ToString().PadBoth(scoreColLen)}{score.Wins.ToString().PadBoth(winsColLen)}\n";

            }

            scoreString += "```";
            ReplyAsync(scoreString);
            return null;
        }

        public void CheckAnswer(SocketMessage rawMessage)
        {
            TriviaBots.TryGetValue(rawMessage.Channel.Id, out ITriviaBotService bot);
            bot?.CheckAnswer(rawMessage);
        }
        #endregion
    }
}
