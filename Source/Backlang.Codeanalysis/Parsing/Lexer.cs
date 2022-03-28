using Backlang.Codeanalysis.Core;
using Backlang.Codeanalysis.Core.Attributes;
using System.Reflection;

namespace Backlang.Codeanalysis.Parsing;

public class Lexer : BaseLexer
{
    private static readonly Dictionary<string, TokenType> _symbolTokens = new();

    static Lexer()
    {
        var typeValues = (TokenType[])Enum.GetValues(typeof(TokenType));

        foreach (var op in typeValues)
        {
            var attributes = op.GetType()
                        .GetField(Enum.GetName(op)).GetCustomAttributes<LexemeAttribute>(true);

            if (attributes != null && attributes.Any())
            {
                foreach (var attribute in attributes)
                {
                    _symbolTokens.Add(attribute.Lexeme, op);
                }
            }
        }

        _symbolTokens = new(_symbolTokens.OrderByDescending(_ => _.Key.Length));
    }

    protected override Token NextToken()
    {
        SkipWhitespaces();
        SkipComments();

        if (_position >= _source.Length)
        {
            return new Token(TokenType.EOF, "\0", _position, _position, _line, _column);
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
        else if (IsMatch("0x"))
        {
            _position += 2;
            _column += 2;

            var oldpos = _position;
            var oldcolumn = _column;

            while (IsHex(Current()) || Current() == '_')
            {
                Advance();
                _column++;
            }

            return new Token(TokenType.HexNumber, _source.Substring(oldpos, _position - oldpos).Replace("_", string.Empty), oldpos, _position, _line, oldcolumn);
        }
        else if (IsMatch("0b"))
        {
            _position += 2;
            _column += 2;

            var oldpos = _position;
            var oldcolumn = _column;

            while (IsBinaryDigit(Current()) || Current() == '_')
            {
                Advance();
                _column++;
            }

            return new Token(TokenType.BinNumber, _source.Substring(oldpos, _position - oldpos).Replace("_", string.Empty), oldpos, _position, _line, oldcolumn);
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
        else if (IsIdentifierStartDigit())
        {
            var oldpos = _position;

            var oldcolumn = _column;
            while (IsIdentifierDigit())
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
                    var oldpos = _position;

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

    private static bool IsBinaryDigit(char c)
    {
        return c == '0' || c == '1';
    }

    private static bool IsHex(char c)
    {
        return c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F';
    }

    private bool IsIdentifierDigit()
    {
        return char.IsLetterOrDigit(Current()) || Current() == '_';
    }

    private bool IsIdentifierStartDigit()
    {
        return char.IsLetter(Current()) || Current() == '_';
    }

    private bool IsMatch(string token)
    {
        bool result = Peek(0) == token[0];

        for (int i = 1; i < token.Length; i++)
        {
            if (result)
            {
                result = result && Peek(i) == token[i];
            }
        }

        return result;
    }

    private void SkipComments()
    {
        if (IsMatch("//"))
        {
            Advance();
            Advance();
            _column++;
            _column++;

            while (Current() != '\n' && Current() != '\r' && Current() != '\0')
            {
                Advance();
                _column++;
            }

            if (Current() == '\n' || Current() == '\r')
            {
                Advance();
                _column++;
            }
        }
        else if (IsMatch("/*"))
        {
            int oldcol = _column;

            Advance();
            Advance();
            _column++;
            _column++;

            while (!IsMatch("*/") && Current() != '\0')
            {
                Advance();
            }

            if (Current() == '\0')
            {
                Messages.Add(Message.Error("Multiline comment is not closed.", _line, oldcol));
            }

            if (IsMatch("*/"))
            {
                Advance();
                _column++;

                Advance();
                _column++;
            }

            SkipWhitespaces();
        }
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