using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Input;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DeckImportModalViewModel
        : IModalViewModel<DeckImportResult?>
        , INotifyPropertyChanged
    {
        #region Private members
        private readonly ISerializer<Decklist> _decklistSerializer;
        private string _importErrorMessage;
        #endregion // Private members

        public DeckImportModalViewModel(
            #region Dependency assignments
            ISerializer<Decklist> decklistSerializer)
        {
            _decklistSerializer =
                decklistSerializer.Require(nameof(decklistSerializer));
            #endregion // Dependency assignments
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
                    ImportErrorMessage = null;

                    try
                    {
                        Decklist decklist;

                        using (var stream = new MemoryStream(Encoding.Default.GetBytes(DeckString)))
                            decklist = _decklistSerializer.Deserialize(stream).Result;

                        OnClosed?.Invoke(new DeckImportResult(DeckTitle, decklist));
                    }
                    catch (Exception)
                    {
                        ImportErrorMessage = "Invalid deckstring provided!";
                    }
                });

        public string ImportErrorMessage
        {
            get => _importErrorMessage;
            set
            {
                _importErrorMessage = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImportErrorMessage)));
            }
        }
    }
}
