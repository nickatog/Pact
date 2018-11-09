using System.Windows;
using System.Windows.Controls;

namespace Pact.Properties
{
    public static class VisibilityToggle
    {
        public static readonly DependencyProperty CollapseButton =
            DependencyProperty.RegisterAttached(
                "CollapseButton",
                typeof(Button),
                typeof(VisibilityToggle),
                new UIPropertyMetadata(null, OnCollapseButtonChanged));

        public static Button GetCollapseButton(
            UIElement element)
        {
            return (Button)element?.GetValue(CollapseButton);
        }

        private static void OnCollapseButtonChanged(
            DependencyObject @object,
            DependencyPropertyChangedEventArgs args)
        {
            if (@object is UIElement element)
            {
                Button button = GetCollapseButton(element);
                if (button == null)
                    return;

                button.Click +=
                    (__1, __2) => element.Visibility = Visibility.Collapsed;
            }
        }

        public static readonly DependencyProperty ShowButton =
            DependencyProperty.RegisterAttached(
                "ShowButton",
                typeof(Button),
                typeof(VisibilityToggle),
                new UIPropertyMetadata(null, OnShowButtonChanged));

        public static Button GetShowButton(
            UIElement element)
        {
            return (Button)element?.GetValue(ShowButton);
        }

        private static void OnShowButtonChanged(
            DependencyObject @object,
            DependencyPropertyChangedEventArgs args)
        {
            if (@object is UIElement element)
            {
                Button button = GetShowButton(element);
                if (button == null)
                    return;

                button.Click +=
                    (__1, __2) => element.Visibility = Visibility.Visible;
            }
        }
    }
}
