namespace ParsingEngine
{
    public class Token
    {
        public TokenType Type { get; }

        public int Offset { get; set; }

        public int Length { get; set; }

        public Token(TokenType type) => Type = type;

        public override string ToString() => $"{Type}(Offset:{Offset},Length:{Length})";
    }

    public enum TokenType
    {
        String, Id, AngularLeft, AngularRight, Eof, Comment, Assign, TagCloserLeft, TagCloserRight,
        SpaceSeparator,
        PrologOpen, PrologClose, TextChar
    }

    public class StringToken : Token
    {
        public string Literal { get; }

        public StringToken(string literal) : base(TokenType.String) => Literal = literal;

        public override string ToString() => $"{base.ToString()} \"{Literal}\"";
    }

    public class IdToken : Token
    {
        public string Literal { get; }

        public IdToken(string literal) : base(TokenType.Id) => Literal = literal;

        public override string ToString() => $"{base.ToString()} {Literal}";
    }

    public class TextToken : Token
    {
        public TextToken() : base(TokenType.TextChar) { }
    }
}