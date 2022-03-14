using Backlang.Codeanalysis.Core.Attributes;

namespace Backlang.Codeanalysis.Parsing;

public enum TokenType
{
    Invalid,
    EOF,
    Identifier,
    StringLiteral,
    Number,
    HexNumber,
    BinNumber,

    [Lexeme(".")]
    Dot,

    [BinaryOperatorInfo(4)]
    [Lexeme("+")]
    Plus,

    [BinaryOperatorInfo(4)]
    [Lexeme("..")]
    RangeOperator,

    [PreUnaryOperatorInfo(6)]
    [BinaryOperatorInfo(4)]
    [Lexeme("-")]
    Minus,

    [PreUnaryOperatorInfo(9)]
    [Lexeme("&")]
    Ampersand,

    [BinaryOperatorInfo(5)]
    [Lexeme("/")]
    Slash,

    [BinaryOperatorInfo(5)]
    [Lexeme("*")]
    Star,

    [BinaryOperatorInfo(5)]
    [Lexeme("%")]
    Percent,

    [PreUnaryOperatorInfo(6)]
    [Lexeme("!")]
    Exclamation,

    [Lexeme("=")]
    [BinaryOperatorInfo(8)]
    EqualsToken,

    [Lexeme(":")]
    Colon,

    [Lexeme("(")]
    OpenParen,

    [Lexeme(")")]
    CloseParen,

    [Lexeme("{")]
    OpenCurly,

    [Lexeme("}")]
    CloseCurly,

    [Lexeme("->")]
    Arrow,

    [Lexeme("=>")]
    GoesTo,

    [Lexeme(",")]
    Comma,

    [Lexeme("==")]
    EqualsEquals,

    [Lexeme("<->")]
    SwapOperator,

    [Lexeme("_")]
    Underscore,

    [Lexeme(";")]
    Semicolon,

    [Lexeme("[")]
    OpenSquare,

    [Lexeme("]")]
    CloseSquare,

    [Keyword("true")]
    TrueLiteral,

    [Keyword("false")]
    FalseLiteral,

    [Keyword("fn")]
    Function,

    [Keyword("let")]
    [Keyword("declare")]
    Declare,

    [Keyword("mut")]
    [Keyword("mutable")]
    Mutable,

    [Keyword("enum")]
    Enum,

    [Keyword("with")]
    With,

    [Keyword("match")]
    Match,

    [Keyword("struct")]
    Struct,
}