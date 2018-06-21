using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class BackgroundWorkModalViewModel
        : IModalViewModel<bool>
        , INotifyPropertyChanged
    {
        private readonly Task _finished;
        private string _status;

        public BackgroundWorkModalViewModel(
            Func<Action<string>, Task> @delegate)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            _finished = taskCompletionSource.Task;

            Task.Run(
                async () =>
                {
                    await @delegate?.Invoke(__status => Status = __status);

                    taskCompletionSource.SetResult(true);
                });
        }

        event Action<bool> IModalViewModel<bool>.OnClosed
        {
            add
            {
                _finished.ContinueWith(__ => value?.Invoke(true));
            }
            remove
            {
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
