using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;

namespace Pact
{
    public sealed class WelcomeViewModel
        : INotifyPropertyChanged
    {
        private string _powerLogFilePath;

        public WelcomeViewModel(
            string powerLogFilePath)
        {
            _powerLogFilePath = powerLogFilePath;

            // check status of logging
            // --- %user%\AppData\Local\Blizzard\Hearthstone\log.config
            // - does file exist?
            // - does file contain
            //[Power]
            //FilePrinting=True

        }

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
                    if (openFileDialog.ShowDialog().Equals(true))
                        PowerLogFilePath = openFileDialog.FileName;
                });

        public string PowerLogFilePath
        {
            get => _powerLogFilePath;
            private set
            {
                _powerLogFilePath = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PowerLogFilePath)));
            }
        }

        public string PowerLogStatusText { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
