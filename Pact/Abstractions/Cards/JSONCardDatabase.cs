using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class JSONCardDatabase
        : ICardDatabase
    {
        private readonly string _filePath;

        public JSONCardDatabase(
            string filePath)
        {
            _filePath = filePath.Require(nameof(filePath));
        }

        Task<IEnumerable<Models.Data.Card>> ICardDatabase.GetCards()
        {
            return
                Task.FromResult<IEnumerable<Models.Data.Card>>(
                    JsonConvert.DeserializeObject<Models.Data.Card[]>(
                        File.ReadAllText(_filePath)));
        }
    }
}
