// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleToAttribute("DotNetEditor.Tests")]

namespace DotNetEditor.CodeRunner
{
    class CSCodeRunner : CodeRunnerBase
    {
        public CSCodeRunner(string code, string inputData, ICodeRunnerOutput outputArea)
            : base(code, inputData, outputArea)
        {
        }

        protected override Compilation GetCompilation()
        {
            var parseOptions = new CSharpParseOptions(
                    preprocessorSymbols: new[] { "DEBUG", "TRACE" });

            SyntaxTree tree = GetSyntaxTree(parseOptions);

            SortedSet<string> uniqueImports = new SortedSet<string>(AssemblyImports)
            {
                "mscorlib.dll",
                "Microsoft.CSharp.dll"
            };

            List<MetadataReference> references = new List<MetadataReference>();
            try
            {
                string dllPath = GetReferenceAssembliesPath();
                foreach (string import in uniqueImports)
                {
                    references.Add(MetadataReference.CreateFromFile(System.IO.Path.Combine(
                                                                        dllPath, import)));
                }
            }
            catch (Exception e)
            {
                throw new CodeRunnerException(e.Message, e);
            }

            var options = new CSharpCompilationOptions(OutputKind.ConsoleApplication);

            return CSharpCompilation.Create(
                "DotNetEditor_Script", new[] { tree }, references, options);
        }

        private SyntaxTree GetSyntaxTree(CSharpParseOptions options)
        {
            string[] templates =
            {
                // the program put inside a class
                "{1} class Program {{ {0} }}",
                // the program put inside Main function
                "{1} class Program {{ static void Main(string[] args) {{ {0} }} }}",
                // the raw program
                "{0}",
            };

            SyntaxTree tree = null;
            int errorCount = int.MaxValue;

            // Select the template with the least number of parsing errors. For ties, select the
            // first template that has the least number of errors.
            foreach (string template in templates)
            {
                string parsedCode = String.Format(template, Code, GetUsings(NSImports));
                SyntaxTree newTree = CSharpSyntaxTree.ParseText(parsedCode, options);
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
