using Microsoft.Win32;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ConfigurationSettingsViewModel
        : INotifyPropertyChanged
    {
        #region Private members
        private readonly IBackgroundWorkInterface _backgroundWorkInterface;
        private readonly IConfigurationSource _configurationSource;
        private readonly IConfigurationStorage _configurationStorage;

        private string _powerLogFilePath;
        #endregion // Private members

        public ConfigurationSettingsViewModel(
            #region Dependency assignments
            IBackgroundWorkInterface backgroundWorkInterface,
            IConfigurationSource configurationSource,
            IConfigurationStorage configurationStorage)
        {
            _backgroundWorkInterface =
                backgroundWorkInterface.Require(nameof(backgroundWorkInterface));

            _configurationSource =
                configurationSource.Require(nameof(configurationSource));

            _configurationStorage =
                configurationStorage.Require(nameof(configurationStorage));

            IConfigurationSettings configurationSettings = _configurationSource.GetSettings();

            CardTextOffset = configurationSettings.CardTextOffset;
            FontSize = configurationSettings.FontSize;
            _powerLogFilePath = configurationSettings.PowerLogFilePath;
            #endregion // Dependency assignments
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand BrowseForPowerLogFilePath =>
            new DelegateCommand(
                () =>
                {
                    var openFileDialog = new OpenFileDialog()
                    {
                        CheckFileExists = false,
                        CheckPathExists = false,
                        FileName = "Power.log",
                        ValidateNames = false
                    };
                    if (openFileDialog.ShowDialog() ?? false)
                        PowerLogFilePath = openFileDialog.FileName;
                });

        public int CardTextOffset { get; set; }

        public int FontSize { get; set; }

        public string PowerLogFilePath
        {
            get => _powerLogFilePath;
            private set
            {
                _powerLogFilePath = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PowerLogFilePath)));
            }
        }

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
                                    FontSize = FontSize,
                                    PowerLogFilePath = PowerLogFilePath
                                });

                            __setStatus?.Invoke("Settings saved!");

                            await Task.Delay(500);
                        },
                        750);
                });
    }
}
