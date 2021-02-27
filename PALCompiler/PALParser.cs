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

        private bool haveStatement()
        {
            return have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") ||
                have("OUTPUT");
        }

        private void recogniseStatement()
        {
            if (have(Token.IdentifierToken)) recogniseAssignment();
            else if (have("UNTIL")) recogniseLoop();
            else if (have("IF")) recogniseConditional();
            else recogniseIO();
        }

        private void recogniseIO() => throw new NotImplementedException();
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
