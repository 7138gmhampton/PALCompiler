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
    /// The PAL scanner implementation.
    public class PALScanner : Scanner
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
            StringBuilder strbuf = null;
            int state = 0;
            //int startLine = 0, startCol = 0;
            //var start = new Position { line = 0, column = 0 };
            var start = new Position(0, 0);

            Token token = null;
            while (token == null) {
                switch (state) {
                    case 0:
                        if (Char.IsWhiteSpace(currentChar)) state = 0;
                        else {
                            //startLine = line; startCol = column;
                            //start = new Position { line = line, column = }
                            start = new Position(line, column);
                            strbuf = new StringBuilder();

                            if (Char.IsLetter(currentChar)) state = 1;
                            else if (Char.IsDigit(currentChar)) state = 2;
                            else if ("+-*/(),=<>".IndexOf(currentChar) != -1) state = 4;
                            else if (currentChar == eofChar) state = 98;
                            else state = 99;
                        }
                        //LexerFSM.traverseState(state, currentChar, );
                        break;
                    case 1:
                        if (Char.IsLetter(currentChar) || Char.IsDigit(currentChar) ) state = 1;
                        else {
                            String s = strbuf.ToString();
                            if (keywords.Contains (s)) token = new Token (s, start.line, start.column);
                            else token = new Token (Token.IdentifierToken, s, start.line, start.column);
                        }
                        break;
                    case 2:
                        if (Char.IsDigit (currentChar))   state = 2;
                        else if (currentChar == '.')      state = 3;
                        else
                            token = new Token (Token.IntegerToken, strbuf.ToString(), start.line, start.column);
                        break;
                    case 3:
                        if (Char.IsDigit (currentChar))
                            state = 3;
                        else token = new Token (Token.RealToken, strbuf.ToString(), start.line, start.column);
                        break;
                    case 4: token = new Token (strbuf.ToString(), start.line, start.column); break;
                    case 98: token = new Token (Token.EndOfFile, start.line, start.column); break;
                    case 99:
                        token = new Token (Token.InvalidChar, strbuf.ToString(), start.line, start.column);
                        break;
                }

                if (token == null) {
                    if (state != 0) strbuf.Append (currentChar);
                    getNextChar();
                }
            }

            return token;
        }

        //private struct Candidate
        //{
        //    StringBuilder
        //}

        private class Candidate
        {
            private StringBuilder value_buffer;
            private Position start;

            public Candidate(Position start)
            {
                value_buffer = new StringBuilder();
                this.start = start;
            }

            public string toString() => value_buffer.ToString();
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

        private static class LexerFSM
        {
            //Position position = new Position { line = 0, column = 0 };
            //public (IToken token, int state) getTransition()
            private delegate (IToken, int) Traversal(char current_char, Position position, ref Candidate candidate);

            //private static Dictionary<int, Func<char, Position, Candidate, (IToken, int)>> states = 
            //    new Dictionary<int, Func<char, Position, Candidate, (IToken, int)>>
            //{
            //        { 0, initialState }
            //};
            private static Dictionary<int, Traversal> states = new Dictionary<int, Traversal>
            {
                { 0, initialState }
            };

            //public static Func<char, Position, Candidate,(IToken,int)> traverseState()
            //{
            //    throw new NotImplementedException();
            //}

            public static (IToken,int) traverseState(
                int current_state, 
                char current_char, 
                Position position, 
                ref Candidate candidate)
            {
                //Traversal currentTraversal = states[current_state];
                return states[current_state].Invoke(current_char, position, ref candidate);
            }

            private static (IToken,int) initialState(
                char current_char, 
                Position position, 
                ref Candidate candidate)
            {
                int state = 0;
                IToken token = null;

                if (char.IsWhiteSpace(current_char)) state = 0;
                else {
                    //startLine = line; startCol = column;
                    //strbuf = new StringBuilder();

                    if (char.IsLetter(current_char)) state = 1;
                    else if (char.IsDigit(current_char)) state = 2;
                    else if ("+-*/(),=<>".IndexOf(current_char) != -1) state = 4;
                    else if (current_char == eofChar) state = 98;
                    else state = 99;
                }

                return (token, state);
            }
        }
    }
}
