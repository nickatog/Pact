using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pact
{
    public interface IPowerLogManager
    {
        Task DeleteSavedLog(
            Guid savedLogID);

        Task<IEnumerable<Models.Client.SavedLog>> GetSavedLogs();

        Task<Models.Client.SavedLog?> SaveCurrentLog(
            string title);

        Task UpdateSavedLog(
            Models.Client.SavedLogDetail savedLogDetail);
    }
}
