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

    [Lexeme("&")]
    [PreUnaryOperatorInfo(9)]
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

    [BinaryOperatorInfo(4)]
    [Lexeme("and")]
    [Lexeme("&&")]
    And,

    [BinaryOperatorInfo(5)]
    [Lexeme("or")]
    [Lexeme("||")]
    Or,

    [PreUnaryOperatorInfo(6)]
    [Lexeme("!")]
    Exclamation,

    [Lexeme("=")]
    [BinaryOperatorInfo(8)]
    EqualsToken,

    [Lexeme("#")]
    [PreUnaryOperatorInfo(10)]
    Hash,

    [Keyword("PTR")]
    [PreUnaryOperatorInfo(1)]
    PTR,

    [Lexeme("<")]
    [BinaryOperatorInfo(5)]
    LessThan,

    [Lexeme(">")]
    [BinaryOperatorInfo(5)]
    GreaterThan,

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

    [Lexeme("$")]
    Dollar,

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

    [Keyword("bitfield")]
    Bitfield,

    [Keyword("register")]
    Register,

    [Keyword("default")]
    Default,

    [Keyword("sizeof")]
    SizeOf,

    [Keyword("none")]
    None,

    [Keyword("type")]
    Type,

    [Keyword("if")]
    If,

    [Keyword("else")]
    Else,

    [Keyword("while")]
    While,

    [Keyword("in")]
    In,

    [Keyword("for")]
    For,

    [Keyword("const")]
    Const,

    [Keyword("global")]
    Global,

    [Keyword("asm")]
    Asm,
}