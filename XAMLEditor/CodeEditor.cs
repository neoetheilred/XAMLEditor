using ICSharpCode.AvalonEdit;
using System;
using System.IO;
using System.Linq;
using System.Windows.Media;
using ParsingEngine;
using System.Text;

namespace XAMLEditor
{
    internal class CodeEditor : TextEditor
    {
        public bool Saved { get; private set; } = true;
        public string FilePath { get; set; }

        private TokenLexer _lexer = new TokenLexer(MakeStreamReaderFromString(""));

        public CodeEditor()
        {
            // SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("XML");
            FontFamily = new FontFamily("Consolas");
            Background = Brushes.White;

            var xamlHighlighter = new XAMLHighlighter();

            Document.TextChanged += (o, e) =>
            {
                _lexer.Reset(MakeStreamReaderFromString(Document.Text));
                xamlHighlighter.Tokens = _lexer.ToList();
                xamlHighlighter.Errors = _lexer.Errors;
                Saved = false;
                TextArea.TextView.Redraw();
            };
            Document.Changed += (o, e) => { Saved = false; };
            DocumentChanged += (_object, eventArgs) => { Saved = false; };
            ShowLineNumbers = true;
            TextArea.TextView.LineTransformers.Add(xamlHighlighter);
            TextArea.Caret.PositionChanged += (sender, args) =>
            {
                // MessageBox.Show($"Changed {TextArea.Caret.Offset}");
                xamlHighlighter.SetCaretOffset(TextArea.Caret.Offset);
            };
        }

        public CodeEditor(string path) : this()
        {
            FilePath = path;
            Document.Text = File.ReadAllText(path);
            Saved = true;
        }

        public void Save()
        {
            if (FilePath == null)
                throw new ArgumentNullException("File path was null");

            if (!File.Exists(FilePath))
            {
                var fileStream = File.Create(FilePath);
                fileStream.Dispose();
            }

            using (FileStream fs = File.Open(FilePath, FileMode.Truncate, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(Document.Text);
                    sw.Flush();
                }
            }
            // MessageBox.Show("SAVED");

            Saved = true;
        }

        private static StreamReader MakeStreamReaderFromString(string str)
        {
            return new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(str ?? "")));
        }
    }
}