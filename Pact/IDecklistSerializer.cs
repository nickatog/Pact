using System.IO;
using System.Threading.Tasks;

namespace Pact
{
    public interface IDecklistSerializer
    {
        Task<Decklist> Deserialize(
            Stream stream);

        Task Serialize(
            Stream stream, Decklist decklist);
    }
}
