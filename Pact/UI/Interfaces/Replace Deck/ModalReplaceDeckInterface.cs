using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ModalReplaceDeckInterface
        : IReplaceDeckInterface
    {
        private readonly ISerializer<Models.Client.Decklist> _decklistSerializer;
        private readonly IModalDisplay _modalDisplay;

        public ModalReplaceDeckInterface(
            ISerializer<Models.Client.Decklist> decklistSerializer,
            IModalDisplay modalDisplay)
        {
            _decklistSerializer = decklistSerializer.Require(nameof(decklistSerializer));
            _modalDisplay = modalDisplay.Require(nameof(modalDisplay));
        }

        Task<Models.Client.Decklist?> IReplaceDeckInterface.GetDecklist()
        {
            var completionSource = new TaskCompletionSource<Models.Client.Decklist?>();

            _modalDisplay.Show(
                new ReplaceDeckModalViewModel(_decklistSerializer),
                __decklist => completionSource.SetResult(__decklist));

            return completionSource.Task;
        }
    }
}
