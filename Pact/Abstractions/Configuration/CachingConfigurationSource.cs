using Valkyrie;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class CachingConfigurationSource
        : IConfigurationSource
    {
        private Models.Client.ConfigurationSettings? _cachedConfigurationSettings;
        private readonly IConfigurationSource _configurationSource;
        private readonly object _lock = new object();
        private readonly IEventDispatcher _eventDispatcher;

        public CachingConfigurationSource(
            IConfigurationSource configurationSource,
            IEventDispatcher eventDispatcher)
        {
            _configurationSource = configurationSource.Require(nameof(configurationSource));
            _eventDispatcher = eventDispatcher.Require(nameof(eventDispatcher));

            _eventDispatcher.RegisterHandler(
                new DelegateEventHandler<ViewEvents.ConfigurationSettingsSaved>(
                    __ =>
                    {
                        lock(_lock)
                            _cachedConfigurationSettings = null;
                    }));
        }

        Models.Client.ConfigurationSettings IConfigurationSource.GetSettings()
        {
            lock (_lock)
            {
                if (!_cachedConfigurationSettings.HasValue)
                    _cachedConfigurationSettings = _configurationSource.GetSettings();

                return _cachedConfigurationSettings.Value;
            }
        }
    }
}
