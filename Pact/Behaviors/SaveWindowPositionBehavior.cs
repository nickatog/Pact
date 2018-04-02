using System;
using System.Threading;
using System.Windows;
using System.Windows.Interactivity;

namespace Pact.Behaviors
{
    public sealed class SaveWindowPositionBehavior
        : Behavior<Window>
    {
        private IConfigurationSettings ConfigurationSettings =>
            ((PlayerDeckTrackerViewModel)AssociatedObject.DataContext).ConfigurationSettings;

        private Timer _timer;

        protected override void OnAttached()
        {
            base.OnAttached();
            
            AssociatedObject.LocationChanged += OnLocationChanged;
        }

        private void OnLocationChanged(object sender, EventArgs args)
        {
            if (_timer == null)
                _timer = new Timer(SaveWindowPosition, null, 1000, Timeout.Infinite);
            else
                _timer?.Change(1000, Timeout.Infinite);
        }

        private void SaveWindowPosition(object state)
        {
            // Save window position and size here
            
            double height = 0f;
            double width = 0f;
            double left = 0f;
            double top = 0f;

            Dispatcher.Invoke(
                () =>
                {
                    height = AssociatedObject.ActualHeight;
                    width = AssociatedObject.ActualWidth;
                    left = AssociatedObject.Left;
                    top = AssociatedObject.Top;
                });

            System.Diagnostics.Debug.WriteLine($"Saving: {width}x{height}, at ({left},{top})");

            _timer = null;
        }
    }
}
