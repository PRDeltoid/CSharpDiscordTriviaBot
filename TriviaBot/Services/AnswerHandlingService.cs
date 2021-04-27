using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriviaBot.Modules;

namespace TriviaBot.Services
{
    public class AnswerHandlingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly ITriviaBotModule _triviaBotManager;

        public AnswerHandlingService(DiscordSocketClient discord, ITriviaBotModule triviaBotManager)
        {
            _discord = discord;
            _triviaBotManager = triviaBotManager;
            // Hook MessageReceived so we can process each message to see
            // if it qualifies as an answern
            _discord.MessageReceived += MessageReceivedAsync;
        }

        /// <summary>
        /// Determines if a given message is an "answer" (ie. 1-4)
        /// Passes it to trivia manage if it is a valid answer
        /// </summary>
        /// <param name="rawMessage">The message to interpret</param>
        private async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;
            if (message.Author.IsBot) { return; }
            if (rawMessage.Content.Length > 1) { return; }

            string messageText = rawMessage.Content;
            if(messageText != "1" && messageText != "2" && messageText != "3" && messageText != "4") { return; }

            _triviaBotManager.CheckAnswer(rawMessage);
        }
    }
}
