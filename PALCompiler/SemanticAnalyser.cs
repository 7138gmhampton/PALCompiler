﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllanMilne.Ardkit;

namespace PALCompiler
{
    using SemanticType = System.Int32;

    class SemanticAnalyser
    {
        private SymbolTable symbols;
        private SyntaxNode syntax_tree;
        private PALParser parser;
        private List<ICompilerError> errors;

        internal SemanticAnalyser(PALParser parser, SyntaxNode syntax_tree, SymbolTable symbols)
        {
            this.symbols = symbols;
            this.syntax_tree = syntax_tree;
            this.parser = parser;

            errors = new List<ICompilerError>();
        }

        internal void analyse()
        {
            Analysers.analyseProgram(this, syntax_tree);
        }

        private void appendSymbol(ISymbol symbol)
        {
            if (!symbols.Add(symbol))
                errors.Add(new AlreadyDeclaredError(symbol.Source, symbols.Get(symbol.Name)));
        }

        private static class Analysers
        {
            internal static void analyseProgram(SemanticAnalyser analyser, SyntaxNode program_node)
            {
                var variable_declarations = program_node.Children.Find(x => x.Symbol == "<VarDecls>");
                if (variable_declarations != null)
                    analyseVariableDeclarations(analyser, variable_declarations);

                var statements = program_node.Children.FindAll(x => x.Symbol == "<Statement>");
                foreach (var statement in statements) analyseStatement(analyser, statement);
            }

            private static void analyseStatement(SemanticAnalyser analyser, SyntaxNode statement_node) 
                => throw new NotImplementedException();

            private static void analyseVariableDeclarations(SemanticAnalyser analyser, SyntaxNode variable_declarations_node)
            {
                var type_nodes = variable_declarations_node
                    .Children
                    .FindAll(x => x.Symbol == "<Type>");
                foreach (var type_node in type_nodes) analyseType(analyser, type_node);

                var ident_lists = variable_declarations_node
                    .Children
                    .FindAll(x => x.Symbol == "<IdentList>");
                for (int iii = 0; iii < ident_lists.Count; ++iii) {
                    SemanticType current_type = type_nodes[iii].Type;
                    foreach (var identifier in ident_lists[iii].Children.FindAll(x => x.Symbol == "Identifier"))
                        analyseIdentifier(analyser, identifier, current_type);
                }
            }

            private static void analyseIdentifier(
                SemanticAnalyser analyser, 
                SyntaxNode identifier, 
                SemanticType type)
            {
                identifier.Type = type;
                analyser.appendSymbol(new VarSymbol(identifier.Token, type));
            }

            private static void analyseType(SemanticAnalyser analyser, SyntaxNode type)
            {
                type.Type = type.OnlyChild.Type;
                if (type.Type == LanguageType.Undefined)
                    analyser.errors.Add(new SemanticError(
                        type.OnlyChild.Token,
                        "Type cannot be determined from declarator"));
            }
        }
    }
}
