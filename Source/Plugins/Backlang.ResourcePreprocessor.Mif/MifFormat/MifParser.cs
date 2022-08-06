using Backlang.ResourcePreprocessor.Mif.MifFormat.AST;

namespace Backlang.ResourcePreprocessor.Mif.MifFormat;

public enum TokenType
{
    EOF,
    Identifier,
    Colon,
    Eq,
    Semicolon,
    DotDot,
    CloseSquare,
    OpenSquare,
}

public class MifParser
{
    private static Tokenizer<TokenType> tokenizer;

    static MifParser()
    {
        tokenizer = new Tokenizer<TokenType>(TokenType.EOF)
            .Token(TokenType.Identifier, @"[A-Z_]+")
            .Token(TokenType.Colon, @":")
            .Token(TokenType.Semicolon, @";")
            .Token(TokenType.DotDot, @"..")
            .Token(TokenType.OpenSquare, @"\[")
            .Token(TokenType.CloseSquare, @"\]")
            .Token(TokenType.Eq, @"=");
    }

    public static MifFile Parse(string src)
    {
        var tokens = tokenizer.Tokenize(src);

        return null;
    }
}