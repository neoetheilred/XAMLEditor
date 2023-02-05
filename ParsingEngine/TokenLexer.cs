using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ParsingEngine
{
    public class TokenLexer : Lexer
    {
        public TokenLexer(StreamReader sr) : base(sr) { }

        public List<TokenError> Errors { get; } = new List<TokenError>();

        public new void Reset(StreamReader sr)
        {
            base.Reset(sr);
            Errors.Clear();
        }

        public override Token ScanToken()
        {
            //while (char.IsWhiteSpace(Peek()) || Peek() == '\n' || Peek() == '\r')
            //{
            //    Match();
            //}

            if (char.IsWhiteSpace(Peek()) || Peek() == '\n' || Peek() == '\r')
            {
                while (char.IsWhiteSpace(Peek()) || Peek() == '\n' || Peek() == '\r')
                {
                    Match();
                }
                return new Token(TokenType.SpaceSeparator) { Length = 1, Offset = CurrentOffset };
            }

            if (Eof)
                return new Token(TokenType.Eof);

            switch (Peek())
            {
                case '?':
                    Match();
                    if (Peek() == '>')
                    {
                        Match();
                        return new Token(TokenType.PrologClose) { Length = 2, Offset = CurrentOffset - 2 };
                    }
                    break;
                case '/':
                    Match();
                    if (Peek() == '>')
                    {
                        Match();
                        return new Token(TokenType.TagCloserRight) { Length = 2, Offset = CurrentOffset - 2 };
                    }
                    break;
                case '=':
                    Match();
                    return new Token(TokenType.Assign) { Length = 1, Offset = CurrentOffset - 1 };
                case '>':
                    Match();
                    return new Token(TokenType.AngularRight) { Length = 1, Offset = CurrentOffset - 1 };
                case '<':
                    Match();
                    if (Peek() == '!')
                    {
                        return ReadComment();
                    }
                    else if (Peek() == '?')
                    {
                        Match();
                        return new Token(TokenType.PrologOpen) { Length = 2, Offset = CurrentOffset - 2 };
                    }
                    else if (Peek() == '/')
                    {
                        Match();
                        return new Token(TokenType.TagCloserLeft) { Length = 2, Offset = CurrentOffset - 2 };
                    }
                    return new Token(TokenType.AngularLeft) { Length = 1, Offset = CurrentOffset - 1 };
                case '"': return ReadString();
            }

            if (char.IsLetter(Peek()) || Peek() == '_')
            {
                return ReadName();
            }

            var retToken = new TextToken();
            Match();
            return retToken;
        }

        private Token ReadName()
        {
            int start = CurrentOffset;
            StringBuilder sb = new StringBuilder();
            sb.Append(Peek());
            Match();
            while (char.IsLetterOrDigit(Peek()) || Peek() == '_' || Peek() == ':' || Peek() == '.')
            {
                sb.Append(Peek());
                Match();
            }

            return new IdToken(sb.ToString())
            {
                Length = CurrentOffset - start,
                Offset = start
            };
        }

        private Token ReadComment()
        {
            int start = CurrentOffset - 1;
            Match();
            for (int i = 0; i < 2; ++i)
            {
                if (Peek() != '-')
                {
                    Error = true;
                    Errors.Add(new TokenError
                    {
                        ErrorMessage = $"Unexpected char: `{Peek()}`",
                        Offset = start,
                    });
                    return null;
                }
                Match();
            }

            StringBuilder sb = new StringBuilder();

            while (!Eof && (sb.Length < 3 || sb.ToString() != "-->"))
            {
                if (sb.Length >= 3)
                    sb.Remove(0, 1);

                sb.Append(Peek());

                Match();
            }

            if (Eof && sb.ToString() != "-->")
            {
                Error = true;
                Errors.Add(new TokenError
                {
                    ErrorMessage = "Expected comment-closing sequence (`-->`)",
                    Offset = CurrentOffset,
                });
                return new Token(TokenType.Eof);
            }

            return new Token(TokenType.Comment)
            {
                Length = CurrentOffset - start,
                Offset = start,
            };
        }

        private Token ReadString()
        {
            int start = CurrentOffset;
            StringBuilder sb = new StringBuilder();
            Match();
            while (!Eof && Peek() != '"')
            {
                sb.Append(Peek());
                Match();
            }

            if (Eof && Peek() != '"')
            {
                Error = true;
                Errors.Add(new TokenError
                {
                    Offset = CurrentOffset,
                    ErrorMessage = "Expected `\"`",
                });
                return new Token(TokenType.Eof);
            }

            Match();

            return new StringToken(sb.ToString())
            {
                Length = CurrentOffset - start,
                Offset = start
            };
        }
    }

    public class TokenError
    {
        public string ErrorMessage { get; set; }
        public int Offset { get; set; }
        public override string ToString() => $"[{Offset}] {ErrorMessage}";
    }
}