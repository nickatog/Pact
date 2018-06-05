#region Namespaces
using System;
using System.Windows.Input;
#endregion // Namespaces

namespace Pact
{
    public sealed class ConfigurationSettingsViewModel
    {
        #region Dependencies
        private readonly IConfigurationSettings _configurationSettings;
        #endregion // Dependencies

        #region Constructors
        public ConfigurationSettingsViewModel(
            IConfigurationSettings configurationSettings)
        {
            _configurationSettings =
                configurationSettings
                ?? throw new ArgumentNullException(nameof(configurationSettings));

            CardTextOffset = _configurationSettings.CardTextOffset;
            FontSize = _configurationSettings.FontSize;
        }
        #endregion // Constructors

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
