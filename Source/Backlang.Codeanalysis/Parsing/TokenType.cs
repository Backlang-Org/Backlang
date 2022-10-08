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
    [BinaryOperatorInfo(BinaryOpPrecedences.Dot)]
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

    [PreUnaryOperatorInfo(UnaryOpPrecedences.Negate)]
    [BinaryOperatorInfo(BinaryOpPrecedences.DashedOps)]
    [Lexeme("-")]
    Minus,

    [Lexeme("&")]
    [PreUnaryOperatorInfo(UnaryOpPrecedences.Ampersand)]
    [BinaryOperatorInfo(BinaryOpPrecedences.Ampersand)]
    Ampersand,

    [Lexeme("|")]
    [BinaryOperatorInfo(BinaryOpPrecedences.Ampersand)]
    Pipe,

    [Lexeme("~")]
    [PreUnaryOperatorInfo(UnaryOpPrecedences.LogicalNot)]
    Tilde,

    [Lexeme("^")]
    [BinaryOperatorInfo(BinaryOpPrecedences.Hat)]
    [PreUnaryOperatorInfo(UnaryOpPrecedences.Hat)]
    Hat,

    [BinaryOperatorInfo(BinaryOpPrecedences.DottedOps)]
    [Lexeme("/")]
    Slash,

    [BinaryOperatorInfo(BinaryOpPrecedences.DottedOps)]
    [PreUnaryOperatorInfo(UnaryOpPrecedences.Hat)]
    [Lexeme("*")]
    Star,

    [BinaryOperatorInfo(BinaryOpPrecedences.Hat)]
    [Lexeme("**")]
    StarStar,

    [BinaryOperatorInfo(BinaryOpPrecedences.Percent)]
    [PostUnaryOperatorInfo(UnaryOpPrecedences.Literals)]
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

    [PreUnaryOperatorInfo(UnaryOpPrecedences.LogicalNot)]
    [Lexeme("!")]
    Exclamation,

    [Lexeme("*=")]
    [Lexeme("/=")]
    [Lexeme("+=")]
    [Lexeme("-=")]
    [Lexeme("|=")]
    [Lexeme("&=")]
    [BinaryOperatorInfo(BinaryOpPrecedences.OperationShortcuts)]
    EqualsShortcutToken,

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

    [Lexeme("|>")]
    [BinaryOperatorInfo(BinaryOpPrecedences.PipeOperator)]
    PipeOperator,

    [Lexeme(",")]
    Comma,

    [Lexeme("$")]
    [PreUnaryOperatorInfo(UnaryOpPrecedences.Dollar)]
    Dollar,

    [Lexeme("==")]
    [BinaryOperatorInfo(BinaryOpPrecedences.EqualsEquals)]
    EqualsEquals,

    [Lexeme("!=")]
    [BinaryOperatorInfo(BinaryOpPrecedences.EqualsEquals)]
    NotEquals,

    [Lexeme("<->")]
    [BinaryOperatorInfo(BinaryOpPrecedences.SwapOperator)]
    SwapOperator,

    [Lexeme("_")]
    Underscore,

    [Lexeme(";")]
    Semicolon,

    [Lexeme("[")]
    OpenSquare,

    [Lexeme("]")]
    CloseSquare,

    [Lexeme("@")]
    At,

    [Lexeme("as")]
    [BinaryOperatorInfo(BinaryOpPrecedences.Casting)]
    As,

    [Keyword("true")]
    TrueLiteral,

    [Keyword("false")]
    FalseLiteral,

    [Keyword("type")]
    Type,

    [Keyword("func")]
    Function,

    [Keyword("constructor")]
    Constructor,

    [Keyword("destructor")]
    Destructor,

    [Keyword("macro")]
    Macro,

    [Keyword("let")]
    Let,

    [Keyword("prop")]
    Property,

    [Keyword("mut")]
    Mutable,

    [Keyword("enum")]
    Enum,

    [Keyword("try")]
    Try,

    [Keyword("catch")]
    Catch,

    [Keyword("finally")]
    Finally,

    [Keyword("with")]
    With,

    [Keyword("match")]
    Match,

    [Keyword("struct")]
    Struct,

    [Keyword("class")]
    Class,

    [Keyword("interface")]
    Interface,

    [Keyword("bitfield")]
    Bitfield,

    [Keyword("default")]
    Default,

    [Keyword("sizeof")]
    SizeOf,

    [Keyword("none")]
    None,

    [Keyword("get")]
    Get,

    [Keyword("set")]
    Set,

    [Keyword("init")]
    Init,

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

    [Keyword("where")]
    Where,

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

    [Keyword("abstract")]
    Abstract,

    [Keyword("extern")]
    Extern,

    [Keyword("override")]
    Override,

    [Keyword("of")]
    Of,

    [Keyword("implement")]
    Implement,

    [Keyword("operator")]
    Operator,

    [Keyword("public")]
    Public,

    [Keyword("internal")]
    Internal,

    [Keyword("protected")]
    Protected,

    [Keyword("private")]
    Private,

    [Keyword("import")]
    Import,

    [Keyword("module")]
    Module,

    [Keyword("using")]
    Using,

    [Keyword("union")]
    Union,

    [Keyword("throw")]
    Throw,

    [Keyword("unit")]
    Unit,
}