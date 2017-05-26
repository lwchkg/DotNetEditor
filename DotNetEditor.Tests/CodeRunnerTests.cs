// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using DotNetEditor.CodeRunner;
using Xunit;

namespace DotNetEditor.Tests
{
    // Tests writing and/or capturing console output must not be run in parallel
    // because they use the same console.
    [Collection("Tests using the console")]
    public class CodeRunnerTests
    {
        const string ConsoleOutputLine = Constants.ConsoleOutputLine;
        const string DebugTraceLine = Constants.DebugTraceLine;
        const string Separator = Constants.Separator;

        [Fact]
        public void RunVBCodeInsideSubMain()
        {
            const string code = "Console.WriteLine(123)";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new VBCodeRunner(code, "", output);
            Assert.True(runner.Run(), output.GetText());
            Assert.Equal(ConsoleOutputLine + "123\r\n", output.GetText());
        }

        [Fact]
        public void RunCSCodeInsideSubMain()
        {
            const string code = "Console.WriteLine(123);";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new CSCodeRunner(code, "", output);
            Assert.True(runner.Run(), output.GetText());
            Assert.Equal(ConsoleOutputLine + "123\r\n", output.GetText());
        }

        [Fact]
        public void RunVBCodeInModule()
        {
            const string code =
                "Sub Main() \n Console.WriteLine(123) \n End Sub";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new VBCodeRunner(code, "", output);
            Assert.True(runner.Run(), output.GetText());
            Assert.Equal(ConsoleOutputLine + "123\r\n", output.GetText());
        }

        [Fact]
        public void RunCSCodeInClass()
        {
            const string code =
                "static void Main() { Console.WriteLine(123); }";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new CSCodeRunner(code, "", output);
            Assert.True(runner.Run(), output.GetText());
            Assert.Equal(ConsoleOutputLine + "123\r\n", output.GetText());
        }

        [Fact]
        public void RunVBCodeAsFullFile()
        {
            const string code =
                "Module Module1 \n Sub Main() \n Console.WriteLine(123) \n End Sub \n End Module";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new VBCodeRunner(code, "", output);
            Assert.True(runner.Run(), output.GetText());
            Assert.Equal(ConsoleOutputLine + "123\r\n", output.GetText());
        }

        [Fact]
        public void RunCSCodeAsFullFile()
        {
            const string code =
                "using System; class Class1 { static void Main() { Console.WriteLine(123); } }";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new CSCodeRunner(code, "", output);
            Assert.True(runner.Run(), output.GetText());
            Assert.Equal(ConsoleOutputLine + "123\r\n", output.GetText());
        }

        [Fact]
        public void RunVBCodeWithDebugOutput()
        {
            const string code = "Debug.WriteLine(456) : Console.WriteLine(123)";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new VBCodeRunner(code, "", output);
            Assert.True(runner.Run(), output.GetText());
            Assert.Equal(
                ConsoleOutputLine + "123\r\n" + Separator + DebugTraceLine + "456\r\n",
                output.GetText());
        }

        [Fact]
        public void RunCSCodeWithDebugOutput()
        {
            const string code = "Debug.WriteLine(456); Console.WriteLine(123);";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new CSCodeRunner(code, "", output);
            Assert.True(runner.Run(), output.GetText());
            Assert.Equal(
                ConsoleOutputLine + "123\r\n" + Separator + DebugTraceLine + "456\r\n",
                output.GetText());
        }

        [Fact]
        public void RunVBCodeWithTraceOutput()
        {
            const string code = "Debug.WriteLine(456) : Console.WriteLine(123)";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new VBCodeRunner(code, "", output);
            Assert.True(runner.Run(), output.GetText());
            Assert.Equal(
                ConsoleOutputLine + "123\r\n" + Separator + DebugTraceLine + "456\r\n",
                output.GetText());
        }

        [Fact]
        public void RunCSCodeWithTraceOutput()
        {
            const string code = "Trace.WriteLine(456); Console.WriteLine(123);";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new CSCodeRunner(code, "", output);
            Assert.True(runner.Run(), output.GetText());
            Assert.Equal(
                ConsoleOutputLine + "123\r\n" + Separator + DebugTraceLine + "456\r\n",
                output.GetText());
        }
    }
}
