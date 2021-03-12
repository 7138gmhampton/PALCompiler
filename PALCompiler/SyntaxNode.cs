using System;
using System.Collections.Generic;
using AllanMilne.Ardkit;

namespace PALCompiler
{
    using SemanticType = System.Int32;

    class SyntaxNode
    {
        private List<SyntaxNode> children;
        private SyntaxNode parent;
        private string symbol;
        private string value;
        private SemanticType semantic_type;
        private IToken token;

        internal SyntaxNode(SyntaxNode parent, string symbol)
        {
            this.symbol = symbol;
            this.parent = parent;
            children = new List<SyntaxNode>();
            value = symbol;
            semantic_type = LanguageType.Undefined;
        }

        internal SyntaxNode(SyntaxNode parent, string symbol, string value) : this(parent, symbol)
        {
            //this.symbol = symbol;
            //children = new List<SyntaxNode>();
            //this.parent = parent;
            this.value = value;
        }

        internal SyntaxNode(SyntaxNode parent, IToken token)
        {
            this.parent = parent;
            this.token = token;

            children = new List<SyntaxNode>();
            semantic_type = LanguageType.Undefined;
        }

        internal List<SyntaxNode> Children { get { return children; } }
        //internal string Symbol { get { return symbol; } }
        internal string Symbol { get { return token.TokenType; } }
        //internal string Value { get { return value; } }
        internal string Value { get { return token.TokenValue; } }
        internal SyntaxNode Parent { get { return parent; } }
        internal SemanticType Type
        {
            get { return semantic_type; }
            set { semantic_type = value; }
        }

        internal void addChild(SyntaxNode child) => children.Add(child);

        internal void printGraphic(string indent, bool last)
        {
            Console.Write(indent);

            if (last) {
                Console.Write("\u2514\u2500");
                indent += "  ";
            }
            else {
                Console.Write("\u251C\u2500");
                indent += "\u2502 ";
            }
            Console.WriteLine((Symbol == Value) ? Symbol : $"{Symbol}({Value})");

            for (int iii = 0; iii < children.Count; ++iii)
                children[iii].printGraphic(indent, iii == children.Count - 1);
        }
    }
}
