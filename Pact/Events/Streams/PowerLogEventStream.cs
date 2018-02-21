using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class PowerLogEventStream
        : IEventStream
    {
        private readonly string _filePath;
        private readonly IPowerLogEventParser _powerLogEventParser;

        public PowerLogEventStream(
            string filePath,
            IPowerLogEventParser powerLogEventParser)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _powerLogEventParser = powerLogEventParser ?? throw new ArgumentNullException(nameof(powerLogEventParser));
        }

        private readonly Queue<object> _parsedEvents = new Queue<object>();
        private string _remainingText = null;
        private long _streamPosition = 0;

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
    }
}
