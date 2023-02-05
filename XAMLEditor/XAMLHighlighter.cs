using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using ParsingEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace XAMLEditor
{
    internal class XAMLHighlighter : DocumentColorizingTransformer
    {
        private int currentCaretOffset;

        public void SetCaretOffset(int offset) => currentCaretOffset = offset;

        public List<Token> Tokens { get; set; } = new List<Token>();

        private Parser _parser;

        public List<TokenError> Errors { get; set; } = new List<TokenError>();

        protected override void ColorizeLine(DocumentLine line)
        {
            int lineOffset = line.Offset;
            string text = CurrentContext.Document.GetText(line);
            if (text.Length == 0)
                return;

            foreach (var token in Tokens
                .Where(x => x.Type != TokenType.Eof)
                .Where(x => x.Offset >= lineOffset && x.Offset <= lineOffset + text.Length
                            || x.Offset + x.Length >= lineOffset).ToList())
            {
                int endOffset = Math.Min(token.Offset + token.Length, lineOffset + text.Length);
                int startOffset = Math.Max(token.Offset, lineOffset);
                if (endOffset <= startOffset)
                    continue;

                base.ChangeLinePart(
                    startOffset,
                    endOffset,
                    element => { ColorizeElement(element, token, false); });
                if (token is StringToken && Regex.IsMatch(((StringToken)token).Literal, @"^\s*\{.*\}\s*$"))
                {
                    Regex r = new Regex(@"\b[\w\d]+\b");
                    int currentOffset = ((StringToken)token).Literal.IndexOf("{") + 1;
                    foreach (Match match in r.Matches(((StringToken)token).Literal))
                    {
                        ChangeLinePart(
                            token.Offset + currentOffset + 1,
                            token.Offset + currentOffset + match.Value.Length + 1,
                            element => { element.TextRunProperties.SetForegroundBrush(Brushes.DarkSlateGray); }
                            );
                        currentOffset += match.Value.Length + 1;
                    }
                }
            }

            //foreach (var tokenError in Errors.Where(x => x.Offset >= lineOffset && x.Offset <= lineOffset + text.Length))
            //{
            //    base.ChangeLinePart(
            //        tokenError.Offset,
            //        tokenError.Offset + 1 <= lineOffset + text.Length ? tokenError.Offset + 1 : tokenError.Offset,
            //        element => { element.BackgroundBrush = Brushes.Red; });
            //}

            try
            {
                _parser = new Parser(Tokens);
                try
                {
                    _parser.Parse();
                }
                catch (Exception) { }

                foreach (var token in _parser.AttributeNames.Where(x =>
                    x.Offset >= lineOffset && x.Offset <= lineOffset + text.Length))
                {
                    int endOffset = Math.Min(token.Offset + token.Length, lineOffset + text.Length);
                    int startOffset = Math.Max(token.Offset, lineOffset);
                    if (endOffset <= startOffset)
                        continue;
                    base.ChangeLinePart(
                        startOffset,
                        endOffset,
                        element => { ColorizeElement(element, token, true); });
                }
            }
            catch (Exception) { }
        }

        private void ColorizeString(int Offset, string token)
        {

        }

        private void ColorizeElement(VisualLineElement element, Token token, bool isAttribute)
        {
            switch (token.Type)
            {
                case TokenType.Comment:
                    element.TextRunProperties.SetForegroundBrush(Brushes.ForestGreen);
                    break;
                case TokenType.AngularLeft:
                case TokenType.AngularRight:
                case TokenType.TagCloserLeft:
                case TokenType.TagCloserRight:
                case TokenType.PrologClose:
                case TokenType.PrologOpen:
                    element.TextRunProperties.SetForegroundBrush(Brushes.Blue);
                    break;
                case TokenType.String:
                    element.TextRunProperties.SetForegroundBrush(Brushes.Orange);
                    break;
                case TokenType.Id:
                    var tf = element.TextRunProperties.Typeface;
                    element.TextRunProperties.SetTypeface(new Typeface(
                        tf.FontFamily, FontStyles.Italic, FontWeights.Bold, FontStretches.Condensed));
                    if (!isAttribute)
                        element.TextRunProperties.SetForegroundBrush(Brushes.BlueViolet);
                    else element.TextRunProperties.SetForegroundBrush(Brushes.Coral);
                    break;
            }
        }
    }
}