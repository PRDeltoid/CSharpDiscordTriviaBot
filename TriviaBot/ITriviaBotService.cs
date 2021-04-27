using Discord;
using Discord.WebSocket;

namespace TriviaBot.Services
{
    public interface ITriviaBotService
    {
        bool IsRunning { get; set; }

        void CheckAnswer(SocketMessage rawMessage);
        void Start(int numberOfQuestions, string difficulty);
        void Stop();
        void VoteSkip(IUser user);
    }
}