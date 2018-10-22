using System;
using System.Threading.Tasks;
using Valkyrie;

namespace Pact
{
    public sealed class EventDispatchingConfigurationStorage
        : IConfigurationStorage
    {
        private readonly IConfigurationStorage _configurationStorage;
        private readonly IEventDispatcher _eventDispatcher;

        public EventDispatchingConfigurationStorage(
            IConfigurationStorage configurationStorage,
            IEventDispatcher eventDispatcher)
        {
            _configurationStorage =
                configurationStorage
                ?? throw new ArgumentNullException(nameof(configurationStorage));

            _eventDispatcher =
                eventDispatcher
                ?? throw new ArgumentNullException(nameof(eventDispatcher));
        }

        async Task IConfigurationStorage.SaveChanges(
            ConfigurationData configurationData)
        {
            await _configurationStorage.SaveChanges(configurationData).ConfigureAwait(false);

            _eventDispatcher.DispatchEvent(new Events.ConfigurationSettingsSaved());
        }
    }
}
