// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using System.Runtime.CompilerServices;
using System.Windows.Media;

[assembly: InternalsVisibleToAttribute("DotNetEditor.Tests")]

namespace DotNetEditor.CodeRunner
{
    interface ICodeRunnerOutput
    {
        // Append text to the output with the specified style.
        void AppendTextWithStyle(string text, Color foregroundColor, Color backgroundColor,
                                 bool? isBold, bool? isItalic);

        // Append text to the output with the default style.
        void AppendText(string text);

        // Remove all text and style from the output.
        void ClearOutput();

        // Return if the output is empty.
        bool IsEmpty();
    }
}
