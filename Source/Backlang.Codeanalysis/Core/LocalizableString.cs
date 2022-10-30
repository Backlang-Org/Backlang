using Backlang.Codeanalysis.Properties;
using System.Resources;

namespace Backlang.Codeanalysis.Core;

public readonly struct LocalizableString
{
    public readonly ErrorID ErrorID;
    public readonly string[] Arguments;
    public readonly string FallbackValue;

    private static readonly ResourceManager resourceManager = new ResourceManager("Backlang.Codeanalysis.Properties.Resources", typeof(Resources).Assembly);

    public LocalizableString(ErrorID errorID, params string[] arguments)
    {
        ErrorID = errorID;
        Arguments = arguments;
    }

    public LocalizableString(ErrorID errorID) : this()
    {
        ErrorID = errorID;
        Arguments = new[] { "NO_VALUE" };
    }

    public LocalizableString(string fallbackValue) : this()
    {
        FallbackValue = fallbackValue;
    }

    public static implicit operator string(LocalizableString lstr)
    {
        if (!string.IsNullOrEmpty(lstr.FallbackValue))
        {
            return lstr.FallbackValue;
        }

        var resourceID = "BL" + (int)lstr.ErrorID;

        return string.Format(resourceID + ": " + resourceManager.GetString(resourceID), args: lstr.Arguments);
    }

    public static implicit operator LocalizableString(ErrorID id)
    {
        return new(id);
    }

    public static implicit operator LocalizableString(string message)
    {
        return new(message);
    }
}

public enum ErrorID : int
{
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
    UnterminatedLiteral = 15,
    NotClosedMultilineComment = 16,
    Expected = 17,
    DuplicateModifier = 18,
    UnexpecedType = 19
}