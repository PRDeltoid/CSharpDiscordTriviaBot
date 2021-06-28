using Discord;
using Discord.WebSocket;

namespace TriviaBot.Services
{
    public class ChatService : IChatService
    {
        DiscordSocketClient _discord;

        public ChatService(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        public void SendMessage(IChannel channel, string message)
        {
            if (_discord.GetChannel(channel.Id) is SocketTextChannel channelVar)
            {
                channelVar.SendMessageAsync(message);
            }
        }
    }
}
