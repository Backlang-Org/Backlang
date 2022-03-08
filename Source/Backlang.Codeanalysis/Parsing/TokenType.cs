using Backlang.Codeanalysis.Core.Attributes;

namespace Backlang.Codeanalysis.Parsing;

public enum TokenType
{
    Invalid,
    EOF,
    Identifier,
    StringLiteral,
    Number,

    [Lexeme(".")]
    Dot,

    [BinaryOperatorInfo(4)]
    [Lexeme("+")]
    Plus,

    [PreUnaryOperatorInfo(6)]
    [BinaryOperatorInfo(4)]
    [Lexeme("-")]
    Minus,

    [BinaryOperatorInfo(5)]
    [Lexeme("/")]
    Slash,

    [BinaryOperatorInfo(5)]
    [Lexeme("*")]
    Star,

    [BinaryOperatorInfo(5)]
    [Lexeme("%")]
    Percent,

    [Lexeme("(")]
    OpenParen,

    [Lexeme(")")]
    CloseParen,

    [PreUnaryOperatorInfo(6)]
    [Lexeme("!")]
    Exclamation,

    [Lexeme(":")]
    Colon,

    [Keyword("true")]
    TrueLiteral,

    [Keyword("false")]
    FalseLiteral,

    [BinaryOperatorInfo(2)]
    [Lexeme(",")]
    Comma,

    [Lexeme("==")]
    EqualsEquals,

    [Lexeme("=")]
    [BinaryOperatorInfo(8)]
    EqualsToken,

    [Lexeme("<->")]
    SwapOperator,

    [Keyword("declare")]
    Declare,

    [Lexeme(";")]
    Semicolon,
    HexNumber,
    BinNumber,
}
