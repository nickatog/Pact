using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class FileBasedConfigurationSettings
        : IConfigurationSettings
    {
        private readonly ISerializer<ConfigurationStorage> _configurationSerializer;
        private ConfigurationStorage _configurationStorage;
        private readonly string _filePath;

        public FileBasedConfigurationSettings(
            ISerializer<ConfigurationStorage> configurationSerializer,
            string filePath)
        {
            _configurationSerializer = configurationSerializer ?? throw new ArgumentNullException(nameof(configurationSerializer));
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            using (var stream = new FileStream(_filePath, FileMode.Open))
                _configurationStorage = _configurationSerializer.Deserialize(stream).Result;
        }

        int IConfigurationSettings.FontSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IConfigurationSettings.PowerLogFilePath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private void SaveToFile()
        {
            using (var stream = new FileStream(_filePath, FileMode.Open))
                _configurationSerializer.Serialize(stream, _configurationStorage).Wait();
        }
    }

    [Serializable]
    public struct ConfigurationStorage
        : ISerializable<ConfigurationStorage>
    {
        private int _fontSize;
        public int FontSize => _fontSize;

        public ConfigurationStorage(
            int fontSize)
        {
            _fontSize = fontSize;
        }

        public static Task<ConfigurationStorage> Deserialize(Stream stream)
        {
            var serializer = new BinaryFormatter();

            return Task.FromResult((ConfigurationStorage)serializer.Deserialize(stream));
        }

        Task ISerializable<ConfigurationStorage>.Serialize(Stream stream)
        {
            var serializer = new BinaryFormatter();

            serializer.Serialize(stream, this);

            return Task.CompletedTask;
        }
    }
}
