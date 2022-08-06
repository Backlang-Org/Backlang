﻿using Backlang.ResourcePreprocessor.Mif.MifFormat.AST;
using Backlang.ResourcePreprocessor.Mif.MifFormat.AST.DataRules;

namespace Backlang.ResourcePreprocessor.Mif.MifFormat;

public class MifParser
{
    private static Tokenizer<TokenType> tokenizer;

    private int _position;

    private Token<TokenType>[] tokens;

    static MifParser()
    {
        tokenizer = new Tokenizer<TokenType>(TokenType.Invalid)
            .Token(TokenType.ContentBegin, @"CONTENT BEGIN")
            .Token(TokenType.Width, @"WIDTH")
            .Token(TokenType.DEPTH, @"DEPTH")
            .Token(TokenType.ADDRESS_RADIX, @"ADDRESS_RADIX")
            .Token(TokenType.DATA_RADIX, @"DATA_RADIX")
            .Token(TokenType.Radix, string.Join('|', Enum.GetNames<Radix>()))

            .Token(TokenType.Number, @"[0-9A-F]+")
            .Token(TokenType.Colon, @":")
            .Ignore(TokenType.Comment, @"--.+")
            .Token(TokenType.Semicolon, @";")
            .Token(TokenType.DotDot, @"\.\.")
            .Token(TokenType.OpenSquare, @"\[")
            .Token(TokenType.CloseSquare, @"\]")
            .Token(TokenType.Eq, @"=");
    }

    public static MifFile Parse(string src)
    {
        var p = new MifParser();
        p.tokens = tokenizer.Tokenize(src)
            .Where(_ => _.Type != TokenType.Invalid)
            .ToArray();

        var result = p.ParseTranslationUnit();

        p.IsMatch(TokenType.EOF);

        return result;
    }

    private MifFile ParseTranslationUnit()
    {
        var file = new MifFile();

        ParseHeader(file);
        ParseContent(file);

        return file;
    }

    private void ParseContent(MifFile file)
    {
        Match(TokenType.ContentBegin);

        ParseDataRules(file);
    }

    private void ParseDataRules(MifFile file)
    {
        while (!IsMatch(TokenType.EOF))
        {
            ParseDataRule(file);
        }
    }

    private void ParseDataRule(MifFile file)
    {
        if (IsMatch(TokenType.Number))
        {
            ParseSimpleDataRule(file);
        }
    }

    private void ParseSimpleDataRule(MifFile file)
    {
        var addr = ParseNumber((Radix)file.Options["ADDRESS_RADIX"]);

        Match(TokenType.Colon);
        var value = ParseNumber((Radix)file.Options["DATA_RADIX"]);

        Match(TokenType.Semicolon);

        file.DataRules.Add(new SimpleDataRule(addr, value));
    }

    private int ParseNumber(Radix radix)
    {
        var token = NextToken();

        switch (radix)
        {
            case Radix.BIN:
                return Convert.ToInt32(token.Value, 2);

            case Radix.OCT:
                return Convert.ToInt32(token.Value, 8);

            case Radix.HEX:
                return Convert.ToInt32(token.Value, 16);

            case Radix.DEC:
                return Convert.ToInt32(token.Value, 10);

            case Radix.UNS:
                return Convert.ToInt32(token.Value, 10);

            default:
                return 0;
        }
    }

    private void ParseHeader(MifFile file)
    {
        while (!IsMatch(TokenType.ContentBegin))
        {
            ParserOption(file);
        }
    }

    private void ParserOption(MifFile file)
    {
        var keyToken = Peek(0);
        var key = ParseOptionName();

        Match(TokenType.Eq);

        object value = null;
        if (keyToken.Type == TokenType.Width || keyToken.Type == TokenType.DEPTH)
        {
            value = ParseNumber();
        }
        else if (keyToken.Type == TokenType.DATA_RADIX || keyToken.Type == TokenType.ADDRESS_RADIX)
        {
            value = ParseRadix();
        }

        if (value != null)
            file.Options.Add(key, value);

        Match(TokenType.Semicolon);
    }

    private string ParseOptionName()
    {
        if (IsMatch(TokenType.Width, TokenType.DEPTH, TokenType.ADDRESS_RADIX, TokenType.DATA_RADIX))
        {
            return NextToken().Value;
        }

        return null;
    }

    private int ParseNumber()
    {
        var token = NextToken();

        if (int.TryParse(token.Value, out var iR))

            return iR;
        else
            return Convert.ToInt32(token.Value, 16);
    }

    private Radix ParseRadix()
    {
        var token = NextToken();

        return Enum.Parse<Radix>(token.Value, true);
    }

    private Token<TokenType> Match(TokenType tokenType)
    {
        if (IsMatch(tokenType))
        {
            return NextToken();
        }

        return new Token<TokenType>(TokenType.EOF, string.Empty);
    }

    private Token<TokenType> NextToken()
    {
        if (_position < tokens.Length)
        {
            return tokens[_position++];
        }

        return new Token<TokenType>(TokenType.EOF, string.Empty);
    }

    private Token<TokenType> Peek(int index)
    {
        if (_position + index < tokens.Length)
        {
            return tokens[_position + index];
        }

        return new Token<TokenType>(TokenType.EOF, string.Empty);
    }

    private bool IsMatch(params TokenType[] types)
    {
        var token = Peek(0);

        foreach (var t in types)
        {
            if (token.Type == t) return true;
        }

        return false;
    }
}