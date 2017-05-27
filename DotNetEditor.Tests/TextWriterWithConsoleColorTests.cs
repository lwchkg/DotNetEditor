// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using DotNetEditor.CodeRunner;
using System.Windows.Media;
using Xunit;

namespace DotNetEditor.Tests
{
    // Tests writing and/or capturing console output must not be run in parallel
    // because they use the same console.
    [Collection("Tests using the console")]
    public class TextWriterWithConsoleColorTests
    {
        const string ConsoleOutputLine = Constants.ConsoleOutputLine;

        [Fact]
        public void TextWriterWithConsoleColor()
        {
            ConsoleUtil.CreateHiddenConsoleWindowIfNotExists();

            const string code = @"
Console.ForegroundColor = ConsoleColor.Red;
Console.Write(12);
Console.BackgroundColor = ConsoleColor.Green;
Console.Write(34);
Console.ForegroundColor = ConsoleColor.Blue;
Console.Write(56);
Console.BackgroundColor = ConsoleColor.White;
Console.Write(78);
";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new CSCodeRunner(code, "", output);
            Assert.True(runner.Run(), output.GetText());
            Assert.Equal(ConsoleOutputLine + "12345678", output.GetText());

            Color red = ConsoleColorScheme.DefaultColorScheme.Colors[12];
            Color green = ConsoleColorScheme.DefaultColorScheme.Colors[10];
            Color blue = ConsoleColorScheme.DefaultColorScheme.Colors[9];
            Color white = ConsoleColorScheme.DefaultColorScheme.Colors[15];

            int basePos = ConsoleOutputLine.Length;
            output.AssertTextFormat(basePos, red, null, null, null);
            output.AssertTextFormat(basePos + 1, red, null, null, null);
            output.AssertTextFormat(basePos + 2, red, green, null, null);
            output.AssertTextFormat(basePos + 3, red, green, null, null);
            output.AssertTextFormat(basePos + 4, blue, green, null, null);
            output.AssertTextFormat(basePos + 5, blue, green, null, null);
            output.AssertTextFormat(basePos + 6, blue, white, null, null);
            output.AssertTextFormat(basePos + 7, blue, white, null, null);
        }
    }
}
