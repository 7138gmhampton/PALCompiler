using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace PALCompilerTestRunner
{
    class PALTestRunner
    {
        private static string compiler;

        static void Main(string[] args)
        {
            compiler = Path.GetFullPath(@"./PALCompiler.exe");
            string[] suites = Directory.GetDirectories(@"./");

            foreach (var suite in suites) {
                Console.WriteLine("-- " + suite + "  --");
                string[] cases = Directory.GetFiles(Path.GetFullPath(suite));
                foreach (var test_case in cases) {
                    Console.Write($"{Path.GetFileNameWithoutExtension(test_case),-64}");
                    if (performTest(test_case)) Console.WriteLine("SUCCESS");
                    else Console.WriteLine("FAIL");
                }
            }
        }

        private static bool performTest(string test_case)
        {
            var then_succeed = new Regex(@"_ThenSuccess$");
            bool to_succeed = then_succeed.Match(test_case).Success;

            var compilation = new Process();
            compilation.StartInfo.FileName = compiler;
            compilation.StartInfo.Arguments = "\"" + test_case + "\"";
            compilation.StartInfo.UseShellExecute = false;
            compilation.StartInfo.RedirectStandardOutput = true;
            compilation.Start();

            string output = compilation.StandardOutput.ReadToEnd();
            if (to_succeed && output.Length == 0) return true;
            else if (!to_succeed && output.Length > 1) return isErrorCorrect(test_case, output);
            else return false;
        }

        // TODO - Specific fail check

        private static bool isErrorCorrect(string test_case, string output)
        {
            switch (test_case) {
                case string syntax_error when new Regex(@"_ThenSyntaxError$").IsMatch(test_case):
                    return new Regex(@"found where").IsMatch(output);
                case string type_error when new Regex(@"_ThenTypeError$").IsMatch(test_case):
                    return new Regex(@"Type conflict").IsMatch(output);
                case string not_declared when new Regex(@"_ThenNotDeclaredError$").IsMatch(test_case):
                    return new Regex(@"not declared").IsMatch(output);
                case string already_declared when new Regex(@"_ThenAlreadyDeclaredError$").IsMatch(test_case):
                    return new Regex(@"already declared").IsMatch(output);
                case string lexical_error when new Regex(@"_ThenLexicalError$").IsMatch(test_case):
                    return new Regex(@"Lexical error").IsMatch(output);
                default:
                    return false;
            }
        }
    }
}
