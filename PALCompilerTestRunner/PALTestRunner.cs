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
            else if (!to_succeed && output.Length > 1) return true;
            else return false;
        }

        // TODO - Specific fail check
    }
}
