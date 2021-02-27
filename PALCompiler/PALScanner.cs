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
        //private delegate 
        
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
            //StringBuilder strbuf = null;
            int state = 0;
            //int startLine = 0, startCol = 0;
            //var start = new Position { line = 0, column = 0 };
            //var start = new Position(0, 0);
            Candidate candidate = null;

            IToken token = null;
            while (token == null) {
                //switch (state) {
                //    case 0:
                //        //if (Char.IsWhiteSpace(currentChar)) state = 0;
                //        //else {
                //        //    //startLine = line; startCol = column;
                //        //    //start = new Position { line = line, column = }
                //        //    //start = new Position(line, column);
                //        //    //strbuf = new StringBuilder();
                //        //    candidate = new Candidate(new Position(line, column));

                //        //    if (Char.IsLetter(currentChar)) state = 1;
                //        //    else if (Char.IsDigit(currentChar)) state = 2;
                //        //    else if ("+-*/(),=<>".IndexOf(currentChar) != -1) state = 4;
                //        //    else if (currentChar == eofChar) state = 98;
                //        //    else state = 99;
                //        //}
                //        //LexerFSM.traverseState(state, currentChar, );
                //        (token, state) = LexerFSM.traverseState(state, currentChar, new Position(line, column), ref candidate);
                //        break;
                //    case 1:
                //        //if (Char.IsLetter(currentChar) || Char.IsDigit(currentChar) ) state = 1;
                //        //else {
                //        //    //String s = strbuf.ToString();
                //        //    if (keywords.Contains(candidate.ToString())) token = new Token(candidate.ToString(), candidate.Start.line, candidate.Start.column);
                //        //    else token = new Token(Token.IdentifierToken, candidate.ToString(), candidate.Start.line, candidate.Start.column);
                //        //}
                //        (token, state) = LexerFSM.traverseState(state, currentChar, new Position(line, column), ref candidate);
                //        break;
                //    case 2:
                //        //if (Char.IsDigit (currentChar))   state = 2;
                //        //else if (currentChar == '.')      state = 3;
                //        //else
                //        //    token = new Token (Token.IntegerToken, candidate.ToString(), candidate.Start.line, candidate.Start.column);
                //        (token, state) = LexerFSM.traverseState(state, currentChar, new Position(line, column), ref candidate);
                //        break;
                //    case 3:
                //        //if (Char.IsDigit (currentChar))
                //        //    state = 3;
                //        //else token = new Token (Token.RealToken, candidate.ToString(), candidate.Start.line, candidate.Start.column);
                //        (token, state) = LexerFSM.traverseState(state, currentChar, new Position(line, column), ref candidate);
                //        break;
                //    //case 4: token = new Token(candidate.ToString(), candidate.ToString(), candidate.Start.line, candidate.Start.column); break;
                //    case 4:
                //        (token, state) = LexerFSM.traverseState(state, currentChar, new Position(line, column), ref candidate);
                //        break;
                //    //case 98: token = new Token(Token.EndOfFile, candidate.ToString(), candidate.Start.line, candidate.Start.column); break;
                //    case 98:
                //        (token, state) = LexerFSM.traverseState(state, currentChar, new Position(line, column), ref candidate);
                //        break;
                //    case 99:
                //        //Console.WriteLine("Sample Candidate: " + candidate.ToString() + " @ " + "[" + candidate.Start.line + "," + candidate.Start.column + "]");
                //        //token = new Token(Token.InvalidChar, candidate.ToString(), candidate.Start.line, candidate.Start.column);
                //        //break;
                //        (token, state) = LexerFSM.traverseState(state, currentChar, new Position(line, column), ref candidate);
                //        break;
                //}

                (token, state) = LexerFSM.traverseState(state, currentChar, new Position(line, column), ref candidate);

                if (token == null) {
                    if (state != 0) candidate.Append(currentChar);
                    //if (state != 0) strbuf.Append (currentChar);
                    getNextChar();
                }
            }

            return token;
        }

        //private struct Candidate
        //{
        //    StringBuilder
        //}

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
                //Console.WriteLine("Create Position [" + line+"," + column+"] -> [" + this.line + "," + this.column + "]");
            }
        }
    }
}
