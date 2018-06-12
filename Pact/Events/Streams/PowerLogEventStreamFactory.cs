using System;
using System.Collections.Generic;

namespace Pact
{
    public sealed class PowerLogEventStreamFactory
        : IEventStreamFactory
    {
        private readonly IConfigurationSource _configurationSource;
        private readonly IEnumerable<IGameStateDebugEventParser> _eventParsers;

        private string _powerLogFilePath;

        public PowerLogEventStreamFactory(
            IConfigurationSource configurationSource,
            IEnumerable<IGameStateDebugEventParser> eventParsers)
        {
            _configurationSource =
                configurationSource
                ?? throw new ArgumentNullException(nameof(configurationSource));

            _eventParsers =
                eventParsers
                ?? throw new ArgumentNullException(nameof(eventParsers));

            _powerLogFilePath = _configurationSource.GetSettings().PowerLogFilePath;
        }

        IEventStream IEventStreamFactory.Create()
        {
            return
                new PowerLogEventStream(
                    _powerLogFilePath,
                    new GameStateDebugPowerLogEventParser(_eventParsers));
        }
    }
}
