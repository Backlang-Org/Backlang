using Backlang.Codeanalysis.Parsing;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Core;

public abstract class BaseLexer
{
    public List<Message> Messages = new();

    protected int _column = 1;
    protected SourceFile<StreamCharSource> _document;
    protected int _line = 1;
    protected int _position = 0;

    public List<Token> Tokenize(SourceFile<StreamCharSource> document)
    {
        _document = document;

        var tokens = new List<Token>();

        Token newToken;
        do
        {
            newToken = NextToken();

            tokens.Add(newToken);
        } while (newToken.Type != TokenType.EOF);

        return tokens;
    }

    protected int Advance()
    {
        return ++_position;
    }

    protected char Current()
    {
        if (_position >= _document.Text.Count)
        {
            return '\0';
        }

        return _document.Text[_position];
    }

    protected abstract Token NextToken();

    protected char Peek(int offset = 0)
    {
        if (_position + offset >= _document.Text.Count)
        {
            return '\0';
        }

        return _document.Text[_position + offset];
    }

    protected void ReportError()
    {
        _column++;

        var range = SourceRange.New(_document, new IndexRange(_position, 1));
        Messages.Add(Message.Error($"Unknown Charakter '{Current()}'", range));
        Advance();
    }
}