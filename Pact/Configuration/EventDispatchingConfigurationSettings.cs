using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Pact.Extensions.Enumerable;

namespace Pact
{
    public sealed class EventDispatchingConfigurationSettings
        : IEditableConfigurationSettings
    {
        private readonly IEditableConfigurationSettings _configurationSettings;
        private readonly Valkyrie.IEventDispatcher _eventDispatcher;

        private readonly IList<object> _pendingEvents = new List<object>();

        public EventDispatchingConfigurationSettings(
            IEditableConfigurationSettings configurationSettings,
            Valkyrie.IEventDispatcher eventDispatcher)
        {
            _configurationSettings = configurationSettings ?? throw new ArgumentNullException(nameof(configurationSettings));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        }

        public int CardTextOffset
        {
            get => _configurationSettings.CardTextOffset;
            set
            {
                _configurationSettings.CardTextOffset = value;

                _pendingEvents.Add(new Events.DeckTrackerCardTextOffsetChanged());
            }
        }

        int IConfigurationSettings.FontSize
        {
            get => _configurationSettings.FontSize;
            set
            {
                _configurationSettings.FontSize = value;

                _pendingEvents.Add(new Events.DeckTrackerFontSizeChanged());
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

        async Task IEditableConfigurationSettings.SaveChanges(
            IEditableConfigurationSettings configurationSettings,
            Action<IEditableConfigurationSettings> configurationChanges)
        {
            await _configurationSettings.SaveChanges(configurationSettings, configurationChanges);

            _pendingEvents.ForEach(__event => _eventDispatcher.DispatchEvent(__event));
            _pendingEvents.Clear();
        }
    }
}
