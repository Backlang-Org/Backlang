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
    [BinaryOperatorInfo(BinaryOpPrecedences.FunctionCalls)]
    Dot,

    [Lexeme("::")]
    [BinaryOperatorInfo(BinaryOpPrecedences.FunctionCalls)]
    ColonColon,

    [BinaryOperatorInfo(BinaryOpPrecedences.DashedOps)]
    [Lexeme("+")]
    Plus,

    [BinaryOperatorInfo(BinaryOpPrecedences.Range)]
    [Lexeme("..")]
    RangeOperator,

    [PreUnaryOperatorInfo(UnaryOpPrecedences.Minus)]
    [BinaryOperatorInfo(BinaryOpPrecedences.DashedOps)]
    [Lexeme("-")]
    Minus,

    [Lexeme("&")]
    [PreUnaryOperatorInfo(UnaryOpPrecedences.Ampersand)]
    [BinaryOperatorInfo(BinaryOpPrecedences.Ampersand)]
    Ampersand,

    [Lexeme("^")]
    [BinaryOperatorInfo(BinaryOpPrecedences.Hat)]
    [PreUnaryOperatorInfo(UnaryOpPrecedences.Hat)]
    Hat,

    [BinaryOperatorInfo(BinaryOpPrecedences.DottedOps)]
    [Lexeme("/")]
    Slash,

    [BinaryOperatorInfo(BinaryOpPrecedences.DottedOps)]
    [Lexeme("*")]
    Star,

    [BinaryOperatorInfo(BinaryOpPrecedences.Percent)]
    [Lexeme("%")]
    Percent,

    [BinaryOperatorInfo(BinaryOpPrecedences.And)]
    [Lexeme("and")]
    [Lexeme("&&")]
    And,

    [BinaryOperatorInfo(BinaryOpPrecedences.Or)]
    [Lexeme("or")]
    [Lexeme("||")]
    Or,

    [PreUnaryOperatorInfo(UnaryOpPrecedences.Negative)]
    [Lexeme("!")]
    Exclamation,

    [Lexeme("*=")]
    [BinaryOperatorInfo(BinaryOpPrecedences.OperationShortcuts)]
    StarEqualsToken,

    [Lexeme("/=")]
    [BinaryOperatorInfo(BinaryOpPrecedences.OperationShortcuts)]
    DivEqualsToken,

    [Lexeme("+=")]
    [BinaryOperatorInfo(BinaryOpPrecedences.OperationShortcuts)]
    PlusEqualsToken,

    [Lexeme("-=")]
    [BinaryOperatorInfo(BinaryOpPrecedences.OperationShortcuts)]
    MinusEqualsToken,

    [Lexeme("=")]
    [BinaryOperatorInfo(BinaryOpPrecedences.Equals)]
    EqualsToken,

    [Lexeme("#")]
    Hash,

    [Lexeme("<=")]
    [BinaryOperatorInfo(BinaryOpPrecedences.Comparisons)]
    LessThanEqual,

    [Lexeme("<")]
    [BinaryOperatorInfo(BinaryOpPrecedences.Comparisons)]
    LessThan,

    [Lexeme(">")]
    [BinaryOperatorInfo(BinaryOpPrecedences.Comparisons)]
    GreaterThan,

    [Lexeme(">=")]
    [BinaryOperatorInfo(BinaryOpPrecedences.Comparisons)]
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
    [PreUnaryOperatorInfo(UnaryOpPrecedences.Dollar)]
    Dollar,

    [Lexeme("==")]
    [BinaryOperatorInfo(BinaryOpPrecedences.EqualsEquals)]
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

    // Unary Ops for Literals
    [Lexeme("u")]
    LiteralOPUnsigned

}