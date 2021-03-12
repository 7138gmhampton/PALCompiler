using System.Collections.Generic;
using AllanMilne.Ardkit;

namespace PALCompiler
{
    internal partial class PALParser : RdParser
    {
        private delegate void Recogniser(PALParser parser, ref SyntaxNode parent);

        private SyntaxNode syntax_tree;

        private static readonly Dictionary<Recogniser, string> nonterminals =
            new Dictionary<Recogniser, string>
            {
                { Recognisers.recogniseAssignment, Nonterminals.ASSIGMENT },
                { Recognisers.recogniseBooleanExpr, Nonterminals.BOOLEAN_EXPRESSION },
                { Recognisers.recogniseConditional, Nonterminals.CONDITIONAL },
                { Recognisers.recogniseExpression, Nonterminals.EXPRESSION },
                { Recognisers.recogniseFactor, Nonterminals.FACTOR },
                { Recognisers.recogniseIdentList, Nonterminals.IDENTIFIER_LIST },
                { Recognisers.recogniseIO, Nonterminals.IO },
                { Recognisers.recogniseLoop, Nonterminals.LOOP },
                { Recognisers.recogniseStatement, Nonterminals.STATEMENT },
                { Recognisers.recogniseTerm, Nonterminals.TERM },
                { Recognisers.recogniseType, Nonterminals.TYPE },
                { Recognisers.recogniseValue, Nonterminals.VALUE },
                { Recognisers.recogniseVarDecls, Nonterminals.VARIABLE_DECLARATION }
            };

        internal PALParser(IScanner scanner) : base(scanner)
        {
            syntax_tree = new SyntaxNode(null, new Token("<Program>", 0, 0));
        }

        protected override void recStarter()
        {
            consume(ref syntax_tree, "PROGRAM");
            consume(ref syntax_tree, Token.IdentifierToken, scanner.CurrentToken.TokenValue);
            consume(ref syntax_tree, "WITH");
            consume(ref syntax_tree, Recognisers.recogniseVarDecls);
            consume(ref syntax_tree, "IN");
            consume(ref syntax_tree, Recognisers.recogniseStatement);
            while (!have("END")) consume(ref syntax_tree, Recognisers.recogniseStatement);
            consume(ref syntax_tree, "END");
        }

        internal SyntaxNode SyntaxTree { get { return syntax_tree; } }

        private bool haveStatement() => 
            have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") || have("OUTPUT");

        private void consume(ref SyntaxNode parent, string symbol)
        {
            if (have(symbol)) parent.addChild(new SyntaxNode(parent, scanner.CurrentToken));
            mustBe(symbol);
        }

        private void consume(ref SyntaxNode parent, string symbol, string value)
        {
            if (have(symbol)) parent.addChild(new SyntaxNode(parent, scanner.CurrentToken));
            mustBe(symbol);
        }

        private void consume(ref SyntaxNode parent, Recogniser recogniser)
        {
            var node = new SyntaxNode(parent, new Token(
                nonterminals[recogniser], 
                scanner.CurrentToken.Line, 
                scanner.CurrentToken.Column));
            recogniser(this, ref node);
            parent.addChild(node);
        }
    }
}
