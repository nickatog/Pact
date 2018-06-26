#region Namespaces
using System;
using System.Threading.Tasks;
using System.Windows.Input;
#endregion // Namespaces

namespace Pact
{
    public sealed class ConfigurationSettingsViewModel
    {
        #region Dependencies
        private readonly IBackgroundWorkInterface _backgroundWorkInterface;
        private readonly IConfigurationSource _configurationSource;
        private readonly IConfigurationStorage _configurationStorage;
        #endregion // Dependencies

        #region Constructors
        public ConfigurationSettingsViewModel(
            IBackgroundWorkInterface backgroundWorkInterface,
            IConfigurationSource configurationSource,
            IConfigurationStorage configurationStorage)
        {
            _backgroundWorkInterface =
                backgroundWorkInterface
                ?? throw new ArgumentNullException(nameof(backgroundWorkInterface));

            _configurationSource =
                configurationSource
                ?? throw new ArgumentNullException(nameof(configurationSource));

            _configurationStorage =
                configurationStorage
                ?? throw new ArgumentNullException(nameof(configurationStorage));

            IConfigurationSettings configurationSettings = _configurationSource.GetSettings();

            CardTextOffset = configurationSettings.CardTextOffset;
            FontSize = configurationSettings.FontSize;
        }
        #endregion // Constructors

        public int CardTextOffset { get; set; }

        public int FontSize { get; set; }

        public ICommand SaveSettings =>
            new DelegateCommand(
                () =>
                {
                    _backgroundWorkInterface.Perform(
                        async __setStatus =>
                        {
                            __setStatus?.Invoke("Saving settings...");

                            await _configurationStorage.SaveChanges(
                                new ConfigurationData(_configurationSource.GetSettings())
                                {
                                    CardTextOffset = CardTextOffset,
                                    FontSize = FontSize
                                });

                            __setStatus?.Invoke("Settings saved!");

                            await Task.Delay(500);
                        });
                });
    }
}
