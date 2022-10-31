namespace Backlang.Codeanalysis.Core;

public enum ErrorID : int
{
    UnexpecedType = 1,
    InvalidModifierCombination = 2,
    UnknownCharacter = 3,
    UnterminatedCharLiteral = 4,
    UnknownExpression = 5,
    UnknownLiteral = 6,
    ForbiddenTrailingComma = 7,
    BitfieldNotLiteral = 8,
    UnexpecedTypeMember = 9,
    ExpectedTypeLiteral = 10,
    UnknownSwitchOption = 11,
    NoCatchBlock = 12,
    EmptyFile = 13,
    ExpectedIdentifier = 14,
    UnterminatedStringLiteral = 15,
    NotClosedMultilineComment = 16,
    Expected = 17,
    DuplicateModifier = 18
}