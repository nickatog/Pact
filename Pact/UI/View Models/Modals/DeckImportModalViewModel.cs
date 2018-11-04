using System;
using System.IO;
using System.Text;
using System.Windows.Input;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DeckImportModalViewModel
        : IModalViewModel<DeckImportResult?>
    {
        #region Private members
        private readonly IDecklistSerializer _decklistSerializer;
        #endregion // Private members

        public DeckImportModalViewModel(
            #region Dependency assignments
            IDecklistSerializer decklistSerializer)
        {
            _decklistSerializer =
                decklistSerializer.Require(nameof(decklistSerializer));
            #endregion // Dependency assignments
        }

        public event Action<DeckImportResult?> OnClosed;

        public ICommand Cancel =>
            new DelegateCommand(
                () => OnClosed?.Invoke(null));

        public string DeckString { get; set; }

        public string DeckTitle { get; set; }

        public ICommand ImportDeck =>
            new DelegateCommand(
                () =>
                {
                    try
                    {
                        Decklist decklist;

                        using (var stream = new MemoryStream(Encoding.Default.GetBytes(DeckString)))
                            decklist = _decklistSerializer.Deserialize(stream).Result;

                        OnClosed?.Invoke(new DeckImportResult(DeckTitle, decklist));
                    }
                    catch (Exception)
                    {
                        // [TODO]: Notify user that deckstring deserialization failed
                    }
                });
    }
}
