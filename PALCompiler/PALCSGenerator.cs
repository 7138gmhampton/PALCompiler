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
        private delegate string Generator(SyntaxNode node);

        private readonly static Dictionary<string, Generator> symbol_to_generator =
            new Dictionary<string, Generator>
            {
                { "<Program>", Generators.generateRoot }
            };

        internal PALCSGenerator() { }

        internal string generate(SyntaxNode syntax_tree)
        {
            //throw new NotImplementedException();
            syntax_tree.printGraphic("", true);

            StringBuilder artifact = new StringBuilder();

            SyntaxNode cursor = null;
            SyntaxNode precursor = null;

            //artifact.Append(Generators.generateRoot());

            cursor = syntax_tree;
            //symbol_to_generator[cursor.Symbol].Invoke();
            artifact.Append(symbol_to_generator[cursor.Symbol].Invoke(syntax_tree));



            return artifact.ToString();
        }

        private static class Generators
        {
            internal static string generateRoot(SyntaxNode node)
            {
                StringBuilder code = new StringBuilder();

                code.AppendLine("using System;\nusing System.IO;\n");

                code.AppendLine("class " + node.Children[1].Value + "\n{");

                code.AppendLine("}");

                return code.ToString();
            }
        }
    }
}
