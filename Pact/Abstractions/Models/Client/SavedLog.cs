using System;

namespace Pact.Models.Client
{
    public struct SavedLog
    {
        public SavedLog(
            Guid id,
            string filePath,
            string title,
            DateTimeOffset timestamp)
        {
            ID = id;
            FilePath = filePath;
            Title = title;
            Timestamp = timestamp;
        }

        public string FilePath { get; }
        public Guid ID { get; }
        public DateTimeOffset Timestamp { get; }
        public string Title { get; }
    }
}
