using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace TriviaBot.Services
{
    public interface ITriviaBotModule
    {
        Task TriviaSkipAsync(IUser user = null);
        [Command("tstart")]
        [Alias("trivia stop")]
        Task TriviaStartAsync(params string[] args);
        Task TriviaStopAsync();
        void CheckAnswer(SocketMessage rawMessage);
    }
}