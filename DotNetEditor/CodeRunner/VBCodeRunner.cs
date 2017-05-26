// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleToAttribute("DotNetEditor.Tests")]

namespace DotNetEditor.CodeRunner
{
    class VBCodeRunner : CodeRunnerBase
    {
        public VBCodeRunner(string code, string inputData, ICodeRunnerOutput outputArea)
            : base(code, inputData, outputArea)
        {
        }

        protected override Compilation GetCompilation()
        {
            var preprocessorSymbols =
                new Dictionary<string, object> { { "DEBUG", true}, { "TRACE", true} };

            var parseOptions = new VisualBasicParseOptions(
                    preprocessorSymbols: preprocessorSymbols);

            SyntaxTree tree = GetSyntaxTree(parseOptions);

            SortedSet<string> uniqueImports = new SortedSet<string>(AssemblyImports);
            uniqueImports.Add("mscorlib.dll");
            uniqueImports.Add("Microsoft.VisualBasic.dll");

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

            var compilationOptions = new VisualBasicCompilationOptions(
                OutputKind.ConsoleApplication,
                globalImports: GlobalImport.Parse(NSImports),
                parseOptions: parseOptions);

            return VisualBasicCompilation.Create(
                "DotNetEditor_Script", new[] { tree }, references, compilationOptions);
        }

        private SyntaxTree GetSyntaxTree(VisualBasicParseOptions options)
        {
            SyntaxTree tree = VisualBasicSyntaxTree.ParseText(Code, options);

            if (IsInsideOfSubMain(tree))
            {
                string parsedCode = String.Format(
                    "Module Module1\nSub Main\n{0}\nEnd Sub\nEnd Module", Code);
                return VisualBasicSyntaxTree.ParseText(parsedCode, options);
            }
            else if (IsInsideOfClass(tree))
            {
                string parsedCode = String.Format(
                    "Module Module1\n{0}\nEnd Module", Code);
                return VisualBasicSyntaxTree.ParseText(parsedCode, options);
            }
            else
            {
                return tree;
            }
        }

        private bool IsInsideOfSubMain(SyntaxTree tree)
        {
            return tree.GetRoot().ChildNodes().All(
                (node) => node is StatementSyntax &&
                          !(node is ClassBlockSyntax) &&
                          !(node is InterfaceBlockSyntax) &&
                          !(node is MethodBlockSyntax) &&
                          !(node is ModuleBlockSyntax) &&
                          !(node is NamespaceBlockSyntax)
            );
        }

        private bool IsInsideOfClass(SyntaxTree tree)
        {
            return tree.GetRoot().ChildNodes().All(
                (node) => node is StatementSyntax &&
                          !(node is ClassBlockSyntax) &&
                          !(node is InterfaceBlockSyntax) &&
                          !(node is ModuleBlockSyntax) &&
                          !(node is NamespaceBlockSyntax)
            );
        }
    }
}
