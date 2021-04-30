using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TriviaBot.Modules;
using TriviaBot.Services;

namespace TriviaBot
{
    public class ConfigurationContainer
    {
        public static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<AnswerHandlingService>()
                .AddSingleton<Database>()
                .AddScoped<ILifetimeScorekeeper, LifetimeScorekeeperService>()
                .AddScoped<ITriviaBotModule, TriviaBotModule>()
                .AddScoped<IScoreKeeperService, ScoreKeeperService>()
                .AddScoped<IQuestionSetManager, QuestionSetManager>()
                .AddScoped<ITriviaBotService, TriviaBotService>()
                .AddScoped<TriviaBotServiceFactory>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }
    }
}
