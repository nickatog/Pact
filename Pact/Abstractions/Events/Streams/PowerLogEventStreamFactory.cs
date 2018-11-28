using System.Collections.Generic;

using Valkyrie;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class PowerLogEventStreamFactory
        : IEventStreamFactory
    {
        private readonly IConfigurationSource _configurationSource;
        private readonly IEnumerable<IGameStateDebugEventParser> _eventParsers;
        private readonly IEventDispatcher _viewEventDispatcher;

        private readonly bool _seekEndWhenFileChanges;

        public PowerLogEventStreamFactory(
            IConfigurationSource configurationSource,
            IEnumerable<IGameStateDebugEventParser> eventParsers,
            IEventDispatcher viewEventDispatcher,
            bool seekEndWhenFileChanges)
        {
            _configurationSource = configurationSource.Require(nameof(configurationSource));
            _eventParsers = eventParsers.Require(nameof(eventParsers));
            _viewEventDispatcher = viewEventDispatcher.Require(nameof(viewEventDispatcher));

            _seekEndWhenFileChanges = seekEndWhenFileChanges;
        }

        IEventStream IEventStreamFactory.Create()
        {
            return
                new PowerLogEventStream(
                    _configurationSource,
                    new GameStateDebugPowerLogEventParser(_eventParsers),
                    _viewEventDispatcher,
                    _seekEndWhenFileChanges);
        }
    }
}
