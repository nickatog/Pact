using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pact
{
    public interface IPowerLogManager
    {
        Task DeleteSavedLog(
            Guid savedLogID);

        Task<IEnumerable<SavedLog>> GetSavedLogs();

        Task<SavedLog?> SaveCurrentLog(
            string title);

        Task UpdateSavedLog(
            SavedLogDetail savedLogDetail);
    }

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

    public struct SavedLogDetail
    {

    }
}
