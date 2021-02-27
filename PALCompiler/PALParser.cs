﻿using System;
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
            //updateTree(ref syntax_tree, "PROGRAM");
            //mustBe("PROGRAM");
            consume(ref syntax_tree, "PROGRAM");
            //updateTree(ref syntax_tree, Token.IdentifierToken, scanner.CurrentToken.TokenValue);
            //mustBe(Token.IdentifierToken);
            consume(ref syntax_tree, Token.IdentifierToken, scanner.CurrentToken.TokenValue);
            //updateTree(ref syntax_tree, "WITH");
            //mustBe("WITH");
            consume(ref syntax_tree, "WITH");
            //SyntaxNode var_declaration_node = new SyntaxNode("<VarDecls>");
            //recogniseVarDecls(ref var_declaration_node);
            //syntax_tree.addChild(var_declaration_node);
            consume(ref syntax_tree, recogniseVarDecls, "<VarDecls>");
            //updateTree(ref syntax_tree, "IN");
            //mustBe("IN");
            consume(ref syntax_tree, "IN");
            //var statement_node = new SyntaxNode("<Statement>");
            //recogniseStatement(ref syntax_tree);
            //syntax_tree.addChild(statement_node);
            consume(ref syntax_tree, recogniseStatement, "<Statement>");
            //while (!have("END")) {
            //    var continuing_statement_node = new SyntaxNode("<Statement>");
            //    recogniseStatement(ref syntax_tree);
            //    syntax_tree.addChild(continuing_statement_node);
            //}
            while (!have("END")) consume(ref syntax_tree, recogniseStatement, "<Statement>");
            //updateTree(ref syntax_tree, "END");
            //mustBe("END");
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

        private void recogniseStatement(ref SyntaxNode parent)
        {
            if (have(Token.IdentifierToken)) recogniseAssignment();
            else if (have("UNTIL")) recogniseLoop(ref parent);
            else if (have("IF")) recogniseConditional(ref parent);
            else recogniseIO(ref parent);
        }

        private void recogniseIO(ref SyntaxNode parent)
        {
            if (have("INPUT")) {
                mustBe("INPUT");
                recogniseIdentList(ref parent);
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

        private void recogniseConditional(ref SyntaxNode parent)
        {
            mustBe("IF");
            recogniseBooleanExpr();
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
            recogniseBooleanExpr();
            mustBe("REPEAT");
            while (have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") || 
                have("OUTPUT")) {
                recogniseStatement(ref parent);
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

        private void recogniseVarDecls(ref SyntaxNode parent)
        {
            while (have(Token.IdentifierToken)) {
                consume(ref parent, recogniseIdentList, "<IdentList>");
                consume(ref parent, "AS");
                consume(ref parent, recogniseType, "<Type>");
            }
        }

        private void recogniseType(ref SyntaxNode parent)
        {
            if (have("REAL")) mustBe("REAL");
            else mustBe("INTEGER");
        }

        private void recogniseIdentList(ref SyntaxNode parent)
        {
            mustBe(Token.IdentifierToken);
            while (have(",")) {
                mustBe(",");
                mustBe(Token.IdentifierToken);
            }
        }
    }
}
