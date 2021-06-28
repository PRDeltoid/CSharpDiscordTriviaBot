using System;
using TriviaBot.Services;

namespace TriviaBot
{
    public class TriviaBotServiceFactory
    {
        private IServiceProvider _serviceProvider;

        public TriviaBotServiceFactory(IServiceProvider serviceProvder)
        {
            _serviceProvider = serviceProvder;
        }

        public ITriviaBotService CreateTriviaBotService()
        {
            return (ITriviaBotService)_serviceProvider.GetService(typeof(ITriviaBotService));
        }
    }
}
