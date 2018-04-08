namespace Pact
{
    public sealed class DeckImportModalViewModelFactory
        : IDeckImportModalViewModelFactory
    {
        private readonly IDecklistSerializer _decklistSerializer;

        IModalViewModel<DeckImportModalResult?> IDeckImportModalViewModelFactory.Create()
        {
            return new DeckImportModalViewModel(_decklistSerializer);
        }
    }
}
