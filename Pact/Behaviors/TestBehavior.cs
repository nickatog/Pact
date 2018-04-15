using System.Windows;
using System.Windows.Controls;

namespace Pact.Behaviors
{
    public static class ToggleVisibilityBehavior
    {
        public static readonly DependencyProperty ToggleButton =
            DependencyProperty.RegisterAttached(
                "ToggleButton",
                typeof(Button),
                typeof(ToggleVisibilityBehavior),
                new UIPropertyMetadata(null, OnToggleButtonChanged));

        public static Button GetToggleButton(UIElement element)
        {
            return (Button)element.GetValue(ToggleButton);
        }

        private static void OnToggleButtonChanged(DependencyObject @object, DependencyPropertyChangedEventArgs args)
        {
            if (@object is UIElement element)
            {
                Button button = GetToggleButton(element);
                if (button == null)
                    return;

                button.Click +=
                    (__1, __2) =>
                        element.Visibility =
                            element.Visibility == Visibility.Visible
                            ? Visibility.Collapsed
                            : Visibility.Visible;
            }
        }
    }
}
