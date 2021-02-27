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

using System.Collections.Generic;

using AllanMilne.Ardkit;

namespace PALCompiler
{
    public partial class PALScanner
    {
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
                { 0, initialState },
                { 1, wordState },
                { 2, numeralState },
                { 3, radixState },
                { 4, shortToken },
                { 98, endOfFile },
                { 99, invalidChar }
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
                    candidate = new Candidate(position);

                    if (char.IsLetter(current_char)) state = 1;
                    else if (char.IsDigit(current_char)) state = 2;
                    else if ("+-*/(),=<>".IndexOf(current_char) != -1) state = 4;
                    else if (current_char == eofChar) state = 98;
                    else state = 99;
                }

                return (token, state);
            }

            private static (IToken, int) wordState(
                char current_char,
                Position position,
                ref Candidate candidate)
            {
                int state = 1;
                IToken token = null;

                if (char.IsLetter(current_char) || char.IsDigit(current_char)) state = 1;
                else {
                    string word = candidate.ToString();
                    //if (keywords.Contains(word)) token = new Token(word, candidate.Line, candidate.Column);
                    //else token = new Token(Token.IdentifierToken, word, candidate.Line, candidate.Column);
                    token = keywords.Contains(word) 
                        ? new Token(word, candidate.Line, candidate.Column)
                        : new Token(Token.IdentifierToken, word, candidate.Line, candidate.Column);
                }

                return (token, state);
            }

            private static (IToken, int) numeralState(
                char current_char,
                Position position,
                ref Candidate candidate)
            {
                int state = 2;
                IToken token = null;

                if (char.IsDigit(current_char)) state = 2;
                else if (current_char == '.') state = 3;
                else token = new Token(Token.IntegerToken, candidate.ToString(), candidate.Line, candidate.Column);

                return (token, state);
            }

            private static (IToken, int) radixState(
                char current_char,
                Position position,
                ref Candidate candidate)
            {
                //throw new NotImplementedException();

                int state = 3;
                IToken token = null;

                if (char.IsDigit(current_char)) state = 3;
                else token = new Token(Token.RealToken, candidate.ToString(), candidate.Start.line, candidate.Start.column);

                return (token, state);
            }

            private static (IToken, int) shortToken(
                char current_char,
                Position position,
                ref Candidate candidate)
            {
                return (new Token(candidate.ToString(), candidate.Line, candidate.Column), 4);
            }

            private static (IToken, int) endOfFile(
                char current_char,
                Position position,
                ref Candidate candidate)
            {
                return (new Token(Token.EndOfFile, candidate.Line, candidate.Column), 4);
            }

            private static (IToken, int) invalidChar(
                char current_char,
                Position position,
                ref Candidate candidate)
            {
                return (new Token(Token.InvalidChar, candidate.ToString(), candidate.Line, candidate.Column), 4);
            }
        }
    }
}
