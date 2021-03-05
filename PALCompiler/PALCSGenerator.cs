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

                code.AppendLine($"class {node.Children[1].Value}\n{{");

                var variable_declaration = node.Children.Find(x => x.Symbol == "<VarDecls>");
                //if (variable_declaration)
                //if (variable_declaration != null) generateVariableDeclarations(variable_declaration);
                if (variable_declaration != null)
                    code.AppendLine(generateVariableDeclarations(variable_declaration));

                code.AppendLine("static void Main()\n{");

                List<SyntaxNode> statements = node.Children.FindAll(x => x.Symbol == "<Statement>");
                //Console.WriteLine("No. of statements - " + statements.Count);
                foreach (var statement in statements)
                    code.AppendLine(generateStatement(statement));

                code.AppendLine("}");
                code.AppendLine("}");

                return code.ToString();
            }

            private static string generateStatement(SyntaxNode node)
            {
                switch (node.Children[0].Symbol) {
                    case "<Assignment>": return generateAssignment(node.Children[0]);
                    case "<Loop>": return generateLoop(node);
                    case "<Conditional>": return generateConditional(node);
                    case "<I-o>": return generateIO(node.Children[0]);
                    default: throw new Exception("Malformed syntax tree");
                }
            }

            private static string generateIO(SyntaxNode node)
            {
                Console.WriteLine("Node symbol - " + node.Symbol);
                StringBuilder code = new StringBuilder();

                if (node.Children[0].Symbol == "INPUT") {
                    //code.AppendLine($"Console.Write({});")
                    foreach (var identifier in node.Children[1].Children.FindAll(x => x.Symbol == "Identifier")) {
                        code.AppendLine($"Console.Write(\"{identifier.Value}: \");");
                        //code.AppendLine($"Console.ReadLine(")
                        code.AppendLine($"if ({identifier.Value}.GetType() is int)");
                        code.AppendLine($"{identifier.Value} = int.Parse(Console.ReadLine());");
                        code.AppendLine($"else {identifier.Value} = float.Parse(Console.ReadLine());");
                    }
                }
                
                //Console.r
                return code.ToString();
            }
            private static string generateConditional(SyntaxNode node) => throw new NotImplementedException();
            private static string generateLoop(SyntaxNode node) => throw new NotImplementedException();
            private static string generateAssignment(SyntaxNode node)
            {
                string left_hand = node.Children[0].Value;
                string right_hand = generateExpression(node.Children[2]);

                return $"{left_hand} = {right_hand};";
            }

            private static string generateExpression(SyntaxNode node)
            {
                StringBuilder code = new StringBuilder();

                foreach (var element in node.Children) {
                    if (element.Symbol == "<Term>") code.Append(generateTerm(element));
                    else code.Append(element.Symbol);
                }

                return code.ToString();
            }

            private static string generateTerm(SyntaxNode node)
            {
                StringBuilder code = new StringBuilder();

                foreach (var element in node.Children) {
                    if (element.Symbol == "<Factor>") code.Append(generateFactor(element));
                    else code.Append(element.Symbol);
                }

                return code.ToString();
            }

            private static string generateFactor(SyntaxNode node)
            {
                StringBuilder code = new StringBuilder();

                foreach (var element in node.Children) {
                    if (element.Symbol == "<Expression>") code.Append(generateExpression(element));
                    else if (element.Symbol == "<Value>") code.Append(element.Children[0].Value);
                    else code.Append(element.Symbol);
                }

                return code.ToString();
            }

            //private static string generateValue(SyntaxNode element)
            //{
            //    if (element.Symbol == "")
            //}

            private static string generateVariableDeclarations(SyntaxNode node)
            {
                //Console.WriteLine("VarDecls Generator called");
                StringBuilder code = new StringBuilder();
                int list_counter = 0;
                List<(string, string)> identifiers = new List<(string, string)>();
                float dummy = 0;

                List<SyntaxNode> identifier_lists = node.Children.FindAll(x => x.Symbol == "<IdentList>");
                //Console.WriteLine("IdentLists found - " + identifier_lists.Count);
                List<SyntaxNode> type_declarators = node.Children.FindAll(x => x.Symbol == "<Type>");
                //Console.WriteLine("Types found - " + type_declarators.Count);

                //for (int iii = 0; iii < identifier_lists.Count; ++iii) {
                //    string type_declarator = (type_declarators[iii].Children[0].Symbol == "INTEGER") ? "int" : "float";
                //    foreach (SyntaxNode identifier_list in identifier_lists) {
                //        //Console.WriteLine("Identifiers found - " + identifier_list.Children.FindAll(x => x.Symbol == "Identifier").Count);
                //        foreach (SyntaxNode identifier in identifier_list.Children.FindAll(x => x.Symbol == "Identifier")) {
                //            //Console.("Identifiers found - " + )
                //            //Console.WriteLine("Appending - " + identifier.Value);
                //            code.AppendLine($"{type_declarator} {identifier.Value} = 0;");
                //        }
                //    }
                //}

                for (int iii = 0; iii < identifier_lists.Count; ++iii) {
                    string type_declarator = (type_declarators[iii].Children[0].Symbol == "int") ? "int" : "float";
                    //Console.WriteLine("Type declarator selected - " + type_declarator);
                    //foreach (SyntaxNode identifier_list in identifier_lists) {
                    //    Console.WriteLine("IdentList:");
                    //    foreach (SyntaxNode identifier in identifier_list.Children) {
                    //        Console.WriteLine("Identifier - " + identifier.Value);
                    //    }
                    //}
                    foreach (SyntaxNode identifier in identifier_lists[iii].Children.FindAll(x => x.Symbol == "Identifier"))
                        //Console.WriteLine("Id - " + identifier.Value);
                        code.AppendLine($"static {type_declarator} {identifier.Value} = 0;");
                }

                //Console.WriteLine("Declarations gathered\n" + code.ToString());
                return code.ToString();
            }
        }
    }
}
