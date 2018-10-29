using System;
using System.IO;
using System.Text;
using System.Windows.Input;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DeckImportViewModel
        : IModalViewModel<DeckImportResult?>
    {
        private readonly IDecklistSerializer _decklistSerializer;

        public DeckImportViewModel(
            IDecklistSerializer decklistSerializer)
        {
            _decklistSerializer =
                decklistSerializer.Require(nameof(decklistSerializer));
        }

        public ICommand Cancel => new DelegateCommand(() => OnClosed?.Invoke(null));

        public string DeckString { get; set; }

        public string DeckTitle { get; set; }

        public ICommand ImportDeck =>
            new DelegateCommand(
                () =>
                {
                    try
                    {
                        Decklist decklist = default;

                        using (var stream = new MemoryStream(Encoding.Default.GetBytes(DeckString)))
                            decklist = _decklistSerializer.Deserialize(stream).Result;

                        OnClosed?.Invoke(new DeckImportResult(DeckTitle, decklist));
                    }
                    catch (Exception)
                    {
                        // notify user that deserialization failed for some reason
                    }
                });

        public event Action<DeckImportResult?> OnClosed;
    }
}
