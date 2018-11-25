using System.IO;
using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class FileBasedConfigurationStorage
        : IConfigurationStorage
    {
        private readonly AsyncSemaphore _asyncMutex;
        private readonly ISerializer<Models.Data.ConfigurationData> _configurationDataSerializer;
        private readonly string _filePath;

        public FileBasedConfigurationStorage(
            AsyncSemaphore asyncMutex,
            ISerializer<Models.Data.ConfigurationData> configurationDataSerializer,
            string filePath)
        {
            _asyncMutex = asyncMutex.Require(nameof(asyncMutex));
            _configurationDataSerializer = configurationDataSerializer.Require(nameof(configurationDataSerializer));
            _filePath = filePath.Require(nameof(filePath));
        }

        async Task IConfigurationStorage.SaveChanges(
            Models.Client.ConfigurationSettings configurationSettings)
        {
            using (await _asyncMutex.WaitAsync().ConfigureAwait(false))
            {
                var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(_filePath));
                if (!directoryInfo.Exists)
                    directoryInfo.Create();

                using (var stream = new FileStream(_filePath, FileMode.Create))
                {
                    await _configurationDataSerializer.Serialize(
                        stream,
                        new Models.Data.ConfigurationData(
                            configurationSettings.CardTextOffset,
                            configurationSettings.FontSize,
                            configurationSettings.HasLoaded,
                            configurationSettings.PowerLogFilePath,
                            configurationSettings.TrackerWindowLocation,
                            configurationSettings.TrackerWindowSize)).ConfigureAwait(false);
                }
            }
        }
    }
}
