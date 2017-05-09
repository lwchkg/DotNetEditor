// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using Microsoft.CodeAnalysis;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

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

        private AvalonRichTextBox _outputArea;

        // Default text colors
        readonly Color defaultTextBgColor = Brushes.Black.Color;
        readonly Color defaultTextFgColor = Brushes.LightGray.Color;
        readonly Color defaultTitleBgColor = Brushes.LightGray.Color;
        readonly Color defaultTitleFgColor = Brushes.Black.Color;

        // TODO: constants to be abstracted
        readonly Color errorFgColor = Brushes.Red.Color;

        protected CodeRunnerBase(string code, string inputData, AvalonRichTextBox outputArea)
        {
            Code = code;
            InputData = inputData;
            NSImports = DefaultNSImports;
            AssemblyImports = DefaultAssemblyImports;
            _outputArea = outputArea;
        }

        public void SetOutputArea(AvalonRichTextBox outputArea)
        {
            _outputArea = outputArea;
        }

        void addStyledText(string s, Color foregroundColor, Color backgroundColor,
                           bool? isBold, bool? isItalic)
        {
            int pos = _outputArea.Text.Length;
            _outputArea.AppendTextWithStyle(s, foregroundColor, backgroundColor, isBold, isItalic);
        }

        void addConsoleColoredText(string s, TextWriterWithConsoleColor.ColorInfo color)
        {
            addStyledText(s, ColorScheme.ConvertColor(color.Foreground),
                          ColorScheme.ConvertColor(color.Background), false, false);
        }

        void addResult(string mainText, string title,
                       Color? textFgColor = null, Color? textBgColor = null,
                       Color? titleFgColor = null, Color? titleBgColor = null)
        {
            if (_outputArea.Text.Length > 0)
            {
                _outputArea.AppendText("\n\n");
            }

            addStyledText(" --- " + title + " --- \n",
                          titleFgColor ?? defaultTitleFgColor,
                          titleBgColor ?? defaultTitleBgColor,
                          true, null);

            addStyledText(mainText,
                          textFgColor ?? defaultTextFgColor,
                          textBgColor ?? defaultTextBgColor,
                          null, null);
        }

        void dumpConsoleBuffer(TextWriterWithConsoleColor.TextWriterWithConsoleColor buffer,
                               string title, Color? titleFgColor = null, Color? titleBgColor = null)
        {
            if (_outputArea.Text.Length > 0)
            {
                _outputArea.AppendText("\n\n");
            }

            addStyledText(" --- " + title + " --- \n",
                          titleFgColor ?? defaultTitleFgColor,
                          titleBgColor ?? defaultTitleBgColor,
                          true, null);

            buffer.ForEachPart(addConsoleColoredText);
        }

        // Return: whether a the code runs successfully without error.
        protected string GetReferenceAssembliesPath()
        {
            return @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.1";
        }

        protected abstract SyntaxTree GetSyntaxTree();

        protected abstract Compilation GetCompilation();

        private string DiagToString(Diagnostic diag)
        {
            return String.Format("({0}) : {1}",
                diag.Location.GetMappedLineSpan().StartLinePosition,
                diag.GetMessage());
        }

        public bool Run()
        {
            _outputArea.Clear();
            Compilation compilation = GetCompilation();

            var stream = new System.IO.MemoryStream();
            var emitResult = compilation.Emit(stream);

            if (!emitResult.Success)
            {
                addResult(Environment.NewLine +
                    emitResult.Diagnostics
                        .Select(diag => DiagToString(diag))
                        .Aggregate((current, next) => current + Environment.NewLine + Environment.NewLine + next),
                    "Compile error", errorFgColor);

                System.Diagnostics.Debug.WriteLine(
                    "Code being compiled with errors:\n" +
                    emitResult.Diagnostics[0].Location.SourceTree.GetText().ToString());

                return false;
            }

            Assembly compiledAssembly = Assembly.Load(stream.ToArray());
            MethodInfo methodToInvoke = compiledAssembly.EntryPoint;

            // Prepare console and debug
            TextWriterWithConsoleColor.TextWriterWithConsoleColor consoleWriter;

            if (ColoredOutput)
            {
                consoleWriter = new TextWriterWithConsoleColor.StringWriterColor();
            }
            else
            {
                consoleWriter = new TextWriterWithConsoleColor.StringWriterBW();
            }

            var debugWriter = new System.IO.StringWriter();
            var debugListener = new System.Diagnostics.TextWriterTraceListener(debugWriter);

            Console.SetOut(consoleWriter);
            if (DumpInput)
            {
                Console.SetIn(new DumpReader(new System.IO.StringReader(InputData), consoleWriter));
            }
            else
            {
                Console.SetIn(new System.IO.StringReader(InputData));
            }

            System.Diagnostics.Debug.Listeners.Add(debugListener);

            // Run code
            object result = null;
            bool hasError = false;
            Exception exStore = null;
            try
            {
                if (methodToInvoke.GetParameters().Any())
                {
                    result = methodToInvoke.Invoke(null, new object[] { new string[] { } });
                }
                else
                {
                    result = methodToInvoke.Invoke(null, null);
                }
            }
            catch (Exception ex)
            {
                hasError = true;
                exStore = ex;
            }
            finally
            {
                System.Diagnostics.Debug.Listeners.Remove(debugListener);
            }

            // Reset console and debug
            System.Diagnostics.Debug.Flush();
            Console.ResetColor();

            // Output results
            _outputArea.Clear();

            dumpConsoleBuffer(consoleWriter, "Console output");

            var s = debugWriter.ToString();
            if (s != "")
            {
                addResult(debugWriter.ToString(), "Debug/trace output");
            }

            if (result != null)
            {
                addResult(result.ToString(), "Return value");
            }

            if (hasError)
            {
                addResult(exStore.InnerException.Message,
                    "Runtime error");//, errorColor)
            }

            return !hasError;
        }
    }
}
