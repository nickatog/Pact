#region Namespaces
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
#endregion // Namespaces

namespace Pact
{
    public sealed class FileBasedConfigurationSettings
        : IEditableConfigurationSettings
    {
        #region Dependencies
        private readonly ISerializer<ConfigurationStorage> _configurationSerializer;
        #endregion // Dependencies

        #region Fields
        private ConfigurationStorage _configurationStorage;
        private readonly string _filePath;
        #endregion // Fields

        public FileBasedConfigurationSettings(
            ISerializer<ConfigurationStorage> configurationSerializer,
            string filePath)
        {
            _configurationSerializer =
                configurationSerializer
                ?? throw new ArgumentNullException(nameof(configurationSerializer));

            _filePath =
                filePath
                ?? throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(_filePath))
                return;

            using (var stream = new FileStream(_filePath, FileMode.Open))
                _configurationStorage = _configurationSerializer.Deserialize(stream).Result;
        }

        public int CardTextOffset
        {
            get => _configurationStorage.CardTextOffset ?? 0;
            set
            {
                _configurationStorage.CardTextOffset = value;
            }
        }

        int IConfigurationSettings.FontSize
        {
            get => _configurationStorage.FontSize ?? 12;
            set
            {
                _configurationStorage.FontSize = value;

                SaveToFile();
            }
        }

        string IConfigurationSettings.PowerLogFilePath
        {
            get => _configurationStorage.PowerLogFilePath ?? @"C:\Program Files (x86)\Hearthstone\Logs\Power.log";
            set
            {
                _configurationStorage.PowerLogFilePath = value;

                SaveToFile();
            }
        }

        Point? IConfigurationSettings.TrackerWindowLocation
        {
            get => _configurationStorage.TrackerWindowLocation;
            set
            {
                _configurationStorage.TrackerWindowLocation = value;

                SaveToFile();
            }
        }

        Size? IConfigurationSettings.TrackerWindowSize
        {
            get => _configurationStorage.TrackerWindowSize;
            set
            {
                _configurationStorage.TrackerWindowSize = value;

                SaveToFile();
            }
        }

        private void SaveToFile()
        {
            using (var stream = new FileStream(_filePath, FileMode.Create))
                _configurationSerializer.Serialize(stream, _configurationStorage).Wait();
        }

        Task IEditableConfigurationSettings.SaveChanges(
            IEditableConfigurationSettings configurationSettings,
            Action<IEditableConfigurationSettings> configurationChanges)
        {
            configurationChanges?.Invoke(configurationSettings);

            using (var stream = new FileStream(_filePath, FileMode.Create))
                return _configurationSerializer.Serialize(stream, _configurationStorage);
        }
    }

    [Serializable]
    public struct ConfigurationStorage
        : ISerializable<ConfigurationStorage>
    {
        private int? _cardTextOffset;
        public int? CardTextOffset
        {
            get => _cardTextOffset;
            set { _cardTextOffset = value; }
        }

        private int? _fontSize;
        public int? FontSize
        {
            get => _fontSize;
            set { _fontSize = value; }
        }

        private string _powerLogFilePath;
        public string PowerLogFilePath
        {
            get => _powerLogFilePath;
            set { _powerLogFilePath = value; }
        }

        private Point? _trackerWindowLocation;
        public Point? TrackerWindowLocation
        {
            get => _trackerWindowLocation;
            set { _trackerWindowLocation = value; }
        }

        private Size? _trackerWindowSize;
        public Size? TrackerWindowSize
        {
            get => _trackerWindowSize;
            set { _trackerWindowSize = value; }
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
