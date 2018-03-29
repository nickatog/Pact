using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Pact
{
    public class OutlineTextControl
        : FrameworkElement
    {
        private const string LETTERS = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz1234567890";

        protected override Size MeasureOverride(Size availableSize)
        {
            var minWidthArgs = (MinWidthText, Font, FontSize, Bold, Italic, Stroke, StrokeThickness);
            double minWidth = GetTextGeometrySize(minWidthArgs).Width;

            var maxHeightArgs = (LETTERS, Font, FontSize, Bold, Italic, Stroke, StrokeThickness);
            double maxHeight = GetTextGeometrySize(maxHeightArgs).Height + FontSize / 8;

            var actualSizeArgs = (Text, Font, FontSize, Bold, Italic, Stroke, StrokeThickness);
            double calculatedWidth = GetTextGeometrySize(actualSizeArgs).Width;

            return new Size(Math.Max(calculatedWidth, Math.Max(minWidth, 0)), Math.Max(maxHeight, 0));
        }

        private static readonly IDictionary<(string, FontFamily, double, bool, bool, Brush, ushort), Geometry> _getTextGeometry_Cache =
            new Dictionary<(string, FontFamily, double, bool, bool, Brush, ushort), Geometry>();

        private static Geometry GetTextGeometry((string, FontFamily, double, bool, bool, Brush, ushort) args)
        {
            if (_getTextGeometry_Cache.TryGetValue(args, out Geometry cachedValue))
                return cachedValue;

            var formattedText =
                new FormattedText(
                    args.Item1,
                    CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    new Typeface(
                        args.Item2,
                        args.Item5 ? FontStyles.Italic : FontStyles.Normal,
                        args.Item4 ? FontWeights.Bold : FontWeights.Medium,
                        FontStretches.Normal),
                    args.Item3,
                    Brushes.Black);

            Geometry textGeometry = formattedText.BuildGeometry(new Point(0, 0));

            _getTextGeometry_Cache.Add(args, textGeometry);

            return textGeometry;
        }

        private static readonly IDictionary<(string, FontFamily, double, bool, bool, Brush, ushort), Size> _getTextGeometrySize_Cache =
            new Dictionary<(string, FontFamily, double, bool, bool, Brush, ushort), Size>();

        private static Size GetTextGeometrySize((string, FontFamily, double, bool, bool, Brush, ushort) args)
        {
            if (_getTextGeometrySize_Cache.TryGetValue(args, out Size cachedValue))
                return cachedValue;

            Size textGeometrySize = GetTextGeometry(args).GetRenderBounds(new Pen(args.Item6, args.Item7), 0, ToleranceType.Absolute).Size;

            _getTextGeometrySize_Cache.Add(args, textGeometrySize);

            return textGeometrySize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var textGeometryArgs = (Text, Font, FontSize, Bold, Italic, Stroke, StrokeThickness);
            Geometry textGeometry = GetTextGeometry(textGeometryArgs);

            drawingContext.DrawGeometry(Fill, new Pen(Stroke, StrokeThickness), textGeometry);
            drawingContext.DrawGeometry(Fill, new Pen(Fill, 0), textGeometry);
        }

        #region Dependency Properties
        public bool Bold
        {
            get { return (bool)GetValue(BoldProperty); }
            set { SetValue(BoldProperty, value); }
        }

        public static readonly DependencyProperty BoldProperty =
            DependencyProperty.Register(
                "Bold",
                typeof(bool),
                typeof(OutlineTextControl),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    null,
                    null));

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register(
                "Fill",
                typeof(Brush),
                typeof(OutlineTextControl),
                new FrameworkPropertyMetadata(
                    new SolidColorBrush(Colors.White),
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    null,
                    null));

        public FontFamily Font
        {
            get { return (FontFamily)GetValue(FontProperty); }
            set { SetValue(FontProperty, value); }
        }

        public static readonly DependencyProperty FontProperty =
            DependencyProperty.Register(
                "Font",
                typeof(FontFamily),
                typeof(OutlineTextControl),
                new FrameworkPropertyMetadata(
                    new FontFamily("Arial"),
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    null,
                    null));

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register(
                "FontSize",
                typeof(double),
                typeof(OutlineTextControl),
                new FrameworkPropertyMetadata(
                    48.0,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    null,
                    null));

        public bool Italic
        {
            get { return (bool)GetValue(ItalicProperty); }
            set { SetValue(ItalicProperty, value); }
        }

        public static readonly DependencyProperty ItalicProperty =
            DependencyProperty.Register(
                "Italic",
                typeof(bool),
                typeof(OutlineTextControl),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    null,
                    null));

        public string MinWidthText
        {
            get { return (string)GetValue(MinWidthTextProperty); }
            set { SetValue(MinWidthTextProperty, value); }
        }

        public static readonly DependencyProperty MinWidthTextProperty =
            DependencyProperty.Register(
                "MinWidthText",
                typeof(string),
                typeof(OutlineTextControl),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    null,
                    null));

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(
                "Stroke",
                typeof(Brush),
                typeof(OutlineTextControl),
                new FrameworkPropertyMetadata(
                    new SolidColorBrush(Colors.Teal),
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    null,
                    null));

        public ushort StrokeThickness
        {
            get { return (ushort)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                "StrokeThickness",
                typeof(ushort),
                typeof(OutlineTextControl),
                new FrameworkPropertyMetadata(
                    (ushort)0,
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    null,
                    null));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(OutlineTextControl),
                new FrameworkPropertyMetadata(
                     string.Empty,
                     FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                     null,
                     null));
    }
    #endregion // Dependency Properties
}
