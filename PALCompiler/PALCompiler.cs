using System;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using AllanMilne.Ardkit;
using System.Collections.Generic;

namespace PALCompiler
{
    class PALCompiler
    {
        static void Main(string[] args)
        {
            var arguments = new List<string>(args);
            //bool display_tree = arguments.Contains("-t") || arguments.Contains("--tree");
            //if (display_tree) arguments.RemoveAll(x => x == "-t" || x == "--tree");
            //string source_file = arguments.Find(x => x.EndsWith(".txt"));
            //string source_file = "";
            //if (arguments.Count == 1) source_file = arguments[0];
            //else throw new ArgumentException("Invalid arguments passed to program");
            bool display_tree = false;
            bool generate_output = false;
            string source_file = "";
            try {
                (display_tree, generate_output, source_file) = parseArguments(new List<string>(args));
            }
            catch (ArgumentException err) {
                Console.WriteLine(err.Message);
                //return 160;
                Environment.Exit(160);
            }

            var parser = performLexicalAnalysis(source_file);
            performSemanticAnalysis(parser);
            if (display_tree) parser.SyntaxTree.displayTree();

            if (parser.Errors.Count > 0)
                foreach (var error in parser.Errors) Console.WriteLine(error.ToString());
            else {
                generateCSArtifact(source_file, parser.SyntaxTree);
                Console.WriteLine("-- BUILD SUCCESSFUL --");
            }
        }

        private static (bool, bool, string) parseArguments(List<string> arguments)
        {
            bool display_tree = arguments.Contains("-t") || arguments.Contains("--tree");
            if (display_tree) arguments.RemoveAll(x => x == "-t" || x == "--tree");
            string source_file = "";
            if (arguments.Count == 1) source_file = arguments[0];
            else throw new ArgumentException(
                "Invalid arguments passed to program\nCorrect usage: PALCompiler.exe <source> [OPTIONS]");

            return (display_tree, false, source_file);
        }

        private static void performSemanticAnalysis(PALParser parser)
        {
            var symbol_table = new SymbolTable();
            var semantic_analyser = new SemanticAnalyser(parser, parser.SyntaxTree, symbol_table);
            try {
                semantic_analyser.analyse();
            }
            catch (Exception err) { Console.WriteLine(err.Message); }
        }

        private static PALParser performLexicalAnalysis(string source_file)
        {
            var scanner = new PALScanner();
            var parser = new PALParser(scanner);
            try {
                parser.Parse(new StreamReader(source_file));
            }
            catch (Exception err) { Console.WriteLine(err.Message); }

            return parser;
        }

        private static void generateCSArtifact(string executable, SyntaxNode syntax_tree)
        {
            string cs_code = new PALCSGenerator(syntax_tree).generate();
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
