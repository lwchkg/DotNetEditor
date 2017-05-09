// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using System;
using System.Text;

namespace DotNetEditor.TextWriterWithConsoleColor
{
    class StringWriterBW : TextWriterWithConsoleColor
    {
        private StringBuilder _buffer = new StringBuilder();

        public override Encoding Encoding { get; } = Encoding.Unicode;

        public override void Write(char value)
        {
            _buffer.Append(value);
        }

        public override void Write(string value)
        {
            _buffer.Append(value);
        }

        public override void ForEachPart(Action<string, ColorInfo> callback)
        {
            callback(this.ToString(),
                new ColorInfo(ConsoleColor.Gray, ConsoleColor.Black));
        }

        public override string ToString()
        {
            return _buffer.ToString();
        }
    }
}
