// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace DotNetEditor
{
    // DumpReader is a TextReader that dumps input into the output

    // Initialization: New DumpReader(reader, writer)
    //     reader: the TextReader to read from
    //     writer: the TextWriter that the read operations dump to
    class DumpReader : TextReader
    {
        const ConsoleColor DumpForegroundColor = System.ConsoleColor.Black;
        const ConsoleColor DumpBackgroundColor = System.ConsoleColor.Gray;

        TextReader _reader;
        TextWriter _writer;

        public DumpReader(TextReader reader, TextWriter writer)
        {
            _reader = reader;
            _writer = writer;
        }

        public override int Peek()
        {
            return _reader.Peek();
        }

        public override int Read()
        {
            ConsoleColor fg = Console.ForegroundColor;
            ConsoleColor bg = Console.BackgroundColor;

            Console.ForegroundColor = DumpForegroundColor;
            Console.BackgroundColor = DumpBackgroundColor;

            int c = _reader.Read();
            if (c >= 0)
                _writer.Write(Convert.ToChar(c));

            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;

            return c;
        }

        public override string ReadLine()
        {
            ConsoleColor fg = Console.ForegroundColor;
            ConsoleColor bg = Console.BackgroundColor;

            Console.ForegroundColor = DumpForegroundColor;
            Console.BackgroundColor = DumpBackgroundColor;

            string s = _reader.ReadLine();
            _writer.WriteLine(s);

            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;

            return s;
        }
    }
}
