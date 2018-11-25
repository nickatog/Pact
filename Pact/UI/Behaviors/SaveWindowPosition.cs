using System;
using System.Threading;
using System.Windows;
using System.Windows.Interactivity;

namespace Pact.Behaviors
{
    public sealed class SaveWindowPosition
        : Behavior<Window>
    {
        private readonly Timer _timer;

        public SaveWindowPosition()
        {
            _timer = new Timer(Save, null, Timeout.Infinite, Timeout.Infinite);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            
            AssociatedObject.LocationChanged += OnLocationChanged;
        }

        private void OnLocationChanged(
            object sender,
            EventArgs args)
        {
            _timer.Change(1000, Timeout.Infinite);
        }

        private void Save(
            object state)
        {
            double left = default;
            double top = default;

            Size size = default;

            Dispatcher.Invoke(
                () =>
                {
                    left = AssociatedObject.Left;
                    top = AssociatedObject.Top;

                    size = AssociatedObject.RenderSize;
                });

            Models.Client.ConfigurationSettings configurationSettings = GlobalConfigurationSource.Instance.GetSettings();
            configurationSettings.TrackerWindowLocation = new Point(left, top);
            configurationSettings.TrackerWindowSize = size;

            GlobalConfigurationStorage.Instance.SaveChanges(configurationSettings);
        }
    }
}
