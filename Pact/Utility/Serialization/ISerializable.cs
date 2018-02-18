using System.IO;
using System.Threading.Tasks;

namespace Pact
{
    public interface ISerializable<T>
    {
        Task Serialize(Stream stream);
    }
}
