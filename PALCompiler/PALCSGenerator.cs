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
        private delegate string Generator(SyntaxNode root, SyntaxNode node);

        private readonly static Dictionary<string, Generator> symbol_to_generator =
            new Dictionary<string, Generator>
            {
                { "<Program>", Generators.generateRoot }
            };

        public SyntaxNode syntax_tree;
        //private ISymbolTable symbol_table;

        internal PALCSGenerator(SyntaxNode syntax_tree)
        {
            //this.symbol_table = symbol_table;
            this.syntax_tree = syntax_tree;
        }

        internal string generate()
        {
            //syntax_tree.printGraphic("", true);

            StringBuilder artifact = new StringBuilder();

            SyntaxNode cursor = null;
            SyntaxNode precursor = null;

            //cursor = syntax_tree;
            //syntax_tree.printGraphic("", true);
            //artifact.Append(symbol_to_generator[cursor.Symbol].Invoke(syntax_tree, syntax_tree));
            artifact.Append(Generators.generateRoot(syntax_tree, syntax_tree));

            return artifact.ToString();
        }

        private static class Generators
        {
            internal static string generateRoot(SyntaxNode root, SyntaxNode node)
            {
                StringBuilder code = new StringBuilder();

                code.AppendLine("using System;\nusing System.IO;\n");

                code.AppendLine($"class {node.Children[1].Value}\n{{");

                var variable_declaration = node.Children.Find(x => x.Symbol == "<VarDecls>");
                if (variable_declaration != null)
                    code.AppendLine(generateVariableDeclarations(root, variable_declaration));

                code.AppendLine("static void Main()\n{");

                List<SyntaxNode> statements = node.Children.FindAll(x => x.Symbol == "<Statement>");
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
                    case "<Conditional>": return generateConditional(root, node);
                    case "<I-o>": return generateIO(root, node.Children[0]);
                    default: throw new Exception("Malformed syntax tree");
                }
            }

            private static string generateIO(SyntaxNode root, SyntaxNode node)
            {
                StringBuilder code = new StringBuilder();
                //Type dummy_type = code.GetType();
                //int dummy = 3;
                //float badger = 0.1f;
                //Type type = badger.GetType();
                //badger = (typeof(type))dummy;

                if (node.Children[0].Symbol == "INPUT") {
                    //foreach (var identifier in node.Children[1].Children.FindAll(x => x.Symbol == "Identifier")) {
                    //    code.AppendLine($"Console.Write(\"{identifier.Value}: \");");
                    //    //code.AppendLine($"if ({identifier.Value}.GetType() == typeof(int))");
                    //    //code.AppendLine($"{identifier.Value} = int.Parse(Console.ReadLine());");
                    //    //code.AppendLine($"else {identifier.Value} = ({identifier.Value}.FieldInfo.FieldType)float.Parse(Console.ReadLine());");
                    //    //code.AppendLine($"{identifier.Value} = ")
                    //    //var variable_declarations = root.Children.Find(x => x.Symbol == "<VarDecls>");
                    //    ////int variable_declarations_index = root.Children.FindIndex(x => x.Symbol == "<VarDecls>");
                    //    //if (variable_declarations != null) {
                    //    //    //var identifier_index = variable_declarations.Children.FindIndex(x => x.Children.Where(y => y.Value == identifier.Value));
                    //    //    //var identifier_list = variable_declarations.Children.Find(x => x.Children.FindAll(y => y.Value == identifier.Value))
                    //    //    int identifier_index = -1;
                    //    //    // TODO - Implement getParent on SyntaxNode to allow finding index of parent
                    //    //    Console.WriteLine($"{identifier.Value} @ {identifier_index}");
                    //    //    var type_node = variable_declarations.Children
                    //    //        .GetRange(identifier_index, variable_declarations.Children.Count - identifier_index)
                    //    //        .Find(x => x.Symbol == "<Type>");
                    //    //    if (type_node.Children[0].Value == "INTEGER")
                    //    //        code.AppendLine($"{identifier.Value} = int.Parse(Console.ReadLine());");
                    //    //    else code.AppendLine($"{identifier.Value} = float.Parse(Console.ReadLine());");
                    //    //}
                    //}
                    List<SyntaxNode> identifiers = new List<SyntaxNode>();
                    foreach (SyntaxNode identifier_list in node.Children.FindAll(x => x.Symbol == "<IdentList>"))
                        foreach (SyntaxNode identifier in identifier_list.Children.FindAll(y => y.Symbol == "Identifier"))
                            identifiers.Add(identifier);

                    SyntaxNode variable_declarations = root.Children.Find(x => x.Symbol == "<VarDecls>");

                    foreach (SyntaxNode identifier in identifiers) {
                        code.AppendLine($"Console.Write(\"{identifier.Value}: \");");
                        //int index = identifier.Parent.Parent.Children.FindIndex(x => x.Children.Contains(identifier));
                        //Console.WriteLine("Identifier(" + identifier.Value + ") list index - " + index);
                        //int no_of_siblings_and_self = identifier.Parent.Children.Count;
                        //SyntaxNode type_node = identifier.Parent.Parent.Children.Skip(index).ToList<SyntaxNode>().Find(y => y.Symbol == "<Type>");
                        //Console.WriteLine("Type node - " + type_node.Symbol);
                        //List<SyntaxNode> type_nodes = identifier.Parent.Parent.Children.FindAll(x => x.Symbol == "<Type>");
                        //SyntaxNode type_node = type_nodes[index];
                        //Console.WriteLine("Type for " + identifier.Value + " - " + type_node.Children[0].Value);
                        //if (type_node.Children[0].Symbol == "INTEGER")
                        //    code.AppendLine($"{identifier.Value} = int.Parse(Console.ReadLine());");
                        //else code.AppendLine($"{identifier.Value} = float.Parse(Console.ReadLine());");
                        int list_index = variable_declarations.Children.FindIndex(x => (x.Children.Find(y => y.Value == identifier.Value)) != null);
                        Console.WriteLine("Identifier(" + identifier.Value + ") list index - " + list_index);
                        List<SyntaxNode> type_nodes = variable_declarations.Children.FindAll(x => x.Symbol == "<Type>");
                        SyntaxNode type_node = type_nodes[list_index];
                        if (type_node.Children[0].Value == "INTEGER")
                            code.AppendLine($"{identifier.Value} = int.Parse(Console.ReadLine());");
                        else code.AppendLine($"{identifier.Value} = float.Parse(Console.ReadLine());");
                    }
                }
                else {
                    List<SyntaxNode> outputs = node.Children.FindAll(x => x.Symbol != "," && x.Symbol != "OUTPUT");
                    foreach (var output in outputs) {
                        code.AppendLine($"Console.WriteLine({generateExpression(root, output)});");
                    }
                }
                
                return code.ToString();
            }
            private static string generateConditional(SyntaxNode root, SyntaxNode node) => throw new NotImplementedException();
            private static string generateLoop(SyntaxNode root, SyntaxNode node)
            {
                StringBuilder code = new StringBuilder();
                Console.WriteLine("Generating - " + node.Value);

                string stop_condition = generateBooleanExpression(root, node.Children[1]);

                code.AppendLine($"while ({stop_condition}) {{");

                List<SyntaxNode> statements_in_block = node.Children.FindAll(x => x.Symbol == "<Statement>");
                foreach (SyntaxNode statement in statements_in_block)
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
                StringBuilder code = new StringBuilder();

                foreach (var element in node.Children) {
                    if (element.Symbol == "<Term>") code.Append(generateTerm(root, element));
                    else code.Append(element.Symbol);
                }

                return code.ToString();
            }

            private static string generateTerm(SyntaxNode root, SyntaxNode node)
            {
                StringBuilder code = new StringBuilder();

                foreach (var element in node.Children) {
                    if (element.Symbol == "<Factor>") code.Append(generateFactor(root, element));
                    else code.Append(element.Symbol);
                }

                return code.ToString();
            }

            private static string generateFactor(SyntaxNode root, SyntaxNode node)
            {
                StringBuilder code = new StringBuilder();

                foreach (var element in node.Children) {
                    if (element.Symbol == "<Expression>") code.Append(generateExpression(root, element));
                    //else if (element.Symbol == "<Value>") code.Append(element.Children[0].Value);
                    else if (element.Symbol == "<Value>") code.Append(generateValue(root, element.Children[0]));
                    else code.Append(element.Symbol);
                }

                return code.ToString();
            }

            private static string generateValue(SyntaxNode root, SyntaxNode node)
            {
                //StringBuilder code = new StringBuilder();

                //if (Regex.Match)
                System.Text.RegularExpressions.Regex dangling_radix = new System.Text.RegularExpressions.Regex(@"\.$");
                //if (dangling_radix.IsMatch(node.Value)) return node.Value + "0";
                //else return node.Value;
                if (node.Symbol == "Real") {
                    if (dangling_radix.IsMatch(node.Value)) return node.Value + "0f";
                    else return node.Value + "f";
                }
                else return node.Value;

                //return code.ToString();
            }

            private static string generateVariableDeclarations(SyntaxNode root, SyntaxNode node)
            {
                StringBuilder code = new StringBuilder();
                int list_counter = 0;
                List<(string, string)> identifiers = new List<(string, string)>();
                float dummy = 0;

                List<SyntaxNode> identifier_lists = node.Children.FindAll(x => x.Symbol == "<IdentList>");
                List<SyntaxNode> type_declarators = node.Children.FindAll(x => x.Symbol == "<Type>");

                for (int iii = 0; iii < identifier_lists.Count; ++iii) {
                    string type_declarator = (type_declarators[iii].Children[0].Symbol == "INTEGER") ? "int" : "float";
                    foreach (SyntaxNode identifier in identifier_lists[iii].Children.FindAll(x => x.Symbol == "Identifier")) {
                        code.AppendLine($"static {type_declarator} {identifier.Value} = 0;");
                        //code.AppendLine($"static Type {identifier.Value}_type = {identifier.Value}.GetType();");
                    }
                }

                return code.ToString();
            }
        }
    }
}
