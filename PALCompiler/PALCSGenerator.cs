using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PALCompiler
{
    class PALCSGenerator
    {
        private delegate void Generator();

        private static Dictionary<string, Generator> symbol_to_generator =
            new Dictionary<string, Generator>
            {
                { "<Program>", Generators.generateProgram }
            };

        internal PALCSGenerator() { }

        internal void generate(TextWriter output)
        {
            throw new NotImplementedException();
        }

        private static class Generators
        {
            internal static void generateProgram() => throw new NotImplementedException();
        }
    }
}
