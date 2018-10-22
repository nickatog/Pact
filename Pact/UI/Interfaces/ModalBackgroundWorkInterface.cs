using System;
using System.Threading.Tasks;

namespace Pact
{
    internal sealed class ModalBackgroundWorkInterface
        : IBackgroundWorkInterface
    {
        // TODO: Reduce some type bloat by getting rid of view model factories?
        // While it might be a best practice in terms of abstraction, not sure I'm gaining a lot from it in this project
        private readonly IBackgroundWorkModalViewModelFactory _backgroundWorkModalViewModelFactory;
        private readonly IModalDisplay _modalDisplay;

        public ModalBackgroundWorkInterface(
            IBackgroundWorkModalViewModelFactory backgroundWorkModalViewModelFactory,
            IModalDisplay modalDisplay)
        {
            _backgroundWorkModalViewModelFactory =
                backgroundWorkModalViewModelFactory
                ?? throw new ArgumentNullException(nameof(backgroundWorkModalViewModelFactory));

            _modalDisplay =
                modalDisplay
                ?? throw new ArgumentNullException(nameof(modalDisplay));
        }

        Task IBackgroundWorkInterface.Perform(
            Func<Action<string>, Task> @delegate,
            int fadeDuration)
        {
            var result = new TaskCompletionSource<bool>();

            _modalDisplay.Show(
                _backgroundWorkModalViewModelFactory.Create(@delegate),
                __ => result.SetResult(true),
                fadeDuration);

            return result.Task;
        }
    }
}
