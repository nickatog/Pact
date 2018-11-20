using Valkyrie;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class CachingConfigurationSource
        : IConfigurationSource
    {
        private ConfigurationSettings? _cachedConfigurationSettings;
        private readonly IConfigurationSource _configurationSource;
        private readonly object _lock = new object();
        private readonly IEventDispatcher _viewEventDispatcher;

        public CachingConfigurationSource(
            IConfigurationSource configurationSource,
            IEventDispatcher viewEventDispatcher)
        {
            _configurationSource = configurationSource.Require(nameof(configurationSource));
            _viewEventDispatcher = viewEventDispatcher.Require(nameof(viewEventDispatcher));

            _viewEventDispatcher.RegisterHandler(
                new DelegateEventHandler<ViewEvents.ConfigurationSettingsSaved>(
                    __ =>
                    {
                        lock(_lock)
                            _cachedConfigurationSettings = null;
                    }));
        }

        ConfigurationSettings IConfigurationSource.GetSettings()
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
