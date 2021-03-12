using System;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using AllanMilne.Ardkit;

namespace PALCompiler
{
    class PALCompiler
    {
        static void Main(string[] args)
        {
            string source_file = (args.Length == 1) ? args[0] : inputSourceFile();

            var scanner = new PALScanner();
            var parser = new PALParser(scanner);
            try {
                if (parser.Parse(new StreamReader(source_file)))
                    Console.WriteLine("Parsing: SUCCESS");
                else Console.WriteLine("Parsing: FAIL");
            }
            catch (Exception err) { Console.WriteLine(err.Message);  }

            parser.SyntaxTree.printGraphic("", true);

            var symbol_table = new SymbolTable();
            var semantic_analyser = new SemanticAnalyser(parser, parser.SyntaxTree, symbol_table);
            semantic_analyser.analyse();

            if (parser.Errors.Count > 0)
                foreach (var error in parser.Errors) Console.WriteLine(error.ToString());
            else {
                generateCSArtifact(args[0], parser);
            }
        }

        private static void generateCSArtifact(string executable, PALParser parser)
        {
            // TODO - Change to just using the syntax tree

            string cs_code = new PALCSGenerator(parser.SyntaxTree).generate();
            File.WriteAllText(executable.Replace("txt", "cs"), cs_code);

            var cs_results = new CSharpCodeProvider()
                .CompileAssemblyFromSource(new CompilerParameters
                {
                    GenerateExecutable = true,
                    OutputAssembly = executable.Replace("txt", "exe")
                }, cs_code);
            foreach (var error in cs_results.Errors) Console.WriteLine(error.ToString());
        }

        private static string inputSourceFile()
        {
            while (true) {
                Console.Write("Source file: ");
                string source_file = Console.ReadLine();

                if (File.Exists(source_file)) return source_file;
                else Console.WriteLine("Invalid file - try again");
            }
        }
    }
}
