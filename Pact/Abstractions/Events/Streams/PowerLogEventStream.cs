using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Pact.Extensions.Enumerable;
using Valkyrie;

namespace Pact
{
    public sealed class PowerLogEventStream
        : IEventStream
    {
        private readonly IConfigurationSource _configurationSource;
        private readonly IPowerLogEventParser _powerLogEventParser;
        private readonly IEventDispatcher _viewEventDispatcher;

        private readonly IList<IEventHandler> _eventHandlers = new List<IEventHandler>();
        private string _filePath;
        private readonly Queue<object> _parsedEvents = new Queue<object>();
        private string _remainingText;
        private long _streamPosition;

        public PowerLogEventStream(
            IConfigurationSource configurationSource,
            IPowerLogEventParser powerLogEventParser,
            IEventDispatcher viewEventDispatcher)
        {
            _configurationSource =
                configurationSource
                ?? throw new ArgumentNullException(nameof(configurationSource));

            _powerLogEventParser =
                powerLogEventParser
                ?? throw new ArgumentNullException(nameof(powerLogEventParser));

            _viewEventDispatcher =
                viewEventDispatcher
                ?? throw new ArgumentNullException(nameof(viewEventDispatcher));

            // ---

            _filePath = _configurationSource.GetSettings().PowerLogFilePath;
            
            _eventHandlers.Add(
                new DelegateEventHandler<ViewEvents.ConfigurationSettingsSaved>(
                    __event =>
                    {
                        string newFilePath = _configurationSource.GetSettings().PowerLogFilePath;
                        if (!string.Equals(newFilePath, _filePath, StringComparison.OrdinalIgnoreCase))
                        {
                            _remainingText = null;
                            _streamPosition = 0L;

                            _filePath = newFilePath;
                        }
                    }));

            _eventHandlers.ForEach(__eventHandler => _viewEventDispatcher.RegisterHandler(__eventHandler));
        }

        async Task<object> IEventStream.ReadNext()
        {
            if (_parsedEvents.Count > 0)
                return _parsedEvents.Dequeue();

            while (true)
            {
                if (!File.Exists(_filePath))
                {
                    await Task.Delay(1000);

                    continue;
                }

                using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (stream.Length < _streamPosition)
                    {
                        _streamPosition = 0;

                        _remainingText = null;
                    }

                    stream.Seek(_streamPosition, SeekOrigin.Begin);

                    using (var streamReader = new StreamReader(stream))
                    {
                        if (_remainingText != null)
                            _remainingText += Environment.NewLine;
                        
                        _remainingText += streamReader.ReadToEnd();

                        _streamPosition = stream.Position;
                    }
                }

                foreach (object parsedEvent in _powerLogEventParser.ParseEvents(ref _remainingText))
                    _parsedEvents.Enqueue(parsedEvent);

                if (_parsedEvents.Count > 0)
                    return _parsedEvents.Dequeue();

                await Task.Delay(1000);
            }
        }

        private bool _disposed;

        private void Dispose(
            bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    _eventHandlers.ForEach(__eventHandler => _viewEventDispatcher.UnregisterHandler(__eventHandler));

                _disposed = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }
    }
}
