using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllanMilne.Ardkit;

namespace PALCompiler
{
    internal partial class PALParser : RdParser
    {
        private delegate void OtherRecogniser(PALParser parser, ref SyntaxNode parent);

        private SyntaxNode syntax_tree;

        internal PALParser(IScanner scanner) : base(scanner)
        {
            syntax_tree = new SyntaxNode("<Program>");
        }

        protected override void recStarter()
        {
            consume(ref syntax_tree, "PROGRAM");
            consume(ref syntax_tree, Token.IdentifierToken, scanner.CurrentToken.TokenValue);
            consume(ref syntax_tree, "WITH");
            consume(ref syntax_tree, Recognisers.recogniseVarDecls, "<VarDecls>");
            consume(ref syntax_tree, "IN");
            consume(ref syntax_tree, Recognisers.recogniseStatement, "<Statement>");
            while (!have("END")) consume(ref syntax_tree, Recognisers.recogniseStatement, "<Statement>");
            consume(ref syntax_tree, "END");
        }

        internal SyntaxNode SyntaxTree { get { return syntax_tree; } }

        private bool haveStatement()
        {
            return have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") ||
                have("OUTPUT");
        }

        private void consume(ref SyntaxNode parent, string symbol)
        {
            if (have(symbol)) parent.addChild(new SyntaxNode(symbol));
            mustBe(symbol);
        }

        private void consume(ref SyntaxNode parent, string symbol, string value)
        {
            if (have(symbol)) parent.addChild(new SyntaxNode(symbol + "(" + value + ")"));
            mustBe(symbol);
        }

        private void consume(ref SyntaxNode parent, OtherRecogniser recogniser, string symbol)
        {
            var node = new SyntaxNode(symbol);
            recogniser(this, ref node);
            parent.addChild(node);
        }
    }
}
