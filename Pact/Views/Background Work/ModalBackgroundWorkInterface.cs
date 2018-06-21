using System;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class ModalBackgroundWorkInterface
        : IBackgroundWorkInterface
    {
        private readonly IBackgroundWorkModalViewModelFactory _backgroundWorkModalViewModelFactory;
        private readonly IModalDisplay _modalDisplay;

        public ModalBackgroundWorkInterface(
            IModalDisplay modalDisplay,
            IBackgroundWorkModalViewModelFactory backgroundWorkModalViewModelFactory)
        {
            _modalDisplay =
                modalDisplay
                ?? throw new ArgumentNullException(nameof(modalDisplay));

            _backgroundWorkModalViewModelFactory =
                backgroundWorkModalViewModelFactory
                ?? throw new ArgumentNullException(nameof(backgroundWorkModalViewModelFactory));
        }

        Task IBackgroundWorkInterface.Perform(
            Func<Action<string>, Task> @delegate)
        {
            var result = new TaskCompletionSource<bool>();

            _modalDisplay.Show(
                _backgroundWorkModalViewModelFactory.Create(@delegate),
                __ => result.SetResult(true));

            return result.Task;
        }
    }
}
