using System;

namespace Pact.ViewCommands
{
    public sealed class DeleteSavedLog
    {
        public DeleteSavedLog(
            Guid savedLogID)
        {
            SavedLogID = savedLogID;
        }

        public Guid SavedLogID { get; }
    }
}
