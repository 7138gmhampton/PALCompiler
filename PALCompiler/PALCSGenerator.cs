using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllanMilne.Ardkit;

namespace PALCompiler
{
    class PALCSGenerator
    {
        //private delegate string Generator(SyntaxNode root, SyntaxNode node);

        //private readonly static Dictionary<string, Generator> symbol_to_generator =
        //    new Dictionary<string, Generator>
        //    {
        //        { "<Program>", Generators.generateRoot }
        //    };

        public SyntaxNode syntax_tree;

        internal PALCSGenerator(SyntaxNode syntax_tree)
        {
            this.syntax_tree = syntax_tree;
        }

        internal string generate()
        {
            var artifact = new StringBuilder();

            //SyntaxNode cursor = null;
            //SyntaxNode precursor = null;

            artifact.Append(Generators.generateProgram(syntax_tree));

            return artifact.ToString();
        }

        private static class Generators
        {
            internal static string generateProgram(SyntaxNode root)
            {
                var code = new StringBuilder();

                code.AppendLine("using System;\nusing System.IO;\n");

                code.AppendLine($"class {root.Children[1].Value}\n{{");

                var variable_declaration = root.Children.Find(x => x.Symbol == "<VarDecls>");
                if (variable_declaration != null)
                    code.AppendLine(generateVariableDeclarations(root, variable_declaration));

                code.AppendLine("static void Main()\n{");

                var statements = root.Children.FindAll(x => x.Symbol == "<Statement>");
                foreach (var statement in statements)
                    code.AppendLine(generateStatement(root, statement));

                code.AppendLine("Console.WriteLine(\"Program terminated...\");");
                code.AppendLine("Console.ReadKey();");
                code.AppendLine("}");
                code.AppendLine("}");

                return code.ToString();
            }

            private static string generateStatement(SyntaxNode root, SyntaxNode node)
            {
                switch (node.Children[0].Symbol) {
                    case "<Assignment>": return generateAssignment(root, node.Children[0]);
                    case "<Loop>": return generateLoop(root, node.Children[0]);
                    case "<Conditional>": return generateConditional(root, node.Children[0]);
                    case "<I-o>": return generateIO(root, node.Children[0]);
                    default: throw new Exception("Malformed syntax tree");
                }
            }

            private static string generateIO(SyntaxNode root, SyntaxNode node)
            {
                var code = new StringBuilder();

                if (node.Children[0].Symbol == "INPUT") {
                    var identifiers = new List<SyntaxNode>();
                    foreach (var identifier_list in node.Children.FindAll(x => x.Symbol == "<IdentList>"))
                        foreach (var identifier in identifier_list.Children.FindAll(y => y.Symbol == "Identifier"))
                            identifiers.Add(identifier);

                    var variable_declarations = root.Children.Find(x => x.Symbol == "<VarDecls>");
                    var type_nodes = variable_declarations.Children.FindAll(x => x.Symbol == "<Type>");

                    foreach (var identifier in identifiers) {
                        code.AppendLine($"Console.Write(\"{identifier.Value}: \");");
                        int list_index = variable_declarations
                            .Children
                            .FindIndex(x => x.Children.Find(y => y.Value == identifier.Value) != null);
                        if (type_nodes[list_index].Children[0].Value == "INTEGER")
                            code.AppendLine($"{identifier.Value} = int.Parse(Console.ReadLine());");
                        else code.AppendLine($"{identifier.Value} = float.Parse(Console.ReadLine());");
                    }
                }
                else {
                    var outputs = node.Children.FindAll(x => x.Symbol != "," && x.Symbol != "OUTPUT");
                    foreach (var output in outputs) {
                        code.AppendLine($"Console.WriteLine({generateExpression(root, output)});");
                    }
                }
                
                return code.ToString();
            }
            private static string generateConditional(SyntaxNode root, SyntaxNode node) => throw new NotImplementedException();
            private static string generateLoop(SyntaxNode root, SyntaxNode node)
            {
                var code = new StringBuilder();
                //Console.WriteLine("Generating - " + node.Value);

                string stop_condition = generateBooleanExpression(root, node.Children[1]);

                code.AppendLine($"while ({stop_condition}) {{");

                var statements_in_block = node.Children.FindAll(x => x.Symbol == "<Statement>");
                foreach (var statement in statements_in_block)
                    code.AppendLine(generateStatement(root, statement));

                code.AppendLine("}");

                return code.ToString();
            }

            private static string generateBooleanExpression(SyntaxNode root, SyntaxNode boolean_node)
            {
                string left_hand = generateExpression(root, boolean_node.Children[0]);
                string right_hand = generateExpression(root, boolean_node.Children[2]);
                string inverted_operator = invertOperator(boolean_node.Children[1].Value);

                return $"{left_hand} {inverted_operator} {right_hand}";
            }

            private static string invertOperator(string boolean_operator)
            {
                if (boolean_operator == "<") return ">=";
                else if (boolean_operator == "=") return "!=";
                else return "<=";
            }

            private static string generateAssignment(SyntaxNode root, SyntaxNode node)
            {
                string left_hand = node.Children[0].Value;
                string right_hand = generateExpression(root, node.Children[2]);

                return $"{left_hand} = {right_hand};";
            }

            private static string generateExpression(SyntaxNode root, SyntaxNode node)
            {
                var code = new StringBuilder();

                foreach (var element in node.Children) {
                    if (element.Symbol == "<Term>") code.Append(generateTerm(root, element));
                    else code.Append(element.Symbol);
                }

                return code.ToString();
            }

            private static string generateTerm(SyntaxNode root, SyntaxNode node)
            {
                var code = new StringBuilder();

                foreach (var element in node.Children) {
                    if (element.Symbol == "<Factor>") code.Append(generateFactor(root, element));
                    else code.Append(element.Symbol);
                }

                return code.ToString();
            }

            private static string generateFactor(SyntaxNode root, SyntaxNode node)
            {
                var code = new StringBuilder();

                foreach (var element in node.Children) {
                    if (element.Symbol == "<Expression>") code.Append(generateExpression(root, element));
                    else if (element.Symbol == "<Value>") code.Append(generateValue(root, element.Children[0]));
                    else code.Append(element.Symbol);
                }

                return code.ToString();
            }

            private static string generateValue(SyntaxNode root, SyntaxNode node)
            {
                var dangling_radix = new System.Text.RegularExpressions.Regex(@"\.$");
                if (node.Symbol == "Real") {
                    if (dangling_radix.IsMatch(node.Value)) return node.Value + "0f";
                    else return node.Value + "f";
                }
                else return node.Value;
            }

            private static string generateVariableDeclarations(SyntaxNode root, SyntaxNode node)
            {
                var code = new StringBuilder();
                var identifiers = new List<(string, string)>();

                var identifier_lists = node.Children.FindAll(x => x.Symbol == "<IdentList>");
                var type_declarators = node.Children.FindAll(x => x.Symbol == "<Type>");

                for (int iii = 0; iii < identifier_lists.Count; ++iii) {
                    string type_declarator = (type_declarators[iii].Children[0].Symbol == "INTEGER") ? "int" : "float";
                    foreach (var identifier in identifier_lists[iii].Children.FindAll(x => x.Symbol == "Identifier")) {
                        code.AppendLine($"static {type_declarator} {identifier.Value} = 0;");
                    }
                }

                return code.ToString();
            }
        }
    }
}
