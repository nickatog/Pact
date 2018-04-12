using System.Windows;

namespace Pact.Behaviors
{
    public static class TestBehavior
    {
        public static readonly DependencyProperty ToggleButton =
            DependencyProperty.RegisterAttached(
                "ToggleButton",
                typeof(UIElement),
                typeof(TestBehavior),
                new UIPropertyMetadata(null, OnToggleButtonChanged));

        public static UIElement GetToggleButton(System.Windows.Controls.Button window)
        {
            return (UIElement)window.GetValue(ToggleButton);
        }

        private static void OnToggleButtonChanged(DependencyObject @object, DependencyPropertyChangedEventArgs args)
        {
            if (@object is System.Windows.Controls.Button window)
            {
                window.Click +=
                    (__1, __2) =>
                    {
                        UIElement target = GetToggleButton(window);

                        Visibility currentVisibility = target.Visibility;

                        target.Visibility = currentVisibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                    };
            }
        }
    }
}
