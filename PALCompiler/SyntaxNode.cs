using System;
using System.Collections.Generic;

namespace PALCompiler
{
    class SyntaxNode
    {
        private List<SyntaxNode> children;
        private SyntaxNode parent;
        private string symbol;
        private string value;

        internal SyntaxNode(SyntaxNode parent, string symbol)
        {
            this.symbol = symbol;
            this.value = symbol;
            children = new List<SyntaxNode>();
            this.parent = parent;
        }

        internal SyntaxNode(SyntaxNode parent, string symbol, string value)
        {
            this.symbol = symbol;
            this.value = value;
            children = new List<SyntaxNode>();
            this.parent = parent;
        }

        internal List<SyntaxNode> Children { get { return children; } }
        internal string Symbol { get { return symbol; } }
        internal string Value { get { return value; } }
        internal SyntaxNode Parent { get { return parent; } }

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
            Console.WriteLine((symbol == value) ? symbol : $"{symbol}({value})");

            for (int iii = 0; iii < children.Count; ++iii)
                children[iii].printGraphic(indent, iii == children.Count - 1);
        }
    }
}
