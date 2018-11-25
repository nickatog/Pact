using System;
using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ModalBackgroundWorkInterface
        : IBackgroundWorkInterface
    {
        private readonly IModalDisplay _modalDisplay;

        public ModalBackgroundWorkInterface(
            IModalDisplay modalDisplay)
        {
            _modalDisplay = modalDisplay.Require(nameof(modalDisplay));
        }

        Task IBackgroundWorkInterface.Perform(
            Func<Action<string>, Task> backgroundWorker,
            int fadeDuration)
        {
            var completionSource = new TaskCompletionSource<object>();

            _modalDisplay.Show(
                new BackgroundWorkModalViewModel(backgroundWorker),
                __ => completionSource.SetResult(null),
                fadeDuration);

            return completionSource.Task;
        }
    }
}
