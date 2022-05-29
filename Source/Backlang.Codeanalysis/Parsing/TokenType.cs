using Backlang.Codeanalysis.Core.Attributes;
using Backlang.Codeanalysis.Parsing.Precedences;

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
    CharLiteral,

    [Lexeme(".")]
    [BinaryOperatorInfo(7)]
    Dot,

    [Lexeme("::")]
    [BinaryOperatorInfo(7)]
    ColonColon,

    [BinaryOperatorInfo(4)]
    [Lexeme("+")]
    Plus,

    [BinaryOperatorInfo(4)]
    [Lexeme("..")]
    RangeOperator,

    [PreUnaryOperatorInfo(UnaryOpPrecedences.NEGATIVE_OR_MINUS)]
    [BinaryOperatorInfo(4)]
    [Lexeme("-")]
    Minus,

    [Lexeme("&")]
    [PreUnaryOperatorInfo(UnaryOpPrecedences.AMPERSAND_OR_HAT)]
    [BinaryOperatorInfo(3)]
    Ampersand,

    [Lexeme("^")]
    [BinaryOperatorInfo(2)]
    [PreUnaryOperatorInfo(UnaryOpPrecedences.AMPERSAND_OR_HAT)]
    Hat,

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

    [PreUnaryOperatorInfo(UnaryOpPrecedences.NEGATIVE_OR_MINUS)]
    [Lexeme("!")]
    Exclamation,

    [Lexeme("*=")]
    [BinaryOperatorInfo(8)]
    StarEqualsToken,

    [Lexeme("/=")]
    [BinaryOperatorInfo(8)]
    DivEqualsToken,

    [Lexeme("+=")]
    [BinaryOperatorInfo(8)]
    PlusEqualsToken,

    [Lexeme("-=")]
    [BinaryOperatorInfo(8)]
    MinusEqualsToken,

    [Lexeme("=")]
    [BinaryOperatorInfo(8)]
    EqualsToken,

    [Lexeme("#")]
    Hash,

    [Lexeme("<=")]
    [BinaryOperatorInfo(5)]
    LessThanEqual,

    [Lexeme("<")]
    [BinaryOperatorInfo(5)]
    LessThan,

    [Lexeme(">")]
    [BinaryOperatorInfo(5)]
    GreaterThan,

    [Lexeme(">=")]
    [BinaryOperatorInfo(5)]
    GreaterThanEqual,

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
    [PreUnaryOperatorInfo(UnaryOpPrecedences.DOLLAR)]
    Dollar,

    [Lexeme("==")]
    [BinaryOperatorInfo(4)]
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

    [Keyword("func")]
    Function,

    [Keyword("let")]
    Declare,

    [Keyword("mut")]
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

    [Keyword("default")]
    Default,

    [Keyword("sizeof")]
    SizeOf,

    [Keyword("none")]
    None,

    [Keyword("type")]
    Type,

    [Keyword("switch")]
    Switch,

    [Keyword("case")]
    Case,

    [Keyword("break")]
    Break,

    [Keyword("continue")]
    Continue,

    [Keyword("return")]
    Return,

    [Keyword("when")]
    When,

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

    [Keyword("static")]
    Static,

    [Keyword("of")]
    Of,

    [Keyword("implement")]
    Implement,

    [Keyword("operator")]
    Operator,

    [Keyword("private")]
    Private,

    [Keyword("import")]
    Import,

    [Keyword("module")]
    Module,
}