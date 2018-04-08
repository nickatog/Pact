using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DeckImportViewModelFactory
        : IDeckImportViewModelFactory
    {
        private readonly IDecklistSerializer _decklistSerializer;

        public DeckImportViewModelFactory(
            IDecklistSerializer decklistSerializer)
        {
            _decklistSerializer = decklistSerializer.Require(nameof(decklistSerializer));
        }

        IModalViewModel<DeckImportResult?> IDeckImportViewModelFactory.Create()
        {
            return new DeckImportViewModel(_decklistSerializer);
        }
    }
}
