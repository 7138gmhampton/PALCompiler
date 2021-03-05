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
        private delegate string Generator();

        private readonly static Dictionary<string, Generator> symbol_to_generator =
            new Dictionary<string, Generator>
            {
                { "<Program>", Generators.generateProgram }
            };

        internal PALCSGenerator() { }

        internal string generate(SyntaxNode syntax_tree)
        {
            //throw new NotImplementedException();
            //syntax_tree.printGraphic("", true);

            StringBuilder artifact = new StringBuilder();

            SyntaxNode cursor = null;
            SyntaxNode precursor = null;

            cursor = syntax_tree;
            //symbol_to_generator[cursor.Symbol].Invoke();
            artifact.Append(symbol_to_generator[cursor.Symbol].Invoke());

            return artifact.ToString();
        }

        private static class Generators
        {
            internal static string generateProgram()
            {
                StringBuilder code = new StringBuilder();

                code.Append("using System;\nusing System.IO;\n\n");

                return code.ToString();
            }
        }
    }
}
