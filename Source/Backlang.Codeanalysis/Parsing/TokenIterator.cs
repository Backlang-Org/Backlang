namespace Backlang.Codeanalysis.Parsing;

public class TokenIterator
{
    public readonly List<Message> Messages = new();
    protected int _position = 0;

    private readonly List<Token> _tokens;

    public TokenIterator(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public Token Current => Peek(0);

    public Token Match(TokenType kind)
    {
        if (Current.Type == kind)
            return NextToken();

        Messages.Add(Message.Error($"Expected {kind} but got {Current.Type}", Current.Line, Current.Column));
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
            return _tokens.Last();

        return _tokens[index];
    }
}