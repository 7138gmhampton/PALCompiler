using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PALCompilerTestRunner
{
    class PALTestRunner
    {
        static void Main(string[] args)
        {
            string[] suites = Directory.GetDirectories(@"./");

            foreach (var suite in suites) {
                Console.WriteLine("-- " + suite + "  --");
            }
        }
    }
}
