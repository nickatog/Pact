using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DeckImportModalViewModelFactory
        : IDeckImportModalViewModelFactory
    {
        private readonly IDecklistSerializer _decklistSerializer;

        public DeckImportModalViewModelFactory(
            IDecklistSerializer decklistSerializer)
        {
            _decklistSerializer = decklistSerializer.Require(nameof(decklistSerializer));
        }

        IModalViewModel<DeckImportModalResult?> IDeckImportModalViewModelFactory.Create()
        {
            return new DeckImportModalViewModel(_decklistSerializer);
        }
    }
}
