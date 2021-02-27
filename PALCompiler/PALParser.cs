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

        private void recogniseStatement()
        {
            if (have(Token.IdentifierToken)) recogniseAssignment();
            else if (have("UNTIL")) recogniseLoop();
            else if (have("IF")) recogniseConditional();
            else recogniseIO();
        }

        private void recogniseIO() => throw new NotImplementedException();
        private void recogniseConditional() => throw new NotImplementedException();
        private void recogniseLoop() => throw new NotImplementedException();
        private void recogniseAssignment() => throw new NotImplementedException();

        private void recogniseVarDecls()
        {
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
