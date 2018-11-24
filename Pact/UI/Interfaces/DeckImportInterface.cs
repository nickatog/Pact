using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DeckImportInterface
        : IDeckImportInterface
    {
        private readonly ISerializer<Models.Client.Decklist> _decklistSerializer;
        private readonly IModalDisplay _modalDisplay;

        public DeckImportInterface(
            ISerializer<Models.Client.Decklist> decklistSerializer,
            IModalDisplay modalDisplay)
        {
            _decklistSerializer = decklistSerializer.Require(nameof(decklistSerializer));
            _modalDisplay = modalDisplay.Require(nameof(modalDisplay));
        }

        Task<DeckImportDetails?> IDeckImportInterface.GetDeckImportDetails()
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
