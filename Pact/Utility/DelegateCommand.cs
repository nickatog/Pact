using System;
using System.Windows.Input;

namespace Pact
{
    public sealed class DelegateCommand
        : ICommand
    {
        private readonly Action<object> _delegate;

        public DelegateCommand(Action<object> @delegate)
        {
            _delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
        }

        public DelegateCommand(Action @delegate)
            : this(__ => @delegate())
        { }

        void ICommand.Execute(object parameter) => _delegate(parameter);

        bool ICommand.CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;
    }
}
