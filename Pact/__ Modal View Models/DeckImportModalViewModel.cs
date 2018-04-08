using System;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace Pact
{
    public struct DeckImportModalResult
    {
        public Decklist Decklist { get; private set; }
        public string Title { get; private set; }

        public DeckImportModalResult(
            string title,
            Decklist decklist)
        {
            Decklist = decklist;
            Title = title;
        }
    }

    public sealed class DeckImportModalViewModel
        : IModalViewModel<DeckImportModalResult?>
    {
        private readonly IDecklistSerializer _decklistSerializer;

        public DeckImportModalViewModel(
            IDecklistSerializer decklistSerializer)
        {
            _decklistSerializer = decklistSerializer ?? throw new ArgumentNullException(nameof(decklistSerializer));
        }

        public string DeckString { get; set; }

        public ICommand Cancel => new DelegateCommand(() => OnClosed?.Invoke(null));

        public ICommand ImportDeck =>
            new DelegateCommand(
                () =>
                {
                    try
                    {
                        Decklist decklist = default;

                        using (var stream = new MemoryStream(Encoding.Default.GetBytes(DeckString)))
                            decklist = _decklistSerializer.Deserialize(stream).Result;

                        OnClosed?.Invoke(new DeckImportModalResult(DeckTitle, decklist));
                    }
                    catch (Exception)
                    {
                        // notify user that deserialization failed for some reason
                    }
                });

        public string DeckTitle { get; set; }

        public event Action<DeckImportModalResult?> OnClosed;
    }
}
