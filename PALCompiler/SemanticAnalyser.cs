using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllanMilne.Ardkit;

namespace PALCompiler
{
    class SemanticAnalyser
    {
        private SymbolTable symbols;
        private SyntaxNode syntax_tree;
        private PALParser parser;

        internal SemanticAnalyser(PALParser parser, SyntaxNode syntax_tree, SymbolTable symbols)
        {
            this.symbols = symbols;
            this.syntax_tree = syntax_tree;
            this.parser = parser;
        }

        internal void analyse()
        {
            Analysers.analyseProgram(this, syntax_tree);
        }

        private void appendSymbol(ISymbol symbol)
        {
            //if (symbols.IsDefined(symbol.Name))
            //    parser.Errors.Add(new AlreadyDeclaredError(symbol.Source, symbols.Get(symbol.Name)));
            //else symbols.Add
            if (!symbols.Add(symbol))
                parser.Errors.Add(new AlreadyDeclaredError(symbol.Source, symbols.Get(symbol.Name)));
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
            private static void analyseVariableDeclarations(
                SemanticAnalyser analyser, 
                SyntaxNode variable_declarations_node) 
                => throw new NotImplementedException();
        }
    }
}
