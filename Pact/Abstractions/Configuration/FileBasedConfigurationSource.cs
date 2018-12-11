using System.IO;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class FileBasedConfigurationSource
        : IConfigurationSource
    {
        private readonly ISerializer<Models.Data.ConfigurationData> _configurationDataSerializer;
        private readonly string _filePath;

        public FileBasedConfigurationSource(
            ISerializer<Models.Data.ConfigurationData> configurationDataSerializer,
            string filePath)
        {
            _configurationDataSerializer = configurationDataSerializer.Require(nameof(configurationDataSerializer));
            _filePath = filePath.Require(nameof(filePath));
        }

        Models.Client.ConfigurationSettings IConfigurationSource.GetSettings()
        {
            try
            {
                Models.Data.ConfigurationData configurationData;

                using (var stream = new FileStream(_filePath, FileMode.Open))
                    configurationData = _configurationDataSerializer.Deserialize(stream).Result;

                return
                    new Models.Client.ConfigurationSettings(
                        configurationData.CardTextOffset,
                        configurationData.FontSize,
                        configurationData.HasLoaded,
                        configurationData.PowerLogFilePath,
                        configurationData.TextEditorFilePath,
                        configurationData.TrackerWindowLocation,
                        configurationData.TrackerWindowSize);
            }
            catch (FileNotFoundException) {}
            catch (DirectoryNotFoundException) {}

            return default;
        }
    }
}
