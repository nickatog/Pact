using System;

namespace Pact.Models.Data
{
    public struct SavedLog
    {
        public Guid ID;
        public DateTimeOffset Timestamp;
        public string Title;

        public SavedLog(
            Guid id,
            string title,
            DateTimeOffset timestamp)
        {
            ID = id;
            Title = title;
            Timestamp = timestamp;
        }
    }
}
