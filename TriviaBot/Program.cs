using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using TriviaBot.Services;
using TriviaBot.Modules;

namespace TriviaBot
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            // You should dispose a service provider created using ASP.NET
            // when you are finished using it, at the end of your app's lifetime.
            // If you use another dependency injection framework, you should inspect
            // its documentation for the best way to do this.
            using (var services = ConfigurationContainer.ConfigureServices())
            {
                LogAsync(new LogMessage(LogSeverity.Info, "TriviaBot", $"Version: {Assembly.GetExecutingAssembly().GetName().Version}"));

                var _discord = services.GetRequiredService<DiscordSocketClient>();
                _discord.Log += LogAsync;
                _discord.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
                _discord.StartAsync();

                services.GetRequiredService<CommandService>().Log += LogAsync;

                services.GetRequiredService<CommandHandlingService>().InitializeAsync();
                services.GetRequiredService<AnswerHandlingService>();
                await Task.Delay(Timeout.Infinite);
            }
        }
        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

    }
}
