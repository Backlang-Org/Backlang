using Backlang_Compiler.Core;

namespace Backlang_Compiler.Parsing;

public enum TokenType
{
    Invalid,
    EOF,
    Identifier,
    StringLiteral,
    Number,

    Dot,

    [BinaryOperatorInfo(4)]
    Plus,

    [PreUnaryOperatorInfo(6)]
    [BinaryOperatorInfo(4)]
    Minus,

    [BinaryOperatorInfo(5)]
    Slash,

    [BinaryOperatorInfo(5)]
    Star,

    OpenParen,
    CloseParen,

    [PreUnaryOperatorInfo(6)]
    Exclamation,

    Colon,

    [Keyword("true")]
    TrueLiteral,

    [Keyword("false")]
    FalseLiteral,

    [BinaryOperatorInfo(2)]
    Comma,

    EqualsEquals,
    EqualsToken,
    SwapOperator,

    [Keyword("declare")]
    Declare,

    Semicolon,
}