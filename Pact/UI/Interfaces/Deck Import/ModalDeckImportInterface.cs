using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ModalDeckImportInterface
        : IDeckImportInterface
    {
        private readonly ISerializer<Models.Client.Decklist> _decklistSerializer;
        private readonly IModalDisplay _modalDisplay;

        public ModalDeckImportInterface(
            ISerializer<Models.Client.Decklist> decklistSerializer,
            IModalDisplay modalDisplay)
        {
            _decklistSerializer = decklistSerializer.Require(nameof(decklistSerializer));
            _modalDisplay = modalDisplay.Require(nameof(modalDisplay));
        }

        Task<Models.Interface.DeckImportDetail?> IDeckImportInterface.GetDetail()
        {
            var completionSource = new TaskCompletionSource<Models.Interface.DeckImportDetail?>();

            _modalDisplay.Show(
                new DeckImportModalViewModel(_decklistSerializer),
                __result => completionSource.SetResult(__result));

            return completionSource.Task;
        }
    }
}
