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

            }
        }
    }
}
