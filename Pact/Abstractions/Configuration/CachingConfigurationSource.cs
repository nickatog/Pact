using Valkyrie;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class CachingConfigurationSource
        : IConfigurationSource
    {
        private IConfigurationSettings _cachedConfigurationSettings;
        private readonly IConfigurationSource _configurationSource;
        private readonly IEventDispatcher _viewEventDispatcher;
        private readonly object _lock = new object();

        public CachingConfigurationSource(
            IConfigurationSource configurationSource,
            IEventDispatcher viewEventDispatcher)
        {
            _configurationSource =
                configurationSource.Require(nameof(configurationSource));

            _viewEventDispatcher =
                viewEventDispatcher.Require(nameof(viewEventDispatcher));

            _viewEventDispatcher.RegisterHandler(
                new DelegateEventHandler<Events.ConfigurationSettingsSaved>(
                    __ => _cachedConfigurationSettings = null));
        }

        IConfigurationSettings IConfigurationSource.GetSettings()
        {
            IConfigurationSettings configurationSettings = _cachedConfigurationSettings;
            if (configurationSettings != null)
                return configurationSettings;

            lock (_lock)
            {
                if (_cachedConfigurationSettings == null)
                    _cachedConfigurationSettings = _configurationSource.GetSettings();

                return _cachedConfigurationSettings;
            }
        }
    }
}
