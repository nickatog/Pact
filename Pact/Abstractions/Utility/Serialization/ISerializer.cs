using System.IO;
using System.Threading.Tasks;

namespace Pact
{
    public interface ISerializer<T>
    {
        Task<T> Deserialize(
            Stream stream);

        Task Serialize(
            Stream stream,
            T item);
    }
}
