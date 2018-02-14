using System.Collections.Generic;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class PowerLogEventStreamFactory
        : IEventStreamFactory
    {
        private readonly IConfigurationSettings _configurationSettings;
        private readonly IEnumerable<IGameStateDebugEventParser> _eventParsers;

        public PowerLogEventStreamFactory(
            IConfigurationSettings configurationSettings,
            IEnumerable<IGameStateDebugEventParser> eventParsers)
        {
            _configurationSettings = configurationSettings.ThrowIfNull(nameof(configurationSettings));
            _eventParsers = eventParsers.ThrowIfNull(nameof(eventParsers));
        }

        IEventStream IEventStreamFactory.Create()
        {
            return
                new PowerLogEventStream(
                    _configurationSettings.PowerLogFilePath,
                    new GameStateDebugPowerLogEventParser(_eventParsers));
        }
    }
}
