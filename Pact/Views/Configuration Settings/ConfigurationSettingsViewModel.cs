using System.Windows.Input;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ConfigurationSettingsViewModel
    {
        private readonly IConfigurationSettings _configurationSettings;

        public ConfigurationSettingsViewModel(
            IConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings.Require(nameof(configurationSettings));

            CardTextOffset = _configurationSettings.CardTextOffset;
            FontSize = _configurationSettings.FontSize;
        }

        public int CardTextOffset { get; set; }

        public int FontSize { get; set; }

        public ICommand SaveSettings =>
            new DelegateCommand(
                () =>
                {
                    if (CardTextOffset != _configurationSettings.CardTextOffset)
                        _configurationSettings.CardTextOffset = CardTextOffset;
                    if (FontSize != _configurationSettings.FontSize)
                        _configurationSettings.FontSize = FontSize;
                });
    }
}
