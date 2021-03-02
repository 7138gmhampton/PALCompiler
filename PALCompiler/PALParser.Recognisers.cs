using AllanMilne.Ardkit;

namespace PALCompiler
{
    internal partial class PALParser
    {
        private static class Recognisers
        {
            internal static void recogniseIdentList(PALParser parser, ref SyntaxNode parent)
            {
                parser.consume(ref parent, Token.IdentifierToken, 
                    parser.scanner.CurrentToken.TokenValue);
                while (parser.have(",")) {
                    parser.consume(ref parent, ",");
                    parser.consume(ref parent, Token.IdentifierToken,
                        parser.scanner.CurrentToken.TokenValue);
                }
            }

            internal static void recogniseType(PALParser parser, ref SyntaxNode parent)
            {
                if (parser.have("REAL")) parser.consume(ref parent, "REAL");
                else parser.consume(ref parent, "INTEGER");
            }

            internal static void recogniseVarDecls(PALParser parser, ref SyntaxNode parent)
            {
                while (parser.have(Token.IdentifierToken)) {
                    parser.consume(ref parent, Recognisers.recogniseIdentList);
                    parser.consume(ref parent, "AS");
                    parser.consume(ref parent, Recognisers.recogniseType);
                }
            }

            internal static void recogniseValue(PALParser parser, ref SyntaxNode parent)
            {
                if (parser.have(Token.IdentifierToken))
                    parser.consume(ref parent, Token.IdentifierToken, parser.scanner.CurrentToken.TokenValue);
                else if (parser.have(Token.IntegerToken))
                    parser.consume(ref parent, Token.IntegerToken, parser.scanner.CurrentToken.TokenValue);
                else
                    parser.consume(ref parent, Token.RealToken, parser.scanner.CurrentToken.TokenValue);
            }

            internal static void recogniseFactor(PALParser parser, ref SyntaxNode parent)
            {
                if (parser.have("+")) parser.consume(ref parent, "+");
                else if (parser.have("-")) parser.consume(ref parent, "-");

                if (parser.have("(")) {
                    parser.consume(ref parent, "(");
                    parser.consume(ref parent, recogniseExpression);
                    parser.consume(ref parent, ")");
                }
                else parser.consume(ref parent, recogniseValue);
            }

            internal static void recogniseTerm(PALParser parser, ref SyntaxNode parent)
            {
                Recognisers.recogniseFactor(parser, ref parent);
                while (parser.have("*") || parser.have("/")) {
                    if (parser.have("*")) parser.consume(ref parent, "*");
                    else parser.consume(ref parent, "/");
                    parser.consume(ref parent, recogniseFactor);
                }
            }

            internal static void recogniseAssignment(PALParser parser, ref SyntaxNode parent)
            {
                parser.consume(ref parent, Token.IdentifierToken, parser.scanner.CurrentToken.TokenValue);
                parser.consume(ref parent, "=");
                parser.consume(ref parent, recogniseExpression);
            }

            internal static void recogniseStatement(PALParser parser, ref SyntaxNode parent)
            {
                if (parser.have(Token.IdentifierToken)) parser.consume(ref parent, recogniseAssignment);
                else if (parser.have("UNTIL")) parser.consume(ref parent, recogniseLoop);
                else if (parser.have("IF")) parser.consume(ref parent, recogniseConditional);
                else parser.consume(ref parent, recogniseIO);
            }

            internal static void recogniseExpression(PALParser parser, ref SyntaxNode parent)
            {
                parser.consume(ref parent, recogniseTerm);
                while (parser.have("+") || parser.have("-")) {
                    if (parser.have("+")) parser.consume(ref parent, "+");
                    else parser.consume(ref parent, "-");
                    parser.consume(ref parent, recogniseTerm);
                }
            }

            internal static void recogniseIO(PALParser parser, ref SyntaxNode parent)
            {
                if (parser.have("INPUT")) {
                    parser.consume(ref parent, "INPUT");
                    parser.consume(ref parent, recogniseIdentList);
                }
                else {
                    parser.consume(ref parent, "OUTPUT");
                    parser.consume(ref parent, recogniseExpression);
                    while (parser.have(",")) {
                        parser.consume(ref parent, ",");
                        parser.consume(ref parent, recogniseExpression);
                    }
                }
            }

            internal static void recogniseConditional(PALParser parser, ref SyntaxNode parent)
            {
                parser.consume(ref parent, "IF");
                parser.consume(ref parent, recogniseBooleanExpr);
                parser.consume(ref parent, "THEN");
                while (parser.haveStatement()) parser.consume(ref parent, recogniseStatement);
                if (parser.have("ELSE")) {
                    parser.consume(ref parent, "ELSE");
                    while (parser.haveStatement()) parser.consume(ref parent, recogniseStatement);
                }
                parser.consume(ref parent, "ENDIF");
            }

            internal static void recogniseLoop(PALParser parser, ref SyntaxNode parent)
            {
                parser.consume(ref parent, "UNTIL");
                parser.consume(ref parent, recogniseBooleanExpr);
                parser.consume(ref parent, "REPEAT");
                while (parser.haveStatement()) parser.consume(ref parent, recogniseStatement);
                parser.consume(ref parent, "ENDLOOP");
            }

            internal static void recogniseBooleanExpr(PALParser parser, ref SyntaxNode parent)
            {
                parser.consume(ref parent, recogniseExpression);
                if (parser.have("<")) parser.consume(ref parent, "<");
                else if (parser.have("=")) parser.consume(ref parent, "=");
                else parser.consume(ref parent, ">");
                parser.consume(ref parent, recogniseExpression);
            }
        }
    }
}
