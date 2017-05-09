// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using System.Windows;
using System.Windows.Media;

using PixelSnapHelpers = ICSharpCode.AvalonEdit.Utils.PixelSnapHelpers;

namespace DotNetEditor.AvalonUtils {

    public static class PixelSnappers
    {
        // Wraps around ICSharpCode.AvalonEdit.Utils.PixelSnapHelpers
        public static Size GetPixelSize(Visual visual)
        {
            return PixelSnapHelpers.GetPixelSize(visual);
        }

        // Aligns the borders of rect to whole pixels.
        public static Rect AlignRectToWholePixel(Rect rect, Size pixelSize)
        {
            return new Rect(PixelSnapHelpers.Round(rect.TopLeft, pixelSize),
                PixelSnapHelpers.Round(rect.BottomRight, pixelSize));
        }
    }
}