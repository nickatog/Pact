using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Pact
{
    public struct DeckImportModalReturn
    { }

    public sealed class DeckImportModalViewModel
        : IModalViewModel<DeckImportModalReturn?>
    {
        private readonly IDecklistSerializer _decklistSerializer;

        public DeckImportModalViewModel(
            IDecklistSerializer decklistSerializer)
        {
            _decklistSerializer = decklistSerializer ?? throw new ArgumentNullException(nameof(decklistSerializer));
        }

        public Decklist Deck { get; private set; }

        public string DeckString { get; set; }

        public ICommand ImportDeck =>
            new DelegateCommand(
                () =>
                {
                    try
                    {
                        using (var stream = new MemoryStream(Encoding.Default.GetBytes(DeckString)))
                            Deck = _decklistSerializer.Deserialize(stream).Result;
                    }
                    catch (Exception)
                    {
                        return;
                    }

                    // how are we closing the modal now that it's not its own window?
                    //DialogResult = true;

                    //Close();
                });

        public string DeckTitle { get; set; }

        public event Action<DeckImportModalReturn?> OnClosed;
    }
}
