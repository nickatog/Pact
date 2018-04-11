using System;
using System.Windows;

namespace Pact
{
    public sealed class EventDispatchingConfigurationSettings
        : IConfigurationSettings
    {
        private readonly IConfigurationSettings _configurationSettings;
        private readonly Valkyrie.IEventDispatcher _eventDispatcher;

        public EventDispatchingConfigurationSettings(
            IConfigurationSettings configurationSettings,
            Valkyrie.IEventDispatcher eventDispatcher)
        {
            _configurationSettings = configurationSettings ?? throw new ArgumentNullException(nameof(configurationSettings));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        }

        int IConfigurationSettings.CardTextOffset
        {
            get => _configurationSettings.CardTextOffset;
            set
            {
                _configurationSettings.CardTextOffset = value;

                _eventDispatcher.DispatchEvent(new Events.DeckTrackerCardTextOffsetChanged());
            }
        }

        int IConfigurationSettings.FontSize
        {
            get => _configurationSettings.FontSize;
            set
            {
                _configurationSettings.FontSize = value;

                _eventDispatcher.DispatchEvent(new Events.DeckTrackerFontSizeChanged());
            }
        }

        string IConfigurationSettings.PowerLogFilePath
        {
            get => _configurationSettings.PowerLogFilePath;
            set
            {
                _configurationSettings.PowerLogFilePath = value;
            }
        }

        Point? IConfigurationSettings.TrackerWindowLocation
        {
            get => _configurationSettings.TrackerWindowLocation;
            set
            {
                _configurationSettings.TrackerWindowLocation = value;
            }
        }

        Size? IConfigurationSettings.TrackerWindowSize
        {
            get => _configurationSettings.TrackerWindowSize;
            set
            {
                _configurationSettings.TrackerWindowSize = value;
            }
        }
    }
}
