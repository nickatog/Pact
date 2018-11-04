using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DeckImportInterface
        : IDeckImportInterface
    {
        #region Private members
        private readonly IDecklistSerializer _decklistSerializer;
        private readonly IModalDisplay _modalDisplay;
        #endregion // Private members

        public DeckImportInterface(
            #region Dependency assignments
            IDecklistSerializer decklistSerializer,
            IModalDisplay modalDisplay)
        {
            _decklistSerializer =
                decklistSerializer.Require(nameof(decklistSerializer));

            _modalDisplay =
                modalDisplay.Require(nameof(modalDisplay));
            #endregion // Dependency assignments
        }

        Task<DeckImportDetails?> IDeckImportInterface.GetDecklist()
        {
            var completionSource = new TaskCompletionSource<DeckImportDetails?>();

            _modalDisplay.Show(
                new DeckImportModalViewModel(_decklistSerializer),
                __result =>
                {
                    DeckImportDetails? details = null;

                    if (__result.HasValue)
                        details = new DeckImportDetails(__result.Value.Title, __result.Value.Decklist);

                    completionSource.SetResult(details);
                });

            return completionSource.Task;
        }
    }
}
