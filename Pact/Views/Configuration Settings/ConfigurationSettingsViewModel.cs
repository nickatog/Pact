#region Namespaces
using System;
using System.Windows.Input;
#endregion // Namespaces

namespace Pact
{
    public sealed class ConfigurationSettingsViewModel
    {
        #region Dependencies
        private readonly IConfigurationSource _configurationSource;
        private readonly IConfigurationStorage _configurationStorage;
        #endregion // Dependencies

        #region Fields
        private readonly IConfigurationSettings _configurationSettings;
        #endregion // Fields

        #region Constructors
        public ConfigurationSettingsViewModel(
            IConfigurationSource configurationSource,
            IConfigurationStorage configurationStorage)
        {
            _configurationSource =
                configurationSource
                ?? throw new ArgumentNullException(nameof(configurationSource));

            _configurationStorage =
                configurationStorage
                ?? throw new ArgumentNullException(nameof(configurationStorage));

            _configurationSettings = _configurationSource.GetSettings().Result;

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
                    _configurationStorage.SaveChanges(
                        new ConfigurationData(_configurationSource.GetSettings().Result)
                        {
                            CardTextOffset = CardTextOffset,
                            FontSize = FontSize
                        });
                });
    }
}
