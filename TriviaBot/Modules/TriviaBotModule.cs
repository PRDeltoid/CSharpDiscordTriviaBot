using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriviaBot.Services;
using static TriviaBot.Services.TriviaManagerService;

namespace TriviaBot.Modules
{
    // Modules must be public and inherit from an IModuleBase
    public class TriviaModule : ModuleBase<SocketCommandContext>
    {
        ITriviaManager _triviaManager;

        public TriviaModule(ITriviaManager triviaManager)
        {
            _triviaManager = triviaManager;

        }

        [Command("tstart")]
        [Alias("trivia start")]
        public Task TriviaStartAsync()
        {
            _triviaManager.Start();
            _triviaManager.QuestionReady += _triviaManager_QuestionReady;
            _triviaManager.QuestionAnswered += _triviaManager_QuestionAnswered;
            //triviaManagerService.QuestionAnswered
            return ReplyAsync("Trivia Starting");
        }

        private void _triviaManager_QuestionAnswered(object sender, System.EventArgs e)
        {
            //TODO: Pass a username as an eventarg to congradulate a specific user
            ReplyAsync("Correct!");
        }

        private void _triviaManager_QuestionReady(object sender, System.EventArgs e)
        {
            string question = ((QuestionEventArgs)e).Question.Question;
            ReplyAsync(question);
        }

        [Command("tstop")]
        [Alias("trivia stop")]
        public Task TriviaStopAsync()
            => ReplyAsync("Trivia Stopping");

        [Command("tskip")]
        [Alias("trivia skip")]
        public Task TriviaSkipAsync()
            => ReplyAsync("Skipping Question");

        /*
        // Get info on a user, or the user who invoked the command if one is not specified
        [Command("userinfo")]
        public async Task UserInfoAsync(IUser user = null)
        {
            user = user ?? Context.User;

            await ReplyAsync(user.ToString());
        }

        // Ban a user
        [Command("ban")]
        [RequireContext(ContextType.Guild)]
        // make sure the user invoking the command can ban
        [RequireUserPermission(GuildPermission.BanMembers)]
        // make sure the bot itself can ban
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanUserAsync(IGuildUser user, [Remainder] string reason = null)
        {
            await user.Guild.AddBanAsync(user, reason: reason);
            await ReplyAsync("ok!");
        }

        // [Remainder] takes the rest of the command's arguments as one argument, rather than splitting every space
        [Command("echo")]
        public Task EchoAsync([Remainder] string text)
            // Insert a ZWSP before the text to prevent triggering other bots!
            => ReplyAsync('\u200B' + text);

        // 'params' will parse space-separated elements into a list
        [Command("list")]
        public Task ListAsync(params string[] objects)
            => ReplyAsync("You listed: " + string.Join("; ", objects));

        // Setting a custom ErrorMessage property will help clarify the precondition error
        [Command("guild_only")]
        [RequireContext(ContextType.Guild, ErrorMessage = "Sorry, this command must be ran from within a server, not a DM!")]
        public Task GuildOnlyCommand()
            => ReplyAsync("Nothing to see here!");*/
    }
}
