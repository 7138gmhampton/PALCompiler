/*
	File: PALScanner.cs
	Purpose: A scanner for the PAL language.
	Version: 1.1
	Date: 2010, 14 April 2011.
	Author: Allan C. Milne.

	Namespace: AllanMilne.PALCompiler
	Uses: Ardkit.dll
	Exposes: PALScanner.

	Description:
	The language PAL is defined in the file PAL.EBNF.txt.
	This scanner implementation is built on version 2 of the Ardkit compiler toolkit framework.
	This version of the scanner uses a state transition FSM algorithm.
	See 'PAL.FSM.txt' for the FSM.
	Invalid characters and tokens are identified and appropriate tokens returned.
*/

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using AllanMilne.Ardkit;

namespace PALCompiler
{
    /// <summary>
    /// Implements the PAL scanner/lexer
    /// </summary>
    public partial class PALScanner : Scanner
    {
        /// list of keywords used in the language - remember they are case-sensitive.
        private static List<String> keywords = new List<String>( new String[] {
           "PROGRAM",
           "WITH",
           "IN",
           "END",
	       "AS",
           "INTEGER",
           "REAL",
           "UNTIL",
           "REPEAT",
           "ENDLOOP",
	       "IF",
           "THEN",
           "ELSE",
           "ENDIF",
           "INPUT",
           "OUTPUT"
        });

        /// find and return the next token using a state transition FSM.
        protected override IToken getNextToken()
        {
            int state = (int)State.INITIAL;
            Candidate candidate = null;

            IToken token = null;
            while (token == null) {
                (token, state) = LexerFSM.traverseState(state, currentChar, 
                    new Position(line, column), ref candidate);

                if (token == null) {
                    if (state != 0) candidate.Append(currentChar);
                    getNextChar();
                }
            }

            return token;
        }

        /// <summary>
        /// Represents the accumulated characters that potentially could form
        /// a token
        /// </summary>
        private class Candidate
        {
            private StringBuilder value_buffer;
            private Position start;

            public Candidate(Position start)
            {
                value_buffer = new StringBuilder();
                this.start = start;
                //Console.WriteLine("Create Candidate @ [" + this.start)
            }

            public Position Start { get { return start; } }
            public int Line { get { return start.line; } }
            public int Column { get { return start.column; } }

            public override string ToString() => value_buffer.ToString();
            public void Append(char addition) => value_buffer.Append(addition);
        }

        private struct Position
        {
            public int line;
            public int column;

            internal Position(int line, int column)
            {
                this.line = line;
                this.column = column;
            }
        }

        private enum State
        {
            INITIAL = 0,
            WORD = 1,
            NUMERAL = 2,
            RADIX = 3,
            SHORT_TOKEN = 4,
            DUMMY = 80,
            EOF = 98,
            INVALID_CHAR = 99
        }
    }
}
