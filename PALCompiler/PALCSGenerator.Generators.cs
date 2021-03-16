using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AllanMilne.Ardkit;

namespace PALCompiler
{
    partial class PALCSGenerator
    {
        /// <summary>
        /// This class contains a full set of generator methods for outputting
        /// a C# artifact
        /// </summary>
        private static class Generators
        {
            internal static string generateProgram(SyntaxNode root)
            {
                var code = new StringBuilder();

                code.AppendLine("using System;\nusing System.IO;\n");
                code.AppendLine($"class {root.Children[1].Value}\n{{");
                var variable_declaration = root
                    .Children
                    .Find(x => x.Syntax == Nonterminals.VARIABLE_DECLARATION);
                if (variable_declaration != null)
                    code.AppendLine(generateVariableDeclarations(variable_declaration));

                code.AppendLine(generateImperatives(root));
                code.AppendLine("}");

                return code.ToString();
            }

            private static string generateImperatives(SyntaxNode root)
            {
                var code = new StringBuilder();

                code.AppendLine("static void Main()\n{");
                var statements = root.Children.FindAll(x => x.Syntax == Nonterminals.STATEMENT);
                foreach (var statement in statements)
                    code.AppendLine(generateStatement(statement));
                code.AppendLine("Console.WriteLine(\"Program terminated...\");");
                code.AppendLine("Console.ReadKey();");
                code.AppendLine("}");

                return code.ToString();
            }

            private static string generateStatement(SyntaxNode node)
            {
                switch (node.Children[0].Syntax) {
                    case Nonterminals.ASSIGMENT: return generateAssignment(node.Children[0]);
                    case Nonterminals.LOOP: return generateLoop(node.Children[0]);
                    case Nonterminals.CONDITIONAL: return generateConditional(node.Children[0]);
                    case Nonterminals.IO: return generateIO(node.Children[0]);
                    default: throw new Exception("Malformed syntax tree");
                }
            }

            private static string generateIO(SyntaxNode node)
            {
                var code = new StringBuilder();

                if (node.Children[0].Syntax == "INPUT") code.AppendLine(generateInput(node));
                else code.AppendLine(generateOutput(node));
                
                return code.ToString();
            }

            private static string generateInput(SyntaxNode io_node)
            {
                var code = new StringBuilder();

                var identifiers = io_node.Children[1].Children.FindAll(x => x.Syntax == "Identifier");
                foreach (var identifier in identifiers) {
                    code.AppendLine($"Console.Write(\"{identifier.Value}: \");");
                    if (identifier.Semantic == LanguageType.Integer)
                        code.AppendLine($"{identifier.Value} = int.Parse(Console.ReadLine());");
                    else code.AppendLine($"{identifier.Value} = float.Parse(Console.ReadLine());");
                }

                return code.ToString();
            }

            private static string generateOutput(SyntaxNode io_node)
            {
                var code = new StringBuilder();

                var outputs = io_node.Children.FindAll(x => x.Syntax != "," && x.Syntax != "OUTPUT");
                foreach (var output in outputs) {
                    code.AppendLine($"Console.WriteLine({generateExpression(output)});");
                }

                return code.ToString();
            }

            private static string generateConditional(SyntaxNode conditional)
            {
                var code = new StringBuilder();
                int else_index = conditional.Children.FindIndex(x => x.Syntax == "ELSE");

                code.AppendLine(
                    $"if ({generateBooleanExpression(conditional.Children[1], false)}) {{");
                var if_statements = conditional
                    .Children
                    .Take(else_index)
                    .Where(x => x.Syntax == Nonterminals.STATEMENT);
                foreach (var statement in if_statements)
                    code.AppendLine(generateStatement(statement));
                if (else_index >= 0) {
                    //code.AppendLine("}");
                    //code.AppendLine("else {");

                    //var else_statements = conditional
                    //    .Children
                    //    .Skip(else_index)
                    //    .Where(x => x.Syntax == Nonterminals.STATEMENT);
                    //foreach (var statement in else_statements)
                    //    code.AppendLine(generateStatement(statement));
                    //generateElseBlock(conditional, else_index);
                    code.AppendLine(generateElseBlock(conditional, else_index));
                }
                code.AppendLine("}");

                return code.ToString();
            }

