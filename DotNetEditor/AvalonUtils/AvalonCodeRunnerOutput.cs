// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

[assembly: InternalsVisibleToAttribute("DotNetEditor.Tests")]

namespace DotNetEditor
{
    // This is the output area of a code runner, which is a customized AvalonEdit.TextEditor control
    // with the ability to store formatting information with text.
    class AvalonCodeRunnerOutput :
        ICSharpCode.AvalonEdit.TextEditor, CodeRunner.ICodeRunnerOutput
    {
        internal class TextFormat : TextSegment
        {
            public Color ForegroundColor = Brushes.LightGray.Color;
            public Color BackgroundColor = Brushes.Black.Color;
            public bool? IsBold;
            public bool? IsItalic;
        }

        private class RTBBackgroundRenderer : IBackgroundRenderer
        {
            private readonly TextSegmentCollection<TextFormat> _formatData;

            public KnownLayer Layer { get; } = KnownLayer.Background;

            public RTBBackgroundRenderer(TextSegmentCollection<TextFormat> formatData)
            {
                _formatData = formatData;
            }

            public void Draw(TextView textView, DrawingContext drawingContext)
            {
                if (textView == null)
                {
                    throw new ArgumentNullException(nameof(textView));
                }
                if (drawingContext == null)
                {
                    throw new ArgumentNullException(nameof(drawingContext));
                }

                if (_formatData == null || !textView.VisualLinesValid)
                {
                    return;
                }

                var visualLines = textView.VisualLines;
                if (visualLines.Count == 0)
                {
                    return;
                }

                int viewStart = visualLines.First().FirstDocumentLine.Offset;
                int viewEnd = visualLines.Last().LastDocumentLine.EndOffset;

                Size pixelSize = AvalonUtils.PixelSnappers.GetPixelSize(textView);

                foreach (var segment in
                    _formatData.FindOverlappingSegments(viewStart, viewEnd - viewStart))
                {
                    Brush brush = new SolidColorBrush(segment.BackgroundColor);

                    foreach (Rect r in
                        BackgroundGeometryBuilder.GetRectsForSegment(textView, segment, false))
                    {
                        // Skip rectangles with width less than two pixels.
                        // These rectangles shouldn't be there.
                        if (r.Width < 2)
                        {
                            continue;
                        }
                        drawingContext.DrawRectangle(brush, null,
                            AvalonUtils.PixelSnappers.AlignRectToWholePixel(r, pixelSize));
                    }
                }
            }
        }

        class RTBColorizer : DocumentColorizingTransformer
        {
            private readonly TextSegmentCollection<TextFormat> _formatData;

            public RTBColorizer(TextSegmentCollection<TextFormat> formatData)
            {
                _formatData = formatData;
            }

            private static void FormatVisualLineElement(TextFormat seg, VisualLineElement element)
            {
                element.TextRunProperties.SetForegroundBrush(
                    new SolidColorBrush(seg.ForegroundColor));

                if (!seg.IsBold.HasValue && !seg.IsItalic.HasValue)
                {
                    return;
                }

                Typeface tf = element.TextRunProperties.Typeface;

                FontStyle style =
                    seg.IsItalic.HasValue
                        ? (seg.IsItalic == true ? FontStyles.Italic : FontStyles.Normal)
                        : tf.Style;

                FontWeight weight =
                    seg.IsBold.HasValue
                        ? (seg.IsBold == true ? FontWeights.Bold : FontWeights.Regular)
                        : tf.Weight;

                element.TextRunProperties.SetTypeface(
                    new Typeface(tf.FontFamily, style, weight, tf.Stretch));
            }

            protected override void ColorizeLine(DocumentLine line)
            {
                TextSegment segment = new TextSegment();
                segment.StartOffset = line.Offset;
                segment.EndOffset = line.EndOffset;

                var segments = _formatData.FindOverlappingSegments(segment);
                foreach (var seg in segments)
                {
                    base.ChangeLinePart(Math.Max(seg.StartOffset, line.Offset),
                                        Math.Min(seg.EndOffset, line.EndOffset),
                                        (element) => FormatVisualLineElement(seg, element));
                }
            }
        }

        private TextSegmentCollection<TextFormat> formatInfo;

        public AvalonCodeRunnerOutput()
        {
            formatInfo = new TextSegmentCollection<TextFormat>(Document);
            TextArea.TextView.LineTransformers.Add(new RTBColorizer(formatInfo));
            TextArea.TextView.BackgroundRenderers.Add(new RTBBackgroundRenderer(formatInfo));
        }

        private void SetStyle(int startOffset, int endOffset,
            Color foregroundColor, Color backgroundColor, bool? IsBold, bool? IsItalic)
        {
            TextFormat fmt = new TextFormat();
            fmt.StartOffset = startOffset;
            fmt.EndOffset = endOffset;
            fmt.ForegroundColor = foregroundColor;
            fmt.BackgroundColor = backgroundColor;
            fmt.IsBold = IsBold;
            fmt.IsItalic = IsItalic;

            formatInfo.Add(fmt);
        }

        #region ICodeRunnerOutput
        public void AppendTextWithStyle(string text, Color foregroundColor, Color backgroundColor,
            bool? isBold, bool? isItalic)
        {
            int start = Text.Length;
            AppendText(text);
            SetStyle(start, Text.Length, foregroundColor, backgroundColor, isBold, isItalic);
        }

        // void AppendText(string text);  (already implemented by AvalonEdit.TextEditor)

        public void ClearOutput()
        {
            Clear();
            formatInfo.Clear();
        }

        public bool IsEmpty()
        {
            return !(Document?.TextLength > 0);
        }
        #endregion
    }
}
