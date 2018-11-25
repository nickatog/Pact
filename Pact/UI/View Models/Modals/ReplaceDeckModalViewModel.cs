using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Input;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ReplaceDeckModalViewModel
        : IModalViewModel<Models.Client.Decklist?>
        , INotifyPropertyChanged
    {
        private readonly ISerializer<Models.Client.Decklist> _decklistSerializer;

        private string _importErrorMessage;

        public ReplaceDeckModalViewModel(
            ISerializer<Models.Client.Decklist> decklistSerializer)
        {
            _decklistSerializer = decklistSerializer.Require(nameof(decklistSerializer));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event Action<Models.Client.Decklist?> OnClosed;

        public ICommand Cancel =>
            new DelegateCommand(
                () => OnClosed?.Invoke(null));

        public string DeckString { get; set; }

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

                        OnClosed?.Invoke(decklist);
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
