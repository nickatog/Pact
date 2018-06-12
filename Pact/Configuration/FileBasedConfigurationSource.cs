using System;
using System.IO;
using System.Threading.Tasks;

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
                configurationSerializer
                ?? throw new ArgumentNullException(nameof(configurationSerializer));

            _filePath =
                filePath
                ?? throw new ArgumentNullException(nameof(filePath));
        }

        async Task<IConfigurationSettings> IConfigurationSource.GetSettings()
        {
            if (!File.Exists(_filePath))
                return new ConfigurationSettings(default);

            using (var stream = new FileStream(_filePath, FileMode.Open))
                return new ConfigurationSettings(await _configurationSerializer.Deserialize(stream));
        }
    }
}
