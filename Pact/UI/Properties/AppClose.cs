using System.Windows;
using System.Windows.Controls;

namespace Pact.Properties
{
    public static class AppClose
    {
        public static readonly DependencyProperty Enable =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(AppClose),
                new UIPropertyMetadata(false, OnEnableChanged));

        public static bool GetEnable(
            UIElement element)
        {
            return (bool?)element?.GetValue(Enable) ?? false;
        }

        private static void OnEnableChanged(
            DependencyObject @object,
            DependencyPropertyChangedEventArgs args)
        {
            if (@object is Button button)
            {
                bool enabled = GetEnable(button);
                if (!enabled)
                    return;

                button.Click +=
                    (__1, __2) => Application.Current.Shutdown();
            }
        }
    }
}
