using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class BackgroundWorkModalViewModel
        : IModalViewModel<bool>
        , INotifyPropertyChanged
    {
        #region Private members
        private readonly Task _backgroundWork;
        private string _statusMessage;
        #endregion // Private members

        public BackgroundWorkModalViewModel(
            #region Dependency assignments
            Func<Action<string>, Task> backgroundWorker)
        {
            if (backgroundWorker == null)
                throw new ArgumentNullException(nameof(backgroundWorker));
            #endregion // Dependency assignments

            _backgroundWork = backgroundWorker(__statusMessage => StatusMessage = __statusMessage);
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
    }
}
