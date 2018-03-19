using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class ConfigurationSettingsViewModel
    {
        public ConfigurationSettingsViewModel()
        {
            AccountName = "Test";
        }

        public string AccountName { get; set; }
    }
}
