using System;
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
                    analyseVariableDeclarations(variable_declarations);

                var statements = program_node.Children.FindAll(x => x.Symbol == "<Statement>");
                foreach (var statement in statements) analyseStatement(analyser, statement);
            }

            private static void analyseStatement(SemanticAnalyser analyser, SyntaxNode statement_node) 
                => throw new NotImplementedException();
            private static void analyseVariableDeclarations(SyntaxNode variable_declarations_node)
            {
                var type_nodes = variable_declarations_node
                    .Children
                    .FindAll(x => x.Symbol == "<Type>");
                Console.WriteLine("Types found - " + type_nodes.Count);
                foreach (var type_node in type_nodes) analyseType(type_node);

                var ident_lists = variable_declarations_node
                    .Children
                    .FindAll(x => x.Symbol == "<IdentList>");
                //foreach (var identifier_list in ident_lists) {
                //    SemanticType current_type = type_nodes
                //}
                Console.WriteLine("Identifier lists found - " + ident_lists.Count);
                for (int iii = 0; iii < ident_lists.Count; ++iii) {
                    SemanticType current_type = type_nodes[iii].Type;
                    foreach (var identifier in ident_lists[iii].Children.FindAll(x => x.Symbol == "Identifier"))
                        analyseIdentifier(identifier, current_type);
                }
            }

            private static void analyseIdentifier(SyntaxNode identifier, int current_type) => throw new NotImplementedException();
            private static void analyseType(SyntaxNode type_node) => throw new NotImplementedException();
        }
    }
}
