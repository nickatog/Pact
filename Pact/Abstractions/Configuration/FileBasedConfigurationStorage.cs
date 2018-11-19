using System.IO;
using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class FileBasedConfigurationStorage
        : IConfigurationStorage
    {
        private readonly ISerializer<ConfigurationData> _configurationSerializer;
        private readonly string _filePath;

        public FileBasedConfigurationStorage(
            ISerializer<ConfigurationData> configurationSerializer,
            string filePath)
        {
            _configurationSerializer = configurationSerializer.Require(nameof(configurationSerializer));
            _filePath = filePath.Require(nameof(filePath));
        }

        async Task IConfigurationStorage.SaveChanges(
            ConfigurationData configurationData)
        {
            using (var stream = new FileStream(_filePath, FileMode.Create))
                await _configurationSerializer.Serialize(stream, configurationData).ConfigureAwait(false);
        }
    }
}
