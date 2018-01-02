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
        private readonly IPowerLogParser _powerLogParser;

        public PowerLogEventStream(
            string filePath,
            IPowerLogParser powerLogParser)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _powerLogParser = powerLogParser ?? throw new ArgumentNullException(nameof(powerLogParser));
        }

        private readonly Queue<object> _parsedEvents = new Queue<object>();
        private string _remainingText = string.Empty;
        private long _streamPosition = 0;

        async Task<object> IEventStream.ReadNext()
        {
            if (_parsedEvents.Count > 0)
                return _parsedEvents.Dequeue();

            while (true)
            {
                using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (stream.Length < _streamPosition)
                    {
                        _streamPosition = 0;

                        _remainingText = string.Empty;
                    }

                    stream.Seek(_streamPosition, SeekOrigin.Begin);

                    using (var streamReader = new StreamReader(stream))
                    {
                        _remainingText += streamReader.ReadToEnd();

                        _streamPosition = stream.Position;
                    }
                }

                foreach (object parsedEvent in _powerLogParser.ParseEvents(ref _remainingText))
                    _parsedEvents.Enqueue(parsedEvent);

                if (_parsedEvents.Count > 0)
                    return _parsedEvents.Dequeue();

                await Task.Delay(1000);
            }
        }
    }
}
