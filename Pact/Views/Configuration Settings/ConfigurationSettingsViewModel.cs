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
        private readonly IConfigurationSource _configurationSource;
        private readonly IConfigurationStorage _configurationStorage;
        private readonly IBackgroundWorkInterface _waitInterface;
        #endregion // Dependencies

        #region Fields
        private readonly IConfigurationSettings _configurationSettings;
        #endregion // Fields

        #region Constructors
        public ConfigurationSettingsViewModel(
            IConfigurationSource configurationSource,
            IConfigurationStorage configurationStorage,
            IBackgroundWorkInterface waitInterface)
        {
            _configurationSource =
                configurationSource
                ?? throw new ArgumentNullException(nameof(configurationSource));

            _configurationStorage =
                configurationStorage
                ?? throw new ArgumentNullException(nameof(configurationStorage));

            _waitInterface =
                waitInterface
                ?? throw new ArgumentNullException(nameof(waitInterface));

            _configurationSettings = _configurationSource.GetSettings();

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
                    _waitInterface.Perform(
                        async __notifyStatus =>
                        {
                            __notifyStatus?.Invoke("Saving settings...");

                            await _configurationStorage.SaveChanges(
                                new ConfigurationData(_configurationSource.GetSettings())
                                {
                                    CardTextOffset = CardTextOffset,
                                    FontSize = FontSize
                                });

                            __notifyStatus?.Invoke("Settings saved!");

                            await Task.Delay(1000);
                        });
                });
    }
}
