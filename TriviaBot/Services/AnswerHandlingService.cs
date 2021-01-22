using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaBot.Services
{
    class AnswerHandlingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly ITriviaManager _trivia_manager;

        public AnswerHandlingService(IServiceProvider services) {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _trivia_manager = services.GetRequiredService<ITriviaManager>();
            _services = services;

            // Hook MessageReceived so we can process each message to see
            // if it qualifies as an answer
            _discord.MessageReceived += MessageReceivedAsync;
        }

        private async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;
            _trivia_manager.CheckAnswer(rawMessage);
        }
    }
}
