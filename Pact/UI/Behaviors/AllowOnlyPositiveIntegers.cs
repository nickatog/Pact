using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Pact.Behaviors
{
    public sealed class AllowOnlyPositiveIntegers
        : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewTextInput += PreviewTextInput;
            AssociatedObject.PreviewKeyDown += PreviewKeyDown;
        }

        private void PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back && (AssociatedObject.Text.Length == 1 || !ResultingTextIsInteger()))
            {
                AssociatedObject.Text = "0";
                AssociatedObject.CaretIndex = 1;

                e.Handled = true;
            }
        }

        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ResultingTextIsInteger(e.Text);
        }

        private bool ResultingTextIsInteger(string input = null)
        {
            int startIndex = AssociatedObject.SelectionStart;

            IEnumerable<char> prefix = AssociatedObject.Text.Take(startIndex);
            IEnumerable<char> suffix = AssociatedObject.Text.Skip(startIndex + AssociatedObject.SelectionLength);

            string previewText = new string(prefix.Concat(input ?? string.Empty).Concat(suffix).ToArray());

            return int.TryParse(previewText, out int _);
        }
    }
}
