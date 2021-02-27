using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PALCompiler
{
    class SyntaxNode
    {
        //private SyntaxNode[] children;
        private List<SyntaxNode> children;
        private string symbol;

        internal SyntaxNode(string symbol)
        {
            this.symbol = symbol;
            children = new List<SyntaxNode>();
        }

        internal List<SyntaxNode> Children { get { return children; } }

        internal void addChild(SyntaxNode child)
        {
            //children.Append(child);
            //children.
            children.Add(child);
        }

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
            Console.WriteLine(symbol);

            for (int iii = 0; iii < children.Count; ++iii)
                children[iii].printGraphic(indent, iii == children.Count - 1);
        }
    }
}
