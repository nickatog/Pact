using System;
using System.Collections.Generic;
using Valkyrie;

namespace Pact
{
    public sealed class PowerLogEventStreamFactory
        : IEventStreamFactory
    {
        private readonly IConfigurationSource _configurationSource;
        private readonly IEnumerable<IGameStateDebugEventParser> _eventParsers;
        private readonly IEventDispatcher _viewEventDispatcher;

        public PowerLogEventStreamFactory(
            IConfigurationSource configurationSource,
            IEnumerable<IGameStateDebugEventParser> eventParsers,
            IEventDispatcher viewEventDispatcher)
        {
            _configurationSource =
                configurationSource
                ?? throw new ArgumentNullException(nameof(configurationSource));

            _eventParsers =
                eventParsers
                ?? throw new ArgumentNullException(nameof(eventParsers));

            _viewEventDispatcher =
                viewEventDispatcher
                ?? throw new ArgumentNullException(nameof(viewEventDispatcher));
        }

        IEventStream IEventStreamFactory.Create()
        {
            return
                new PowerLogEventStream(
                    _configurationSource,
                    new GameStateDebugPowerLogEventParser(_eventParsers),
                    _viewEventDispatcher);
        }
    }
}
