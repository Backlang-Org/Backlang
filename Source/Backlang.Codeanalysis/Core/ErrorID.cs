﻿namespace Backlang.Codeanalysis.Core;

public enum ErrorID : int
{
    UnexpecedType               = 0001,
    InvalidModifierCombination  = 0002,
    UnknownCharacter            = 0003,
    UnterminatedCharLiteral     = 0004,
    UnknownExpression           = 0005,
    UnknownLiteral              = 0006,
    ForbiddenTrailingComma      = 0007,
    BitfieldNotLiteral          = 0008,
    UnexpecedTypeMember         = 0009,
    ExpectedTypeLiteral         = 0010,
    UnknownSwitchOption         = 0011,
    NoCatchBlock                = 0012,
    EmptyFile                   = 0013,
    ExpectedIdentifier          = 0014,
    UnterminatedStringLiteral   = 0015,
    NotClosedMultilineComment   = 0016,
    Expected                    = 0017,
    DuplicateModifier           = 0018,
    NamespaceAlreadyImported    = 0019,
    RunnableTypeButNoEntrypoint = 0020,
    CannotBeFoundDidYouMean     = 0021,
    CannotImplementTypeNotFound = 0022,
    CannotImplementUnitType     = 0023,
    AlreadyDeclared             = 0024,
    TypeMismatch                = 0025,
    UnitTypeMismatch            = 0026,
    CannotBeResolved            = 0027,
    CannotFindFunction          = 0028,
    TargetNotFound              = 0029,
}