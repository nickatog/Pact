using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Pact
{
    public interface ICollectionSerializer<T>
    {
        Task<IEnumerable<T>> Deserialize(
            Stream stream);

        Task Serialize(
            Stream stream,
            IEnumerable<T> items);
    }
}
