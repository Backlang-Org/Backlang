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
    Not,
    Colon,

    TrueLiteral,
    FalseLiteral,

    Comma,
}