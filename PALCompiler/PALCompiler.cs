using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllanMilne.Ardkit;

namespace PALCompiler
{
    class PALCompiler
    {
        static void Main(string[] args)
        {
            string source_file = (args.Length == 1) ? args[0] : inputSourceFile();

            //var tokens = new List<IToken>();
            //var errors = new List<ICompilerError>();

            var scanner = new PALScanner();
            //scanner.Init(new StreamReader(source_file), errors);

            //while (!scanner.EndOfFile) tokens.Add(scanner.NextToken());

            //if (errors.Count != 0)
            //    foreach (var error in errors)
            //        Console.WriteLine(error.ToString());
            //else {
            //    if (File.Exists("output.txt")) archiveOldOutput();
            //    using (var output_file = new StreamWriter("output.txt", false)) {
            //        foreach (var token in tokens) output_file.WriteLine(token.ToString());
            //    }
            //}
            var parser = new PALParser(scanner);
            try {
                //parser.Parse(new StreamReader(source_file));
                if (parser.Parse(new StreamReader(source_file)))
                    Console.WriteLine("Parsing: SUCCESS");
                else Console.WriteLine("Parsing: FAIL");
            }
            catch (Exception err) { Console.WriteLine(err.Message);  }

            if (parser.Errors.Count > 0)
                foreach (var error in parser.Errors) Console.WriteLine(error.ToString());
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
