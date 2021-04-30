using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
