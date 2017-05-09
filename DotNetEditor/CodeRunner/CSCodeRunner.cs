// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetEditor.CodeRunner
{
    class CSCodeRunner : CodeRunnerBase
    {
        public CSCodeRunner(string code, string inputData, AvalonRichTextBox outputArea)
            : base(code, inputData, outputArea)
        {
        }

        protected override SyntaxTree GetSyntaxTree()
        {
            string[] templates =
            {
                // the raw program
                "{0}",
                // the program put inside a class
                "{1} class Program {{ {0} }}",
                // the program put inside Main function
                "{1} class Program {{ static void Main(string[] args) {{ {0} }} }}"
            };

            SyntaxTree tree = null;
            int errorCount = int.MaxValue;

            // Select the template with the least number of parsing errors. For ties, select the
            // first template that has the least number of errors.
            foreach (string template in templates)
            {
                string parsedCode = String.Format(template, Code, GetUsings(NSImports));
                SyntaxTree newTree = CSharpSyntaxTree.ParseText(parsedCode);
                int newErrorCount = newTree.GetDiagnostics().Count();
                if (newErrorCount < errorCount)
                {
                    errorCount = newErrorCount;
                    tree = newTree;
                    if (errorCount == 0)
                    {
                        break;
                    }
                }
            }

            return tree;
        }

        protected override Compilation GetCompilation()
        {
            SyntaxTree tree = GetSyntaxTree();

            SortedSet<string> uniqueImports = new SortedSet<string>(AssemblyImports);
            uniqueImports.Add("mscorlib.dll");
            uniqueImports.Add("Microsoft.CSharp.dll");

            string dllPath = GetReferenceAssembliesPath();
            IEnumerable<MetadataReference> references =
                uniqueImports.Select(import => MetadataReference.CreateFromFile(
                    System.IO.Path.Combine(dllPath, import)));

            var options = new CSharpCompilationOptions(OutputKind.ConsoleApplication);

            return CSharpCompilation.Create(
                "DotNetEditor_Script", new[] { tree }, references, options);
        }

        private string GetUsings(string[] usings)
        {
            System.Text.StringBuilder str = new System.Text.StringBuilder();
            foreach (string s in usings)
            {
                str.Append("using ");
                str.Append(s);
                str.Append("; ");
            }
            return str.ToString();
        }
    }
}
