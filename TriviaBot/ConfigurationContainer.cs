using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
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
                .AddScoped<TriviaBotModule>()
                .AddScoped<ILifetimeScorekeeper, LifetimeScorekeeperService>()
                .AddScoped<IScoreKeeperService, ScoreKeeperService>()
                .AddScoped<IQuestionSetManager, QuestionSetManager>()
                .AddScoped<IChatService, ChatService>()
                .AddScoped<ITriviaBotService, TriviaBotService>()
                .AddScoped<TriviaBotServiceFactory>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }
    }
}
