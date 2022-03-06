namespace Backlang_Compiler.Parsing;

public enum TokenType
{
    Invalid,
    EOF,
    Identifier,
    StringLiteral,
    Number,

    Dot,
    Plus,
    Minus,
    Slash,
    Star,
    OpenParen,
    CloseParen,
    Exclamation,
    Colon,

    TrueLiteral,
    FalseLiteral,

    Comma,
    EqualsEquals,
    EqualsToken,
    SwapOperator,
}