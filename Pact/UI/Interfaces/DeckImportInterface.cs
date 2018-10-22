using System;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class DeckImportInterface
        : IDeckImportInterface
    {
        private readonly IModalDisplay _modalDisplay;
        private readonly IDeckImportViewModelFactory _viewModelFactory;

        public DeckImportInterface(
            IModalDisplay modalDisplay,
            IDeckImportViewModelFactory viewModelFactory)
        {
            _modalDisplay =
                modalDisplay
                ?? throw new ArgumentNullException(nameof(modalDisplay));

            _viewModelFactory =
                viewModelFactory
                ?? throw new ArgumentNullException(nameof(viewModelFactory));
        }

        Task<DeckImportDetails?> IDeckImportInterface.GetDecklist()
        {
            // create view model (via factory?)
            // pass to modal display, along with result handler
            // handler inspects result and creates deck import details object for task completion source

            var result = new TaskCompletionSource<DeckImportDetails?>();

            _modalDisplay.Show(
                _viewModelFactory.Create(),
                __result =>
                {
                    DeckImportDetails? res = null;

                    if (__result.HasValue)
                        res = new DeckImportDetails(__result.Value.Title, __result.Value.Decklist);

                    result.SetResult(res);
                });

            //var view = new DeckImportView(_decklistSerializer) { Owner = MainWindow.Window };
            //if (!(view.ShowDialog() ?? false))
            //    return Task.FromResult<DeckImportDetails?>(default);

            //return Task.FromResult<DeckImportDetails?>(new DeckImportDetails(view.DeckTitle, view.Deck));

            return result.Task;
        }
    }
}
