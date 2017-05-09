// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using System;

namespace DotNetEditor.TextWriterWithConsoleColor
{
    struct ColorInfo
    {
        public ConsoleColor Foreground;
        public ConsoleColor Background;

        public ColorInfo(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            Foreground = foregroundColor;
            Background = backgroundColor;
        }

        public bool Equals(ColorInfo other)
        {
            return this.Foreground == other.Foreground && this.Background == other.Background;
        }
    }
}
