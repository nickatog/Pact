using System;
using System.Windows.Input;

namespace Pact
{
    public sealed class DelegateCommand
        : ICommand
    {
        private readonly Func<bool> _canExecute;
        private readonly Action<object> _delegate;

        public DelegateCommand(
            Action<object> @delegate,
            Func<bool> canExecute = null,
            Action<Action> canExecuteChangedClient = null)
        {
            _delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));

            _canExecute = canExecute;

            canExecuteChangedClient?.Invoke(() => CanExecuteChanged?.Invoke(this, null));
        }

        public DelegateCommand(
            Action @delegate,
            Func<bool> canExecute = null,
            Action<Action> canExecuteChangedClient = null)
            : this(__ => @delegate(), canExecute, canExecuteChangedClient)
        { }

        void ICommand.Execute(object parameter) => _delegate(parameter);

        bool ICommand.CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public event EventHandler CanExecuteChanged;
    }
}
