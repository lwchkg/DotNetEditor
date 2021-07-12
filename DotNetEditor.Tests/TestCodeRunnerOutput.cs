// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using ICSharpCode.AvalonEdit.Document;
using System.Text;
using System.Windows.Media;
using Xunit;

namespace DotNetEditor.Tests
{
    public class TestCodeRunnerOutput : CodeRunner.ICodeRunnerOutput
    {
        public TestCodeRunnerOutput()
        {
            _text = new StringBuilder();
            _format = new TextSegmentCollection<AvalonCodeRunnerOutput.TextFormat>();
        }

        public void AppendText(string text)
        {
            _text.Append(text);
        }

        public void AppendTextWithStyle(string text, Color foregroundColor, Color backgroundColor,
                                        bool? isBold, bool? isItalic)
        {
            int startOffset = _text.Length;
            _text.Append(text);
            int endOffset = _text.Length;
            AddFormatData(startOffset, endOffset, foregroundColor, backgroundColor, isBold, isItalic);
        }

        public string GetText()
        {
            return _text.ToString();
        }

        public void AssertTextFormat(int offset, Color? foregroundColor, Color? backgroundColor,
            bool? IsBold, bool? IsItalic)
        {
            Color? actualForegroundColor = null;
            Color? actualBackgroundColor = null;
            bool actualIsBold = false;
            bool actualIsItalic = false;
            foreach (var segment in _format.FindSegmentsContaining(offset))
            {
                actualForegroundColor = segment.ForegroundColor;
                actualBackgroundColor = segment.BackgroundColor;
                actualIsBold = segment.IsBold ?? actualIsBold;
                actualIsItalic = segment.IsItalic ?? actualIsItalic;
            }

            if (foregroundColor.HasValue)
            {
                Assert.Equal(foregroundColor, actualForegroundColor);
            }
            if (backgroundColor.HasValue)
            {
                Assert.Equal(backgroundColor, actualBackgroundColor);
            }
            if (IsBold.HasValue)
            {
                Assert.Equal(IsBold, actualIsBold);
            }
            if (IsItalic.HasValue)
            {
                Assert.Equal(IsItalic, actualIsItalic);
            }
            return;
        }

        public void ClearOutput()
        {
            _text.Clear();
            _format.Clear();
        }

        public bool IsEmpty()
        {
            return _text.Length == 0;
        }

        private void AddFormatData(int startOffset, int endOffset,
            Color foregroundColor, Color backgroundColor, bool? IsBold, bool? IsItalic)
        {
            AvalonCodeRunnerOutput.TextFormat fmt = new AvalonCodeRunnerOutput.TextFormat();
            fmt.StartOffset = startOffset;
            fmt.EndOffset = endOffset;
            fmt.ForegroundColor = foregroundColor;
            fmt.BackgroundColor = backgroundColor;
            fmt.IsBold = IsBold;
            fmt.IsItalic = IsItalic;

            _format.Add(fmt);
        }

        private StringBuilder _text;
        private TextSegmentCollection<AvalonCodeRunnerOutput.TextFormat> _format;
    }
}
