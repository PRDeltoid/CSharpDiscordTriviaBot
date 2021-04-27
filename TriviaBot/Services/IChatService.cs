using Discord;

namespace TriviaBot.Services
{
    public interface IChatService
    {
        void SendMessage(IChannel channel, string message);
    }
}