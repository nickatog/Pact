#region Namespaces
using System;
using System.Windows.Input;
#endregion // Namespaces

namespace Pact
{
    public sealed class ConfigurationSettingsViewModel
    {
        #region Dependencies
        private readonly IEditableConfigurationSettings _configurationSettings;
        #endregion // Dependencies

        #region Constructors
        public ConfigurationSettingsViewModel(
            IEditableConfigurationSettings configurationSettings)
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
                    _configurationSettings.SaveChanges(
                        _configurationSettings,
                        __configurationSettings =>
                        {
                            __configurationSettings.CardTextOffset = CardTextOffset;
                            __configurationSettings.FontSize = FontSize;
                        });
                });
    }
}
