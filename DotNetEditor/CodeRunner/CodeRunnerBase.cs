// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

[assembly: InternalsVisibleToAttribute("DotNetEditor.Tests")]

namespace DotNetEditor.CodeRunner
{
    abstract class CodeRunnerBase
    {
        public static readonly string[] DefaultNSImports = {
            "Microsoft.VisualBasic",
            "System",
            "System.Diagnostics",
            "System.Drawing",
            "System.Windows.Forms"
        };

        public static readonly string[] DefaultAssemblyImports = {
            "System.dll",
            "System.Drawing.dll",
            "System.Windows.Forms.dll"
        };

        public string Code { get; set; }
        public string InputData { get; set; }
        public string[] NSImports { get; set; }
        public string[] AssemblyImports { get; set; }
        public bool ColoredOutput { get; set; } = true;
        public bool DumpInput { get; set; } = true;
        public ConsoleColorScheme ColorScheme { get; set; } = ConsoleColorScheme.DefaultColorScheme;

        private ICodeRunnerOutput _outputArea;
        private Thread _thread;

        private TextWriterWithConsoleColor.TextWriterWithConsoleColor _consoleWriter;
        private System.IO.StringWriter _debugWriter;

        // Results returned by the thread. Do not read them while the thread is alive.
        private object _result = null;
        private Exception _exStore = null;
        private bool _hasError = false;

        // Default text colors
        readonly Color defaultTextBgColor = Brushes.Black.Color;
        readonly Color defaultTextFgColor = Brushes.LightGray.Color;
        readonly Color defaultTitleBgColor = Brushes.LightGray.Color;
        readonly Color defaultTitleFgColor = Brushes.Black.Color;

        // TODO: constants to be abstracted
        readonly Color errorFgColor = Brushes.Red.Color;

        protected CodeRunnerBase(string code, string inputData, ICodeRunnerOutput outputArea)
        {
            Code = code;
            InputData = inputData;
            NSImports = DefaultNSImports;
            AssemblyImports = DefaultAssemblyImports;
            _outputArea = outputArea;
        }

        public void SetOutputArea(ICodeRunnerOutput outputArea)
        {
            _outputArea = outputArea;
        }

        void AppendStyledText(string s, Color foregroundColor, Color backgroundColor,
                              bool? isBold, bool? isItalic)
        {
            _outputArea.AppendTextWithStyle(s, foregroundColor, backgroundColor, isBold, isItalic);
        }

        void AppendConsoleColoredText(string s, TextWriterWithConsoleColor.ColorInfo color)
        {
            AppendStyledText(s, ColorScheme.ConvertColor(color.Foreground),
                             ColorScheme.ConvertColor(color.Background), false, false);
        }

        void AppendResult(string mainText, string title,
                          Color? textFgColor = null, Color? textBgColor = null,
                          Color? titleFgColor = null, Color? titleBgColor = null)
        {
            if (!_outputArea.IsEmpty())
            {
                _outputArea.AppendText("\n\n");
            }

            AppendStyledText(" --- " + title + " --- \n",
                             titleFgColor ?? defaultTitleFgColor,
                             titleBgColor ?? defaultTitleBgColor,
                             true, null);

            AppendStyledText(mainText,
                             textFgColor ?? defaultTextFgColor,
                             textBgColor ?? defaultTextBgColor,
                             null, null);
        }

        void DumpConsoleBuffer(TextWriterWithConsoleColor.TextWriterWithConsoleColor buffer,
                               string title, Color? titleFgColor = null, Color? titleBgColor = null)
        {
            if (!_outputArea.IsEmpty())
            {
                _outputArea.AppendText("\n\n");
            }

            AppendStyledText(" --- " + title + " --- \n",
                             titleFgColor ?? defaultTitleFgColor,
                             titleBgColor ?? defaultTitleBgColor,
                             true, null);

            buffer.ForEachPart(AppendConsoleColoredText);
        }

        // Get the path to the optimally selected version of the .NET reference assemblies. If no
        // matches are found, throw DirectoryNotFoundException.
        protected string GetReferenceAssembliesPath()
        {
            string basepath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86,
                                          Environment.SpecialFolderOption.DoNotVerify),
                "Reference Assemblies",
                "Microsoft",
                "Framework",
                ".NETFramework");

            // List of .NET reference assembly directories, with glob wildcards characters.
            string[] globPatternList = { "v4.6.*", "v4.*" };

