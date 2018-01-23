using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Pact.Extensions.Contract;

namespace Pact
{
    public partial class DeckImportView
        : Window
        , INotifyPropertyChanged
    {
        private readonly IDecklistSerializer _decklistSerializer;

        public DeckImportView(
            IDecklistSerializer decklistSerializer)
        {
            _decklistSerializer = decklistSerializer.OrThrow(nameof(decklistSerializer));

            InitializeComponent();

            DataContext = this;
        }

        public Decklist Deck { get; private set; }

        public string DeckString { get; set; }

        private bool _hasDeck = false;
        public bool HasDeck => _hasDeck;

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

                    _hasDeck = true;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasDeck"));
                });

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
