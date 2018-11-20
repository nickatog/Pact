using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ModalReplaceDeckInterface
        : IReplaceDeckInterface
    {
        #region Private members
        private readonly ISerializer<Decklist> _decklistSerializer;
        private readonly IModalDisplay _modalDisplay;
        #endregion // Private members

        public ModalReplaceDeckInterface(
        #region Dependency assignments
            ISerializer<Decklist> decklistSerializer,
            IModalDisplay modalDisplay)
        {
            _decklistSerializer =
                decklistSerializer.Require(nameof(decklistSerializer));

            _modalDisplay =
                modalDisplay.Require(nameof(modalDisplay));
            #endregion // Dependency assignments
        }

        Task<Decklist?> IReplaceDeckInterface.GetReplacementDecklist()
        {
            var completionSource = new TaskCompletionSource<Decklist?>();

            _modalDisplay.Show(
                new ReplaceDeckModalViewModel(_decklistSerializer),
                __decklist => completionSource.SetResult(__decklist));

            return completionSource.Task;
        }
    }
}
