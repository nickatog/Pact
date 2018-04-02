using System;
using System.Windows.Input;

namespace Pact
{
    public sealed class ConfigurationSettingsViewModel
    {
        private readonly IConfigurationSettings _configurationSettings;

        public ConfigurationSettingsViewModel(
            IConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings ?? throw new ArgumentNullException(nameof(configurationSettings));

            FontSize = _configurationSettings.FontSize;
        }

        public int FontSize { get; set; }

        public ICommand SaveSettings =>
            new DelegateCommand(
                () =>
                {
                    if (FontSize != _configurationSettings.FontSize)
                        _configurationSettings.FontSize = FontSize;
                });
    }
}
