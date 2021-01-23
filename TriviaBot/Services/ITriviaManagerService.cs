using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace TriviaBot.Services
{
    public interface ITriviaManagerService
    {
        bool IsRunning { get; set; }
        void CheckAnswer(SocketMessage rawMessage);
        void Start();
        void Stop();

        event EventHandler TriviaStarted;
        event EventHandler TriviaStopped;
        event EventHandler OutOfQuestions;

        event EventHandler QuestionAnswered;
        event EventHandler QuestionTimedOut;
        event EventHandler QuestionSkipped;
        event EventHandler QuestionReady;
    }
}