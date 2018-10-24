using System;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class DeckImportInterface
        : IDeckImportInterface
    {
        private readonly IDecklistSerializer _decklistSerializer;
        private readonly IModalDisplay _modalDisplay;

        public DeckImportInterface(
            IDecklistSerializer decklistSerializer,
            IModalDisplay modalDisplay)
        {
            _decklistSerializer =
                decklistSerializer
                ?? throw new ArgumentNullException(nameof(decklistSerializer));

            _modalDisplay =
                modalDisplay
                ?? throw new ArgumentNullException(nameof(modalDisplay));
        }

        Task<DeckImportDetails?> IDeckImportInterface.GetDecklist()
        {
            var result = new TaskCompletionSource<DeckImportDetails?>();

            _modalDisplay.Show(
                new DeckImportViewModel(_decklistSerializer),
                __result =>
                {
                    DeckImportDetails? res = null;

                    if (__result.HasValue)
                        res = new DeckImportDetails(__result.Value.Title, __result.Value.Decklist);

                    result.SetResult(res);
                });

            return result.Task;
        }
    }
}
