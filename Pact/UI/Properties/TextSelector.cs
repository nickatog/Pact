using System.Windows;
using System.Windows.Controls;

namespace Pact.Properties
{
    public static class TextSelector
    {
        public static readonly DependencyProperty Button =
            DependencyProperty.RegisterAttached(
                "Button",
                typeof(Button),
                typeof(TextSelector),
                new UIPropertyMetadata(null, OnButtonChanged));

        public static Button GetButton(
            UIElement element)
        {
            return (Button)element?.GetValue(Button);
        }

        private static void OnButtonChanged(
            DependencyObject @object,
            DependencyPropertyChangedEventArgs args)
        {
            if (@object is TextBox textBox)
            {
                Button button = GetButton(textBox);
                if (button == null)
                    return;

                button.Click +=
                    (__1, __2) =>
                    {
                        textBox.Focus();
                        textBox.SelectAll();
                    };
            }
        }
    }
}
