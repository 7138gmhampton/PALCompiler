using System;
using System.IO;
using System.CodeDom.Compiler;
//using Microsoft.CodeDom.Providers;
//using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using Microsoft.CSharp;

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

            if (parser.Errors.Count > 0)
                foreach (var error in parser.Errors) Console.WriteLine(error.ToString());
            else {
                PALCSGenerator generator = new PALCSGenerator();
                string cs_code = generator.generate(parser.SyntaxTree);
                File.WriteAllText("output.cs", cs_code);
            }

            CSharpCodeProvider code_provider = new CSharpCodeProvider();
            CompilerParameters compiler_params = new CompilerParameters();
            compiler_params.GenerateExecutable = true;
            compiler_params.OutputAssembly = args[0].Replace("txt", "exe");
            CompilerResults cs_results = code_provider.CompileAssemblyFromFile(compiler_params, "output.cs");
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

        private static void archiveOldOutput()
        {
            File.Delete("archive.txt");
            File.Move("output.txt", "archive.txt");
        }
    }
}
