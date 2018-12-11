using System;

namespace Pact.Models.Client
{
    public struct SavedLogDetail
    {
        public SavedLogDetail(
            Guid id,
            string title)
        {
            ID = id;
            Title = title;
        }

        public Guid ID { get; }
        public string Title { get; }
    }
}
