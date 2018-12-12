using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Pact
{
    public sealed class AboutPageModalViewModel
        : IModalViewModel<object>
    {
        public event Action<object> OnClosed;

        public ICommand Close =>
            new DelegateCommand(
                () => OnClosed?.Invoke(null));

        public ICommand VisitGitHub =>
            new DelegateCommand(
                () => Process.Start("https://github.com/nickatog"));

        public ICommand VisitTwitter =>
            new DelegateCommand(
                () => Process.Start("https://twitter.com/nickatog"));
    }
}
