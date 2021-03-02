using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllanMilne.Ardkit;

namespace PALCompiler
{
    internal class PALParser : RdParser
    {
        private delegate void Recogniser(ref SyntaxNode parent);
        private delegate void OtherRecogniser(PALParser parser, ref SyntaxNode parent);

        //private Recognisers recognisers;
        private SyntaxNode syntax_tree;

        internal PALParser(IScanner scanner) : base(scanner)
        {
            //recognisers = new Recognisers();
            syntax_tree = new SyntaxNode("<Program>");
        }

        protected override void recStarter()
        {
            consume(ref syntax_tree, "PROGRAM");
            consume(ref syntax_tree, Token.IdentifierToken, scanner.CurrentToken.TokenValue);
            consume(ref syntax_tree, "WITH");
            consume(ref syntax_tree, Recognisers.recogniseVarDecls, "<VarDecls>");
            consume(ref syntax_tree, "IN");
            consume(ref syntax_tree, recogniseStatement, "<Statement>");
            while (!have("END")) consume(ref syntax_tree, recogniseStatement, "<Statement>");
            consume(ref syntax_tree, "END");
        }

        internal SyntaxNode SyntaxTree { get { return syntax_tree; } }

        private bool haveStatement()
        {
            return have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") ||
                have("OUTPUT");
        }

        private void updateTree(ref SyntaxNode node, string symbol)
        {
            if (have(symbol)) node.addChild(new SyntaxNode(symbol));
        }

        private void updateTree(ref SyntaxNode node, string symbol, string value)
        {
            if (have(symbol)) node.addChild(new SyntaxNode(symbol + "(" + value + ")"));
        }

        private void consume(ref SyntaxNode node, string symbol)
        {
            updateTree(ref node, symbol);
            mustBe(symbol);
        }

        private void consume(ref SyntaxNode node, string symbol, string value)
        {
            updateTree(ref node, symbol, value);
            mustBe(symbol);
        }

        private void consume(ref SyntaxNode parent, Recogniser recogniser, string symbol)
        {
            var node = new SyntaxNode(symbol);
            recogniser(ref node);
            parent.addChild(node);
        }

        private void consume(ref SyntaxNode parent, OtherRecogniser recogniser, string symbol)
        {
            var node = new SyntaxNode(symbol);
            recogniser(this, ref node);
            parent.addChild(node);
        }

        private void recogniseStatement(ref SyntaxNode parent)
        {
            if (have(Token.IdentifierToken)) recogniseAssignment(ref parent);
            else if (have("UNTIL")) recogniseLoop(ref parent);
            else if (have("IF")) recogniseConditional(ref parent);
            else recogniseIO(ref parent);
        }

        private void recogniseIO(ref SyntaxNode parent)
        {
            if (have("INPUT")) {
                mustBe("INPUT");
                Recognisers.recogniseIdentList(this, ref parent);
            }
            else {
                mustBe("OUTPUT");
                recogniseExpression(ref parent);
                while (have(",")) {
                    mustBe(",");
                    recogniseExpression(ref parent);
                }
            }
        }

        private void recogniseConditional(ref SyntaxNode parent)
        {
            mustBe("IF");
            recogniseBooleanExpr(ref parent);
            mustBe("THEN");
            while (haveStatement()) recogniseStatement(ref parent);
            if (have("ELSE")) {
                mustBe("ELSE");
                while (haveStatement()) recogniseStatement(ref parent);
            }
            mustBe("ENDIF");
        }

        private void recogniseLoop(ref SyntaxNode parent)
        {
            mustBe("UNTIL");
            recogniseBooleanExpr(ref parent);
            mustBe("REPEAT");
            while (have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") || 
                have("OUTPUT")) {
                recogniseStatement(ref parent);
            }
            mustBe("ENDLOOP");
        }

        private void recogniseBooleanExpr(ref SyntaxNode parent)
        {
            recogniseExpression(ref parent);
            if (have("<")) mustBe("<");
            else if (have("=")) mustBe("=");
            else mustBe(">");
            recogniseExpression(ref parent);
        }

        private void recogniseAssignment(ref SyntaxNode parent)
        {
            mustBe(Token.IdentifierToken);
            mustBe("=");
            recogniseExpression(ref parent);
        }

        private void recogniseExpression(ref SyntaxNode parent)
        {
            recogniseTerm(ref parent);
            while (have("+") || have("-")) {
                if (have("+")) mustBe("+");
                else mustBe("-");
                recogniseTerm(ref parent);
            }
        }

        private void recogniseTerm(ref SyntaxNode parent)
        {
            Recognisers.recogniseFactor(this, ref parent);
            while (have("*") || have("/")) {
                if (have("*")) mustBe("*");
                else mustBe("/");
                Recognisers.recogniseFactor(this, ref parent);
            }
        }