            private static string generateElseBlock(SyntaxNode conditional, int else_index)
            {
                var code = new StringBuilder();

                code.AppendLine("}");
                code.AppendLine("else {");

                var else_statements = conditional
                    .Children
                    .Skip(else_index)
                    .Where(x => x.Syntax == Nonterminals.STATEMENT);
                foreach (var statement in else_statements)
                    code.AppendLine(generateStatement(statement));

                return code.ToString();
            }

            private static string generateLoop(SyntaxNode node)
            {
                var code = new StringBuilder();
                string stop_condition = generateBooleanExpression(node.Children[1], true);

                code.AppendLine($"while ({stop_condition}) {{");
                var statements_in_block = node.Children.FindAll(x => x.Syntax == Nonterminals.STATEMENT);
                foreach (var statement in statements_in_block)
                    code.AppendLine(generateStatement(statement));
                code.AppendLine("}");

                return code.ToString();
            }

            private static string generateBooleanExpression(SyntaxNode boolean_node, bool invert)
            {
                string left_hand = generateExpression(boolean_node.Children[0]);
                string right_hand = generateExpression(boolean_node.Children[2]);
                string logical_operator = convertOperator(boolean_node.Children[1].Value, invert);

                return $"{left_hand} {logical_operator} {right_hand}";
            }

            private static string invertOperator(string boolean_operator)
            {
                if (boolean_operator == "<") return ">=";
                else if (boolean_operator == "=") return "!=";
                else return "<=";
            }

            private static string convertOperator(string logical_operator, bool invert)
            {
                string converted_operator;

                if (logical_operator == "<") converted_operator = (invert) ? ">=" : "<";
                else if (logical_operator == "=") converted_operator = invert ? "!=" : "==";
                else converted_operator = invert ? "<=" : ">";

                return converted_operator;
            }

            private static string generateAssignment(SyntaxNode node)
            {
                string left_hand = node.Children[0].Value;
                string right_hand = generateExpression(node.Children[2]);

                return $"{left_hand} = {right_hand};";
            }

            private static string generateExpression(SyntaxNode node)
            {
                var code = new StringBuilder();

                foreach (var element in node.Children) {
                    if (element.Syntax == Nonterminals.TERM)
                        code.Append(generateTerm(element));
                    else code.Append(element.Syntax);
                }

                return code.ToString();
            }

            private static string generateTerm(SyntaxNode node)
            {
                var code = new StringBuilder();

                foreach (var element in node.Children) {
                    if (element.Syntax == Nonterminals.FACTOR)
                        code.Append(generateFactor(element));
                    else code.Append(element.Syntax);
                }

                return code.ToString();
            }

            private static string generateFactor(SyntaxNode node)
            {
                var code = new StringBuilder();

                foreach (var element in node.Children) {
                    if (element.Syntax == Nonterminals.EXPRESSION)
                        code.Append(generateExpression(element));
                    else if (element.Syntax == Nonterminals.VALUE)
                        code.Append(generateValue(element.Children[0]));
                    else code.Append(element.Syntax);
                }

                return code.ToString();
            }

            private static string generateValue(SyntaxNode node)
            {
                var dangling_radix = new System.Text.RegularExpressions.Regex(@"\.$");
                if (node.Syntax == "Real") {
                    if (dangling_radix.IsMatch(node.Value)) return node.Value + "0f";
                    else return node.Value + "f";
                }
                else return node.Value;
            }

            private static string generateVariableDeclarations(SyntaxNode node)
            {
                var code = new StringBuilder();
                var identifiers = new List<(string, string)>();

                var identifier_lists = node
                    .Children
                    .FindAll(x => x.Syntax == Nonterminals.IDENTIFIER_LIST);
                var types = node.Children.FindAll(x => x.Syntax == Nonterminals.TYPE);

                for (int iii = 0; iii < identifier_lists.Count; ++iii) {
                    string type = (types[iii].Children[0].Syntax == "INTEGER") ? "int" : "float";
                    foreach (var identifier in identifier_lists[iii]
                        .Children
                        .FindAll(x => x.Syntax == "Identifier")) {
                        if (type == "int") code.AppendLine($"static {type} {identifier.Value} = 0;");
                        else code.AppendLine($"static {type} {identifier.Value} = 0.0f;");
                    }
                }

                return code.ToString();
            }
        }
    }
}
