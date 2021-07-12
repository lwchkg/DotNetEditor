// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using DotNetEditor.CodeRunner;
using System.Threading.Tasks;
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
        const string ReturnValueLine = Constants.ReturnValueLine;
        const string AbortLine = Constants.AbortLine;
        const string Separator = Constants.Separator;

        [Fact]
        public async void RunVBCodeInsideSubMain()
        {
            const string code = "Console.WriteLine(123)";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new VBCodeRunner(code, "", output);
            Assert.True(await runner.Run(), output.GetText());
            Assert.Equal(ConsoleOutputLine + "123\r\n", output.GetText());
        }

        [Fact]
        public async void RunCSCodeInsideSubMain()
        {
            const string code = "Console.WriteLine(123);";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new CSCodeRunner(code, "", output);
            Assert.True(await runner.Run(), output.GetText());
            Assert.Equal(ConsoleOutputLine + "123\r\n", output.GetText());
        }

        [Fact]
        public async void RunVBCodeInModule()
        {
            const string code =
                "Sub Main() \n Console.WriteLine(123) \n End Sub";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new VBCodeRunner(code, "", output);
            Assert.True(await runner.Run(), output.GetText());
            Assert.Equal(ConsoleOutputLine + "123\r\n", output.GetText());
        }

        [Fact]
        public async void RunCSCodeInClass()
        {
            const string code =
                "static void Main() { Console.WriteLine(123); }";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new CSCodeRunner(code, "", output);
            Assert.True(await runner.Run(), output.GetText());
            Assert.Equal(ConsoleOutputLine + "123\r\n", output.GetText());
        }

        [Fact]
        public async void RunVBCodeAsFullFile()
        {
            const string code =
                "Module Module1 \n Sub Main() \n Console.WriteLine(123) \n End Sub \n End Module";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new VBCodeRunner(code, "", output);
            Assert.True(await runner.Run(), output.GetText());
            Assert.Equal(ConsoleOutputLine + "123\r\n", output.GetText());
        }

        [Fact]
        public async void RunCSCodeAsFullFile()
        {
            const string code =
                "using System; class Class1 { static void Main() { Console.WriteLine(123); } }";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new CSCodeRunner(code, "", output);
            Assert.True(await runner.Run(), output.GetText());
            Assert.Equal(ConsoleOutputLine + "123\r\n", output.GetText());
        }

        [Fact]
        public async void RunVBCodeWithDebugOutput()
        {
            const string code = "Debug.WriteLine(456) : Console.WriteLine(123)";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new VBCodeRunner(code, "", output);
            Assert.True(await runner.Run(), output.GetText());
            Assert.Equal(
                ConsoleOutputLine + "123\r\n" + Separator + DebugTraceLine + "456\r\n",
                output.GetText());
        }

        [Fact]
        public async void RunCSCodeWithDebugOutput()
        {
            const string code = "Debug.WriteLine(456); Console.WriteLine(123);";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new CSCodeRunner(code, "", output);
            Assert.True(await runner.Run(), output.GetText());
            Assert.Equal(
                ConsoleOutputLine + "123\r\n" + Separator + DebugTraceLine + "456\r\n",
                output.GetText());
        }

        [Fact]
        public async void RunVBCodeWithTraceOutput()
        {
            const string code = "Debug.WriteLine(456) : Console.WriteLine(123)";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new VBCodeRunner(code, "", output);
            Assert.True(await runner.Run(), output.GetText());
            Assert.Equal(
                ConsoleOutputLine + "123\r\n" + Separator + DebugTraceLine + "456\r\n",
                output.GetText());
        }

        [Fact]
        public async void RunCSCodeWithTraceOutput()
        {
            const string code = "Trace.WriteLine(456); Console.WriteLine(123);";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new CSCodeRunner(code, "", output);
            Assert.True(await runner.Run(), output.GetText());
            Assert.Equal(
                ConsoleOutputLine + "123\r\n" + Separator + DebugTraceLine + "456\r\n",
                output.GetText());
        }

        [Fact]
        public async void ReturnCodeVB()
        {
            const string code = "Function Main() As Integer \n Return 2 \nEnd Function";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new VBCodeRunner(code, "", output);

            Assert.True(await runner.Run(), output.GetText());
            Assert.Equal(ConsoleOutputLine + Separator + ReturnValueLine + "2\n", output.GetText());
        }

        [Fact]
        public async void ReturnCodeCS()
        {
            const string code = "static int Main() { return 2; }";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new CSCodeRunner(code, "", output);

            Assert.True(await runner.Run(), output.GetText());
            Assert.Equal(ConsoleOutputLine + Separator + ReturnValueLine + "2\n", output.GetText());
        }

        [Fact]
        public async void TerminateVBCodeAtStart()
        {
            const string code = "Console.WriteLine(123)";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new VBCodeRunner(code, "", output);
            Task<bool> task = runner.Run();
            runner.Terminate();

            Assert.False(await task, output.GetText());
            Assert.True(output.GetText().Contains(AbortLine), output.GetText());
        }

        [Fact]
        public async void TerminateVBCodeAtMiddle()
        {
            const string code = "Console.WriteLine(123) : DO : LOOP";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new VBCodeRunner(code, "", output);
            Task<bool> task = runner.Run();
            await Task.Delay(100);
            runner.Terminate();

            Assert.False(await task, output.GetText());
            Assert.Equal(ConsoleOutputLine + "123\r\n" + Separator + AbortLine, output.GetText());
        }

        [Fact]
        public async void TerminateCSCodeAtStart()
        {
            const string code = "Console.WriteLine(123);";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new CSCodeRunner(code, "", output);
            Task<bool> task = runner.Run();
            runner.Terminate();

            Assert.False(await task, output.GetText());
            Assert.True(output.GetText().Contains(AbortLine), output.GetText());
        }

        [Fact]
        public async void TerminateCSCodeAtMiddle()
        {
            const string code = "Console.WriteLine(123); while (true) ;";

            TestCodeRunnerOutput output = new TestCodeRunnerOutput();
            CodeRunnerBase runner = new CSCodeRunner(code, "", output);
            Task<bool> task = runner.Run();
            await Task.Delay(100);
            runner.Terminate();

            Assert.False(await task, output.GetText());
            Assert.Equal(ConsoleOutputLine + "123\r\n" + Separator + AbortLine, output.GetText());
        }
    }
}
