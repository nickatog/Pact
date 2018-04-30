﻿using System;
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

        private readonly Timer _timer;

        public SaveWindowPositionBehavior()
        {
            _timer = new Timer(SaveWindowPosition, null, Timeout.Infinite, Timeout.Infinite);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.LocationChanged += OnLocationChanged;
        }

        private void OnLocationChanged(object sender, EventArgs args)
        {
            _timer.Change(1000, Timeout.Infinite);
        }

        private void SaveWindowPosition(object state)
        {
            double left = default;
            double top = default;

            Size size = default;

            IConfigurationSettings configurationSettings = null;

            Dispatcher.Invoke(
                () =>
                {
                    left = AssociatedObject.Left;
                    top = AssociatedObject.Top;

                    size = AssociatedObject.RenderSize;

                    configurationSettings = ConfigurationSettings;
                });

            configurationSettings.TrackerWindowLocation = new Point(left, top);
            configurationSettings.TrackerWindowSize = size;
        }
    }
}
