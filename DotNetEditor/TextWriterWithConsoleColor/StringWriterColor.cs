// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetEditor.TextWriterWithConsoleColor
{
    class StringWriterColor : TextWriterWithConsoleColor
    {
        struct ColorSegment
        {
            public int Start;
            public ColorInfo Color;
            public ColorSegment(int start, ColorInfo color)
            {
                Start = start;
                Color = color;
            }
        }

        private StringBuilder _buffer = new StringBuilder();
        private ColorInfo currentColor;
        private List<ColorSegment> colorData = new List<ColorSegment>();

        public override Encoding Encoding { get; } = Encoding.Unicode;

        void WriteImpl<T>(T value)
        {
            ColorInfo color =
                new ColorInfo(Console.ForegroundColor, Console.BackgroundColor);
            int start = _buffer.Length;
            if (start == 0 || !color.Equals(currentColor))
            {
                colorData.Add(new ColorSegment(start, color));
                currentColor = color;
            }

            _buffer.Append(value);
        }

        public override void Write(char value)
        {
            WriteImpl(value);
        }

        public override void Write(string value)
        {
            WriteImpl(value);
        }

        public override void ForEachPart(Action<string, ColorInfo> callback)
        {
            int length = colorData.Count;
            if (length == 0)
                return;

            for (int i = 0; i < length - 1; ++i)
            {
                callback(_buffer.ToString(
                             colorData[i].Start,
                             colorData[i+1].Start - colorData[i].Start),
                             colorData[i].Color);
            }

            callback(_buffer.ToString(
                         colorData.Last().Start,
                         _buffer.Length - colorData.Last().Start),
                         colorData.Last().Color);
        }

        public override string ToString()
        {
            return _buffer.ToString();
        }
    }
}
