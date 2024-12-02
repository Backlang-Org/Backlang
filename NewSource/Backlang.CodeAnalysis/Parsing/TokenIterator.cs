using Backlang.Codeanalysis.Core;
using Backlang.Codeanalysis.Core.Attributes;
using Loyc.Syntax;
using System.Reflection;

namespace Backlang.Codeanalysis.Parsing;

public sealed class TokenIterator
{
    private readonly SourceFile<StreamCharSource> _document;
    private readonly List<Token> _tokens;
    public readonly List<Message> Messages = new();

    public TokenIterator(List<Token> tokens, SourceFile<StreamCharSource> document)
    {
        _tokens = tokens;
        _document = document;
    }

    public Token Current => Peek(0);
    public int Position { get; set; }
    public Token Prev => Peek(-1);

    public static string GetTokenRepresentation(TokenType kind)
    {
        var field = kind.GetType().GetField(Enum.GetName(kind));

        var lexeme = field.GetCustomAttribute<LexemeAttribute>();
        var keyword = field.GetCustomAttribute<KeywordAttribute>();

        if (lexeme is not null)
        {
            return lexeme.Lexeme;
        }

        if (keyword is not null)
        {
            return keyword.Keyword;
        }

        return Enum.GetName(kind);
    }

    public bool ConsumeIfMatch(TokenType kind)
    {
        var result = false;
        if (IsMatch(kind))
        {
            result = true;
            NextToken();
        }

        return result;
    }

    public bool IsMatch(TokenType kind)
    {
        return Current.Type == kind;
    }

    public bool IsMatch(params TokenType[] kinds)
    {
        var result = false;
        foreach (var kind in kinds)
        {
            result |= IsMatch(kind);
        }

        return result;
    }

    public Token Match(TokenType kind)
    {
        if (Current.Type == kind)
        {
            return NextToken();
        }

        Messages.Add(
            Message.Error(
                new LocalizableString(ErrorID.Expected, GetTokenRepresentation(kind),
                    GetTokenRepresentation(Current.Type)),
                new SourceRange(_document, Current.Start, Current.Text.Length)));

        NextToken();

        return Token.Invalid;
    }

    public Token NextToken()
    {
        var current = Current;
        Position++;
        return current;
    }

    public Token Peek(int offset)
    {
        var index = Position + offset;
        if (index >= _tokens.Count)
        {
            return _tokens[_tokens.Count - 1];
        }

        return _tokens[index];
    }
}