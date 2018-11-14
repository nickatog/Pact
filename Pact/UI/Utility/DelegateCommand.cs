using System;
using System.Windows.Input;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DelegateCommand
        : ICommand
    {
        #region Private members
        private readonly Func<bool> _canExecute;
        private readonly Action<object> _delegate;
        #endregion // Private members

        public DelegateCommand(
            #region Dependency assignments
            Action<object> @delegate,
            Func<bool> canExecute = null,
            Action<Action> canExecuteChangedClient = null)
        {
            _delegate =
                @delegate.Require(nameof(@delegate));

            _canExecute = canExecute;
            #endregion // Dependency assignments

            canExecuteChangedClient?.Invoke(() => CanExecuteChanged?.Invoke(this, null));
        }

        public DelegateCommand(
            Action @delegate,
            Func<bool> canExecute = null,
            Action<Action> canExecuteChangedClient = null)
            : this(__ => @delegate(), canExecute, canExecuteChangedClient)
        {
        }

        bool ICommand.CanExecute(
            object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public event EventHandler CanExecuteChanged;

        void ICommand.Execute(
            object parameter)
        {
            _delegate(parameter);
        }
    }
}
