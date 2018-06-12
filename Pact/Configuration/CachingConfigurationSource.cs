using System;
using Valkyrie;

namespace Pact
{
    public sealed class CachingConfigurationSource
        : IConfigurationSource
    {
        private readonly IConfigurationSource _configurationSource;
        private readonly IEventDispatcher _eventDispatcher;

        private IConfigurationSettings _cachedConfigurationSettings;

        public CachingConfigurationSource(
            IConfigurationSource configurationSource,
            IEventDispatcher eventDispatcher)
        {
            _configurationSource =
                configurationSource
                ?? throw new ArgumentNullException(nameof(configurationSource));

            _eventDispatcher =
                eventDispatcher
                ?? throw new ArgumentNullException(nameof(eventDispatcher));

            _eventDispatcher.RegisterHandler(
                new DelegateEventHandler<Events.ConfigurationSettingsSaved>(
                    __ => _cachedConfigurationSettings = null));
        }

        IConfigurationSettings IConfigurationSource.GetSettings()
        {
            if (_cachedConfigurationSettings != null)
                return _cachedConfigurationSettings;

            _cachedConfigurationSettings = _configurationSource.GetSettings();

            return _cachedConfigurationSettings;
        }
    }
}
