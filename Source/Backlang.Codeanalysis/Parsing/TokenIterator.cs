namespace Backlang.Codeanalysis.Parsing;

public sealed class TokenIterator
{
    public readonly List<Message> Messages = new();
    private readonly SourceDocument _document;
    private readonly List<Token> _tokens;
    private int _position = 0;

    public TokenIterator(List<Token> tokens, SourceDocument document)
    {
        _tokens = tokens;
        this._document = document;
    }

    public Token Current => Peek(0);

    public bool IsMatch(TokenType kind)
    {
        return Current.Type == kind;
    }

    public Token Match(TokenType kind)
    {
        if (Current.Type == kind)
            return NextToken();

        Messages.Add(Message.Error(_document, $"Expected {kind} but got {Current.Type}", Current.Line, Current.Column));
        NextToken();

        return Token.Invalid;
    }

    public Token NextToken()
    {
        var current = Current;
        _position++;
        return current;
    }

    public Token Peek(int offset)
    {
        var index = _position + offset;
        if (index >= _tokens.Count)
            return _tokens[_tokens.Count - 1];

        return _tokens[index];
    }
}