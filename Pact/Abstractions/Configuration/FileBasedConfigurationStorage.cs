using System.IO;
using System.Threading.Tasks;

using Valkyrie;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class FileBasedConfigurationStorage
        : IConfigurationStorage
    {
        private readonly ISerializer<ConfigurationData> _configurationSerializer;
        private readonly string _filePath;
        private readonly IEventDispatcher _viewEventDispatcher;

        public FileBasedConfigurationStorage(
            ISerializer<ConfigurationData> configurationSerializer,
            string filePath,
            IEventDispatcher viewEventDispatcher)
        {
            _configurationSerializer =
                configurationSerializer.Require(nameof(configurationSerializer));
            
            _filePath =
                filePath.Require(nameof(filePath));

            _viewEventDispatcher =
                viewEventDispatcher.Require(nameof(viewEventDispatcher));
        }

        async Task IConfigurationStorage.SaveChanges(
            ConfigurationData configurationData)
        {
            using (var stream = new FileStream(_filePath, FileMode.Create))
                await _configurationSerializer.Serialize(stream, configurationData).ConfigureAwait(false);

            _viewEventDispatcher.DispatchEvent(new Events.ConfigurationSettingsSaved());
        }
    }
}
