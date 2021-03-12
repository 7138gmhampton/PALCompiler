using System;
using System.Collections.Generic;
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

            parser.Errors.AddRange(errors);
            parser.Errors.Sort();
        }

        private void appendSymbol(ISymbol symbol)
        {
            if (!symbols.Add(symbol))
                errors.Add(new AlreadyDeclaredError(symbol.Source, symbols.Get(symbol.Name)));
        }

        private static class Analysers
        {
            internal static void analyseProgram(SemanticAnalyser analyser, SyntaxNode program)
            {
                var variable_declarations = program
                    .Children
                    .Find(x => x.Symbol == Nonterminals.VARIABLE_DECLARATION);
                if (variable_declarations != null)
                    analyseVariableDeclarations(analyser, variable_declarations);

                var statements = program.Children.FindAll(x => x.Symbol == Nonterminals.STATEMENT);
                foreach (var statement in statements) analyseStatement(analyser, statement);
            }

            private static void analyseStatement(SemanticAnalyser analyser, SyntaxNode statement)
            {
                switch (statement.OnlyChild.Symbol) {
                    case Nonterminals.ASSIGMENT:
                        analyseAssignment(analyser, statement.OnlyChild); break;
                    case Nonterminals.CONDITIONAL:
                        analyseConditional(analyser, statement.OnlyChild); break;
                    case Nonterminals.IO:
                        analyseIO(analyser, statement.OnlyChild); break;
                    case Nonterminals.LOOP:
                        analyseLoop(analyser, statement.OnlyChild); break;
                    default:
                        analyser.errors.Add(new SemanticError(
                            statement.Token,
                            "Unrecognised semantic construction"));
                        break;
                }
            }

            private static void analyseLoop(SemanticAnalyser analyser, SyntaxNode onlyChild) => throw new NotImplementedException();
            private static void analyseIO(SemanticAnalyser analyser, SyntaxNode onlyChild) => throw new NotImplementedException();
            private static void analyseConditional(SemanticAnalyser analyser, SyntaxNode conditional)
            {
                analyseBooleanExpression(analyser, conditional.Children[1]);

                var statements = conditional.Children.FindAll(x => x.Symbol == Nonterminals.STATEMENT);
                foreach (var statement in statements) analyseStatement(analyser, statement);
            }

            private static void analyseBooleanExpression(SemanticAnalyser analyser, SyntaxNode syntaxNode) => throw new NotImplementedException();

            private static void analyseAssignment(SemanticAnalyser analyser, SyntaxNode assignment)
            {
                SemanticType left_hand_type = analyseIdentifierUse(analyser, assignment.Children[0]);
                SemanticType right_hand_type = analyseExpression(analyser, assignment.Children[2]);

                if (left_hand_type != right_hand_type)
                    analyser.errors.Add(new TypeConflictError(
                        assignment.Children[2].Token,
                        right_hand_type,
                        left_hand_type));
            }

            private static int analyseExpression(SemanticAnalyser analyser, SyntaxNode expression)
            {
                var terms = expression.Children.FindAll(x => x.Symbol == Nonterminals.TERM);

                expression.Type = -1;
                foreach (var term in terms) {
                    SemanticType current_type = analyseTerm(analyser, term);
                    if (expression.Type < 0) expression.Type = current_type;
                    else if (current_type != expression.Type)
                        analyser.errors.Add(new TypeConflictError(term.Token, current_type, expression.Type));
                }

                return expression.Type;
            }

            private static int analyseTerm(SemanticAnalyser analyser, SyntaxNode term)
            {
                var factors = term.Children.FindAll(x => x.Symbol == Nonterminals.FACTOR);

                term.Type = -1;
                foreach (var factor in factors) {
                    SemanticType current_type = analyseFactor(analyser, factor);
                    if (term.Type < 0) term.Type = current_type;
                    else if (current_type != term.Type)
                        analyser.errors.Add(new TypeConflictError(term.Token, current_type, term.Type));
                }

                return term.Type;
            }

            private static int analyseFactor(SemanticAnalyser analyser, SyntaxNode factor)
            {
                var element = factor.Children.Find(x => x.Symbol != "+" && x.Symbol != "-");

                if (element.Symbol == Nonterminals.VALUE) element.Type = element.OnlyChild.Type;
                else if (element.Symbol == Nonterminals.EXPRESSION)
                    element.Type = analyseExpression(analyser, element);

                factor.Type = element.Type;
                return factor.Type;
            }

            private static int analyseIdentifierUse(SemanticAnalyser analyser, SyntaxNode identifier)
            {
                var symbol = analyser.symbols.Get(identifier.Value);

                if (symbol == null) analyser.errors.Add(new NotDeclaredError(identifier.Token));
                else identifier.Type = symbol.Type;

                return identifier.Type;
            }

            private static void analyseVariableDeclarations(
                SemanticAnalyser analyser, 
                SyntaxNode variable_declarations_node)
            {
                var type_nodes = variable_declarations_node
                    .Children
                    .FindAll(x => x.Symbol == Nonterminals.TYPE);
                foreach (var type_node in type_nodes) analyseType(analyser, type_node);

                var ident_lists = variable_declarations_node
                    .Children
                    .FindAll(x => x.Symbol == Nonterminals.IDENTIFIER_LIST);
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
