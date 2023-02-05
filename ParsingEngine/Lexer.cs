using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ParsingEngine
{
    public abstract class Lexer : ILexer, IEnumerable<Token>
    {
        public abstract Token ScanToken();

        private StreamReader _streamReader;

        protected int CurrentOffset { get; private set; }

        public bool Eof => _streamReader.EndOfStream;

        public bool Error { get; protected set; }

        protected Lexer(StreamReader sr) => _streamReader = sr;

        /// <summary>
        /// Gets current character from the stream
        /// </summary>
        /// <returns> current character </returns>
        protected char Peek() => (char)_streamReader.Peek();

        /// <summary>
        /// Moves to next character in the stream
        /// </summary>
        protected void Match()
        {
            _streamReader.Read();
            ++CurrentOffset;
        }

        public void Reset(StreamReader sr)
        {
            _streamReader = sr;
            Error = false;
            CurrentOffset = 0;
        }

        public virtual IEnumerator<Token> GetEnumerator()
        {
            while (!Eof)
            {
                Token token = ScanToken();
                if (token != null)
                    yield return token;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public interface ILexer
    {
        Token ScanToken();

        bool Eof { get; }

        bool Error { get; }
    }
}