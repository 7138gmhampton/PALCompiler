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
            var symbol_table = new SymbolTable();
            var parser = new PALParser(scanner);
            try {
                if (parser.Parse(new StreamReader(source_file)))
                    Console.WriteLine("Parsing: SUCCESS");
                else Console.WriteLine("Parsing: FAIL");
            }
            catch (Exception err) { Console.WriteLine(err.Message);  }

            if (parser.Errors.Count > 0)
                foreach (var error in parser.Errors) Console.WriteLine(error.ToString());
            else {
                parser.SyntaxTree.printGraphic("", true);
                generateCSArtifact(args[0], parser);
            }
        }

        private static void generateCSArtifact(string executable, PALParser parser)
        {
            PALCSGenerator generator = new PALCSGenerator(parser.SyntaxTree);
            string cs_code = generator.generate();
            File.WriteAllText(executable.Replace("txt", "cs"), cs_code);

            CSharpCodeProvider code_provider = new CSharpCodeProvider();
            CompilerParameters compiler_params = new CompilerParameters();
            compiler_params.GenerateExecutable = true;
            compiler_params.OutputAssembly = executable.Replace("txt", "exe");
            CompilerResults cs_results = code_provider.CompileAssemblyFromSource(compiler_params, cs_code);
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
