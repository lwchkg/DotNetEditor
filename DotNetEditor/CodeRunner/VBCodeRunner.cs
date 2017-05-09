using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetEditor.CodeRunner
{
    class VBCodeRunner : CodeRunnerBase
    {
        public VBCodeRunner(string code, string inputData, AvalonRichTextBox outputArea)
            : base(code, inputData, outputArea)
        {
        }

        protected override SyntaxTree GetSyntaxTree()
        {
            SyntaxTree tree = VisualBasicSyntaxTree.ParseText(Code);

            if (IsInsideOfSubMain(tree))
            {
                string parsedCode = String.Format(
                    "Module Module1\nSub Main\n{0}\nEnd Sub\nEnd Module", Code);
                return VisualBasicSyntaxTree.ParseText(parsedCode);
            }
            else if (IsInsideOfClass(tree))
            {
                string parsedCode = String.Format(
                    "Module Module1\n{0}\nEnd Module", Code);
                return VisualBasicSyntaxTree.ParseText(parsedCode);
            }
            else
            {
                return tree;
            }
        }

        protected override Compilation GetCompilation()
        {
            SyntaxTree tree = GetSyntaxTree();

            SortedSet<string> uniqueImports = new SortedSet<string>(AssemblyImports);
            uniqueImports.Add("mscorlib.dll");
            uniqueImports.Add("Microsoft.VisualBasic.dll");

            string dllPath = GetReferenceAssembliesPath();
            IEnumerable<MetadataReference> references =
                uniqueImports.Select(import => MetadataReference.CreateFromFile(
                    System.IO.Path.Combine(dllPath, import)));

            var options = new VisualBasicCompilationOptions(
                OutputKind.ConsoleApplication,
                globalImports: GlobalImport.Parse(NSImports),
                optionInfer: true,
                optionExplicit: true,
                optionCompareText: false);

            return VisualBasicCompilation.Create(
                "DotNetEditor_Script", new[] { tree }, references, options);
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
