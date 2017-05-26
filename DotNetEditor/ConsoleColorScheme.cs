// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using System.Windows.Media;

[assembly: InternalsVisibleToAttribute("DotNetEditor.Tests")]

namespace DotNetEditor
{
    class ConsoleColorScheme
    {
        private static readonly Color[] DefaultColors = {
                Brushes.Black.Color,
                Brushes.Navy.Color,
                Brushes.Green.Color,
                Brushes.Teal.Color,
                Brushes.Maroon.Color,
                Brushes.Purple.Color,
                Brushes.Olive.Color,
                Brushes.Silver.Color,
                Brushes.Gray.Color,
                Brushes.Blue.Color,
                Brushes.Lime.Color,
                Brushes.Cyan.Color,
                Brushes.Red.Color,
                Brushes.Magenta.Color,
                Brushes.Yellow.Color,
                Brushes.White.Color
        };

        [Serializable]
        public class ColorCountException : Exception
        {
            public ColorCountException()
                : base("The color scheme must have exactly 16 colors.") {}

            public ColorCountException(string message)
                : base(message) {}

            public ColorCountException(string message, Exception inner)
                : base(message, inner) {}
        }

        private Color[] _colors;
        public Color[] Colors {
            get { return _colors; }
            set
            {
                if (value.Length != 16)
                {
                    throw new ColorCountException();
                }

                _colors = value;
            }
        }

        public ConsoleColorScheme(Color[] colors)
        {
            Colors = colors;
        }

        public static readonly ConsoleColorScheme DefaultColorScheme =
            new ConsoleColorScheme(DefaultColors);

        public Color ConvertColor(ConsoleColor color)
        {
            return Colors[Convert.ToInt32(color)];
        }
    }
}
