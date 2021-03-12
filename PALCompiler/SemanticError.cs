using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllanMilne.Ardkit;

namespace PALCompiler
{
    class SemanticError : CompilerError
    {
        private readonly string message;

        internal SemanticError(IToken token, string message) : base(token)
        {
            this.message = message;
        }

        public override string ToString()
        {
            return $"At line {token.Line}, column {token.Column}: {message}";
        }
    }
}
