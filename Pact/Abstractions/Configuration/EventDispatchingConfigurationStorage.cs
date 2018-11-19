using System.Threading.Tasks;

using Valkyrie;

using Pact.Extensions.Contract;

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
            _configurationStorage = configurationStorage.Require(nameof(configurationStorage));
            _eventDispatcher = eventDispatcher.Require(nameof(eventDispatcher));
        }

        async Task IConfigurationStorage.SaveChanges(
            ConfigurationData configurationData)
        {
            await _configurationStorage.SaveChanges(configurationData);

            _eventDispatcher.DispatchEvent(new ViewEvents.ConfigurationSettingsSaved());
        }
    }
}
