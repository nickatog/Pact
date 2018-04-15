using System.Windows;
using System.Windows.Controls;

namespace Pact.Behaviors
{
    public static class CollapseOnClickBehavior
    {
        public static readonly DependencyProperty ToggleButton =
            DependencyProperty.RegisterAttached(
                "ToggleButton",
                typeof(Button),
                typeof(CollapseOnClickBehavior),
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
                    (__1, __2) => Panel.SetZIndex(element, -1);
            }
        }
    }

    public static class ShowOnClickBehavior
    {
        public static readonly DependencyProperty ToggleButton =
            DependencyProperty.RegisterAttached(
                "ToggleButton",
                typeof(Button),
                typeof(ShowOnClickBehavior),
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
                    (__1, __2) => Panel.SetZIndex(element, 1);
            }
        }
    }

    public static class SetVisibleOnClickBehavior
    {
        public static readonly DependencyProperty ToggleButton =
            DependencyProperty.RegisterAttached(
                "ToggleButton",
                typeof(Button),
                typeof(SetVisibleOnClickBehavior),
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

                button.Click += (__1, __2) => element.Visibility = Visibility.Visible;
            }
        }
    }

    public static class SetCollapsedOnClickBehavior
    {
        public static readonly DependencyProperty ToggleButton =
            DependencyProperty.RegisterAttached(
                "ToggleButton",
                typeof(Button),
                typeof(SetCollapsedOnClickBehavior),
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

                button.Click += (__1, __2) => element.Visibility = Visibility.Collapsed;
            }
        }
    }

    public static class SelectAllOnClickBehavior
    {
        public static readonly DependencyProperty ToggleButton =
            DependencyProperty.RegisterAttached(
                "ToggleButton",
                typeof(Button),
                typeof(SelectAllOnClickBehavior),
                new UIPropertyMetadata(null, OnToggleButtonChanged));

        public static Button GetToggleButton(UIElement element)
        {
            return (Button)element.GetValue(ToggleButton);
        }

        private static void OnToggleButtonChanged(DependencyObject @object, DependencyPropertyChangedEventArgs args)
        {
            if (@object is TextBox textBox)
            {
                Button button = GetToggleButton(textBox);
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
