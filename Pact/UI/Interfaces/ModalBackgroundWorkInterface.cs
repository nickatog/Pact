using System;
using System.Threading.Tasks;

namespace Pact
{
    internal sealed class ModalBackgroundWorkInterface
        : IBackgroundWorkInterface
    {
        private readonly IModalDisplay _modalDisplay;

        public ModalBackgroundWorkInterface(
            IModalDisplay modalDisplay)
        {
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
                new BackgroundWorkModalViewModel(@delegate),
                __ => result.SetResult(true),
                fadeDuration);

            return result.Task;
        }
    }
}
