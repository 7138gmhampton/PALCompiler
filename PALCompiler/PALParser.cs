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

        private SyntaxNode syntax_tree;

        internal PALParser(IScanner scanner) : base(scanner)
        {
            syntax_tree = new SyntaxNode("<Program>");
        }

        protected override void recStarter()
        {
            //syntax_tree.addChild(new SyntaxNode("<Program>"));
            //updateTree("<Program>");
            //syntax_tree.addChild(new SyntaxNode("PROGRAM"));
            updateTree(ref syntax_tree, "PROGRAM");
            mustBe("PROGRAM");
            //syntax_tree.addChild(new SyntaxNode("Identifier(" + scanner.CurrentToken.TokenValue + ")"));
            updateTree(ref syntax_tree, Token.IdentifierToken, scanner.CurrentToken.TokenValue);
            mustBe(Token.IdentifierToken);
            //syntax_tree.addChild(new SyntaxNode("WITH"));
            updateTree(ref syntax_tree, "WITH");
            mustBe("WITH");
            //syntax_tree.addChild()
            SyntaxNode var_declaration_node = new SyntaxNode("<VarDecls>");
            recogniseVarDecls(ref var_declaration_node);
            syntax_tree.addChild(var_declaration_node);
            //syntax_tree.addChild(new SyntaxNode("IN"));
            updateTree(ref syntax_tree, "IN");
            mustBe("IN");
            var statement_node = new SyntaxNode("<Statement>");
            recogniseStatement();
            syntax_tree.addChild(statement_node);
            //if (!have("END")) 
            while (!have("END")) {
                var continuing_statement_node = new SyntaxNode("<Statement>");
                recogniseStatement();
                syntax_tree.addChild(continuing_statement_node);
            }
            //syntax_tree.addChild(new SyntaxNode("END"));
            updateTree(ref syntax_tree, "END");
            mustBe("END");
            //syntax_tree.addChild(new SyntaxNode("Program"))
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
            recogniser(ref parent);
            parent.addChild(node);
        }

        private void recogniseStatement()
        {
            //syntax_tree.addChild(new SyntaxNode("<Statement>"));
            if (have(Token.IdentifierToken)) recogniseAssignment();
            else if (have("UNTIL")) recogniseLoop();
            else if (have("IF")) recogniseConditional();
            else recogniseIO();
        }

        private void recogniseIO()
        {
            if (have("INPUT")) {
                mustBe("INPUT");
                recogniseIdentList();
            }
            else {
                mustBe("OUTPUT");
                recogniseExpression();
                while (have(",")) {
                    mustBe(",");
                    recogniseExpression();
                }
            }
        }

        private void recogniseConditional()
        {
            mustBe("IF");
            recogniseBooleanExpr();
            mustBe("THEN");
            while (haveStatement()) recogniseStatement();
            if (have("ELSE")) {
                mustBe("ELSE");
                while (haveStatement()) recogniseStatement();
            }
            mustBe("ENDIF");
        }

        private void recogniseLoop()
        {
            mustBe("UNTIL");
            recogniseBooleanExpr();
            mustBe("REPEAT");
            while (have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") || 
                have("OUTPUT")) {
                recogniseStatement();
            }
            mustBe("ENDLOOP");
        }

        private void recogniseBooleanExpr()
        {
            recogniseExpression();
            if (have("<")) mustBe("<");
            else if (have("=")) mustBe("=");
            else mustBe(">");
            recogniseExpression();
        }

        private void recogniseAssignment()
        {
            mustBe(Token.IdentifierToken);
            mustBe("=");
            recogniseExpression();
        }

        private void recogniseExpression()
        {
            recogniseTerm();
            while (have("+") || have("-")) {
                if (have("+")) mustBe("+");
                else mustBe("-");
                recogniseTerm();
            }
        }

        private void recogniseTerm()
        {
            recogniseFactor();
            while (have("*") || have("/")) {
                if (have("*")) mustBe("*");
                else mustBe("/");
                recogniseFactor();
            }
        }

        private void recogniseFactor()
        {
            if (have("+")) mustBe("+");
            else if (have("-")) mustBe("-");

            if (have("(")) {
                mustBe("(");
                recogniseExpression();
                mustBe(")");
            }
            else recogniseValue();
        }

        private void recogniseValue()
        {
            if (have(Token.IdentifierToken)) mustBe(Token.IdentifierToken);
            else if (have(Token.IntegerToken)) mustBe(Token.IntegerToken);
            else mustBe(Token.RealToken);
        }

        private void recogniseVarDecls(ref SyntaxNode node)
        {
            //syntax_tree.addChild(new SyntaxNode("<VarDecls>"));
            //if (have(Token.IdentifierToken)) recogniseIdentList();
            while (have(Token.IdentifierToken)) {
                recogniseIdentList();
                mustBe("AS");
                recogniseType();
            }
        }

        private void recogniseType()
        {
            if (have("REAL")) mustBe("REAL");
            else mustBe("INTEGER");
        }

        private void recogniseIdentList()
        {
            mustBe(Token.IdentifierToken);
            while (have(",")) {
                mustBe(",");
                mustBe(Token.IdentifierToken);
            }
        }
    }
}
