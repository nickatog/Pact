using System.Threading.Tasks;

namespace Pact
{
    public interface IDeckImportInterface
    {
        Task<DeckImportDetails?> GetDeckImportDetails();
    }
}
