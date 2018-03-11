using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Pact.Behaviors
{
    public sealed class DeckDragBehavior
        : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            //AssociatedObject.AllowDrop = true;
            //AssociatedObject.DragEnter += AssociatedObject_DragEnter;
            //AssociatedObject.DragOver += AssociatedObject_DragOver;
            //AssociatedObject.DragLeave += AssociatedObject_DragLeave;
            //AssociatedObject.Drop += AssociatedObject_Drop;
            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            AssociatedObject.GiveFeedback += AssociatedObject_GiveFeedback;
            AssociatedObject.QueryContinueDrag += AssociatedObject_QueryContinueDrag;
        }

        // Probably not the actual list of event handlers I'll need; will need to review use case for drag source vs. drop target
        // (These are originally from an example for a drop target)
        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void AssociatedObject_DragLeave(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            e.Handled = true;

            //System.Diagnostics.Debug.WriteLine("DragEnter");
        }

        private Point _dragStartPoint;
        private int _originalZIndex;

        private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _dragStartPoint = Mouse.GetPosition(Application.Current.MainWindow);
                _originalZIndex = System.Windows.Controls.Panel.GetZIndex(AssociatedObject);
                DependencyObject d = VisualTreeHelper.GetParent(AssociatedObject);
                d.SetValue(System.Windows.Controls.Panel.ZIndexProperty, int.MaxValue);
                DragDrop.DoDragDrop(AssociatedObject, "test", DragDropEffects.Move);
            }

            e.Handled = true;
        }

        private void AssociatedObject_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            e.Handled = true;

            //System.Diagnostics.Debug.WriteLine("Feedback");

            //AssociatedObject.RenderTransform = new System.Windows.Media.TranslateTransform(100, 100);

            Point currentPoint = MouseUtilities.GetMousePosition(Application.Current.MainWindow);

            AssociatedObject.RenderTransform = new TranslateTransform(0, currentPoint.Y - _dragStartPoint.Y);
        }

        private void AssociatedObject_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(e.Action);

            if ((e.KeyStates & DragDropKeyStates.LeftMouseButton) == 0)
            {
                System.Diagnostics.Debug.WriteLine("Drag stopped");

                AssociatedObject.RenderTransform = null;
                DependencyObject d = VisualTreeHelper.GetParent(AssociatedObject);
                d.SetValue(System.Windows.Controls.Panel.ZIndexProperty, _originalZIndex);
            }

            //e.Handled = true;

            //System.Diagnostics.Debug.WriteLine("Feedback");

            //AssociatedObject.RenderTransform = new System.Windows.Media.TranslateTransform(100, 100);

            //System.Diagnostics.Debug.WriteLine(Mouse.GetPosition(Application.Current.MainWindow));
        }
    }

    public class MouseUtilities
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Win32Point pt);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hwnd, ref Win32Point pt);

        public static Point GetMousePosition(Visual relativeTo)
        {
            Win32Point mouse = new Win32Point();
            GetCursorPos(ref mouse);

            System.Windows.Interop.HwndSource presentationSource =
                (System.Windows.Interop.HwndSource)PresentationSource.FromVisual(relativeTo);

            ScreenToClient(presentationSource.Handle, ref mouse);

            GeneralTransform transform = relativeTo.TransformToAncestor(presentationSource.RootVisual);

            Point offset = transform.Transform(new Point(0, 0));

            return new Point(mouse.X - offset.X, mouse.Y - offset.Y);
        }
    }
}
