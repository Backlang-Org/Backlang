namespace Backlang_Compiler.Parsing;

public static class TokenUtils
{
    public static TokenType GetTokenType(string name)
    {
        return name switch
        {
            "true" => TokenType.TrueLiteral,
            "false" => TokenType.FalseLiteral,

            "declare" => TokenType.Declare,

            _ => TokenType.Identifier,
        };
    }
}