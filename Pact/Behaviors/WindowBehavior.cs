using System.Windows;

namespace Pact.Behaviors
{
    public static class WindowBehavior
    {
        public static readonly DependencyProperty Close =
            DependencyProperty.RegisterAttached(
                "Close",
                typeof(bool?),
                typeof(WindowBehavior),
                new UIPropertyMetadata(null, OnCloseChanged));

        public static bool? GetClose(Window window)
        {
            return (bool?)window.GetValue(Close);
        }

        public static void SetClose(Window window, bool? value)
        {
            window.SetValue(Close, value);
        }

        private static void OnCloseChanged(DependencyObject @object, DependencyPropertyChangedEventArgs args)
        {
            if (@object is Window window)
            {
                // move to view command itself? view will be able to close itself without a behavior
                window.DialogResult = GetClose(window);

                window.Close();
            }
        }
    }
}
