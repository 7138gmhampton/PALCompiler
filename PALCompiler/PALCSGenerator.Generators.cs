﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PALCompiler
{
    partial class PALCSGenerator
    {
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
                    code.AppendLine(generateVariableDeclarations(root, variable_declaration));

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
                    code.AppendLine(generateStatement(root, statement));
                code.AppendLine("Console.WriteLine(\"Program terminated...\");");
                code.AppendLine("Console.ReadKey();");
                code.AppendLine("}");

                return code.ToString();
            }

            private static string generateStatement(SyntaxNode root, SyntaxNode node)
            {
                switch (node.Children[0].Syntax) {
                    case Nonterminals.ASSIGMENT: return generateAssignment(root, node.Children[0]);
                    case Nonterminals.LOOP: return generateLoop(root, node.Children[0]);
                    case Nonterminals.CONDITIONAL: return generateConditional(root, node.Children[0]);
                    case Nonterminals.IO: return generateIO(root, node.Children[0]);
                    default: throw new Exception("Malformed syntax tree");
                }
            }

            private static string generateIO(SyntaxNode root, SyntaxNode node)
            {
                var code = new StringBuilder();

                if (node.Children[0].Syntax == "INPUT") code.AppendLine(generateInput(root, node));
                else code.AppendLine(generateOutput(root, node));
                
                return code.ToString();
            }

            private static string generateInput(SyntaxNode root,  SyntaxNode io_node)
            {
                var code = new StringBuilder();

                var identifiers = io_node.Children[1].Children.FindAll(x => x.Syntax == "Identifier");
                var variable_declarations = root
                    .Children
                    .Find(x => x.Syntax == Nonterminals.VARIABLE_DECLARATION);
                var type_nodes = variable_declarations
                    .Children
                    .FindAll(x => x.Syntax == Nonterminals.TYPE);

                foreach (var identifier in identifiers) {
                    code.AppendLine($"Console.Write(\"{identifier.Value}: \");");
                    int list_index = variable_declarations
                        .Children
                        .FindIndex(x => x.Children.Find(y => y.Value == identifier.Value) != null);
                    if (type_nodes[list_index].Children[0].Value == "INTEGER")
                        code.AppendLine($"{identifier.Value} = int.Parse(Console.ReadLine());");
                    else code.AppendLine($"{identifier.Value} = float.Parse(Console.ReadLine());");
                }

                return code.ToString();
            }

            private static string generateOutput(SyntaxNode root, SyntaxNode io_node)
            {
                var code = new StringBuilder();

                var outputs = io_node.Children.FindAll(x => x.Syntax != "," && x.Syntax != "OUTPUT");
                foreach (var output in outputs) {
                    code.AppendLine($"Console.WriteLine({generateExpression(root, output)});");
                }

                return code.ToString();
            }

            private static string generateConditional(SyntaxNode root, SyntaxNode conditional)
            {
                var code = new StringBuilder();
                int else_index = conditional.Children.FindIndex(x => x.Syntax == "ELSE");

                code.AppendLine($"if ({generateBooleanExpression(root, conditional.Children[1])}) {{");
                var if_statements = conditional
                    .Children
                    .Take(else_index)
                    .Where(x => x.Syntax == Nonterminals.STATEMENT);
                foreach (var statement in if_statements)
                    code.AppendLine(generateStatement(root, statement));
                if (else_index >= 0) {
                    code.AppendLine("}");
                    code.AppendLine("else {");

                    var else_statements = conditional
                        .Children
                        .Skip(else_index)
                        .Where(x => x.Syntax == Nonterminals.STATEMENT);
                    foreach (var statement in else_statements)
                        code.AppendLine(generateStatement(root, statement));
                    //code.AppendLine("}");
                }
                code.AppendLine("}");

                return code.ToString();
            }
            private static string generateLoop(SyntaxNode root, SyntaxNode node)
            {
                var code = new StringBuilder();
                string stop_condition = generateBooleanExpression(root, node.Children[1]);

                code.AppendLine($"while ({stop_condition}) {{");
                var statements_in_block = node.Children.FindAll(x => x.Syntax == Nonterminals.STATEMENT);
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
                    if (element.Syntax == Nonterminals.TERM)
                        code.Append(generateTerm(root, element));
                    else code.Append(element.Syntax);
                }

                return code.ToString();
            }

            private static string generateTerm(SyntaxNode root, SyntaxNode node)
            {
                var code = new StringBuilder();

                foreach (var element in node.Children) {
                    if (element.Syntax == Nonterminals.FACTOR)
                        code.Append(generateFactor(root, element));
                    else code.Append(element.Syntax);
                }

                return code.ToString();
            }

            private static string generateFactor(SyntaxNode root, SyntaxNode node)
            {
                var code = new StringBuilder();

                foreach (var element in node.Children) {
                    if (element.Syntax == Nonterminals.EXPRESSION)
                        code.Append(generateExpression(root, element));
                    else if (element.Syntax == Nonterminals.VALUE)
                        code.Append(generateValue(root, element.Children[0]));
                    else code.Append(element.Syntax);
                }

                return code.ToString();
            }

            private static string generateValue(SyntaxNode root, SyntaxNode node)
            {
                var dangling_radix = new System.Text.RegularExpressions.Regex(@"\.$");
                if (node.Syntax == "Real") {
                    if (dangling_radix.IsMatch(node.Value)) return node.Value + "0f";
                    else return node.Value + "f";
                }
                else return node.Value;
            }

            private static string generateVariableDeclarations(SyntaxNode root, SyntaxNode node)
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