            // Try to match the first pattern. If no match, try the second pattern, etc.
            foreach (string pattern in globPatternList)
            {
                string[] directories = System.IO.Directory.GetDirectories(basepath, pattern);
                if (directories.Any())
                {
                    // If multiple versions are found, return the path to the latest version.
                    return directories.Max();
                }
            }

            // If no .NET reference assembly is found, throw an exception.
            string dotNetDeveloperPackURL =
                "https://www.microsoft.com/en-us/download/details.aspx?id=49978";
            throw new System.IO.DirectoryNotFoundException(
                String.Format("No .NET 4.x reference assemblies is found. Please install .NET Framework 4.6.1 Developer Pack at {0}",
                              dotNetDeveloperPackURL));
        }

        protected abstract Compilation GetCompilation();

        private string DiagToString(Diagnostic diag)
        {
            return String.Format("({0}) : {1}",
                diag.Location.GetMappedLineSpan().StartLinePosition,
                diag.GetMessage());
        }

        // This method is callable by the UI to refresh the partial result.
        public void OutputPartialResult()
        {
            // Reset console and debug
            System.Diagnostics.Debug.Flush();

            // Output results
            _outputArea.ClearOutput();

            DumpConsoleBuffer(_consoleWriter, "Console output");

            var s = _debugWriter.ToString();
            if (s != "")
            {
                AppendResult(s, "Debug/trace output");
            }
        }

        private void OutputCompleteResult()
        {
            OutputPartialResult();

            if (_result != null)
            {
                AppendResult(_result.ToString() + "\n", "Return value");
            }

            if (_hasError)
            {
                if (_exStore is ThreadAbortException)
                    AppendResult("", "Execution aborted", errorFgColor);
                else
                    AppendResult(_exStore.InnerException.Message, "Runtime error", errorFgColor);
            }
        }

        public async Task<bool> Run()
        {
            Compilation compilation;
            try
            {
                compilation = GetCompilation();
            }
            catch (CodeRunnerException e)
            {
                AppendResult(Environment.NewLine + e.Message, "Unable to compile", errorFgColor);
                return false;
            }

            var stream = new System.IO.MemoryStream();
            var emitResult = compilation.Emit(stream);

            if (!emitResult.Success)
            {
                _outputArea.ClearOutput();

                AppendResult(Environment.NewLine +
                    emitResult.Diagnostics
                        .Select(diag => DiagToString(diag))
                        .Aggregate((current, next) =>
                            current + Environment.NewLine + Environment.NewLine + next),
                    "Compile error",
                    errorFgColor);

                System.Diagnostics.Debug.WriteLine(
                    "Code being compiled with errors:\n" +
                    emitResult.Diagnostics[0].Location.SourceTree?.GetText() ?? "unknown");

                return false;
            }

            Assembly compiledAssembly = Assembly.Load(stream.ToArray());
            MethodInfo methodToInvoke = compiledAssembly.EntryPoint;

            // Prepare console and debug
            if (ColoredOutput)
            {
                _consoleWriter = new TextWriterWithConsoleColor.StringWriterColor();
            }
            else
            {
                _consoleWriter = new TextWriterWithConsoleColor.StringWriterBW();
            }

            _debugWriter = new System.IO.StringWriter();
            var debugListener = new System.Diagnostics.TextWriterTraceListener(_debugWriter);

            Console.SetOut(_consoleWriter);
            if (DumpInput)
            {
                Console.SetIn(new DumpReader(new System.IO.StringReader(InputData), _consoleWriter));
            }
            else
            {
                Console.SetIn(new System.IO.StringReader(InputData));
            }

            System.Diagnostics.Debug.Listeners.Add(debugListener);

            // Run code
            _thread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    if (methodToInvoke.GetParameters().Any())
                        _result = methodToInvoke.Invoke(null, new object[] { new string[] { } });
                    else
                        _result = methodToInvoke.Invoke(null, null);
                }
                catch(Exception ex)
                {
                    _hasError = true;
                    _exStore = ex;
                }
            }));
            _thread.IsBackground = true;
            _thread.Start();

            while (_thread.ThreadState != ThreadState.Stopped &&
                   _thread.ThreadState != ThreadState.Aborted)
            {
                await Task.Delay(5);
            }

            _thread.Join();
            _thread = null;

            System.Diagnostics.Debug.Listeners.Remove(debugListener);

            OutputCompleteResult();
            Console.ResetColor();

            _consoleWriter.Dispose();
            debugListener.Dispose();
            _debugWriter.Dispose();

            return !_hasError;
        }

        public void Terminate()
        {
            if (_thread != null)
                _thread.Abort();
        }
    }
}
