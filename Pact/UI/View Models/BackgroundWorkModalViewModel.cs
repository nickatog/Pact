using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Pact
{
    internal sealed class BackgroundWorkModalViewModel
        : IModalViewModel<bool>
        , INotifyPropertyChanged
    {
        private readonly Task _backgroundWork;
        private string _statusMessage;

        public BackgroundWorkModalViewModel(
            Func<Action<string>, Task> backgroundWorker)
        {
            if (backgroundWorker == null)
                throw new ArgumentNullException(nameof(backgroundWorker));

            _backgroundWork = backgroundWorker(__statusMessage => StatusMessage = __statusMessage);
        }

        event Action<bool> IModalViewModel<bool>.OnClosed
        {
            add
            {
                _backgroundWork.ContinueWith(__ => value?.Invoke(true));
            }
            remove
            { }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessage)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
