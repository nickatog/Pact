using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pact
{
    public static class ConfigurationSettings
    {
        private static IConfigurationSettings _global;
        public static IConfigurationSettings Global
        {
            get
            {
                // default to something

                return _global;
            }
            set
            {
                _global = value;
            }
        }
    }
}