        //private void recogniseFactor(ref SyntaxNode parent)
        //{
        //    if (have("+")) mustBe("+");
        //    else if (have("-")) mustBe("-");

        //    if (have("(")) {
        //        mustBe("(");
        //        recogniseExpression(ref parent);
        //        mustBe(")");
        //    }
        //    //else recogniseValue();
        //    else Recognisers.recogniseValue(this, ref parent);
        //}

        //private void recogniseValue()
        //{
        //    if (have(Token.IdentifierToken)) mustBe(Token.IdentifierToken);
        //    else if (have(Token.IntegerToken)) mustBe(Token.IntegerToken);
        //    else mustBe(Token.RealToken);
        //}

        //private void recogniseVarDecls(ref SyntaxNode parent)
        //{
        //    while (have(Token.IdentifierToken)) {
        //        consume(ref parent, Recognisers.recogniseIdentList, "<IdentList>");
        //        consume(ref parent, "AS");
        //        consume(ref parent, Recognisers.recogniseType, "<Type>");
        //    }
        //}

        //private void recogniseType(ref SyntaxNode parent)
        //{
        //    if (have("REAL")) mustBe("REAL");
        //    else mustBe("INTEGER");
        //}

        //private void recogniseIdentList(ref SyntaxNode parent)
        //{
        //    mustBe(Token.IdentifierToken);
        //    while (have(",")) {
        //        mustBe(",");
        //        mustBe(Token.IdentifierToken);
        //    }
        //}

        private static class Recognisers
        {
            internal static void recogniseIdentList(PALParser parser, ref SyntaxNode parent)
            {
                //parser.mustBe(Token.IdentifierToken);
                parser.consume(ref parent, Token.IdentifierToken, 
                    parser.scanner.CurrentToken.TokenValue);
                while (parser.have(",")) {
                    //parser.mustBe(",");
                    parser.consume(ref parent, ",");
                    //parser.mustBe(Token.IdentifierToken);
                    parser.consume(ref parent, Token.IdentifierToken,
                        parser.scanner.CurrentToken.TokenValue);
                }
            }

            internal static void recogniseType(PALParser parser, ref SyntaxNode parent)
            {
                //if (have("REAL")) mustBe("REAL");
                if (parser.have("REAL")) parser.consume(ref parent, "REAL");
                //else mustBe("INTEGER");
                else parser.consume(ref parent, "INTEGER");
            }

            internal static void recogniseVarDecls(PALParser parser, ref SyntaxNode parent)
            {
                while (parser.have(Token.IdentifierToken)) {
                    parser.consume(ref parent, Recognisers.recogniseIdentList, "<IdentList>");
                    parser.consume(ref parent, "AS");
                    parser.consume(ref parent, Recognisers.recogniseType, "<Type>");
                }
            }

            internal static void recogniseValue(PALParser parser, ref SyntaxNode parent)
            {
                //if (have(Token.IdentifierToken)) mustBe(Token.IdentifierToken);
                //else if (have(Token.IntegerToken)) mustBe(Token.IntegerToken);
                //else mustBe(Token.RealToken);
                if (parser.have(Token.IdentifierToken))
                    parser.consume(ref parent, Token.IdentifierToken, parser.scanner.CurrentToken.TokenValue);
                else if (parser.have(Token.IntegerToken))
                    parser.consume(ref parent, Token.IntegerToken, parser.scanner.CurrentToken.TokenValue);
                else
                    parser.consume(ref parent, Token.RealToken, parser.scanner.CurrentToken.TokenValue);
            }

            internal static void recogniseFactor(PALParser parser, ref SyntaxNode parent)
            {
                //if (have("+")) mustBe("+");
                //else if (have("-")) mustBe("-");
                if (parser.have("+")) parser.consume(ref parent, "+");
                else if (parser.have("-")) parser.consume(ref parent, "-");

                //if (have("(")) {
                //    mustBe("(");
                //    recogniseExpression(ref parent);
                //    mustBe(")");
                //}
                if (parser.have("(")) {
                    parser.consume(ref parent, "(");
                    parser.consume(ref parent, parser.recogniseExpression, "<Expression>");
                    parser.consume(ref parent, ")");
                }
                //else recogniseValue();
                //else Recognisers.recogniseValue(this, ref parent);
                else recogniseValue(parser, ref parent);
            }
        }

        //private class Recognisers
        //{
        //    private void recogniseIdentList(ref SyntaxNode parent)
        //    {
        //        mustBe(Token.IdentifierToken);
        //        while (have(",")) {
        //            mustBe(",");
        //            mustBe(Token.IdentifierToken);
        //        }
        //    }
        //}
    }
}
