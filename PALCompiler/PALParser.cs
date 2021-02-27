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
        internal PALParser(IScanner scanner) : base(scanner) { }

        protected override void recStarter()
        {
            mustBe("PROGRAM");
            mustBe(Token.IdentifierToken);
            mustBe("WITH");
            recogniseVarDecls();
            mustBe("IN");
            recogniseStatement();
            //if (!have("END")) 
            while (!have("END")) recogniseStatement();
            mustBe("END");
        }

        private void recogniseStatement() => throw new NotImplementedException();

        private void recogniseVarDecls()
        {
            //if (have(Token.IdentifierToken)) recogniseIdentList();
            while (have(Token.IdentifierToken)) {
                recogniseIdentList();
                mustBe("AS");
                recogniseType();
            }
        }

        private void recogniseType() => throw new NotImplementedException();
        private void recogniseIdentList() => throw new NotImplementedException();
    }
}
