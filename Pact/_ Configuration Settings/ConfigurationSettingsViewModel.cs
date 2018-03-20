using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class ConfigurationSettingsViewModel
    {
        private readonly IConfigurationSettings _configurationSettings;

        public ConfigurationSettingsViewModel(
            IConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings ?? throw new ArgumentNullException(nameof(configurationSettings));
        }

        public int FontSize
        {
            get
            {
                return _configurationSettings.FontSize;
            }
            set
            {
                _configurationSettings.FontSize = value;
            }
        }
    }
}
