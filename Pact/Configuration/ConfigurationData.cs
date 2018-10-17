﻿using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;

namespace Pact
{
    [Serializable]
    public struct ConfigurationData
        : ISerializable<ConfigurationData>
    {
        public ConfigurationData(
            IConfigurationSettings configurationSettings)
        {
            if (configurationSettings == null)
                throw new ArgumentNullException(nameof(configurationSettings));

            _cardTextOffset = configurationSettings.CardTextOffset;
            _fontSize = configurationSettings.FontSize;
            _hasLoaded = configurationSettings.HasLoaded;
            _powerLogFilePath = configurationSettings.PowerLogFilePath;
            _trackerWindowLocation = configurationSettings.TrackerWindowLocation;
            _trackerWindowSize = configurationSettings.TrackerWindowSize;
        }

        private int _cardTextOffset;
        public int CardTextOffset
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

        private bool _hasLoaded;
        public bool HasLoaded
        {
            get => _hasLoaded;
            set { _hasLoaded = value; }
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

        public static Task<ConfigurationData> Deserialize(
            Stream stream)
        {
            var serializer = new BinaryFormatter();

            return Task.FromResult((ConfigurationData)serializer.Deserialize(stream));
        }

        Task ISerializable<ConfigurationData>.Serialize(
            Stream stream)
        {
            var serializer = new BinaryFormatter();

            serializer.Serialize(stream, this);

            return Task.CompletedTask;
        }
    }
}