using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Input;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DeckImportModalViewModel
        : IModalViewModel<Models.Interface.DeckImportDetail?>
        , INotifyPropertyChanged
    {
        private readonly ISerializer<Models.Client.Decklist> _decklistSerializer;

        private string _importErrorMessage;

        public DeckImportModalViewModel(
            ISerializer<Models.Client.Decklist> decklistSerializer)
        {
            _decklistSerializer = decklistSerializer.Require(nameof(decklistSerializer));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event Action<Models.Interface.DeckImportDetail?> OnClosed;

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
                        Models.Client.Decklist decklist;

                        using (var stream = new MemoryStream(Encoding.Default.GetBytes(DeckString)))
                            decklist = _decklistSerializer.Deserialize(stream).Result;

                        OnClosed?.Invoke(new Models.Interface.DeckImportDetail(DeckTitle, decklist));
                    }
                    catch (Exception)
                    {
                        ImportErrorMessage = "Invalid deckstring provided!";
                    }
                });

        public string ImportErrorMessage
        {
            get => _importErrorMessage;
            private set
            {
                _importErrorMessage = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImportErrorMessage)));
            }
        }
    }
}
