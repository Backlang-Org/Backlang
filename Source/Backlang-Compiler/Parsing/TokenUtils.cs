namespace Backlang_Compiler.Parsing;

public static class TokenUtils
{
    public static TokenType GetTokenType(string name)
    {
        return name switch
        {
            "negate" => TokenType.Minus,
            "true" => TokenType.TrueLiteral,
            "false" => TokenType.FalseLiteral,

            _ => TokenType.Identifier,
        };
    }
}