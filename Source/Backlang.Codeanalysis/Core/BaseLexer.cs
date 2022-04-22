using Backlang.Codeanalysis.Parsing;

namespace Backlang.Codeanalysis.Core;

public abstract class BaseLexer
{
    public List<Message> Messages = new();

    protected int _column = 1;
    protected SourceDocument _document;
    protected int _line = 1;
    protected int _position = 0;

    public List<Token> Tokenize(SourceDocument document)
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
        return _position++;
    }

    protected char Current()
    {
        if (_position >= _document.Source.Length)
        {
            return '\0';
        }

        return _document.Source[_position];
    }

    protected abstract Token NextToken();

    protected char Peek(int offset = 0)
    {
        if (_position + offset >= _document.Source.Length)
        {
            return '\0';
        }

        return _document.Source[_position + offset];
    }

    protected void ReportError()
    {
        Messages.Add(Message.Error(_document, $"Unknown Charackter '{Current()}'", _line, _column++));
        Advance();
    }
}