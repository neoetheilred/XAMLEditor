using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParsingEngine
{

    /*
     * XamlDoc ::= Prolog Root
     * Prolog ::= <? xml Attributes ?>
     * Root ::= < Tag > Tags </ Tag > | < Tag />
     * Tag ::= Name Attributes
     * Attributes ::= (Name = String (SpaceSeparator Name = String)*)?
     * Tags ::= (< Tag > Tags </ Tag > | < Tag />)*
     */

    public abstract class ParserBase
    {
        protected List<Token> Tokens;

        protected int CurrentPos { get; private set; }

        protected Token Peek
        {
            get
            {
                if (CurrentPos >= Tokens.Count)
                    return new Token(TokenType.Eof);
                if (Tokens[CurrentPos].Type == TokenType.Comment)
                {
                    ++CurrentPos;
                    return Peek;
                }
                return Tokens[CurrentPos];
            }
        }

        public List<TokenError> Errors = new List<TokenError>();

        protected bool Match(TokenType type)
        {
            if (Peek.Type == type) return true;
            return false;
        }

        protected void Move()
        {
            if (CurrentPos >= Tokens.Count)
                return;
            ++CurrentPos;
        }
    }

    /// <summary>
    /// </summary>
    public class Parser : ParserBase
    {
        public Parser(IEnumerable<Token> tokens) => Tokens = tokens.ToList();

        public List<IdToken> AttributeNames { get; } = new List<IdToken>();

        public XamlDocument Parse()
        {
            return XamlDocument();
        }

        private XamlDocument XamlDocument()
        {
            XamlProlog prolog = null;
            if (Peek.Type == TokenType.PrologOpen)
                prolog = Prolog();

            XamlItem root = Root();
            return new XamlDocument() { Prolog = prolog, Root = root };
        }

        private XamlItem Root()
        {
            Console.WriteLine("Root parsing");
            SkipWhitespaces();
            XamlItem item;
            if (!Match(TokenType.AngularLeft))
            {
                Move();
                Errors.Add(new TokenError { ErrorMessage = "Expected Tag", Offset = Peek.Offset });
                return null;
            }

            Move();
            SkipWhitespaces();
            if (!Match(TokenType.Id))
            {
                Move();
                Errors.Add(new TokenError { ErrorMessage = "Expected Tag Name", Offset = Peek.Offset });
                return null;
            }
            item = new XamlItem(((IdToken)Peek).Literal);

            Move();
            SkipWhitespaces();
            (string Name, string Value)? attribute;
            while ((attribute = Attribute()) != null)
            {
                item.Attributes.Add(attribute.Value.Name, attribute.Value.Value);
            }
            SkipWhitespaces();
            if (Match(TokenType.AngularRight))
            {
                Move();
                SkipWhitespaces();
                while (Peek.Type == TokenType.AngularLeft)
                {
                    item.Children.Add(Root());
                    SkipWhitespaces();
                }
                SkipWhitespaces();
                if (!Match(TokenType.TagCloserLeft))
                {
                    Errors.Add(new TokenError() { ErrorMessage = "Expected tag closer `</`", Offset = Peek.Offset });
                    Move();
                    return null;
                }
                Move();
                SkipWhitespaces();
                if (!Match(TokenType.Id) || ((IdToken)Peek).Literal != item.Name)
                {
                    Errors.Add(new TokenError() { ErrorMessage = "Expected closing tag id", Offset = Peek.Offset });
                    Move();
                    return null;
                }
                Move();
                SkipWhitespaces();
                if (!Match(TokenType.AngularRight))
                {
                    Errors.Add(new TokenError() { ErrorMessage = "Expected closing tag `>`", Offset = Peek.Offset });
                    Move();
                    return null;
                }
                Move();
                return item;
            }
            if (Match(TokenType.TagCloserRight))
            {
                Move();
                return item;
            }
            Move();
            return null;
        }

        private void SkipWhitespaces()
        {
            while (Match(TokenType.SpaceSeparator))
                Move();
        }

        private XamlProlog Prolog()
        {
            XamlProlog prolog = new XamlProlog();
            if (Match(TokenType.PrologOpen))
            {
                Move();
                if (!Match(TokenType.Id) || ((IdToken)Peek).Literal != "xaml")
                {
                    Errors.Add(new TokenError() { ErrorMessage = "Expected xaml prolog", Offset = Peek.Offset });
                    return null;
                }
                Move();
                (string Name, string Value)? attribute;
                while ((attribute = Attribute()) != null)
                {
                    prolog.Attributes.Add(attribute.Value.Name, attribute.Value.Value);
                }

                if (Match(TokenType.PrologClose))
                {
                    Move();
                    return prolog;
                }
            }

            return null;
        }

        private (string Name, string Value)? Attribute()
        {
            while (Match(TokenType.SpaceSeparator)) Move();
            IdToken attrName = null;
            (string Name, string Value) res = ("", "");
            if (Match(TokenType.Id))
            {
                attrName = (IdToken)Peek;
                AttributeNames.Add(attrName);
                // Console.WriteLine($"{Peek}");
                res.Name = ((IdToken)Peek).Literal;
                Move();
            }
            else return null;
            while (Match(TokenType.SpaceSeparator)) Move();
            if (!Match(TokenType.Assign))
            {
                Errors.Add(new TokenError() { Offset = Peek.Offset, ErrorMessage = "Assign expected" });
                return null;
            }
            Move();
            if (Match(TokenType.String))
            {
                res.Value = ((StringToken)Peek).Literal;
                Move();
            }
            else
            {
                Console.WriteLine($"{Peek}");
                Errors.Add(new TokenError() { Offset = Peek.Offset, ErrorMessage = "Value expected" });
                return null;
            }

            return res;
        }
    }

    public class XamlDocument
    {
        public XamlProlog Prolog { get; set; }
        public XamlItem Root { get; set; }

        public override string ToString() => $"{Prolog} {Root}";
    }

    public class XamlProlog
    {
        public Dictionary<string, string> Attributes { get; } = new Dictionary<string, string>();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("<?xaml");
            foreach (var keyValuePair in Attributes)
            {
                sb.Append($" {keyValuePair.Key}=\"{keyValuePair.Value}\"");
            }

            sb.Append("?>");

            return sb.ToString();
        }
    }

    public class XamlItem
    {
        public Dictionary<string, string> Attributes { get; } = new Dictionary<string, string>();

        public List<XamlItem> Children { get; } = new List<XamlItem>();

        public string Name { get; }

        public XamlItem(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"<{Name}");
            foreach (var keyValuePair in Attributes)
            {
                sb.Append($" {keyValuePair.Key}=\"{keyValuePair.Value}\"");
            }

            if (Children.Count > 0)
            {
                sb.Append(">");
                Children.ForEach(x => sb.Append(x));
                sb.Append($"</{Name}>");
            }
            else sb.Append("/>");

            return sb.ToString();
        }
    }
}