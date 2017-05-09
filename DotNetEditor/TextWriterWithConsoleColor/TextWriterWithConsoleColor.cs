using System;

namespace DotNetEditor.TextWriterWithConsoleColor
{
    abstract class TextWriterWithConsoleColor :
        System.IO.TextWriter, IWithConsoleColor
    {
        public abstract void ForEachPart(Action<string, ColorInfo> callback);
    }
}
