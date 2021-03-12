using System;
using System.Collections.Generic;
using AllanMilne.Ardkit;

namespace PALCompiler
{
    partial class SemanticAnalyser
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

            parser.Errors.AddRange(errors);
            parser.Errors.Sort();
        }

        private void appendSymbol(ISymbol symbol)
        {
            if (!symbols.Add(symbol))
                errors.Add(new AlreadyDeclaredError(symbol.Source, symbols.Get(symbol.Name)));
        }
    }
}
