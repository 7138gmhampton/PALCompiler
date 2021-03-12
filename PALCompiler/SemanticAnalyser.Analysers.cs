using AllanMilne.Ardkit;

namespace PALCompiler
{
    using SemanticType = System.Int32;

    /// <summary>
    /// This subclass contains the full set of semantic analysers for each of 
    /// the Pretty Auful Language's rules
    /// </summary>
    partial class SemanticAnalyser
    {
        private static class Analysers
        {
            internal static void analyseProgram(SemanticAnalyser analyser, SyntaxNode program)
            {
                var variable_declarations = program
                    .Children
                    .Find(x => x.Syntax == Nonterminals.VARIABLE_DECLARATION);
                if (variable_declarations != null)
                    analyseVariableDeclarations(analyser, variable_declarations);

                var statements = program.Children.FindAll(x => x.Syntax == Nonterminals.STATEMENT);
                foreach (var statement in statements) analyseStatement(analyser, statement);
            }

            private static void analyseStatement(SemanticAnalyser analyser, SyntaxNode statement)
            {
                switch (statement.OnlyChild.Syntax) {
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

            private static void analyseLoop(SemanticAnalyser analyser, SyntaxNode loop)
            {
                analyseBooleanExpression(analyser, loop.Children[1]);

                var statements = loop.Children.FindAll(x => x.Syntax == Nonterminals.STATEMENT);
                foreach (var statement in statements) analyseStatement(analyser, statement);
            }

            private static void analyseIO(SemanticAnalyser analyser, SyntaxNode io)
            {
                if (io.Children[1].Syntax == Nonterminals.IDENTIFIER_LIST) {
                    var identifiers = io.Children[1].Children.FindAll(x => x.Syntax == "Identifer");
                    foreach (var identifier in identifiers) analyseIdentifierUse(analyser, identifier);
                }
                else {
                    var expressions = io.Children.FindAll(x => x.Syntax == Nonterminals.EXPRESSION);
                    foreach (var expression in expressions) analyseExpression(analyser, expression);
                }
            }

            private static void analyseConditional(SemanticAnalyser analyser, SyntaxNode conditional)
            {
                analyseBooleanExpression(analyser, conditional.Children[1]);

                var statements = conditional.Children.FindAll(x => x.Syntax == Nonterminals.STATEMENT);
                foreach (var statement in statements) analyseStatement(analyser, statement);
            }

            private static void analyseBooleanExpression(SemanticAnalyser analyser, SyntaxNode boolean)
            {
                SemanticType left_hand_type = analyseExpression(analyser, boolean.Children[0]);
                SemanticType right_hand_type = analyseExpression(analyser, boolean.Children[2]);

                if (left_hand_type != right_hand_type)
                    analyser.errors.Add(new TypeConflictError(
                        boolean.Children[2].Token,
                        right_hand_type,
                        left_hand_type));
            }

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
                var terms = expression.Children.FindAll(x => x.Syntax == Nonterminals.TERM);

                expression.Semantic = -1;
                foreach (var term in terms) {
                    SemanticType current_type = analyseTerm(analyser, term);
                    if (expression.Semantic < 0) expression.Semantic = current_type;
                    else if (current_type != expression.Semantic)
                        analyser.errors.Add(new TypeConflictError(term.Token, current_type, expression.Semantic));
                }

                return expression.Semantic;
            }

            private static SemanticType analyseTerm(SemanticAnalyser analyser, SyntaxNode term)
            {
                var factors = term.Children.FindAll(x => x.Syntax == Nonterminals.FACTOR);

                term.Semantic = -1;
                foreach (var factor in factors) {
                    SemanticType current_type = analyseFactor(analyser, factor);
                    if (term.Semantic < 0) term.Semantic = current_type;
                    else if (current_type != term.Semantic)
                        analyser.errors.Add(new TypeConflictError(term.Token, current_type, term.Semantic));
                }

                return term.Semantic;
            }

            private static SemanticType analyseFactor(SemanticAnalyser analyser, SyntaxNode factor)
            {
                //var element = factor.Children.Find(x => x.Syntax != "+" && x.Syntax != "-");
                var element = factor
                    .Children
                    .Find(x => x.Syntax == Nonterminals.VALUE || x.Syntax == Nonterminals.EXPRESSION);

                if (element.Syntax == Nonterminals.VALUE)
                    element.Semantic = analyseValue(analyser, element);
                else if (element.Syntax == Nonterminals.EXPRESSION)
                    element.Semantic = analyseExpression(analyser, element);

                factor.Semantic = element.Semantic;
                return factor.Semantic;
            }

            private static SemanticType analyseValue(SemanticAnalyser analyser, SyntaxNode element)
            {
                if (element.OnlyChild.Syntax == "Identifier")
                    return analyseIdentifierUse(analyser, element.OnlyChild);
                else return element.OnlyChild.Semantic;
            }

            private static int analyseIdentifierUse(SemanticAnalyser analyser, SyntaxNode identifier)
            {
                var symbol = analyser.symbols.Get(identifier.Value);

                if (symbol == null) analyser.errors.Add(new NotDeclaredError(identifier.Token));
                else identifier.Semantic = symbol.Type;
                
                return identifier.Semantic;
            }

            private static void analyseVariableDeclarations(
                SemanticAnalyser analyser, 
                SyntaxNode variable_declarations_node)
            {
                var type_nodes = variable_declarations_node
                    .Children
                    .FindAll(x => x.Syntax == Nonterminals.TYPE);
                foreach (var type_node in type_nodes) analyseType(analyser, type_node);

                var identifier_lists = variable_declarations_node
                    .Children
                    .FindAll(x => x.Syntax == Nonterminals.IDENTIFIER_LIST);
                for (int iii = 0; iii < identifier_lists.Count; ++iii) {
                    foreach (var identifier in identifier_lists[iii]
                        .Children
                        .FindAll(x => x.Syntax == "Identifier"))
                        analyseIdentifier(analyser, identifier, type_nodes[iii].Semantic);
                }
            }

            private static void analyseIdentifier(
                SemanticAnalyser analyser, 
                SyntaxNode identifier, 
                SemanticType type)
            {
                identifier.Semantic = type;
                analyser.appendSymbol(new VarSymbol(identifier.Token, type));
            }

            private static void analyseType(SemanticAnalyser analyser, SyntaxNode type)
            {
                type.Semantic = type.OnlyChild.Semantic;
                if (type.Semantic == LanguageType.Undefined)
                    analyser.errors.Add(new SemanticError(
                        type.OnlyChild.Token,
                        "Type cannot be determined from declarator"));
            }
        }
    }
}
