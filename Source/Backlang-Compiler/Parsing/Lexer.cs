using Backlang_Compiler.Core;

namespace Backlang_Compiler.Parsing;

public class Lexer : BaseLexer
{
    private readonly Dictionary<string, TokenType> _symbolTokens = new()
    {
        ["."] = TokenType.Dot,
        ["-"] = TokenType.Minus,
        ["+"] = TokenType.Plus,
        ["*"] = TokenType.Star,
        ["/"] = TokenType.Slash,

        ["("] = TokenType.OpenParen,
        [")"] = TokenType.CloseParen,

        [":"] = TokenType.Colon,
        ["!"] = TokenType.Exclamation,
        [","] = TokenType.Comma,
        [";"] = TokenType.Semicolon,

        ["<->"] = TokenType.SwapOperator,
        ["=="] = TokenType.EqualsEquals,

        ["="] = TokenType.EqualsToken,
    };

    public Lexer()
    {
        _symbolTokens = new(_symbolTokens.OrderByDescending(_ => _.Key.Length));
    }

    protected override Token NextToken()
    {
        SkipWhitespaces();

        if (_position >= _source.Length)
        {
            return new Token(TokenType.EOF, "\0", _position, _position, _line, _column);
        }
        else if (_symbolTokens.ContainsKey(Current().ToString()))
        {
            return new Token(_symbolTokens[Current().ToString()], Current().ToString(), Advance(), _position, _line, _column++);
        }
        else if (Current() == '\'')
        {
            var oldpos = ++_position;
            var oldColumn = _column;

            while (Peek() != '\'' && Peek() != '\0')
            {
                if (Current() == '\n' || Current() == '\r')
                {
                    Messages.Add(Message.Error($"Unterminated String", _line, oldColumn));
                }

                Advance();
                _column++;
            }

            _column += 2;

            return new Token(TokenType.StringLiteral, _source.Substring(oldpos, _position - oldpos), oldpos - 1, ++_position, _line, oldColumn);
        }
        else if (Current() == '"')
        {
            var oldpos = ++_position;
            var oldColumn = _column;

            while (Peek() != '"' && Peek() != '\0')
            {
                if (Current() == '\n' || Current() == '\r')
                {
                    Messages.Add(Message.Error($"Unterminated String", _line, oldColumn));
                }

                Advance();
                _column++;
            }

            _column += 2;

            return new Token(TokenType.StringLiteral, _source.Substring(oldpos, _position - oldpos), oldpos - 1, ++_position, _line, oldColumn);
        }
        else if (char.IsDigit(Current()))
        {
            var oldpos = _position;
            var oldcolumn = _column;

            while (char.IsDigit(Peek(0)))
            {
                Advance();
                _column++;
            }

            if (char.IsDigit(Peek(1)) && Peek(0) == '.')
            {
                Advance();
                _column++;

                while (char.IsDigit(Peek(0)))
                {
                    Advance();
                    _column++;
                }
            }

            return new Token(TokenType.Number, _source.Substring(oldpos, _position - oldpos), oldpos, _position, _line, oldcolumn);
        }
        else if (char.IsLetter(Current()))
        {
            var oldpos = _position;

            var oldcolumn = _column;
            while (char.IsLetterOrDigit(Peek(0)))
            {
                Advance();
                _column++;
            }

            var tokenText = _source.Substring(oldpos, _position - oldpos);

            return new Token(TokenUtils.GetTokenType(tokenText), tokenText, oldpos, _position, _line, oldcolumn);
        }
        else
        {
            foreach (var symbol in _symbolTokens)
            {
                if (IsMatch(symbol.Key))
                {
                    int oldpos = _position;

                    _position += symbol.Key.Length;
                    _column += symbol.Key.Length;

                    string text = _source.Substring(oldpos, symbol.Key.Length);

                    return new Token(_symbolTokens[text], text, oldpos, _position, _line, _column);
                }
            }

            ReportError();
        }

        return Token.Invalid;
    }

    private bool IsMatch(string token)
    {
        bool result = false;
        for (int i = 0; i < token.Length; i++)
        {
            result = Peek(i) == token[i];
        }

        return result;
    }

    private void SkipWhitespaces()
    {
        while (char.IsWhiteSpace(Current()) && _position <= _source.Length)
        {
            if (Current() == '\r')
            {
                _line++;
                _column = 1;
                Advance();

                if (Current() == '\n')
                {
                    Advance();
                }
            }
            else
            {
                Advance();
                _column++;
            }
        }
    }
}