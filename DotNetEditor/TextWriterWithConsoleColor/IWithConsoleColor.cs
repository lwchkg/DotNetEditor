using System;

namespace DotNetEditor.TextWriterWithConsoleColor
{
    interface IWithConsoleColor
    {
        void ForEachPart(Action<string, ColorInfo> callback);
    }
}
