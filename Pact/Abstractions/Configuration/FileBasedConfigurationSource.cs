using System;
using System.IO;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class FileBasedConfigurationSource
        : IConfigurationSource
    {
        private readonly ISerializer<ConfigurationData> _configurationSerializer;
        private readonly string _filePath;

        public FileBasedConfigurationSource(
            ISerializer<ConfigurationData> configurationSerializer,
            string filePath)
        {
            _configurationSerializer =
                configurationSerializer.Require(nameof(configurationSerializer));

            _filePath =
                filePath.Require(nameof(filePath));
        }

        IConfigurationSettings IConfigurationSource.GetSettings()
        {
            try
            {
                using (var stream = new FileStream(_filePath, FileMode.Open))
                    return new ConfigurationSettings(_configurationSerializer.Deserialize(stream).Result);
            }
            catch (Exception)
            {
            }

            return new ConfigurationSettings(default);
        }
    }
}
