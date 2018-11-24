using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class BackgroundWorkModalViewModel
        : IModalViewModel<bool>
        , INotifyPropertyChanged
    {
        private readonly Task _backgroundWork;
        private string _statusMessage;

        public BackgroundWorkModalViewModel(
            Func<Action<string>, Task> backgroundWorker)
        {
            backgroundWorker.Require(nameof(backgroundWorker));

            _backgroundWork = backgroundWorker(__statusMessage => StatusMessage = __statusMessage);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        event Action<bool> IModalViewModel<bool>.OnClosed
        {
            add
            {
                _backgroundWork.ContinueWith(__ => value?.Invoke(true));
            }
            remove {}
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                _statusMessage = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessage)));
            }
        }
    }
}
