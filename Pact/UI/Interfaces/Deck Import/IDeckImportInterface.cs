using System.Threading.Tasks;

namespace Pact
{
    public interface IDeckImportInterface
    {
        Task<Models.Interface.DeckImportDetail?> GetDetail();
    }
}
